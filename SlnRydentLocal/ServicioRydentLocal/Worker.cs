
using AutoMapper.Execution;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Serialization;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.TablasFraccionadas.TAnamnesis;
using ServicioRydentLocal.LogicaDelNegocio.Facturatech;
using ServicioRydentLocal.LogicaDelNegocio.Helpers;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico.Resultados;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico.Solicitudes;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Whatsap;
using ServicioRydentLocal.LogicaDelNegocio.Repositorio;
using ServicioRydentLocal.LogicaDelNegocio.Services;
using ServicioRydentLocal.LogicaDelNegocio.Services.Dataico;
using ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad;
using ServicioRydentLocal.LogicaDelNegocio.Services.Rips;
using ServicioRydentLocal.LogicaDelNegocio.Services.TAnamnesis;
using SixLabors.ImageSharp;
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;
using STJ = System.Text.Json.JsonSerializer;


public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private HubConnection _hubConnection;
    private readonly IConfiguration _configuration;
    private readonly LNRips _lnRips;
	private readonly ApiIntermediaClient _api;
	private readonly IEstadoCuentaService _estadoCuentaService;

	// private readonly AppDbContext _dbContext;

	private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1); //esto es reciente 16/08/24
	private CancellationTokenSource? _heartbeatCts;
	private Task? _heartbeatTask;

	// ✅ Método único y seguro para asegurar conexión (anti doble start) 17-02-2026
	private async Task EnsureConnectedAsync(CancellationToken ct = default)
	{
		// Si ya está conectando/reconectando/conectado, no hacemos nada
		if (_hubConnection.State == HubConnectionState.Connected ||
			_hubConnection.State == HubConnectionState.Connecting ||
			_hubConnection.State == HubConnectionState.Reconnecting)
			return;

		await _connectionLock.WaitAsync(ct);
		try
		{
			// Re-check por si otro hilo ya conectó mientras esperábamos el lock
			if (_hubConnection.State == HubConnectionState.Connected ||
				_hubConnection.State == HubConnectionState.Connecting ||
				_hubConnection.State == HubConnectionState.Reconnecting)
				return;

			await _hubConnection.StartAsync(ct);
			_logger.LogInformation("Worker connected to SignalR hub.");
		}
		finally
		{
			_connectionLock.Release();
		}
	}




	// Constructor: Recibe una instancia de ILogger para realizar el registro de eventos.
	public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration, ApiIntermediaClient api)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
		_api = api;
		_lnRips = new LNRips(_configuration); // ✅ Pasamos IConfiguration
		string url = _configuration.GetValue<string>("signalRServer:url");
		_hubConnection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1) })
            .Build();

        _hubConnection.Reconnecting += error =>
        {
            _logger.LogWarning($"Reconnecting due to: {error?.Message}");  // Log de advertencia cuando se intenta reconectar
			_isDeviceRegistered = false; // ✅ al reconectar puede cambiar ConnectionId se agrego 17-02-2026
			return Task.CompletedTask;
        };

		
		_hubConnection.Reconnected += async (connectionId) =>
		{
			_logger.LogInformation($"Reconnected. Connection ID: {connectionId}");
			_isDeviceRegistered = false;

			try
			{
				using var scope = _scopeFactory.CreateScope();
				var srv = scope.ServiceProvider.GetRequiredService<ITDATOSCLIENTESServicios>();
				var datos = await srv.ConsultarPorId(Environment.MachineName);

				await RegisterDeviceAsync(retornarEntrada(datos.ENTRADA));
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error al registrar tras reconexión: {ex.Message}", ex);
			}
		};

		

		//remplaza el anteriro 17-02-2026
		_hubConnection.Closed += error =>
		{
			_logger.LogError($"Connection closed due to: {error?.Message}");

			// ✅ Marcamos que debe revalidar registro cuando vuelva a conectar
			_isDeviceRegistered = false;

			// ✅ NO reconectamos manualmente aquí
			// WithAutomaticReconnect + EnsureConnectedAsync (en el loop) se encargan.
			return Task.CompletedTask;
		};


		if (UseWhatsAppQueue())
		{
			_logger.LogInformation("WhatsApp Queue ENABLED: iniciando WhatsAppPump...");
			StartWhatsAppPump();
		}
		else
		{
			_logger.LogInformation("WhatsApp Queue DISABLED: envío directo (como siempre).");
		}
	}

	

	private void StartHeartbeatLoop(CancellationToken stoppingToken)
	{
		// Evita arrancar 2 veces
		if (_heartbeatTask != null) return;

		_heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

		_heartbeatTask = Task.Run(async () =>
		{
			var ct = _heartbeatCts.Token;

			// Jitter inicial para que no arranquen todos al mismo segundo
			await Task.Delay(Random.Shared.Next(1000, 5000), ct);

			while (!ct.IsCancellationRequested)
			{
				try
				{
					// Asegura conexión (anti doble start, respeta reconnect)
					await EnsureConnectedAsync(ct);

					if (_hubConnection.State == HubConnectionState.Connected)
					{
						// 🔥 Llamada liviana al HUB
						// Debe existir el método Heartbeat() en el hub
						await _hubConnection.InvokeAsync("Heartbeat", cancellationToken: ct);
					}

					// ✅ Intervalo 20–30 segundos con jitter
					var next = Random.Shared.Next(20, 31);
					await Task.Delay(TimeSpan.FromSeconds(next), ct);
				}
				catch (OperationCanceledException)
				{
					// normal al apagar
					break;
				}
				catch (Exception)
				{
					// ❌ Sin log spam: solo esperamos un poquito y seguimos
					await Task.Delay(TimeSpan.FromSeconds(10), ct);
				}
			}
		}, _heartbeatCts.Token);
	}



	private string retornarEntrada(string entra)
	{
		string? sede = _configuration.GetValue<string>("sedes:sede");
        return string.IsNullOrEmpty(sede) ? entra : entra + "::" + sede;

	}

	// Método para iniciar la conexión a SignalR
	
	private async Task StartConnectionAsync()
	{
		await EnsureConnectedAsync();
	}


	private bool UseWhatsAppQueue()
	{
		// si no existe en appsettings, queda false por defecto
		return _configuration?.GetValue<bool>("WhatsApp:UseQueue") ?? false;
	}

	private static string ArmarMensaje(string templateNombre, List<string> parametros)
	{
		if (string.IsNullOrWhiteSpace(templateNombre))
			templateNombre = "";

		if (parametros == null) parametros = new List<string>();

		string msg = templateNombre;
		for (int i = 0; i < parametros.Count; i++)
			msg = msg.Replace($"{{{i}}}", parametros[i] ?? "");

		return msg.Replace("\\n", "\n");
	}

	private async Task<bool> EnviarWhatsAppSeguro(
		WhatsAppService servicioWhatsApp,
		string haciaNumero,
		string templateNombre,
		List<string> parametros,
		string tipoJob, // "AGENDA" o "RECORDAR"
		DateTime? fecha = null,
		int? silla = null,
		TimeSpan? hora = null
	)
	{
		var mensajeFormateado = ArmarMensaje(templateNombre, parametros);

		if (string.IsNullOrWhiteSpace(haciaNumero))
			return false;

		// ✅ Lee bandera desde appsettings
		var useQueue = _configuration.GetValue<bool>("WhatsApp:UseQueue");

		if (useQueue)
		{
			try
			{
				var job = new WaJob
				{
					Numero = haciaNumero,
					Mensaje = mensajeFormateado,
					Tipo = tipoJob,
					Fecha = fecha,
					Silla = silla,
					Hora = hora
				};

				// AGENDA va a High, RECORDAR a Normal
				if (tipoJob == "AGENDA")
					WhatsAppQueue.High.Enqueue(job);
				else
					WhatsAppQueue.Normal.Enqueue(job);

				return true; // programado
			}
			catch
			{
				// si falla la cola, fallback al envío directo
			}
		}

		// ✅ fallback al comportamiento actual
		// si quieres, aquí puedes llamar EnviarMensajePlano con mensajeFormateado
		return await servicioWhatsApp.EnviarMensajePlano(haciaNumero, mensajeFormateado);
	}

	private void StartWhatsAppPump()
	{
		Task.Run(async () =>
		{
			_logger.LogInformation("WhatsAppPump iniciado.");

			var wa = new WhatsAppService();
			var citasSrv = new TDETALLECITASServicios();

			int enviadosEnLaHora = 0;
			var inicioHora = DateTime.UtcNow;

			// 🔧 Settings desde appsettings.json
			var maxPerHour = _configuration.GetValue<int?>("WhatsApp:MaxPerHour") ?? 10;
			var delayAgendaMin = _configuration.GetValue<int?>("WhatsApp:DelayAgendaMinMs") ?? 2000;
			var delayAgendaMax = _configuration.GetValue<int?>("WhatsApp:DelayAgendaMaxMs") ?? 6000;
			var delayMassMin = _configuration.GetValue<int?>("WhatsApp:DelayMassMinMs") ?? 20000;
			var delayMassMax = _configuration.GetValue<int?>("WhatsApp:DelayMassMaxMs") ?? 40000;

			while (true)
			{
				try
				{
					// 🔁 Reset del contador cada hora
					if (DateTime.UtcNow - inicioHora >= TimeSpan.FromHours(1))
					{
						inicioHora = DateTime.UtcNow;
						enviadosEnLaHora = 0;
						_logger.LogInformation("WhatsAppPump: contador horario reiniciado.");
					}

					// ⛔ Límite por hora
					if (enviadosEnLaHora >= maxPerHour)
					{
						_logger.LogWarning($"WhatsAppPump: límite horario alcanzado ({maxPerHour}). Pausando 5 minutos...");
						await Task.Delay(TimeSpan.FromMinutes(5));
						continue;
					}

					// 📥 1) Prioridad ALTA (AGENDAR / REAGENDAR)
					if (!WhatsAppQueue.High.TryDequeue(out var job))
					{
						// 📥 2) Prioridad NORMAL (RECORDAR)
						WhatsAppQueue.Normal.TryDequeue(out job);
					}

					// 😴 Nada que hacer
					if (job == null)
					{
						await Task.Delay(1000);
						continue;
					}

					_logger.LogInformation($"WhatsAppPump: procesando job Tipo={job.Tipo} Numero={job.Numero}");

					// 🔍 Revalidación SOLO para RECORDAR
					if (job.Tipo == "RECORDAR" &&
						job.Fecha.HasValue &&
						job.Silla.HasValue &&
						job.Hora.HasValue)
					{
						var cita = await citasSrv.ConsultarPorFechaSillaHora(
							job.Fecha.Value.Date,
							job.Silla.Value,
							job.Hora.Value
						);

						// ❌ No existe o ya fue enviado
						if (cita == null || !cita.Any() || cita[0].CEDULA == "SI")
						{
							_logger.LogWarning("WhatsAppPump: cita no válida o ya enviada. Se omite.");
							continue;
						}
					}

					// ⏱️ Delay humano
					if (job.Tipo == "AGENDA")
					{
						var delay = Random.Shared.Next(delayAgendaMin, delayAgendaMax);
						await Task.Delay(delay);
					}
					else
					{
						var delay = Random.Shared.Next(delayMassMin, delayMassMax);
						await Task.Delay(delay);
					}

					// 📤 Envío real
					var ok = await wa.EnviarMensajePlano(job.Numero, job.Mensaje);

					if (ok)
					{
						enviadosEnLaHora++;

						_logger.LogInformation(
							$"WhatsAppPump: mensaje enviado OK a {job.Numero}. Total hora: {enviadosEnLaHora}/{maxPerHour}"
						);

						// 🟢 Marcar RECORDAR como enviado
						if (job.Tipo == "RECORDAR" &&
							job.Fecha.HasValue &&
							job.Silla.HasValue &&
							job.Hora.HasValue)
						{
							await citasSrv.ActualizarCampo(
								job.Fecha.Value.Date,
								job.Silla.Value,
								job.Hora.Value,
								"SI"
							);
						}
					}
					else
					{
						_logger.LogError($"WhatsAppPump: fallo al enviar mensaje a {job.Numero}");
					}
				}
				catch (Exception ex)
				{
					// 🧯 Nunca se cae el worker
					_logger.LogError(ex, "WhatsAppPump: error inesperado, se reintenta en 5s.");
					await Task.Delay(5000);
				}
			}
		});
	}



	private string TemplateAleatorioRecordar()
	{
		var list = new List<string>
	{
		"Hola {0} 👋\nTe recordamos tu cita el {1} a las {2} con {3}.",

		"Buen día {0} 😊\nQueremos recordarte tu cita programada para el {1} a las {2}.",

		"Hola {0}.\nRecuerda que tienes una cita el {1} a las {2} con {3}.",

		"¡Hola {0}!\n⏰ Recordatorio de tu cita\n📅 {1}\n🕒 {2}\n👨‍⚕️ {3}",

		"Hola {0},\nTe escribimos para recordarte tu cita el {1} a las {2}.",

		"Hola {0} 😊\nTu cita está prevista para el {1} a las {2}. Te esperamos.",

		"Buen día {0} 👋\nEste es un recordatorio de tu cita el {1} a las {2} con {3}.",

		"Hola {0},\nNo olvides tu cita agendada para el {1} a las {2}.",

		"¡Hola {0}!\nTe recordamos tu cita:\n📅 {1}\n⏰ {2}\n👨‍⚕️ {3}",

		"Hola {0} 👋\nQueremos confirmarte que tienes una cita el {1} a las {2}.",

		"Buen día {0}.\nTe esperamos en tu cita el {1} a las {2} con {3}.",

		"Hola {0}, 😊\nEste mensaje es para recordarte tu cita el {1} a las {2}.",

		"Hola {0} 👋\nTienes una cita programada para el {1} a las {2}.\n¡Te esperamos!",

		"¡Hola {0}!\nRecuerda tu cita el {1} a las {2} con {3}.",

		"Hola {0},\nTe enviamos un recordatorio amable de tu cita el {1} a las {2}."
	};

		return list[Random.Shared.Next(list.Count)];
	}


	private async Task registrarSuscripciones()
    {
        _hubConnection.On<string, string, int>("ObtenerPin", async (clientId, pin, maxIdAnamnesis) =>
        {
            await RecibirPinRydent(pin, clientId, maxIdAnamnesis);
        });

		_hubConnection.On<string, int>("RecibirLotePacientesAgenda", async (clientId, maxIdAnamnesis) =>
		{
			await RecibirLotePacientesAgenda(clientId, maxIdAnamnesis);
		});

		_hubConnection.On<string, string>("ObtenerDoctor", async (clientId, idDoctor) =>
        {
            await ConsultarPorIdDoctor(clientId, Convert.ToInt32(idDoctor));
        });

        _hubConnection.On<string, string>("ObtenerDoctorSiLoCambian", async (clientId, idDoctor) =>
        {
            await ObtenerDoctorSiLoCambian(clientId, Convert.ToInt32(idDoctor));
        });

        _hubConnection.On<string, string, string>("BuscarPaciente", async (clientId, tipoBuqueda, valorDeBusqueda) =>
        {
            await BuscarPaciente(valorDeBusqueda, Convert.ToInt32(tipoBuqueda), clientId);
        });

        _hubConnection.On<string, string>("BuscarCitasPacienteAgenda", async (clientId, valorBuscarAgenda) =>
        {
            await BuscarCitasPacienteAgenda(valorBuscarAgenda, clientId);
        });

        _hubConnection.On<string, string>("ObtenerDatosPersonalesCompletosPaciente", async (clientId, idAnanesis) =>
        {
            await BuscarDatosPersonalesCompletosPacientes(clientId, Convert.ToInt32(idAnanesis));
        });

        _hubConnection.On<string, string>("ObtenerAntecedentesPaciente", async (clientId, idAnanesis) =>
        {
            await BuscarAntecedentesPacientes(clientId, Convert.ToInt32(idAnanesis));
        });

        _hubConnection.On<string, string>("ObtenerDatosEvolucion", async (clientId, idAnanesis) =>
        {
            await ObtenerDatosEvolucion(clientId, Convert.ToInt32(idAnanesis));
        });

        _hubConnection.On<string, string>("GuardarDatosEvolucion", async (clientId, datosEvolucion) =>
        {
            Console.WriteLine("***************************");
            await GuardarDatosEvolucion(clientId, datosEvolucion);
        });

        _hubConnection.On<string, string>("GuardarDatosPersonales", async (clientId, datosPersonales) =>
        {
            Console.WriteLine("***************************");
            await GuardarDatosPersonales(clientId, datosPersonales);
        });

        _hubConnection.On<string, string>("EditarDatosPersonales", async (clientId, datosPersonales) =>
        {
            Console.WriteLine("***************************");
            await EditarDatosPersonales(clientId, datosPersonales);
        });

        _hubConnection.On<string, string>("EditarAntecedentes", async (clientId, antecedentesPaciente) =>
        {
            Console.WriteLine("***************************");
            await EditarAntecedentes(clientId, antecedentesPaciente);
        });

        _hubConnection.On<string, string>("GuardarDatosRips", async (clientId, datosRips) =>
        {
            await GuardarDatosRips(clientId, datosRips);
        });

		_hubConnection.On<string, string>("GenerarRdaDesdeRipsExistente", async (clientId, payloadJson) =>
		{
			await GenerarRdaDesdeRipsExistente(clientId, payloadJson);
		});

		_hubConnection.On<string, string>("ConsultarRipsExistentes", async (clientId, payloadJson) =>
		{
			await ConsultarRipsExistentes(clientId, payloadJson);
		});

		_hubConnection.On<string, string>("EliminarRipsPorLlave", async (clientId, payloadJson) =>
		{
			await EliminarRipsPorLlave(clientId, payloadJson);
		});

		_hubConnection.On<string, string>("ConsultarRipsDetallePorLlave", async (clientId, payloadJson) =>
		{
			await ConsultarRipsDetallePorLlave(clientId, payloadJson);
		});

		_hubConnection.On<string, string>("ObtenerFacturasPorIdEntreFechas", async (clientId, modeloDatosParaConsultarFacturasEntreFechas) =>
        {
            await ObtenerFacturasPorIdEntreFechas(clientId, modeloDatosParaConsultarFacturasEntreFechas);
        });

        _hubConnection.On<string, int, string>("GenerarRips", async (clientId, identificador, objGenerarRips) =>
        {
            await GenerarRips(clientId, identificador, objGenerarRips);
        });

        _hubConnection.On<string, int, string>("PresentarRips", async (clientId, identificador, objPresentarRips) =>
        {
            await PresentarRips(clientId, identificador, objPresentarRips);
        });

		_hubConnection.On<string, string>("ConsultarRdaControl", async (clientId, filtroJson) =>
		{
			await ConsultarRdaControl(clientId, filtroJson);
		});

		_hubConnection.On<string, int>("ReenviarRda", async (clientId, idRda) =>
		{
			await ReenviarRda(clientId, idRda);
		});

		_hubConnection.On<string, int>("RegenerarRda", async (clientId, idRda) =>
		{
			await RegenerarRda(clientId, idRda);
		});

		_hubConnection.On<string, int>("ConsultarDetalleRda", async (clientId, idRda) =>
		{
			await ConsultarDetalleRda(clientId, idRda);
		});

		_hubConnection.On<string, int>("ConsultarHistorialRda", async (clientId, idRda) =>
		{
			await ConsultarHistorialRda(clientId, idRda);
		});

		_hubConnection.On<string, string>("ReenviarRdaLote", async (clientId, idsJson) =>
		{
			await ReenviarRdaLote(clientId, idsJson);
		});

		_hubConnection.On<string, string>("RegenerarRdaLote", async (clientId, idsJson) =>
		{
			await RegenerarRdaLote(clientId, idsJson);
		});

		_hubConnection.On<string, string>("ConsultarPacienteInteroperabilidadExacto", async (clientId, filtroJson) =>
		{
			await ConsultarPacienteInteroperabilidadExacto(clientId, filtroJson);
		});

		_hubConnection.On<string, string>("ConsultarPacienteInteroperabilidadSimilar", async (clientId, filtroJson) =>
		{
			await ConsultarPacienteInteroperabilidadSimilar(clientId, filtroJson);
		});

		_hubConnection.On<string, string>("ConsultarRdaPacienteInteroperabilidad", async (clientId, filtroJson) =>
		{
			await ConsultarRdaPacienteInteroperabilidad(clientId, filtroJson);
		});

		_hubConnection.On<string, string>("ConsultarEncuentrosPacienteInteroperabilidad", async (clientId, filtroJson) =>
		{
			await ConsultarEncuentrosPacienteInteroperabilidad(clientId, filtroJson);
		});

		_hubConnection.On<string>("ObtenerFacturasPendientes", async (clientId) =>
        {
            await ObtenerFacturasPendientes(clientId);
        });

		_hubConnection.On<string, string>("ObtenerFacturasCreadas", async (clientId, Factura) =>
		{
			await ObtenerFacturasCreadas(clientId, Factura);
		});

		_hubConnection.On<string, string>("PresentarFacturasEnDian", async (clienteIdDestino, payloadJson) =>
		{
			await PresentarFacturasEnDian(clienteIdDestino, payloadJson);
		});

		_hubConnection.On<string, string>("DescargarJsonFacturaPendiente", async (clienteIdDestino, payloadJson) =>
		{
	        await DescargarJsonFacturaPendiente(clienteIdDestino, payloadJson);
        });

		_hubConnection.On<string>("ObtenerCodigosEps", async (clientId) =>
        {
            await ObtenerCodigosEps(clientId);
        });

        _hubConnection.On<string, int, DateTime, DateTime>("ObtenerDatosAdministrativos", async (clientId, idDoctor, fechaInicio, fechaFin) =>
        {
            await ObtenerDatosAdministrativos(clientId, idDoctor, fechaInicio, fechaFin);
        });


        _hubConnection.On<string, string, DateTime>("ObtenerConsultaPorDiaYPorUnidad", async (clientId, silla, fecha) =>
        {
            await ConsultarPorDiaYPorUnidad(clientId, Convert.ToInt32(silla), fecha);
        });

        _hubConnection.On<string, string>("RealizarAccionesEnCitaAgendada", async (clientId, modeloRealizarAccionesCitaAgendada) =>
        {
            await RealizarAccionesEnCitaAgendada(clientId, modeloRealizarAccionesCitaAgendada);
        });

        //clientId es el id de la conexion del cliente angular que pidio  ConsultarPorDiaYPorUnidad
        //el punto On indica que se esta suscribiendo a un evento llamado ConsultarPorDiaYPorUnidad los parametros <string, int, DateTime>
        //determinan los tipos de datos que se van a recibir en el evento y que vemos reflejados async (clientId, silla, fecha)
        _hubConnection.On<string, string>("AgendarCita", async (clientId, modelocrearcita) =>
        {
            await ObtenerValidacionesAgenda(clientId, modelocrearcita);
        });

        _hubConnection.On<string, string>("ConsultarEstadoCuenta", async (clientId,modeloDatosParaConsultarEstadoCuenta) =>
        {
            await ConsultarEstadoCuenta(clientId, modeloDatosParaConsultarEstadoCuenta);
        });

		// NUEVO: Preparar (precargar diálogo)
		_hubConnection.On<string, string>("PrepararEstadoCuenta", async (clientId, modelo) =>
		{
			await PrepararEstadoCuenta(clientId, modelo);
		});

		// NUEVO: Crear (guardar en BD)
		_hubConnection.On<string, string>("CrearEstadoCuenta", async (clientId, modelo) =>
		{
			await CrearEstadoCuenta(clientId, modelo);
		});

		_hubConnection.On<string, string>("PrepararEditarEstadoCuenta", async (clientId, modelo) =>
		{
			await PrepararEditarEstadoCuenta(clientId, modelo);
		});

		_hubConnection.On<string, string>("EditarEstadoCuenta", async (clientId, modelo) =>
		{
			await EditarEstadoCuenta(clientId, modelo);
		});

		_hubConnection.On<string, string>("BorrarEstadoCuenta", async (clientId, modelo) =>
		{
			await BorrarEstadoCuenta(clientId, modelo);
		});

		_hubConnection.On<string, string>("ConsultarSugeridosAbono", async (clientId, modelo) =>
		{
			await ConsultarSugeridosAbono(clientId, modelo);
		});


		_hubConnection.On<string, string>("PrepararInsertarAbono", async (clientId, modelo) =>
		{
			await PrepararInsertarAbono(clientId, modelo);
		});

		_hubConnection.On<string, string>("InsertarAbono", async (clientId, modelo) =>
		{
			await InsertarAbono(clientId, modelo);
		});

		_hubConnection.On<string, string>("PrepararInsertarAdicional", async (clientId, modelo) =>
		{
			await PrepararInsertarAdicional(clientId, modelo);
		});

		_hubConnection.On<string, string>("InsertarAdicional", async (clientId, modelo) =>
		{
			await InsertarAdicional(clientId, modelo);
		});


		_hubConnection.On<string, string>("PrepararBorrarAbono", async (clientId, modelo) =>
		{
			await PrepararBorrarAbono(clientId, modelo);
		});

		_hubConnection.On<string, string>("BorrarAbono", async (clientId, modelo) =>
		{
			await BorrarAbono(clientId, modelo);
		});
	}
	//----------------------Pasos:
	//1. Conectar con el servicio de SignalR usando la funcion ConnectToServer 
	//2. Recibir el pin de acceso de Rydent se usa el evento _hubConnection.On<string, string>("ObtenerPin"
	//3. Autenticar el pin de acceso de Rydent se usando await RecibirPinRydent(pin, clientId);
	//4. Enviar el pin de acceso de Rydent al servidor de Rydent
	// Método para conectar al servidor y registrar el equipo


	public async Task ConnectToServer(IServiceProvider serviceProvider)
    {
        try
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var _tDATOSCLIENTESServicios = scope.ServiceProvider.GetRequiredService<ITDATOSCLIENTESServicios>();
                var datosClientes = await _tDATOSCLIENTESServicios.ConsultarPorId(System.Environment.MachineName);

                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    await RegisterDeviceAsync(retornarEntrada(datosClientes.ENTRADA));
                    _isDeviceRegistered = true; // Marca como registrado
                }
                else
                {
                    _logger.LogWarning("No se pudo registrar el equipo porque la conexión a SignalR no está establecida.");
                    await AttemptReconnectAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la conexión al servidor: {ex.Message}", ex);
        }
    }



	

	private bool _isDeviceRegistered = false;
	private long _idSedeDetectada = 0;

	private async Task RegisterDeviceAsync(string entrada)
	{
		try
		{
			if (_hubConnection.State != HubConnectionState.Connected)
				return;

			// 1) Preguntar si ya hay un activo para ese identificadorLocal
			var activoId = await _hubConnection.InvokeAsync<string>(
				"GetActiveConnectionIdByIdentificadorLocal",
				entrada
			);

			// ✅ Helper único: V3 -> fallback V2
			async Task RegistrarSeguroAsync()
			{
				_idSedeDetectada = 0;

				try
				{
					var idSede = await _hubConnection.InvokeAsync<long>("RegistrarEquipoV3", entrada);
					if (idSede > 0)
					{
						_idSedeDetectada = idSede;
						return;
					}

					_logger.LogWarning("RegistrarEquipoV3 devolvió 0. Fallback a V2...");
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "RegistrarEquipoV3 falló. Fallback a V2...");
				}

				// Fallback V2
				await _hubConnection.InvokeAsync("RegistrarEquipoV2", entrada);
			}

			// ✅ Caso A: no hay activo -> registramos
			if (string.IsNullOrWhiteSpace(activoId))
			{
				await RegistrarSeguroAsync();
				_isDeviceRegistered = true;
				_logger.LogInformation($"Equipo registrado (no había activo). idSedeDetectada={_idSedeDetectada}");
				return;
			}

			// ✅ Caso B: el activo soy yo -> ya ok
			if (string.Equals(activoId, _hubConnection.ConnectionId, StringComparison.Ordinal))
			{
				_isDeviceRegistered = true;
				_logger.LogInformation($"La sede ya está activa y SOY YO. ConnectionId: {activoId}");
				return;
			}

			// ✅ Caso C: hay activo pero NO soy yo -> re-registrar
			_logger.LogWarning(
				$"Activo distinto detectado. Activo={activoId} / Yo={_hubConnection.ConnectionId}. Re-registrando..."
			);

			await RegistrarSeguroAsync();
			_isDeviceRegistered = true;
			_logger.LogInformation($"Equipo re-registrado (override). idSedeDetectada={_idSedeDetectada}");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Error al registrar el equipo: {ex.Message}");
			_isDeviceRegistered = false;
		}
	}


	//se remplaza la anteriro por esta 17-02-2026
	private async Task AttemptReconnectAsync()
	{
		await EnsureConnectedAsync();
	}



	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
		await EnsureConnectedAsync(stoppingToken); // ✅ único método
		await registrarSuscripciones();            // ✅ solo 1 vez (bien)
		StartHeartbeatLoop(stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Iniciando la consulta a la tabla...");

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
					// Verificar estado de conexión y reconectar si es necesario
					/* if (_hubConnection.State != HubConnectionState.Connected)
					 {
						 _logger.LogWarning("Conexión a SignalR perdida. Intentando reconectar...");
						 await AttemptReconnectAsync();
					 }*/
					// ✅ Asegura conexión sin choques (si ya está reconectando, no hace nada)
					await EnsureConnectedAsync(stoppingToken);

					// Registrar dispositivo si no está registrado
					
					if (!_isDeviceRegistered && _hubConnection.State == HubConnectionState.Connected)
					{
						var _tDATOSCLIENTESServicios = scope.ServiceProvider.GetRequiredService<ITDATOSCLIENTESServicios>();
						var datosClientes = await _tDATOSCLIENTESServicios.ConsultarPorId(Environment.MachineName);

						await RegisterDeviceAsync(retornarEntrada(datosClientes.ENTRADA));
					}

					//var estadoCuentaService = scope.ServiceProvider.GetRequiredService<IRadoIntegrationService>();
					//var x = estadoCuentaService.TryEnviarIngresoPorIdRelacionAsync(29092);

					_logger.LogInformation("Consulta completada correctamente.");
                }


				// Esperar antes de la siguiente iteración
				await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operación cancelada por solicitud.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error durante la ejecución del método: {ex.Message}");
                // Retraso dinámico en caso de error para evitar bucles rápidos
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		try
		{
			_heartbeatCts?.Cancel();
			if (_heartbeatTask != null)
				await _heartbeatTask;
		}
		catch { /* ignore */ }

		await base.StopAsync(cancellationToken);
	}




	public async Task RecibirPinRydent(string pinacceso, string clientId, int maxIdAnamnesis)
    {
        var respuestaPin = new RespuestaPinModel();
        var objPINACCESO = new TCLAVEServicios();
        var objDOCTORES = new TDATOSDOCTORESServicios();
        var objCONVENIOS = new T_CONVENIOSServicios();
        var objINFORMACIONREPORTES = new TINFORMACIONREPORTESServicios();
        //var objEPS = new TCODIGOS_EPSServicios();
        //var objPROCEDIMIENTOS = new TCODIGOS_PROCEDIMIENTOSServicios();
        //var objCONSULTAS = new TCODIGOS_CONSLUTASServicios();
        //var objDepartamentos = new TCODIGOS_DEPARTAMENTOServicios();
        //var objCiudades = new TCODIGOS_CIUDADServicios();
        var objFrasesXEvolucion = new T_FRASE_XEVOLUCIONServicios();
        var objHorariosAgenda = new THORARIOSAGENDAServicios();
        var objHorariosAsuntos = new THORARIOSASUNTOSServicios();
        var objFestivos = new TFESTIVOSServicios();
        var objConfiguracionesRydent = new TCONFIGURACIONES_RYDENTServicios();
        var objTAnamnesisParaAgendayBuscadores = new TANAMNESISServicios();
        respuestaPin.clave = await objPINACCESO.ConsultarPorId(pinacceso);
		//esto se hace para evitar enviar la clave en el objeto respuestaPin el cual quedaria expuesto en el navegador web
		//if (respuestaPin.clave.CLAVE != null)
		if (respuestaPin.clave != null && respuestaPin.clave.CLAVE != null)
		{
            respuestaPin.clave.CLAVE = "";
            respuestaPin.acceso = true;
            var listDoctores = await objDOCTORES.ConsultarTodos();
            var listConvenios = await objCONVENIOS.ConsultarTodos();
            var listInformacionReporte = await objINFORMACIONREPORTES.ConsultarTodos();
            var listDoctoresConPrestador = await objINFORMACIONREPORTES.ObtenerDoctoresConCodigoPrestadorAsync();
            //var listEps = await objEPS.ConsultarTodos();
            //var listProcedimientos = await objPROCEDIMIENTOS.ConsultarTodos();
            //var listConsultas = await objCONSULTAS.ConsultarTodos();
            //var listDepartamentos = await objDepartamentos.ConsultarTodos();
            //var listCiudades = await objCiudades.ConsultarTodos();
            var lstFrasesXEvolucion = await objFrasesXEvolucion.ConsultarTodos();
            var listHorariosAgenda = await objHorariosAgenda.ConsultarTodos();
            var listHorariosAsuntos = await objHorariosAsuntos.ConsultarTodos();
            var listFestivos = await objFestivos.ConsultarTodos();
            var listConfiguracionesRydent = await objConfiguracionesRydent.ConsultarTodos();
            var listAnamnesisParaAgendayBuscadores = await objTAnamnesisParaAgendayBuscadores.ConsultarDatosPacientesParaCargarEnAgenda(maxIdAnamnesis);
            if (listDoctores != null && listDoctores.Count() > 0)
            {
                //respuestaPin.lstEps = listEps;
                //respuestaPin.lstProcedimientos = listProcedimientos;
                //respuestaPin.lstConsultas = listConsultas;
                //respuestaPin.lstDepartamentos = listDepartamentos;
                //respuestaPin.lstCiudades = listCiudades;
                respuestaPin.lstDoctores = listDoctores.ConvertAll(item => new ListadoItemModel() { id = item.ID.ToString(), nombre = (item.NOMBRE ?? "") });
                respuestaPin.lstConvenios = listConvenios.ConvertAll(item => new ListadoItemModel() { id = item.ID.ToString(), nombre = (item.NOMBRE ?? "") });
				respuestaPin.lstInformacionReporte = listInformacionReporte.ConvertAll(item => new ListadoItemModel() { id = item.ID.ToString(), nombre = (item.NOMBRE ?? "") });
				respuestaPin.lstDoctoresConPrestador = listDoctoresConPrestador;
				respuestaPin.lstFrasesXEvolucion = lstFrasesXEvolucion;
                respuestaPin.lstHorariosAgenda = listHorariosAgenda;
                respuestaPin.lstHorariosAsuntos = listHorariosAsuntos;
                respuestaPin.lstFestivos = listFestivos;
                respuestaPin.lstConfiguracionesRydent = listConfiguracionesRydent;
                if (listAnamnesisParaAgendayBuscadores != null && listAnamnesisParaAgendayBuscadores.Count() > 0)
                {
                    respuestaPin.lstAnamnesisParaAgendayBuscadores = listAnamnesisParaAgendayBuscadores;
                }
            }
        }
        else
        {
            respuestaPin.acceso = false;
        }   
        
        try
        {
            var s = JsonConvert.SerializeObject(respuestaPin);
            var respuestaPinComprimido = ArchivosHelper.CompressString(s);
            await _hubConnection.InvokeAsync("RespuestaObtenerPin", clientId, respuestaPinComprimido);
        }
        catch (Exception ex)
        {

            throw;
        }
        
    }

	public async Task RecibirLotePacientesAgenda(string clientId, int maxIdAnamnesis)
	{
		try
		{
			var objTAnamnesis = new TANAMNESISServicios();

			var lista = await objTAnamnesis
				.ConsultarDatosPacientesParaCargarEnAgenda(maxIdAnamnesis);

			var s = JsonConvert.SerializeObject(lista);
			var comprimido = ArchivosHelper.CompressString(s);

			await _hubConnection.InvokeAsync(
				"RespuestaLotePacientesAgenda",
				clientId,
				comprimido
			);
		}
		catch (Exception ex)
		{
			await _hubConnection.InvokeAsync(
				"ErrorConexion",
				clientId,
				"Error cargando lote de pacientes agenda: " + ex.Message
			);
		}
	}

	public async Task ConsultarPorIdDoctor(string clientId, int idDoctor)
    {
        var objDOCTORES = new TDATOSDOCTORESServicios();
        var objAname = new TANAMNESISServicios();
        var respuestaObtenerDoctor = new RespuestaObtenerDoctorModel();
        respuestaObtenerDoctor.doctor = await objDOCTORES.ConsultarPorId(idDoctor);
        respuestaObtenerDoctor.totalPacientes = await objAname.ConsultarTotalPacientesPorDoctor(idDoctor);

		var objInformacionReportes = new TINFORMACIONREPORTESServicios();
		var objInfoRep = await objInformacionReportes.ConsultarPorId(respuestaObtenerDoctor.doctor.IDREPORTE ?? 0);
        respuestaObtenerDoctor.facturaElectronica = objInfoRep.PROVEEDOR_FE == "DATAICO";

		await _hubConnection.InvokeAsync("RespuestaObtenerDoctor", clientId, JsonConvert.SerializeObject(respuestaObtenerDoctor));
    }

    public async Task ObtenerDoctorSiLoCambian(string clientId, int idDoctor)
    {
        var objDOCTORES = new TDATOSDOCTORESServicios();
        var objAname = new TANAMNESISServicios();
        var respuestaObtenerDoctor = new RespuestaObtenerDoctorModel();
        respuestaObtenerDoctor.doctor = await objDOCTORES.ConsultarPorId(idDoctor);
        respuestaObtenerDoctor.totalPacientes = await objAname.ConsultarTotalPacientesPorDoctor(idDoctor);

		var objInformacionReportes = new TINFORMACIONREPORTESServicios();
		var objInfoRep = await objInformacionReportes.ConsultarPorId(respuestaObtenerDoctor.doctor.IDREPORTE ?? 0);
		respuestaObtenerDoctor.facturaElectronica = objInfoRep.PROVEEDOR_FE == "DATAICO";


		await _hubConnection.InvokeAsync("RespuestaObtenerDoctorSiLoCambian", clientId, JsonConvert.SerializeObject(respuestaObtenerDoctor));
    }


    public async Task BuscarCitasPacienteAgenda(string valorBuscarAgenda, string clientId)
    {
        var objDetalleCitas = new TDETALLECITASServicios();
        var listDetalleCitas = await objDetalleCitas.ConsultarCitasDePacientePorTipo(valorBuscarAgenda, DateTime.Now.Date);
        
        var respuestaBuscarCitasPacienteAgenda = listDetalleCitas.ConvertAll(item => new RespuestaBusquedaCitasPacienteModel()
        {
            ID=item.ID,
            NOMBRE_PACIENTE=item.NOMBRE,
            TELEFONO_PACIENTE=item.TELEFONO,
            FECHA_CITA= item.FECHA?.Date,
            HORA_CITA =item.HORA,
            SILLA_CITA=item.SILLA.ToString(),
            DOCTOR=item.DOCTOR,
            NUMDOCUMENTO = item.CEDULA,
            OBSERVACIONES = item.OBSERVACIONES,
            ASUNTO = item.ASUNTO,
            IDCONSECUTIVO = item.IDCONSECUTIVO

        });
        await _hubConnection.InvokeAsync("RespuestaBuscarCitasPacienteAgenda", clientId, JsonConvert.SerializeObject(respuestaBuscarCitasPacienteAgenda));
    }
    public async Task BuscarPaciente(string valorDeBusqueda, int tipoBusqueda,  string clientId)
    {
        var objAname = new TANAMNESISServicios();
        
        var listAnamnesis = await objAname.BuscarPacientePorTipo(tipoBusqueda, valorDeBusqueda);
        //var respuestaNotaImportante = objAname.BuscarNotaImportante(IDANAMNESIS);
        var respuestaBuscarPaciente = listAnamnesis.ConvertAll(item => new RespuestaBusquedaPacienteModel()
        {
           IDANAMNESIS=item.IDANAMNESIS,
           NOMBRE_PACIENTE=item.NOMBRE_PACIENTE,
           IDANAMNESISTEXTO=item.IDANAMNESISTEXTO,
           NUMDOCUMENTO=item.NUMDOCUMENTO,
           DOCTOR=item.DOCTOR,
           PERFIL=item.PERFIL,
           NUMAFILIACION=item.NUMAFILIACION,
           TELEFONO=item.TELEFONO
        });
        
        var respuestaBuscarPacienteSerializado = JsonConvert.SerializeObject(respuestaBuscarPaciente);
        var respuestaBuscarPacienteSerializadosComprimidos = ArchivosHelper.CompressString(respuestaBuscarPacienteSerializado);
        await _hubConnection.InvokeAsync("RespuestaBuscarPaciente", clientId, respuestaBuscarPacienteSerializadosComprimidos);
    }
    
    public async Task BuscarDatosPersonalesCompletosPacientes(string clientId, int idAnanesis)
    {
        var objDatosPersonales = new DatosPersonalesServicios();
        var datosPersonales = await objDatosPersonales.ConsultarPorId(idAnanesis);
        var respuestaBuscarDatosPersonalesCompletosPacientes=new RespuestaDatosPersonales();
        respuestaBuscarDatosPersonalesCompletosPacientes.datosPersonales = datosPersonales;
        var fotoBase64 = await new TFOTOSFRONTALESServicios().ConsultarBase64PorId(idAnanesis);
        if (!string.IsNullOrEmpty(fotoBase64))
        {
            respuestaBuscarDatosPersonalesCompletosPacientes.strFotoFrontal = new ArchivosHelper().obtenerBase64ConPrefijo(fotoBase64);

        }
        var respuestaBuscarDatosPersonalesCompletosPacientesSerializado = JsonConvert.SerializeObject(respuestaBuscarDatosPersonalesCompletosPacientes);    
        var respuestaBuscarDatosPersonalesCompletosPacientesSerializadosComprimidos = ArchivosHelper.CompressString(respuestaBuscarDatosPersonalesCompletosPacientesSerializado);
        await _hubConnection.InvokeAsync("RespuestaObtenerDatosPersonalesCompletosPaciente", clientId, respuestaBuscarDatosPersonalesCompletosPacientesSerializadosComprimidos);
    }

    public async Task BuscarAntecedentesPacientes(string clientId, int idAnanesis)
    {
        var objAntecedentes = new AntecedentesServicios();
        var antecedentes = await objAntecedentes.ConsultarPorId(idAnanesis);
        var respuestaBuscarAntecedentesPacientes = antecedentes;
        var respuestaBuscarAntecedentesPacientesSerializado = JsonConvert.SerializeObject(respuestaBuscarAntecedentesPacientes);
        var respuestaBuscarAntecedentesPacientesSerializadosComprimidos = ArchivosHelper.CompressString(respuestaBuscarAntecedentesPacientesSerializado);
        await _hubConnection.InvokeAsync("RespuestaObtenerAntecedentesPaciente", clientId, respuestaBuscarAntecedentesPacientesSerializadosComprimidos);
    }

    public async Task ObtenerCodigosEps (string clientId)
    {
        var objEps = new TCODIGOS_EPSServicios();
        var listEps = await objEps.ConsultarTodos();
        var respuestaBuscarEps = listEps;
        await _hubConnection.InvokeAsync("RespuestaObtenerCodigosEps", clientId, JsonConvert.SerializeObject(respuestaBuscarEps));
    }

    public async Task ObtenerDatosAdministrativos (string clientId, int idDoctor, DateTime fechaInicio, DateTime fechaFin)
    {
        using (var _dbcontext = new AppDbContext())
        {
            var objAnamnesis = new TANAMNESISServicios();
            var objPacientesNuevos = await objAnamnesis.ConsultarTotalPacientesNuevosEntreFechas(fechaInicio.Date, fechaFin.Date);
            var objDetalleCitas = new TDETALLECITASServicios();
            var objPacientesAsistieron = await objDetalleCitas.ConsultarPacientesAsistieronEntreFechas(fechaInicio.Date, fechaFin.Date);
            var objPacientesNoAsistieron = await objDetalleCitas.ConsultarPacientesNoAsistieronEntreFechas(fechaInicio.Date, fechaFin.Date);
            var objT_Adicionales_Abonos = new T_ADICIONALES_ABONOSServicios();
            var objPacientesAbonaron =  await objT_Adicionales_Abonos.ConsultarPacientesAbonaronEntreFechas(fechaInicio.Date, fechaFin.Date);
            var objTotalAbonado = await objT_Adicionales_Abonos.ConsultarTotalAbonadoEntreFechas(fechaInicio.Date, fechaFin.Date);
            var objTCitasCanceladas = new TCITASCANCELADASServicios();
            var objCitasCanceladas = await objTCitasCanceladas.ConsultarCitasCanceladasEntreFechas(fechaInicio.Date, fechaFin.Date);
            var objTEgresos = new TEGRESOServicios();
            var objTotalEgresos = await objTEgresos.BuscarTotalEgresosPorFecha(fechaInicio.Date, fechaFin.Date);
            var objTINGRESOS = new TINGRESOServicios();
            var objTotalIngresos = await objTINGRESOS.BuscarTotalIngresosPorFecha(fechaInicio.Date, fechaFin.Date);
            var fecha = new DateTime(1990, 1, 1);
            //var fechaFormatted = DateTime.ParseExact(fecha.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var obj = await _dbcontext.P_CONSULTAR_ESTACUENTA_TOTAL(idDoctor, fecha.Date, 0);  // Convertimos a lista para manipulación en C#
                                                                                                               // Sumar valores específicos
            var totalMoraActual = obj.Sum(x => x.MORA_ACTUAL);  // Reemplaza con el nombre real de la columna
            var totalMoraTotal = obj.Sum(x => x.MORATOTAL);    // Reemplaza con el nombre real de la columna

            var respuestaDatosAdministrativos = new RespuestaDatosAdministrativos();
            respuestaDatosAdministrativos.totalPacientesNuevos = objPacientesNuevos.Count;
            respuestaDatosAdministrativos.pacientesAsistieron = objPacientesAsistieron;
            respuestaDatosAdministrativos.pacientesNoAsistieron = objPacientesNoAsistieron;
            respuestaDatosAdministrativos.pacientesAbonaron = objPacientesAbonaron;
            respuestaDatosAdministrativos.citasCanceladas = objCitasCanceladas;
            respuestaDatosAdministrativos.totalAbonos = objTotalAbonado;
            respuestaDatosAdministrativos.lstPacientesNuevos = objPacientesNuevos;
            decimal decimalTotalIngreos = (decimal)objTotalIngresos;
            decimal decimalTotalEgresos = (decimal)objTotalEgresos;

            respuestaDatosAdministrativos.totalEgresos = decimalTotalEgresos;
            respuestaDatosAdministrativos.totalIngresos = decimalTotalIngreos;
            respuestaDatosAdministrativos.moraTotal = (decimal)totalMoraActual;
            respuestaDatosAdministrativos.totalCartera = (decimal)totalMoraTotal;
            await _hubConnection.InvokeAsync("RespuestaObtenerDatosAdministrativos", clientId, JsonConvert.SerializeObject(respuestaDatosAdministrativos));
        }
    }

    public async Task ConsultarEstadoCuenta (string clientId, string modeloDatosParaConsultarEstadoCuenta)
    {
        var settings = new JsonSerializerSettings();
        var objDatosParaConsultarEstadoCuenta = JsonConvert.DeserializeObject<RespuestaConsultarEstadoCuenta>(modeloDatosParaConsultarEstadoCuenta, settings);
        var objAdicionalesAbonos = new T_ADICIONALES_ABONOSServicios();
        var objRespuestaConsultarEstadoCuenta = new RespuestaConsultarEstadoCuenta();
        objRespuestaConsultarEstadoCuenta.ID = objDatosParaConsultarEstadoCuenta.ID;
        objRespuestaConsultarEstadoCuenta.IDDOCTOR = objDatosParaConsultarEstadoCuenta.IDDOCTOR;
        var objDefinicionTratamiento = new T_DEFINICION_TRATAMIENTOServicios();
        var valor = 0m;
        var descuento = 0m;
        var abonos = 0m;
        var descuentos = 0m;
        var restante = 0m;
        var costoTratamiento = 0m;
        using (var _dbcontext = new AppDbContext())
        {
            var lstDefinicionTratamiento = await objDefinicionTratamiento.ConsultarPorIdAnamnesisIdDoctor(objDatosParaConsultarEstadoCuenta.ID ?? 0, objDatosParaConsultarEstadoCuenta.IDDOCTOR ?? 0);
            if (lstDefinicionTratamiento.Count <= 0)
            {
                objRespuestaConsultarEstadoCuenta.mensajeSinTratamiento = true;
                var objRespuestaConsultarEstadoCuentaSerializado = JsonConvert.SerializeObject(objRespuestaConsultarEstadoCuenta);
                var objRespuestaConsultarEstadoCuentaSerializadoComprimido = ArchivosHelper.CompressString(objRespuestaConsultarEstadoCuentaSerializado);
                await _hubConnection.InvokeAsync("RespuestaConsultarEstadoCuenta", clientId, objRespuestaConsultarEstadoCuentaSerializadoComprimido);
                return;
            }
            objRespuestaConsultarEstadoCuenta.mensajeSinTratamiento= false;
            var listaFases = lstDefinicionTratamiento.Select(x => x.FASE).OrderBy(fase => fase).ToList();
            objRespuestaConsultarEstadoCuenta.lstFases = listaFases;
            if (objDatosParaConsultarEstadoCuenta.FASE == null || objDatosParaConsultarEstadoCuenta.FASE <= 0)
            {
                objRespuestaConsultarEstadoCuenta.FASE = listaFases.LastOrDefault();
            }
            else
            {
                objRespuestaConsultarEstadoCuenta.FASE = objDatosParaConsultarEstadoCuenta.FASE;
            }
            var objDefinicionTratamientoPorFase= await objDefinicionTratamiento.ConsultarPorId(objDatosParaConsultarEstadoCuenta.ID ?? 0, objDatosParaConsultarEstadoCuenta.IDDOCTOR ?? 0, objRespuestaConsultarEstadoCuenta.FASE ?? 0);
            objRespuestaConsultarEstadoCuenta.CONSECUTIVO = objDefinicionTratamientoPorFase.CONSECUTIVO;
            if(objDefinicionTratamientoPorFase.VALOR_TRATAMIENTO == 0 && objDefinicionTratamientoPorFase.NUMERO_CUOTAS == 1)
            {
                objRespuestaConsultarEstadoCuenta.tratamientoSinFinanciar = true;
            }
            else
            {
                objRespuestaConsultarEstadoCuenta.tratamientoSinFinanciar = false;
            }
            var objPConsultarEstadoCuenta = new List<P_CONSULTAR_ESTACUENTA>();
            objPConsultarEstadoCuenta = await _dbcontext.P_CONSULTAR_ESTACUENTA(objRespuestaConsultarEstadoCuenta.ID ?? 0, objRespuestaConsultarEstadoCuenta.FASE ?? 0, objRespuestaConsultarEstadoCuenta.IDDOCTOR ?? 0);
            if (objPConsultarEstadoCuenta.Count > 0)
            {
                objRespuestaConsultarEstadoCuenta.P_CONSULTAR_ESTACUENTA = objPConsultarEstadoCuenta;
                var objValorTtoSinFincanciarXpresupuesto = new RespuestasQuerysEstadoCuenta();
                objValorTtoSinFincanciarXpresupuesto = await objAdicionalesAbonos.ConsultarValorDescuentoPorIdMaestra(objPConsultarEstadoCuenta.FirstOrDefault().IDPRESUPUESTOMAESTRA ?? 0);
                if (objValorTtoSinFincanciarXpresupuesto!=null)
                {
                    valor= objValorTtoSinFincanciarXpresupuesto.VALOR ?? 0;
                    descuento = objValorTtoSinFincanciarXpresupuesto.DESCUENTO ?? 0;
                    costoTratamiento = valor-descuento;
                    var objSumaAbonosDescuentos = await objAdicionalesAbonos.ConsultarTotalSumaAbonosYDescuentos(objDatosParaConsultarEstadoCuenta.ID ?? 0, objDatosParaConsultarEstadoCuenta.FASE ?? 0, objDatosParaConsultarEstadoCuenta.IDDOCTOR ?? 0);
                    if (objSumaAbonosDescuentos != null)
                    {
                        abonos= objSumaAbonosDescuentos.ABONOS ?? 0;
                        descuentos = objSumaAbonosDescuentos.DESCUENTOS ?? 0;
                        restante = valor - (abonos +descuento+descuentos);
                    }
                }
            }
            objRespuestaConsultarEstadoCuenta.costoTratamiento = costoTratamiento;
            objRespuestaConsultarEstadoCuenta.descuentos = descuento+descuentos;
            objRespuestaConsultarEstadoCuenta.restante = restante;
            objRespuestaConsultarEstadoCuenta.pagosRealizados    = abonos;
            
            var objConsultarEstadoCuentaPaciente = new List<P_CONSULTAR_ESTACUENTAPACIENTE>();
            objConsultarEstadoCuentaPaciente = await _dbcontext.P_CONSULTAR_ESTACUENTAPACIENTE(objRespuestaConsultarEstadoCuenta.ID ?? 0);
            objRespuestaConsultarEstadoCuenta.P_CONSULTAR_ESTACUENTAPACIENTE = objConsultarEstadoCuentaPaciente;
            var objRespuestaConsultarSaldoPorDoctor = await objAdicionalesAbonos.ConsultarSaldoPorDoctor(objDatosParaConsultarEstadoCuenta.ID ?? 0);
            objRespuestaConsultarEstadoCuenta.RespuestaSaldoPorDoctor = objRespuestaConsultarSaldoPorDoctor;
        }
        //var lstDefinicionTratamiento = await objDefinicionTratamiento.ConsultarPorIdAnamnesisIdDoctor(objDatosParaConsultarEstadoCuenta.ID, objDatosParaConsultarEstadoCuenta.IDDOCTOR);

        var objRespuestaConsultarEstadoCuentaSerializado2 = JsonConvert.SerializeObject(objRespuestaConsultarEstadoCuenta);
        var objRespuestaConsultarEstadoCuentaSerializadoComprimido2 = ArchivosHelper.CompressString(objRespuestaConsultarEstadoCuentaSerializado2);
        await _hubConnection.InvokeAsync("RespuestaConsultarEstadoCuenta", clientId, objRespuestaConsultarEstadoCuentaSerializadoComprimido2);
    }

	private async Task PrepararEstadoCuenta(string clientId, string modeloJson)
	{
		PrepararNuevoEstadoCuentaResponse res;

		try
		{
			// 1) Validación básica
			if (string.IsNullOrWhiteSpace(modeloJson))
				throw new Exception("modeloJson llegó vacío.");

			// 2) Deserializar (si falla, lanzamos error y no seguimos)
			var req = System.Text.Json.JsonSerializer.Deserialize<PrepararNuevoEstadoCuentaRequest>(
				modeloJson,
				new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
			) ?? throw new Exception("No se pudo deserializar PrepararNuevoEstadoCuentaRequest (req=null).");

			// 3) Validar IDs (evita problemas de nulls en consultas por IDs 0)
			if (req.PacienteId <= 0 || req.DoctorId <= 0)
				throw new Exception($"Request inválido. PacienteId={req.PacienteId}, DoctorId={req.DoctorId}");

			// 4) Resolver el servicio (SCOPED) dentro de un scope
			using var scope = _scopeFactory.CreateScope();
			var estadoCuentaService = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			// 5) Ejecutar
			res = await estadoCuentaService.PrepararNuevoAsync(req)
				  ?? new PrepararNuevoEstadoCuentaResponse
				  {
					  Ok = false,
					  Mensaje = "PrepararNuevoAsync devolvió null."
				  };
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			res = new PrepararNuevoEstadoCuentaResponse
			{
				Ok = false,
				Mensaje = $"Error en worker (PrepararEstadoCuenta): {ex.Message}"
			};
		}

		try
		{
			var json = System.Text.Json.JsonSerializer.Serialize(res);
			var comprimido = ArchivosHelper.CompressString(json);

			await _hubConnection.InvokeAsync("RespuestaPrepararEstadoCuenta", clientId, comprimido);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error enviando RespuestaPrepararEstadoCuenta: {ex}");
		}
	}

	private async Task CrearEstadoCuenta(string clientId, string modeloJson)
	{
		CrearEstadoCuentaResponse res;

		try
		{
			if (string.IsNullOrWhiteSpace(modeloJson))
				throw new Exception("modeloJson llegó vacío.");

			var req = System.Text.Json.JsonSerializer.Deserialize<CrearEstadoCuentaRequest>(
				modeloJson,
				new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
			) ?? throw new Exception("No se pudo deserializar CrearEstadoCuentaRequest (req=null).");

			// Validaciones mínimas típicas
			if (req.PacienteId <= 0 || req.DoctorId <= 0)
				throw new Exception($"Request inválido. PacienteId={req.PacienteId}, DoctorId={req.DoctorId}");
			if (req.Fase <= 0)
				throw new Exception($"Request inválido. Fase={req.Fase}");

			using var scope = _scopeFactory.CreateScope();
			var estadoCuentaService = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			res = await estadoCuentaService.CrearAsync(req)
				  ?? new CrearEstadoCuentaResponse
				  {
					  Ok = false,
					  Mensaje = "CrearAsync devolvió null."
				  };
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			res = new CrearEstadoCuentaResponse
			{
				Ok = false,
				Mensaje = $"Error en worker (CrearEstadoCuenta): {ex.Message}"
			};
		}

		try
		{
			var json = System.Text.Json.JsonSerializer.Serialize(res);
			var comprimido = ArchivosHelper.CompressString(json);

			await _hubConnection.InvokeAsync("RespuestaCrearEstadoCuenta", clientId, comprimido);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error enviando RespuestaCrearEstadoCuenta: {ex}");
		}
	}

	private async Task PrepararEditarEstadoCuenta(string clientId, string modeloJson)
	{
		PrepararEditarEstadoCuentaResponse res;

		try
		{
			var req = System.Text.Json.JsonSerializer.Deserialize<PrepararEditarEstadoCuentaRequest>(
				modeloJson,
				new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
			) ?? throw new Exception("No se pudo deserializar PrepararEditarEstadoCuentaRequest.");

			using var scope = _scopeFactory.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			res = await svc.PrepararEditarAsync(req);
		}
		catch (Exception ex)
		{
			res = new PrepararEditarEstadoCuentaResponse { Ok = false, Mensaje = ex.Message };
		}

		var json = System.Text.Json.JsonSerializer.Serialize(res);
		var comprimido = ArchivosHelper.CompressString(json);
		await _hubConnection.InvokeAsync("RespuestaPrepararEditarEstadoCuenta", clientId, comprimido);
	}

	private async Task EditarEstadoCuenta(string clientId, string modeloJson)
	{
		EditarEstadoCuentaResponse res;

		try
		{
			var req = System.Text.Json.JsonSerializer.Deserialize<EditarEstadoCuentaRequest>(
				modeloJson,
				new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
			) ?? throw new Exception("No se pudo deserializar EditarEstadoCuentaRequest.");

			using var scope = _scopeFactory.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			res = await svc.EditarAsync(req);
		}
		catch (Exception ex)
		{
			res = new EditarEstadoCuentaResponse { Ok = false, Mensaje = ex.Message };
		}

		var json = System.Text.Json.JsonSerializer.Serialize(res);
		var comprimido = ArchivosHelper.CompressString(json);
		await _hubConnection.InvokeAsync("RespuestaEditarEstadoCuenta", clientId, comprimido);
	}


	private async Task BorrarEstadoCuenta(string clientId, string modeloJson)
	{
		BorrarEstadoCuentaResponse res;

		try
		{
			var req = System.Text.Json.JsonSerializer.Deserialize<BorrarEstadoCuentaRequest>(
				modeloJson,
				new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
			) ?? throw new Exception("No se pudo deserializar BorrarEstadoCuentaRequest.");

			using var scope = _scopeFactory.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			res = await svc.BorrarAsync(req);
		}
		catch (Exception ex)
		{
			res = new BorrarEstadoCuentaResponse { Ok = false, Mensaje = ex.Message };
		}

		var json = System.Text.Json.JsonSerializer.Serialize(res);
		var comprimido = ArchivosHelper.CompressString(json);
		await _hubConnection.InvokeAsync("RespuestaBorrarEstadoCuenta", clientId, comprimido);
	}


	private async Task ConsultarSugeridosAbono(string clientId, string modeloJson)
	{
		ConsultarSugeridosAbonoResponse res;

		try
		{
			var req = System.Text.Json.JsonSerializer.Deserialize<ConsultarSugeridosAbonoRequest>(
				modeloJson, JsonHelper.Options
			) ?? throw new Exception("No se pudo deserializar ConsultarSugeridosAbonoRequest.");

			using var scope = _scopeFactory.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			res = await svc.ConsultarSugeridosAbonoAsync(req);
		}
		catch (Exception ex)
		{
			res = new ConsultarSugeridosAbonoResponse
			{
				Ok = false,
				Mensaje = ex.Message,
				OcultarFactura = false,
				ReciboSugerido = null,
				FacturaSugerida = null,
				IdResolucionDian = null
			};
		}

		var json = System.Text.Json.JsonSerializer.Serialize(res, JsonHelper.Options);
		var comprimido = ArchivosHelper.CompressString(json);

		await _hubConnection.InvokeAsync("RespuestaConsultarSugeridosAbono", clientId, comprimido);
	}


	private async Task PrepararInsertarAbono(string clientId, string modeloJson)
	{
		PrepararInsertarAbonoResponse res;

		try
		{
			var req = System.Text.Json.JsonSerializer.Deserialize<PrepararInsertarAbonoRequest>(
				modeloJson, JsonHelper.Options
			) ?? throw new Exception("No se pudo deserializar PrepararInsertarAbonoRequest.");

			using var scope = _scopeFactory.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			res = await svc.PrepararInsertarAbonoAsync(req);
		}
		catch (Exception ex)
		{
			res = new PrepararInsertarAbonoResponse { Ok = false, Mensaje = ex.Message };
		}

		var json = System.Text.Json.JsonSerializer.Serialize(res, JsonHelper.Options);
		var comprimido = ArchivosHelper.CompressString(json);

		await _hubConnection.InvokeAsync("RespuestaPrepararInsertarAbono", clientId, comprimido);
	}

	private async Task InsertarAbono(string clientId, string modeloJson)
	{
		InsertarAbonoResponse res;

		try
		{
			var req = System.Text.Json.JsonSerializer.Deserialize<InsertarAbonoRequest>(
				modeloJson, JsonHelper.Options
			) ?? throw new Exception("No se pudo deserializar InsertarAbonoRequest.");

			using var scope = _scopeFactory.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			res = await svc.InsertarAbonoAsync(req);
		}
		catch (Exception ex)
		{
			res = new InsertarAbonoResponse { Ok = false, Mensaje = ex.Message };
		}

		var json = System.Text.Json.JsonSerializer.Serialize(res, JsonHelper.Options);
		var comprimido = ArchivosHelper.CompressString(json);

		await _hubConnection.InvokeAsync("RespuestaInsertarAbono", clientId, comprimido);
	}

	private async Task PrepararInsertarAdicional(string clientId, string modeloJson)
	{
		PrepararInsertarAdicionalResponse res;

		try
		{
			var req = System.Text.Json.JsonSerializer.Deserialize<PrepararInsertarAdicionalRequest>(
				modeloJson, JsonHelper.Options
			) ?? throw new Exception("No se pudo deserializar PrepararInsertarAdicionalRequest.");

			using var scope = _scopeFactory.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			res = await svc.PrepararInsertarAdicionalAsync(req);
		}
		catch (Exception ex)
		{
			res = new PrepararInsertarAdicionalResponse { Ok = false, Mensaje = ex.Message };
		}

		var json = System.Text.Json.JsonSerializer.Serialize(res, JsonHelper.Options);
		var comprimido = ArchivosHelper.CompressString(json);

		await _hubConnection.InvokeAsync("RespuestaPrepararInsertarAdicional", clientId, comprimido);
	}

	private async Task InsertarAdicional(string clientId, string modeloJson)
	{
		InsertarAdicionalResponse res;

		try
		{
			var req = System.Text.Json.JsonSerializer.Deserialize<InsertarAdicionalRequest>(
				modeloJson, JsonHelper.Options
			) ?? throw new Exception("No se pudo deserializar InsertarAdicionalRequest.");

			using var scope = _scopeFactory.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			res = await svc.InsertarAdicionalAsync(req);
		}
		catch (Exception ex)
		{
			res = new InsertarAdicionalResponse { Ok = false, Mensaje = ex.Message };
		}

		var json = System.Text.Json.JsonSerializer.Serialize(res, JsonHelper.Options);
		var comprimido = ArchivosHelper.CompressString(json);

		await _hubConnection.InvokeAsync("RespuestaInsertarAdicional", clientId, comprimido);
	}



	// ---------------------------------------------------------
	// 1) PREPARAR BORRAR ABONO (NO borra, solo trae contexto)
	// ---------------------------------------------------------
	private async Task PrepararBorrarAbono(string clientId, string modeloJson)
	{
		PrepararBorrarAbonoResponse res;

		try
		{
			var req = System.Text.Json.JsonSerializer.Deserialize<PrepararBorrarAbonoRequest>(
				modeloJson, JsonHelper.Options
			) ?? throw new Exception("No se pudo deserializar PrepararBorrarAbonoRequest.");

			using var scope = _scopeFactory.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			// ✅ Aquí NO borramos nada.
			// Solo traemos contexto (igual que Delphi arma el texto y pide motivo),
			// y devolvemos lo que el front necesita para confirmar.
			res = await svc.PrepararBorrarAbonoAsync(req);
		}
		catch (Exception ex)
		{
			res = new PrepararBorrarAbonoResponse { Ok = false, Mensaje = ex.Message };
		}

		var json = System.Text.Json.JsonSerializer.Serialize(res, JsonHelper.Options);
		var comprimido = ArchivosHelper.CompressString(json);

		await _hubConnection.InvokeAsync("RespuestaPrepararBorrarAbono", clientId, comprimido);
	}

	private async Task BorrarAbono(string clientId, string modeloJson)
	{
		BorrarAbonoResponse res;

		try
		{
			var req = System.Text.Json.JsonSerializer.Deserialize<BorrarAbonoRequest>(
				modeloJson, JsonHelper.Options
			) ?? throw new Exception("No se pudo deserializar BorrarAbonoRequest.");

			using var scope = _scopeFactory.CreateScope();
			var svc = scope.ServiceProvider.GetRequiredService<IEstadoCuentaService>();

			// ✅ Aquí SÍ se ejecuta el flujo Delphi:
			// 1) Insertar a T_ADICIONALES_ABONOS_BORRAR (copia + motivo)
			// 2) Delete de T_ADICIONALES_ABONOS (por IDENTIFICADOR)
			// 3) Delete de T_ABONOS_TIPO_PAGO (por IDRELACION)
			// 4) Delete de T_ADICIONALES_ABONOS_MOTIVOS (por IDRELACION)
			// 5) Delete de T_ABONOS_XDR_PAGAR (por IDABONO/IDADICIONAL)
			// 6) Execute procedure ACTUALIZARESTACUENTA
			res = await svc.BorrarAbonoAsync(req);
		}
		catch (Exception ex)
		{
			res = new BorrarAbonoResponse { Ok = false, Mensaje = ex.Message };
		}

		var json = System.Text.Json.JsonSerializer.Serialize(res, JsonHelper.Options);
		var comprimido = ArchivosHelper.CompressString(json);

		await _hubConnection.InvokeAsync("RespuestaBorrarAbono", clientId, comprimido);
	}


	public async Task ObtenerDatosEvolucion(string clientId, int idAnanesis)
    {
        var objEvolucion = new TEVOLUCIONServicios();
        var listEvolucion = await objEvolucion.ConsultarPorAnamnesisUltimas50(idAnanesis);
        var respuestaBuscarEvolucion = new List<RespuestaEvolucionPacienteModel>();
        var archivosHelper = new ArchivosHelper();
        foreach (var item in listEvolucion)
        {
            var objEvolucion1 = new RespuestaEvolucionPacienteModel();
            objEvolucion1.evolucion = item;
            objEvolucion1.imgFirmaPaciente = "";
            objEvolucion1.imgFirmaDoctor = "";
            if (item.FIRMA != null)
            {
                if (item.FIRMA > 0)
                {
                    try
                    {
                        objEvolucion1.imgFirmaPaciente = item.FIRMA == null ? "" : (item.FIRMA <= 0 ? "" : await RetornarFotoEnBase64ConPrefijo(item.FIRMA ?? -1, 1));
                        objEvolucion1.imgFirmaDoctor = item.FIRMA == null ? "" : (item.FIRMA <= 0 ? "" : await RetornarFotoEnBase64ConPrefijo(item.FIRMA ?? -1, 2));
                    }
                    catch (Exception e)
                    {

                        objEvolucion1.imgFirmaPaciente = "";
                        objEvolucion1.imgFirmaDoctor = "";
                    }
                   
                }
            }   
           
            respuestaBuscarEvolucion.Add(objEvolucion1);
            
        }

        var objRespuestaObtenerDatosEvolucion = JsonConvert.SerializeObject(respuestaBuscarEvolucion);
        try
        {
            var objRespuestaObtenerDatosEvolucionComprimido = ArchivosHelper.CompressString(objRespuestaObtenerDatosEvolucion);
            await _hubConnection.InvokeAsync("RespuestaObtenerDatosEvolucion", clientId, objRespuestaObtenerDatosEvolucionComprimido);
        }
        catch (Exception e)
        {

            throw;
        }
        
    }


	

	public async Task GuardarDatosRips(string clientId, string datosRips)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(datosRips))
			{
				await _hubConnection.InvokeAsync("RespuestaGuardarDatosRips", clientId, false, "Payload vacío.");
				return;
			}

			var objRips = JsonConvert.DeserializeObject<DatosGuardarRips>(datosRips);

			if (objRips == null)
			{
				await _hubConnection.InvokeAsync("RespuestaGuardarDatosRips", clientId, false, "No fue posible deserializar el payload.");
				return;
			}

			if ((objRips.IDANAMNESIS ?? 0) <= 0)
			{
				await _hubConnection.InvokeAsync("RespuestaGuardarDatosRips", clientId, false, "IDANAMNESIS inválido.");
				return;
			}

			using var db = new AppDbContext();
			using var trx = await db.Database.BeginTransactionAsync();

			var objTAnamnesis = await db.TANAMNESIS
				.FirstOrDefaultAsync(x => x.IDANAMNESIS == objRips.IDANAMNESIS);

			if (objTAnamnesis == null)
			{
				await _hubConnection.InvokeAsync("RespuestaGuardarDatosRips", clientId, false, "No existe la anamnesis.");
				await trx.RollbackAsync();
				return;
			}

			var modo = NormalizarModo(objRips.MODO);
			var fechaAtencion = objRips.FECHACONSULTA?.Date ?? DateTime.Now.Date;

			var codigoEntidad = LimpiarTexto(objRips.CODIGOENTIDAD, "000000");
			var extranjero = LimpiarTexto(objRips.EXTRANJERO, "NO").ToUpperInvariant();
			var pais = LimpiarTexto(objRips.PAIS, string.Empty).ToUpperInvariant();
			var numeroAutorizacion = LimpiarTexto(objRips.NUMEROAUTORIZACION, string.Empty);

			var tieneConsulta = TieneConsultaReal(objRips);
			var procedimientos = NormalizarProcedimientos(objRips);
			var tieneProcedimientos = procedimientos.Any();

			if (!tieneConsulta && !tieneProcedimientos)
			{
				await _hubConnection.InvokeAsync("RespuestaGuardarDatosRips", clientId, false, "Debe enviar una consulta o al menos un procedimiento.");
				return;
			}

			string facturaTrabajo;
			TimeSpan horaLote;

			string facturaOriginal = LimpiarTexto(objRips.FACTURAORIGINAL, string.Empty);

			if (modo == "EDITAR")
			{
				facturaTrabajo = LimpiarTexto(objRips.FACTURA, string.Empty);

				if (string.Equals(facturaTrabajo, "AUTO", StringComparison.OrdinalIgnoreCase))
				{
					facturaTrabajo = await db.GEN_CONSECUTIVO_RIPS();
				}
				else if (string.IsNullOrWhiteSpace(facturaTrabajo))
				{
					facturaTrabajo = facturaOriginal;
				}

				if (string.IsNullOrWhiteSpace(facturaTrabajo))
				{
					await _hubConnection.InvokeAsync(
						"RespuestaGuardarDatosRips",
						clientId,
						false,
						"Para editar debe enviar FACTURA, FACTURAORIGINAL o AUTO."
					);
					return;
				}

				// En edición conservamos la hora vieja si vino.
				var horaOriginalNormalizada = HoraVacia(objRips.HORAORIGINAL)
					? (TimeSpan?)null
					: NormalizarHora(objRips.HORAORIGINAL);

				horaLote = horaOriginalNormalizada ?? ObtenerHoraLoteNormalizada();
			}
			else
			{
				if (string.Equals(objRips.FACTURA, "AUTO", StringComparison.OrdinalIgnoreCase))
				{
					objRips.FACTURA = await db.GEN_CONSECUTIVO_RIPS();
				}

				facturaTrabajo = string.IsNullOrWhiteSpace(objRips.FACTURA)
					? "PENDIENTE"
					: objRips.FACTURA.Trim();

				horaLote = ObtenerHoraLoteNormalizada();
			}

			objRips.FACTURA = facturaTrabajo;
			objRips.HORALOTE = horaLote;

			var codigoPrestador = await ObtenerCodigoPrestadorAsync(db, objRips.IDDOCTOR ?? 0);
			codigoPrestador = LimpiarTexto(codigoPrestador, "000000");

			if (modo == "EDITAR" || (objRips.REEMPLAZAR_EXISTENTE ?? false))
			{
				var horaOriginalParaEliminar = HoraVacia(objRips.HORAORIGINAL)
					? (TimeSpan?)null
					: NormalizarHora(objRips.HORAORIGINAL);

				var fechaOriginalParaEliminar = objRips.FECHAORIGINAL?.Date ?? fechaAtencion;

				var rdaConsultaEnviado = await EstaRdaConsultaEnviadoAsync(
					db,
					objRips.IDANAMNESIS ?? 0,
					fechaOriginalParaEliminar,
					horaOriginalParaEliminar
				);

				if (rdaConsultaEnviado)
				{
					await _hubConnection.InvokeAsync(
						"RespuestaGuardarDatosRips",
						clientId,
						false,
						"No se puede editar este RIPS porque su RDA de consulta ya fue enviado."
					);
					return;
				}

				await EliminarRipsExistenteAsync(
					db,
					objRips.IDANAMNESIS ?? 0,
					string.IsNullOrWhiteSpace(facturaOriginal) ? facturaTrabajo : facturaOriginal,
					fechaOriginalParaEliminar,
					horaOriginalParaEliminar
				);

				await EliminarRdaConsultaExistenteAsync(
					db,
					objRips.IDANAMNESIS ?? 0,
					fechaOriginalParaEliminar,
					horaOriginalParaEliminar
				);
			}
			else
			{
				var yaExiste = await ExisteRipsAsync(
					db,
					objRips.IDANAMNESIS ?? 0,
					facturaTrabajo,
					fechaAtencion,
					horaLote
				);

				if (yaExiste)
				{
					await _hubConnection.InvokeAsync(
						"RespuestaGuardarDatosRips",
						clientId,
						false,
						$"Ya existe un RIPS igual para la anamnesis {objRips.IDANAMNESIS}, factura {facturaTrabajo}, fecha {fechaAtencion:yyyy-MM-dd} y hora {horaLote}."
					);
					return;
				}
			}

			if (tieneConsulta)
			{
				var dx = new T_RIPS_DX
				{
					IDANAMNESIS = objRips.IDANAMNESIS,
					IDDOCTOR = objRips.IDDOCTOR,
					FACTURA = facturaTrabajo,
					CODIGOPRESTADOR = codigoPrestador,
					TIPOIDENTIFICACION = LimpiarTexto(objTAnamnesis.DOCUMENTO_IDENTIDAD, string.Empty),
					IDENTIFICACION = LimpiarTexto(objTAnamnesis.CEDULA_NUMERO, string.Empty),
					FECHACONSULTA = fechaAtencion,
					NUMEROAUTORIZACION = numeroAutorizacion,
					CODIGOCONSULTA = LimpiarTexto(objRips.CODIGOCONSULTA, string.Empty),
					FINALIDADCONSULTA = LimpiarTexto(objRips.FINALIDADCONSULTA, string.Empty),
					CAUSAEXTERNA = LimpiarTexto(objRips.CAUSAEXTERNA, string.Empty),
					DX1 = LimpiarTexto(objRips.CODIGODIAGNOSTICOPRINCIPAL, string.Empty),
					DX2 = LimpiarTexto(objRips.CODIGODIAGNOSTICO2, string.Empty),
					DX3 = LimpiarTexto(objRips.CODIGODIAGNOSTICO3, string.Empty),
					DX4 = LimpiarTexto(objRips.CODIGODIAGNOSTICO4, string.Empty),
					TIPODIAGNOSTICO = LimpiarTexto(objRips.TIPODIAGNOSTICO, string.Empty),
					VALORCONSULTA = NormalizarNumero(objRips.VALORCONSULTA),
					VALORCUOTAMODERADORA = NormalizarNumero(objRips.VALORCUOTAMODERADORA),
					VALORNETO = NormalizarNumero(objRips.VALORNETO),
					CODIGOENTIDAD = codigoEntidad,
					EXTRANJERO = extranjero,
					PAIS = pais,
					HORA = horaLote
				};

				db.T_RIPS_DX.Add(dx);
			}

			if (tieneProcedimientos)
			{
				foreach (var proc in procedimientos)
				{
					var entidadProc = new T_RIPS_PROCEDIMIENTOS
					{
						IDANAMNESIS = objRips.IDANAMNESIS,
						IDDOCTOR = objRips.IDDOCTOR,
						FACTURA = facturaTrabajo,
						CODIGOPRESTADOR = codigoPrestador,
						TIPOIDENTIFICACION = LimpiarTexto(objTAnamnesis.DOCUMENTO_IDENTIDAD, string.Empty),
						IDENTIFICACION = LimpiarTexto(objTAnamnesis.CEDULA_NUMERO, string.Empty),
						FECHAPROCEDIMIENTO = fechaAtencion,
						NUMEROAUTORIZACION = numeroAutorizacion,
						CODIGOPROCEDIMIENTO = LimpiarTexto(proc.CODIGOPROCEDIMIENTO, string.Empty),
						AMBITOREALIZACION = LimpiarTexto(proc.AMBITOREALIZACION, string.Empty),
						FINALIDADPROCEDIMIENTI = LimpiarTexto(proc.FINALIDADPROCEDIMIENTI, string.Empty),
						PERSONALQUEATIENDE = LimpiarTexto(proc.PERSONALQUEATIENDE, string.Empty),
						DXPRINCIPAL = LimpiarTexto(
							string.IsNullOrWhiteSpace(proc.DXPRINCIPAL)
								? objRips.CODIGODIAGNOSTICOPRINCIPAL
								: proc.DXPRINCIPAL,
							string.Empty
						),
						DXRELACIONADO = LimpiarTexto(proc.DXRELACIONADO, string.Empty),
						COMPLICACION = LimpiarTexto(proc.COMPLICACION, string.Empty),
						FORMAREALIZACIONACTOQUIR = LimpiarTexto(proc.FORMAREALIZACIONACTOQUIR, string.Empty),
						VALORPROCEDIMIENTO = NormalizarNumero(proc.VALORPROCEDIMIENTO),
						CODIGOENTIDAD = codigoEntidad,
						EXTRANJERO = extranjero,
						PAIS = pais,
						HORA = horaLote
					};

					db.T_RIPS_PROCEDIMIENTOS.Add(entidadProc);
				}
			}

			await db.SaveChangesAsync();

			db.THISTORIAL.Add(new THISTORIAL
			{
				FECHA = DateTime.Now.Date,
				HORA = DateTime.Now.TimeOfDay,
				USUARIO = "",
				DESCRIPCION =
					$"RIPS {(modo == "EDITAR" ? "editado" : "guardado")} - Factura {facturaTrabajo} - Fecha {fechaAtencion:yyyy-MM-dd} - Hora lote {horaLote} - Paciente {objTAnamnesis.IDANAMNESIS_TEXTO} - {objTAnamnesis.NOMBRE_PACIENTE}"
			});

			await db.SaveChangesAsync();
			await trx.CommitAsync();

			// Interoperabilidad se ejecuta por fuera de la transacción del RIPS
			var rdaOrquestador = new RdaOrquestadorService(_configuration);

			var resultadoPaciente = await rdaOrquestador.GenerarPacientePorAnamnesisAsync(
				objRips.IDANAMNESIS ?? 0,
				objRips.IDDOCTOR,
				fechaAtencion,
				true
			);

			var resultadoConsulta = await rdaOrquestador.GenerarDesdeRipsAsync(objRips, true);

			// OJO:
			// La operación principal aquí es GUARDAR RIPS.
			// Si el RIPS quedó guardado, respondemos ok=true y mandamos detalle de RDAs.
			var mensajeFinal =
				$"Factura:{facturaTrabajo}|Fecha:{fechaAtencion:yyyy-MM-dd}|Hora:{horaLote}" +
				$"|RdaPaciente:{resultadoPaciente.Estado}|RdaConsulta:{resultadoConsulta.Estado}" +
				$"|MsgPaciente:{resultadoPaciente.Mensaje}|MsgConsulta:{resultadoConsulta.Mensaje}";

			await _hubConnection.InvokeAsync(
				"RespuestaGuardarDatosRips",
				clientId,
				true,
				mensajeFinal
			);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"[Worker] Error GuardarDatosRips: {ex}");
			await _hubConnection.InvokeAsync("RespuestaGuardarDatosRips", clientId, false, ex.Message);
		}
	}

	public async Task GenerarRdaDesdeRipsExistente(string clientId, string payloadJson)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(payloadJson))
			{
				await _hubConnection.InvokeAsync(
					"RespuestaGenerarRdaDesdeRipsExistente",
					clientId,
					false,
					"Payload vacío.");
				return;
			}

			var request = JsonConvert.DeserializeObject<GenerarRdaDesdeRipsRequest>(payloadJson);

			if (request == null)
			{
				await _hubConnection.InvokeAsync(
					"RespuestaGenerarRdaDesdeRipsExistente",
					clientId,
					false,
					"No fue posible deserializar el payload.");
				return;
			}

			if (request.IDANAMNESIS <= 0)
			{
				await _hubConnection.InvokeAsync(
					"RespuestaGenerarRdaDesdeRipsExistente",
					clientId,
					false,
					"IDANAMNESIS inválido.");
				return;
			}

			if (!request.FECHA.HasValue)
			{
				await _hubConnection.InvokeAsync(
					"RespuestaGenerarRdaDesdeRipsExistente",
					clientId,
					false,
					"FECHA inválida.");
				return;
			}

			var horaAtencion = string.IsNullOrWhiteSpace(request.HORA)
				? (TimeSpan?)null
				: TimeSpan.Parse(request.HORA);

			var fechaAtencion = request.FECHA.Value.Date;
			var factura = string.IsNullOrWhiteSpace(request.FACTURA)
				? null
				: request.FACTURA.Trim();

			var objRdaDocumentoServicios = new TRDADOCUMENTOServicios();

			var existenteConsulta = await objRdaDocumentoServicios.ConsultarConsultaExistente(
				request.IDANAMNESIS,
				fechaAtencion,
				horaAtencion,
				"RDA_CONSULTA_INTERNO");

			if (existenteConsulta.ID > 0)
			{
				await _hubConnection.InvokeAsync(
					"RespuestaGenerarRdaDesdeRipsExistente",
					clientId,
					false,
					"Este RIPS ya tiene RDA de consulta generado.");
				return;
			}

			var rebuildService = new RdaRipsRebuildService();

			var datosRips = await rebuildService.ReconstruirExactoAsync(
				request.IDANAMNESIS,
				fechaAtencion,
				horaAtencion,
				factura);

			if (datosRips == null)
			{
				await _hubConnection.InvokeAsync(
					"RespuestaGenerarRdaDesdeRipsExistente",
					clientId,
					false,
					"No se pudo reconstruir el RIPS.");
				return;
			}

			var rdaOrquestador = new RdaOrquestadorService(_configuration);

			var resultadoPaciente = await rdaOrquestador.GenerarPacientePorAnamnesisAsync(
				datosRips.IDANAMNESIS ?? 0,
				datosRips.IDDOCTOR,
				datosRips.FECHACONSULTA,
				true
			);

			var resultadoConsulta = await rdaOrquestador.GenerarDesdeRipsAsync(
				datosRips,
				true
			);

			var mensajeFinal =
				$"RDA generado correctamente. " +
				$"Paciente: {resultadoPaciente.Estado}. " +
				$"Consulta: {resultadoConsulta.Estado}.";

			await _hubConnection.InvokeAsync(
				"RespuestaGenerarRdaDesdeRipsExistente",
				clientId,
				resultadoConsulta.Ok,
				mensajeFinal);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"[Worker] Error GenerarRdaDesdeRipsExistente: {ex}");
			await _hubConnection.InvokeAsync(
				"RespuestaGenerarRdaDesdeRipsExistente",
				clientId,
				false,
				ex.Message);
		}
	}



	public async Task ConsultarRipsExistentes(string clientId, string payloadJson)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(payloadJson))
			{
				await _hubConnection.InvokeAsync("RespuestaConsultarRipsExistentes", clientId, "[]");
				return;
			}

			var request = JsonConvert.DeserializeObject<ConsultarRipsRequest>(payloadJson);

			if (request == null || (request.IDANAMNESIS ?? 0) <= 0)
			{
				await _hubConnection.InvokeAsync("RespuestaConsultarRipsExistentes", clientId, "[]");
				return;
			}

			using var db = new AppDbContext();

			var fechaIni = request.FECHAINI?.Date ?? DateTime.Now.Date.AddMonths(-1);
			var fechaFin = request.FECHAFIN?.Date ?? DateTime.Now.Date;

			var dx = await db.T_RIPS_DX
				.Where(x =>
					x.IDANAMNESIS == request.IDANAMNESIS &&
					x.FECHACONSULTA >= fechaIni &&
					x.FECHACONSULTA <= fechaFin)
				.Select(x => new
				{
					x.IDANAMNESIS,
					x.FACTURA,
					FECHA = x.FECHACONSULTA,
					x.HORA,
					TIENE_DX = 1,
					CODIGOENTIDAD = x.CODIGOENTIDAD
				})
				.ToListAsync();

			var procs = await db.T_RIPS_PROCEDIMIENTOS
				.Where(x =>
					x.IDANAMNESIS == request.IDANAMNESIS &&
					x.FECHAPROCEDIMIENTO >= fechaIni &&
					x.FECHAPROCEDIMIENTO <= fechaFin)
				.Select(x => new
				{
					x.IDANAMNESIS,
					x.FACTURA,
					FECHA = x.FECHAPROCEDIMIENTO,
					x.HORA,
					TIENE_DX = 0,
					CODIGOENTIDAD = x.CODIGOENTIDAD
				})
				.ToListAsync();

			var unidos = dx.Concat(procs)
				.Select(x => new
				{
					x.IDANAMNESIS,
					FACTURA = LimpiarTexto(x.FACTURA, string.Empty),
					x.FECHA,
					HORA = HoraVacia(x.HORA) ? (TimeSpan?)null : NormalizarHora(x.HORA),
					x.TIENE_DX,
					x.CODIGOENTIDAD
				})
				.GroupBy(x => new
				{
					x.IDANAMNESIS,
					x.FACTURA,
					x.FECHA,
					x.HORA
				})
				.Select(g => new RipsListadoItem
				{
					IDANAMNESIS = g.Key.IDANAMNESIS,
					FACTURA = g.Key.FACTURA,
					FECHA = g.Key.FECHA,
					HORA = g.Key.HORA,
					TIENECONSULTA = g.Any(y => y.TIENE_DX == 1),
					CANTIDADPROCEDIMIENTOS = g.Count(y => y.TIENE_DX == 0),
					ENTIDAD = g.Select(y => y.CODIGOENTIDAD).FirstOrDefault(),
					DESCRIPCION =
						$"Consulta: {(g.Any(y => y.TIENE_DX == 1) ? "SI" : "NO")} | Procedimientos: {g.Count(y => y.TIENE_DX == 0)}"
				})
				.OrderByDescending(x => x.FECHA)
				.ThenByDescending(x => x.HORA)
				.ToList();

			await _hubConnection.InvokeAsync(
				"RespuestaConsultarRipsExistentes",
				clientId,
				JsonConvert.SerializeObject(unidos)
			);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"[Worker] Error ConsultarRipsExistentes: {ex}");
			await _hubConnection.InvokeAsync("RespuestaConsultarRipsExistentes", clientId, "[]");
		}
	}

	public async Task EliminarRipsPorLlave(string clientId, string payloadJson)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(payloadJson))
			{
				await _hubConnection.InvokeAsync(
					"RespuestaEliminarRipsPorLlave",
					clientId,
					false,
					"Payload vacío."
				);
				return;
			}

			var request = JsonConvert.DeserializeObject<EliminarRipsRequest>(payloadJson);

			if (request == null)
			{
				await _hubConnection.InvokeAsync(
					"RespuestaEliminarRipsPorLlave",
					clientId,
					false,
					"Request inválido."
				);
				return;
			}

			if ((request.IDANAMNESIS ?? 0) <= 0 || request.FECHA == null)
			{
				await _hubConnection.InvokeAsync(
					"RespuestaEliminarRipsPorLlave",
					clientId,
					false,
					"Faltan datos para eliminar."
				);
				return;
			}

			using var db = new AppDbContext();
			using var trx = await db.Database.BeginTransactionAsync();

			var factura = LimpiarTexto(request.FACTURA, string.Empty);
			var fecha = request.FECHA.Value.Date;
			var hora = HoraVacia(request.HORA) ? (TimeSpan?)null : NormalizarHora(request.HORA);

			var rdaConsultaEnviado = await EstaRdaConsultaEnviadoAsync(
				db,
				request.IDANAMNESIS ?? 0,
				fecha,
				hora
			);

			if (rdaConsultaEnviado)
			{
				await _hubConnection.InvokeAsync(
					"RespuestaEliminarRipsPorLlave",
					clientId,
					false,
					"No se puede eliminar este RIPS porque su RDA de consulta ya fue enviado."
				);
				return;
			}

			await EliminarRipsExistenteAsync(
				db,
				request.IDANAMNESIS ?? 0,
				factura,
				fecha,
				hora
			);

			await EliminarRdaConsultaExistenteAsync(
				db,
				request.IDANAMNESIS ?? 0,
				fecha,
				hora
			);

			db.THISTORIAL.Add(new THISTORIAL
			{
				FECHA = DateTime.Now.Date,
				HORA = DateTime.Now.TimeOfDay,
				USUARIO = "",
				DESCRIPCION =
					$"RIPS eliminado - Factura {factura} - Fecha {fecha:yyyy-MM-dd} - Hora {(hora == null ? "SIN HORA" : hora.ToString())}"
			});

			await db.SaveChangesAsync();
			await trx.CommitAsync();

			await _hubConnection.InvokeAsync(
				"RespuestaEliminarRipsPorLlave",
				clientId,
				true,
				"RIPS eliminado correctamente."
			);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"[Worker] Error EliminarRipsPorLlave: {ex}");
			await _hubConnection.InvokeAsync(
				"RespuestaEliminarRipsPorLlave",
				clientId,
				false,
				ex.Message
			);
		}
	}

	public async Task ConsultarRipsDetallePorLlave(string clientId, string payloadJson)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(payloadJson))
			{
				await _hubConnection.InvokeAsync("RespuestaConsultarRipsDetallePorLlave", clientId, "null");
				return;
			}

			var request = JsonConvert.DeserializeObject<ConsultarRipsDetalleRequest>(payloadJson);

			if (request == null ||
				(request.IDANAMNESIS ?? 0) <= 0 ||
				request.FECHA == null)
			{
				await _hubConnection.InvokeAsync("RespuestaConsultarRipsDetallePorLlave", clientId, "null");
				return;
			}

			using var db = new AppDbContext();

			var fecha = request.FECHA.Value.Date;
			var fechaSolo = fecha.Date;
			var fechaFin = fechaSolo.AddDays(1);
			var hora = HoraVacia(request.HORA) ? (TimeSpan?)null : NormalizarHora(request.HORA);
			var factura = LimpiarTexto(request.FACTURA, string.Empty);
			var idAnamnesis = request.IDANAMNESIS ?? 0;

			T_RIPS_DX? dx = null;
			List<T_RIPS_PROCEDIMIENTOS> procedimientos = new();

			// =========================
			// 1. intento exacto con hora
			// =========================
			if (hora != null)
			{
				if (TieneFacturaFuerte(factura))
				{
					dx = await db.T_RIPS_DX
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FACTURA == factura &&
							x.FECHACONSULTA >= fechaSolo &&
							x.FECHACONSULTA < fechaFin &&
							x.HORA == hora)
						.FirstOrDefaultAsync();

					procedimientos = await db.T_RIPS_PROCEDIMIENTOS
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FACTURA == factura &&
							x.FECHAPROCEDIMIENTO >= fechaSolo &&
							x.FECHAPROCEDIMIENTO < fechaFin &&
							x.HORA == hora)
						.OrderBy(x => x.ID)
						.ToListAsync();
				}
				else
				{
					dx = await db.T_RIPS_DX
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FECHACONSULTA >= fechaSolo &&
							x.FECHACONSULTA < fechaFin &&
							x.HORA == hora)
						.FirstOrDefaultAsync();

					procedimientos = await db.T_RIPS_PROCEDIMIENTOS
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FECHAPROCEDIMIENTO >= fechaSolo &&
							x.FECHAPROCEDIMIENTO < fechaFin &&
							x.HORA == hora)
						.OrderBy(x => x.ID)
						.ToListAsync();
				}
			}

			// =========================
			// 2. fallback exacto sin hora (legacy)
			// =========================
			if (dx == null && !procedimientos.Any())
			{
				if (TieneFacturaFuerte(factura))
				{
					dx = await db.T_RIPS_DX
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FACTURA == factura &&
							x.FECHACONSULTA >= fechaSolo &&
							x.FECHACONSULTA < fechaFin &&
							x.HORA == null)
						.FirstOrDefaultAsync();

					procedimientos = await db.T_RIPS_PROCEDIMIENTOS
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FACTURA == factura &&
							x.FECHAPROCEDIMIENTO >= fechaSolo &&
							x.FECHAPROCEDIMIENTO < fechaFin &&
							x.HORA == null)
						.OrderBy(x => x.ID)
						.ToListAsync();
				}
				else
				{
					dx = await db.T_RIPS_DX
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FECHACONSULTA >= fechaSolo &&
							x.FECHACONSULTA < fechaFin &&
							x.HORA == null)
						.FirstOrDefaultAsync();

					procedimientos = await db.T_RIPS_PROCEDIMIENTOS
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FECHAPROCEDIMIENTO >= fechaSolo &&
							x.FECHAPROCEDIMIENTO < fechaFin &&
							x.HORA == null)
						.OrderBy(x => x.ID)
						.ToListAsync();
				}
			}

			// =========================
			// 3. fallback amplio legacy sin filtrar hora
			// =========================
			if (dx == null && !procedimientos.Any())
			{
				if (TieneFacturaFuerte(factura))
				{
					dx = await db.T_RIPS_DX
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FACTURA == factura &&
							x.FECHACONSULTA >= fechaSolo &&
							x.FECHACONSULTA < fechaFin)
						.OrderBy(x => x.HORA)
						.FirstOrDefaultAsync();

					procedimientos = await db.T_RIPS_PROCEDIMIENTOS
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FACTURA == factura &&
							x.FECHAPROCEDIMIENTO >= fechaSolo &&
							x.FECHAPROCEDIMIENTO < fechaFin)
						.OrderBy(x => x.HORA)
						.ThenBy(x => x.ID)
						.ToListAsync();
				}
				else
				{
					dx = await db.T_RIPS_DX
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FECHACONSULTA >= fechaSolo &&
							x.FECHACONSULTA < fechaFin)
						.OrderBy(x => x.HORA)
						.FirstOrDefaultAsync();

					procedimientos = await db.T_RIPS_PROCEDIMIENTOS
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FECHAPROCEDIMIENTO >= fechaSolo &&
							x.FECHAPROCEDIMIENTO < fechaFin)
						.OrderBy(x => x.HORA)
						.ThenBy(x => x.ID)
						.ToListAsync();
				}
			}

			if (dx == null && !procedimientos.Any())
			{
				await _hubConnection.InvokeAsync("RespuestaConsultarRipsDetallePorLlave", clientId, "null");
				return;
			}

			var codigoEntidad = dx?.CODIGOENTIDAD
				?? procedimientos.Select(x => x.CODIGOENTIDAD).FirstOrDefault()
				?? string.Empty;

			string nombreEntidad = string.Empty;

			if (!string.IsNullOrWhiteSpace(codigoEntidad))
			{
				nombreEntidad = await db.TCODIGOS_EPS
					.Where(x => x.CODIGO == codigoEntidad)
					.Select(x => x.NOMBRE)
					.FirstOrDefaultAsync() ?? string.Empty;
			}

			var horaRespuestaRaw = dx?.HORA ?? procedimientos.Select(x => x.HORA).FirstOrDefault();
			var horaRespuesta = HoraVacia(horaRespuestaRaw) ? (TimeSpan?)null : NormalizarHora(horaRespuestaRaw);

			var response = new RipsDetalleResponse
			{
				IDANAMNESIS = dx?.IDANAMNESIS ?? procedimientos.Select(x => x.IDANAMNESIS).FirstOrDefault(),
				IDDOCTOR = dx?.IDDOCTOR ?? procedimientos.Select(x => x.IDDOCTOR).FirstOrDefault(),

				FACTURA = factura,
				FECHACONSULTA = dx?.FECHACONSULTA ?? procedimientos.Select(x => x.FECHAPROCEDIMIENTO).FirstOrDefault(),
				HORA = horaRespuesta,

				CODIGOENTIDAD = codigoEntidad,
				NOMBREENTIDAD = nombreEntidad,

				NUMEROAUTORIZACION = dx?.NUMEROAUTORIZACION ?? procedimientos.Select(x => x.NUMEROAUTORIZACION).FirstOrDefault(),
				EXTRANJERO = dx?.EXTRANJERO ?? procedimientos.Select(x => x.EXTRANJERO).FirstOrDefault(),
				PAIS = dx?.PAIS ?? procedimientos.Select(x => x.PAIS).FirstOrDefault(),

				CODIGOCONSULTA = dx?.CODIGOCONSULTA,
				FINALIDADCONSULTA = dx?.FINALIDADCONSULTA,
				CAUSAEXTERNA = dx?.CAUSAEXTERNA,

				CODIGODIAGNOSTICOPRINCIPAL = dx?.DX1,
				CODIGODIAGNOSTICO2 = dx?.DX2,
				CODIGODIAGNOSTICO3 = dx?.DX3,
				CODIGODIAGNOSTICO4 = dx?.DX4,

				TIPODIAGNOSTICO = dx?.TIPODIAGNOSTICO,

				VALORCONSULTA = dx?.VALORCONSULTA,
				VALORCUOTAMODERADORA = dx?.VALORCUOTAMODERADORA,
				VALORNETO = dx?.VALORNETO,

				PROCEDIMIENTOS = procedimientos
					.Select(x => new RipsProcedimientoItem
					{
						CODIGOPROCEDIMIENTO = x.CODIGOPROCEDIMIENTO,
						NOMBREPROCEDIMIENTO = string.Empty,
						DXPRINCIPAL = x.DXPRINCIPAL,
						DXRELACIONADO = x.DXRELACIONADO,
						AMBITOREALIZACION = x.AMBITOREALIZACION,
						FINALIDADPROCEDIMIENTI = x.FINALIDADPROCEDIMIENTI,
						PERSONALQUEATIENDE = x.PERSONALQUEATIENDE,
						VALORPROCEDIMIENTO = x.VALORPROCEDIMIENTO,
						COMPLICACION = x.COMPLICACION,
						FORMAREALIZACIONACTOQUIR = x.FORMAREALIZACIONACTOQUIR
					})
					.ToList()
			};

			await _hubConnection.InvokeAsync(
				"RespuestaConsultarRipsDetallePorLlave",
				clientId,
				JsonConvert.SerializeObject(response)
			);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"[Worker] Error ConsultarRipsDetallePorLlave: {ex}");
			await _hubConnection.InvokeAsync("RespuestaConsultarRipsDetallePorLlave", clientId, "null");
		}
	}

	//---------------------------helper para guardar rips--------------------------------------------//

	private string NormalizarModo(string? modo)
	{
		var valor = (modo ?? string.Empty).Trim().ToUpperInvariant();
		return valor == "EDITAR" ? "EDITAR" : "CREAR";
	}

	private bool HoraVacia(TimeSpan? hora)
	{
		return hora == null || hora.Value == TimeSpan.Zero;
	}
	private string LimpiarTexto(string? valor, string porDefecto = "")
	{
		return string.IsNullOrWhiteSpace(valor) ? porDefecto : valor.Trim();
	}

	private double NormalizarNumero(double? valor)
	{
		return valor ?? 0;
	}

	private bool TieneConsultaReal(DatosGuardarRips obj)
	{
		return !string.IsNullOrWhiteSpace(obj.CODIGOCONSULTA)
			&& !string.IsNullOrWhiteSpace(obj.CODIGODIAGNOSTICOPRINCIPAL);
	}

	private List<RipsProcedimientoItem> NormalizarProcedimientos(DatosGuardarRips obj)
	{
		var lista = new List<RipsProcedimientoItem>();

		if (obj.PROCEDIMIENTOS != null && obj.PROCEDIMIENTOS.Any())
		{
			lista.AddRange(
				obj.PROCEDIMIENTOS
					.Where(x => x != null)
					.Select(x => new RipsProcedimientoItem
					{
						CODIGOPROCEDIMIENTO = LimpiarTexto(x.CODIGOPROCEDIMIENTO, ""),
						NOMBREPROCEDIMIENTO = LimpiarTexto(x.NOMBREPROCEDIMIENTO, ""),
						DXPRINCIPAL = LimpiarTexto(x.DXPRINCIPAL, ""),
						DXRELACIONADO = LimpiarTexto(x.DXRELACIONADO, ""),
						AMBITOREALIZACION = LimpiarTexto(x.AMBITOREALIZACION, "1"),
						FINALIDADPROCEDIMIENTI = LimpiarTexto(x.FINALIDADPROCEDIMIENTI, "02"),
						PERSONALQUEATIENDE = LimpiarTexto(x.PERSONALQUEATIENDE, ""),
						VALORPROCEDIMIENTO = x.VALORPROCEDIMIENTO ?? 0,
						COMPLICACION = LimpiarTexto(x.COMPLICACION, ""),
						FORMAREALIZACIONACTOQUIR = LimpiarTexto(x.FORMAREALIZACIONACTOQUIR, "")
					})
					.Where(TieneProcedimientoReal)
			);
		}
		else
		{
			var legacy = new RipsProcedimientoItem
			{
				CODIGOPROCEDIMIENTO = LimpiarTexto(obj.CODIGOPROCEDIMIENTO, ""),
				DXPRINCIPAL = LimpiarTexto(obj.DXPRINCIPAL, ""),
				DXRELACIONADO = LimpiarTexto(obj.DXRELACIONADO, ""),
				AMBITOREALIZACION = LimpiarTexto(obj.AMBITOREALIZACION, "1"),
				FINALIDADPROCEDIMIENTI = LimpiarTexto(obj.FINALIDADPROCEDIMIENTI, "02"),
				PERSONALQUEATIENDE = LimpiarTexto(obj.PERSONALQUEATIENDE, ""),
				VALORPROCEDIMIENTO = obj.VALORPROCEDIMIENTO ?? 0,
				COMPLICACION = LimpiarTexto(obj.COMPLICACION, ""),
				FORMAREALIZACIONACTOQUIR = LimpiarTexto(obj.FORMAREALIZACIONACTOQUIR, "")
			};

			if (TieneProcedimientoReal(legacy))
			{
				lista.Add(legacy);
			}
		}

		return lista;
	}

	private bool TieneProcedimientoReal(RipsProcedimientoItem item)
	{
		return !string.IsNullOrWhiteSpace(item.CODIGOPROCEDIMIENTO);
	}

	private async Task<bool> ExisteRipsAsync(
	AppDbContext db,
	int idAnamnesis,
	string factura,
	DateTime fecha,
	TimeSpan hora)
	{
		hora = NormalizarHora(hora);
		var fechaSolo = fecha.Date;
		var fechaFin = fechaSolo.AddDays(1);

		if (TieneFacturaFuerte(factura))
		{
			var existeDxId = await db.T_RIPS_DX
				.Where(x =>
					x.IDANAMNESIS == idAnamnesis &&
					x.FECHACONSULTA >= fechaSolo &&
					x.FECHACONSULTA < fechaFin &&
					x.FACTURA == factura &&
					x.HORA == hora)
				.Select(x => (int?)x.ID)
				.FirstOrDefaultAsync();

			if (existeDxId.HasValue)
				return true;

			var existeProcId = await db.T_RIPS_PROCEDIMIENTOS
				.Where(x =>
					x.IDANAMNESIS == idAnamnesis &&
					x.FECHAPROCEDIMIENTO >= fechaSolo &&
					x.FECHAPROCEDIMIENTO < fechaFin &&
					x.FACTURA == factura &&
					x.HORA == hora)
				.Select(x => (int?)x.ID)
				.FirstOrDefaultAsync();

			return existeProcId.HasValue;
		}
		else
		{
			var existeDxId = await db.T_RIPS_DX
				.Where(x =>
					x.IDANAMNESIS == idAnamnesis &&
					x.FECHACONSULTA >= fechaSolo &&
					x.FECHACONSULTA < fechaFin &&
					x.HORA == hora)
				.Select(x => (int?)x.ID)
				.FirstOrDefaultAsync();

			if (existeDxId.HasValue)
				return true;

			var existeProcId = await db.T_RIPS_PROCEDIMIENTOS
				.Where(x =>
					x.IDANAMNESIS == idAnamnesis &&
					x.FECHAPROCEDIMIENTO >= fechaSolo &&
					x.FECHAPROCEDIMIENTO < fechaFin &&
					x.HORA == hora)
				.Select(x => (int?)x.ID)
				.FirstOrDefaultAsync();

			return existeProcId.HasValue;
		}
	}

	private async Task EliminarRipsExistenteAsync(
	AppDbContext db,
	int idAnamnesis,
	string factura,
	DateTime fecha,
	TimeSpan? hora)
	{
		var fechaSolo = fecha.Date;
		var fechaFin = fechaSolo.AddDays(1);
		var horaNormalizada = HoraVacia(hora) ? (TimeSpan?)null : NormalizarHora(hora);

		List<T_RIPS_DX> dx;
		List<T_RIPS_PROCEDIMIENTOS> procedimientos;

		if (TieneFacturaFuerte(factura))
		{
			if (horaNormalizada != null)
			{
				dx = await db.T_RIPS_DX
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.FACTURA == factura &&
						x.FECHACONSULTA >= fechaSolo &&
						x.FECHACONSULTA < fechaFin &&
						x.HORA == horaNormalizada)
					.ToListAsync();

				procedimientos = await db.T_RIPS_PROCEDIMIENTOS
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.FACTURA == factura &&
						x.FECHAPROCEDIMIENTO >= fechaSolo &&
						x.FECHAPROCEDIMIENTO < fechaFin &&
						x.HORA == horaNormalizada)
					.ToListAsync();

				// fallback legacy: por si el registro viejo estaba con HORA NULL
				if (!dx.Any() && !procedimientos.Any())
				{
					dx = await db.T_RIPS_DX
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FACTURA == factura &&
							x.FECHACONSULTA >= fechaSolo &&
							x.FECHACONSULTA < fechaFin &&
							x.HORA == null)
						.ToListAsync();

					procedimientos = await db.T_RIPS_PROCEDIMIENTOS
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FACTURA == factura &&
							x.FECHAPROCEDIMIENTO >= fechaSolo &&
							x.FECHAPROCEDIMIENTO < fechaFin &&
							x.HORA == null)
						.ToListAsync();
				}
			}
			else
			{
				dx = await db.T_RIPS_DX
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.FACTURA == factura &&
						x.FECHACONSULTA >= fechaSolo &&
						x.FECHACONSULTA < fechaFin &&
						x.HORA == null)
					.ToListAsync();

				procedimientos = await db.T_RIPS_PROCEDIMIENTOS
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.FACTURA == factura &&
						x.FECHAPROCEDIMIENTO >= fechaSolo &&
						x.FECHAPROCEDIMIENTO < fechaFin &&
						x.HORA == null)
					.ToListAsync();

				// fallback por si algún registro vino con hora no nula
				if (!dx.Any() && !procedimientos.Any())
				{
					dx = await db.T_RIPS_DX
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FACTURA == factura &&
							x.FECHACONSULTA >= fechaSolo &&
							x.FECHACONSULTA < fechaFin)
						.ToListAsync();

					procedimientos = await db.T_RIPS_PROCEDIMIENTOS
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FACTURA == factura &&
							x.FECHAPROCEDIMIENTO >= fechaSolo &&
							x.FECHAPROCEDIMIENTO < fechaFin)
						.ToListAsync();
				}
			}
		}
		else
		{
			if (horaNormalizada != null)
			{
				dx = await db.T_RIPS_DX
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.FECHACONSULTA >= fechaSolo &&
						x.FECHACONSULTA < fechaFin &&
						x.HORA == horaNormalizada)
					.ToListAsync();

				procedimientos = await db.T_RIPS_PROCEDIMIENTOS
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.FECHAPROCEDIMIENTO >= fechaSolo &&
						x.FECHAPROCEDIMIENTO < fechaFin &&
						x.HORA == horaNormalizada)
					.ToListAsync();

				// fallback legacy: hora null
				if (!dx.Any() && !procedimientos.Any())
				{
					dx = await db.T_RIPS_DX
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FECHACONSULTA >= fechaSolo &&
						    x.FECHACONSULTA < fechaFin &&
							x.HORA == null)
						.ToListAsync();

					procedimientos = await db.T_RIPS_PROCEDIMIENTOS
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FECHAPROCEDIMIENTO >= fechaSolo &&
							x.FECHAPROCEDIMIENTO < fechaFin &&
							x.HORA == null)
						.ToListAsync();
				}
			}
			else
			{
				dx = await db.T_RIPS_DX
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.FECHACONSULTA >= fechaSolo &&
						x.FECHACONSULTA < fechaFin &&
						x.HORA == null)
					.ToListAsync();

				procedimientos = await db.T_RIPS_PROCEDIMIENTOS
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.FECHAPROCEDIMIENTO >= fechaSolo &&
						x.FECHAPROCEDIMIENTO < fechaFin &&
						x.HORA == null)
					.ToListAsync();

				// fallback amplio legacy
				if (!dx.Any() && !procedimientos.Any())
				{
					dx = await db.T_RIPS_DX
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FECHACONSULTA >= fechaSolo &&
							x.FECHACONSULTA < fechaFin)
						.ToListAsync();

					procedimientos = await db.T_RIPS_PROCEDIMIENTOS
						.Where(x =>
							x.IDANAMNESIS == idAnamnesis &&
							x.FECHAPROCEDIMIENTO >= fechaSolo &&
							x.FECHAPROCEDIMIENTO < fechaFin)
						.ToListAsync();
				}
			}
		}

		if (dx.Any())
			db.T_RIPS_DX.RemoveRange(dx);

		if (procedimientos.Any())
			db.T_RIPS_PROCEDIMIENTOS.RemoveRange(procedimientos);

		if (dx.Any() || procedimientos.Any())
		{
			await db.SaveChangesAsync();
		}
	}

	private async Task<string> ObtenerCodigoPrestadorAsync(AppDbContext db, int idDoctor)
	{
		if (idDoctor <= 0) return "000000";

		string query = @"
    SELECT i.CODIGO_PRESTADOR
    FROM TINFORMACIONREPORTES i
    INNER JOIN TDATOSDOCTORES d ON d.IDREPORTE = i.ID
    WHERE d.ID = {0}";

		var codigoPrestador = await db.TINFORMACIONREPORTES
			.FromSqlRaw(query, idDoctor)
			.Select(i => i.CODIGO_PRESTADOR)
			.FirstOrDefaultAsync();

		return string.IsNullOrWhiteSpace(codigoPrestador) ? "000000" : codigoPrestador.Trim();
	}

	private TimeSpan ObtenerHoraLoteNormalizada()
	{
		var ahora = DateTime.Now;
		return new TimeSpan(ahora.Hour, ahora.Minute, 0);
	}

	private TimeSpan NormalizarHora(TimeSpan? hora)
	{
		if (hora == null) return TimeSpan.Zero;
		return new TimeSpan(hora.Value.Hours, hora.Value.Minutes, 0);
	}

	private bool TieneFacturaFuerte(string? factura)
	{
		if (string.IsNullOrWhiteSpace(factura)) return false;

		var valor = factura.Trim().ToUpperInvariant();
		return valor != "PENDIENTE";
	}

	private async Task EliminarRdaConsultaExistenteAsync(
	AppDbContext db,
	int idAnamnesis,
	DateTime? fechaAtencion,
	TimeSpan? horaAtencion)
	{
		if (idAnamnesis <= 0 || !fechaAtencion.HasValue)
			return;

		var fechaSolo = fechaAtencion.Value.Date;
		var fechaFin = fechaSolo.AddDays(1);
		var horaNormalizada = HoraVacia(horaAtencion)
			? (TimeSpan?)null
			: NormalizarHora(horaAtencion);

		var queryBase = db.TRDA_DOCUMENTO.Where(x =>
			x.IDANAMNESIS == idAnamnesis &&
			x.TIPO_DOCUMENTO == "RDA_CONSULTA_INTERNO" &&
			x.FECHA_ATENCION >= fechaSolo &&
			x.FECHA_ATENCION < fechaFin);

		List<TRDA_DOCUMENTO> rows;

		if (horaNormalizada.HasValue)
		{
			rows = await queryBase
				.Where(x => x.HORA_ATENCION == horaNormalizada.Value)
				.ToListAsync();

			if (!rows.Any())
			{
				rows = await queryBase
					.Where(x => x.HORA_ATENCION == null)
					.ToListAsync();
			}
		}
		else
		{
			rows = await queryBase
				.Where(x => x.HORA_ATENCION == null)
				.ToListAsync();

			if (!rows.Any())
			{
				rows = await queryBase.ToListAsync();
			}
		}

		if (rows.Any())
		{
			db.TRDA_DOCUMENTO.RemoveRange(rows);
			await db.SaveChangesAsync();
		}
	}

	private async Task<bool> EstaRdaConsultaEnviadoAsync(
		AppDbContext db,
		int idAnamnesis,
		DateTime fechaAtencion,
		TimeSpan? horaAtencion)
	{
		var fechaSolo = fechaAtencion.Date;
		var fechaFin = fechaSolo.AddDays(1);
		var horaNormalizada = HoraVacia(horaAtencion)
			? (TimeSpan?)null
			: NormalizarHora(horaAtencion);

		var queryBase = db.TRDA_DOCUMENTO.Where(x =>
			x.IDANAMNESIS == idAnamnesis &&
			x.TIPO_DOCUMENTO == "RDA_CONSULTA_INTERNO" &&
			x.FECHA_ATENCION >= fechaSolo &&
			x.FECHA_ATENCION < fechaFin);

		if (horaNormalizada.HasValue)
		{
			var exacto = await queryBase.CountAsync(x =>
				x.HORA_ATENCION == horaNormalizada.Value &&
				x.ESTADO == "ENVIADO") > 0;

			if (exacto)
				return true;

			var legacyNull = await queryBase.CountAsync(x =>
				x.HORA_ATENCION == null &&
				x.ESTADO == "ENVIADO") > 0;

			return legacyNull;
		}
		else
		{
			var nullExacto = await queryBase.CountAsync(x =>
				x.HORA_ATENCION == null &&
				x.ESTADO == "ENVIADO") > 0;

			if (nullExacto)
				return true;

			var fallbackAmplio = await queryBase.CountAsync(x =>
				x.ESTADO == "ENVIADO") > 0;

			return fallbackAmplio;
		}
	}
		
	//----------------------------------------------------------------------------------------------------------------------//

	public async Task ObtenerFacturasPorIdEntreFechas(string clientId, string modeloDatosParaConsultarFacturasEntreFechas)
    {
        var settings = new JsonSerializerSettings();
        var objDatosParaConsultarFacturasEntreFechas = JsonConvert.DeserializeObject<RespuestaConsultarFacturasEntreFechas>(modeloDatosParaConsultarFacturasEntreFechas, settings);
        var objAdicionalesAbonos = new T_ADICIONALES_ABONOSServicios();
        var objRespuestaConsultarFacturasEntreFechas = new List<RespuestaConsultarFacturasEntreFechas>();
        objRespuestaConsultarFacturasEntreFechas = await objAdicionalesAbonos.ConsultarFacturasPorIdEntreFechas(objDatosParaConsultarFacturasEntreFechas.IDANAMNESIS ?? 0, objDatosParaConsultarFacturasEntreFechas.FECHAINI ?? DateTime.Today, objDatosParaConsultarFacturasEntreFechas.FECHAFIN ?? DateTime.Today);
        
        await _hubConnection.InvokeAsync("RespuestaObtenerFacturasPorIdEntreFechas", clientId, JsonConvert.SerializeObject(objRespuestaConsultarFacturasEntreFechas));
    }


	/*public async Task GenerarRips(string clientId, string objGenerarRips)
    {
        var objGenerarRipsModel = JsonConvert.DeserializeObject<GenerarRipsModel>(objGenerarRips);
        var infoReportes = _lnRips.InformacionReportesXId(objGenerarRipsModel.IDREPORTE);
        var ripsModel = _lnRips.ConsultarRips(
                                             objGenerarRipsModel.FECHAINI,
                                             objGenerarRipsModel.FECHAFIN,
                                             objGenerarRipsModel.EPS,
                                             objGenerarRipsModel.FACTURA,
                                             objGenerarRipsModel.IDREPORTE,
                                             objGenerarRipsModel.IDDOCTOR,
                                             objGenerarRipsModel.EXTRANJERO
                                            );
        
        ripsModel = _lnRips.MapearRipsSinFactura(ripsModel, (infoReportes.CONFACTURA ?? 0) == 1);
        var jsonRIps = JsonConvert.SerializeObject(ripsModel);
        string respuesta = jsonRIps ;
        var respuestaSerializadaComprimida = ArchivosHelper.CompressString(respuesta);
        await _hubConnection.InvokeAsync("RespuestaGenerarRips", clientId, respuestaSerializadaComprimida);// Notificar a Angular a través de SignalR
    }*/

	/*public async Task GenerarRips(string clientId, int identificador, string objGenerarRips)
    {
        // Deserialización en un hilo separado para evitar bloqueos
        var objGenerarRipsModel = await Task.Run(() =>
            JsonConvert.DeserializeObject<GenerarRipsModel>(objGenerarRips));

        // Estas funciones son síncronas, por lo que NO debes usarlas con `await`
        var infoReportes = _lnRips.InformacionReportesXId(objGenerarRipsModel.IDREPORTE);
        var ripsModel = _lnRips.ConsultarRips(
                                        objGenerarRipsModel.FECHAINI,
                                        objGenerarRipsModel.FECHAFIN,
                                        objGenerarRipsModel.EPS,
                                        objGenerarRipsModel.FACTURA,
                                        objGenerarRipsModel.IDREPORTE,
                                        objGenerarRipsModel.IDDOCTOR,
                                        objGenerarRipsModel.EXTRANJERO
                                    );
		var proveedorFe = infoReportes?.PROVEEDOR_FE ?? "";
		// Mapear datos
		ripsModel = _lnRips.MapearRipsSinFactura(ripsModel, (infoReportes.CONFACTURA ?? 0) == 1, proveedorFe);

        // Serialización optimizada
        var jsonRips = JsonConvert.SerializeObject(ripsModel);

        // Comprimir el JSON
        string respuesta = jsonRips;

        // Notificar a Angular a través de SignalR
        await _hubConnection.InvokeAsync("RespuestaGenerarRips", clientId, respuesta);
        
    }*/

	public async Task GenerarRips(string clientId, int identificador, string objGenerarRips)
	{
		// Settings camelCase para que Angular lea "accion", "total", etc.
		var settings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		async Task EmitirProgresoAsync(ProgresoRipsDto p)
		{
			try
			{
				p.Accion = "GENERAR";
				var json = JsonConvert.SerializeObject(p, settings);
				await _hubConnection.InvokeAsync("ProgresoRips", clientId, json);
			}
			catch
			{
				// no tumbes la generación si falla el progreso
			}
		}

		await EmitirProgresoAsync(new ProgresoRipsDto
		{
			Total = 0,
			Procesadas = 0,
			Exitosas = 0,
			Fallidas = 0,
			Mensaje = "Iniciando generación..."
		});

		// Deserialización
		var objGenerarRipsModel = await Task.Run(() =>
			JsonConvert.DeserializeObject<GenerarRipsModel>(objGenerarRips));

		await EmitirProgresoAsync(new ProgresoRipsDto
		{
			Mensaje = "Consultando datos..."
		});

		// Consultas
		var infoReportes = _lnRips.InformacionReportesXId(objGenerarRipsModel.IDREPORTE);

		var ripsModel = _lnRips.ConsultarRips(
			objGenerarRipsModel.FECHAINI,
			objGenerarRipsModel.FECHAFIN,
			objGenerarRipsModel.EPS,
			objGenerarRipsModel.FACTURA,
			objGenerarRipsModel.IDREPORTE,
			objGenerarRipsModel.IDDOCTOR,
			objGenerarRipsModel.EXTRANJERO
		);

		bool conFactura = (infoReportes?.CONFACTURA ?? 0) == 1;
		var proveedorFe = infoReportes?.PROVEEDOR_FE ?? "";

		// Progreso inicial “por cantidad”
		int total = ripsModel?.Count ?? 0;

		await EmitirProgresoAsync(new ProgresoRipsDto
		{
			Total = total,
			Procesadas = 0,
			Exitosas = 0,
			Fallidas = 0,
			Mensaje = conFactura ? "Cargando XML de facturas..." : "Preparando RIPS sin factura..."
		});

		// ✅ Mapear con progreso real (throttle)
		ripsModel = _lnRips.MapearRipsSinFacturaConProgreso(
			ripsModel,
			conFactura,
			proveedorFe,
			async (p) =>
			{
				// Asegura que sea GENERAR y envía al front
				await EmitirProgresoAsync(p);
			}
		);

		await EmitirProgresoAsync(new ProgresoRipsDto
		{
			Total = total,
			Procesadas = total,
			Mensaje = "Construyendo JSON..."
		});

		// Serialización (esto puede demorar mucho si el JSON es grande)
		var jsonRips = JsonConvert.SerializeObject(ripsModel);

		await EmitirProgresoAsync(new ProgresoRipsDto
		{
			Total = 1,
			Procesadas = 1,
			Exitosas = 1,
			Fallidas = 0,
			Mensaje = "Generación finalizada."
		});

		await _hubConnection.InvokeAsync("RespuestaGenerarRips", clientId, jsonRips);
	}



	// Conversor para manejar TimeSpan en JSON
	/*public class TimeSpanConverterJson : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((TimeSpan)value).ToString(@"hh\:mm\:ss"));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return TimeSpan.TryParse(reader.Value?.ToString(), out var timeSpan) ? timeSpan : TimeSpan.Zero;
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(TimeSpan);
    }*/


	/*public async Task PresentarRips(string clientId, int identificador, string objPresentarRips)
	{
		var objPresentarRipsModel = JsonConvert.DeserializeObject<GenerarRipsModel>(objPresentarRips);

		var infoReportes = _lnRips.InformacionReportesXId(objPresentarRipsModel.IDREPORTE);
		var token = _lnRips.obtenerTokenRips(infoReportes);

		var ripsModel = _lnRips.ConsultarRips(
			objPresentarRipsModel.FECHAINI,
			objPresentarRipsModel.FECHAFIN,
			objPresentarRipsModel.EPS,
			objPresentarRipsModel.FACTURA,
			objPresentarRipsModel.IDREPORTE,
			objPresentarRipsModel.IDDOCTOR,
			objPresentarRipsModel.EXTRANJERO
		);

		bool conFactura = (infoReportes.CONFACTURA ?? 0) == 1;
		var proveedorFe = infoReportes?.PROVEEDOR_FE ?? "";

		try
		{
			ripsModel = _lnRips.MapearRipsSinFactura(ripsModel, conFactura, proveedorFe);
		}
		catch
		{
			// No tumbar el proceso: si algo falla cargando XML, se sigue sin xmlFevFile
		}

		var settings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		// ✅ Callback de progreso que reenvía al HUB (y el hub al navegador)
		Func<ProgresoRipsDto, Task> reportProgress = async (p) =>
		{
			try
			{
				var json = JsonConvert.SerializeObject(p, settings);
				await _hubConnection.InvokeAsync("ProgresoRips", clientId, json);
			}
			catch
			{
				// Si falla el progreso, NO debes tumbar la presentación
			}
		};

		// ✅ Ahora sí: esperamos la lista real + va reportando progreso
		var result = await _lnRips.CargarRipsSinFacturaAsync(
			ripsModel,
			token,
			conFactura,
			reportProgress
		);

		var respuesta = JsonConvert.SerializeObject(result);
		await _hubConnection.InvokeAsync("RespuestaPresentarRips", clientId, respuesta);
	}*/

	// ✅ PRESENTAR: NO carga XML aquí (para no duplicar y para que el progreso sea real)
	//    Pasa proveedorFe a CargarRipsSinFacturaAsync, que es donde se toma la decisión FacturaTec/Dataico.
	public async Task PresentarRips(string clientId, int identificador, string objPresentarRips)
	{
		var objPresentarRipsModel = JsonConvert.DeserializeObject<GenerarRipsModel>(objPresentarRips);

		var infoReportes = _lnRips.InformacionReportesXId(objPresentarRipsModel.IDREPORTE);
		var token = _lnRips.obtenerTokenRips(infoReportes);

		var ripsModel = _lnRips.ConsultarRips(
			objPresentarRipsModel.FECHAINI,
			objPresentarRipsModel.FECHAFIN,
			objPresentarRipsModel.EPS,
			objPresentarRipsModel.FACTURA,
			objPresentarRipsModel.IDREPORTE,
			objPresentarRipsModel.IDDOCTOR,
			objPresentarRipsModel.EXTRANJERO
		);

		bool conFactura = (infoReportes?.CONFACTURA ?? 0) == 1;
		var proveedorFe = infoReportes?.PROVEEDOR_FE ?? "";

		var settings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		// ✅ Callback de progreso que reenvía al HUB (y el hub al navegador)
		Func<ProgresoRipsDto, Task> reportProgress = async (p) =>
		{
			try
			{
				var json = JsonConvert.SerializeObject(p, settings);
				await _hubConnection.InvokeAsync("ProgresoRips", clientId, json);
			}
			catch
			{
				// Si falla el progreso, NO debes tumbar la presentación
			}
		};

		// ✅ Ahora sí: esperamos la lista real + va reportando progreso (incluye carga XML si aplica)
		var result = await _lnRips.CargarRipsSinFacturaAsync(
			ripsModel,
			token,
			conFactura,
			proveedorFe,
			reportProgress
		);

		var respuesta = JsonConvert.SerializeObject(result);
		await _hubConnection.InvokeAsync("RespuestaPresentarRips", clientId, respuesta);
	}

	//-------------------------------------interoperabilidad------------------------------------------------//

	public async Task ConsultarRdaControl(string clientId, string filtroJson)
	{
		try
		{
			var filtro = string.IsNullOrWhiteSpace(filtroJson)
				? new ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.RdaControlFiltro()
				: JsonConvert.DeserializeObject<ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.RdaControlFiltro>(filtroJson)
				  ?? new ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.RdaControlFiltro();

			var service = new RdaControlConsultaService();
			var respuesta = await service.ConsultarAsync(filtro);
			var payload = JsonConvert.SerializeObject(respuesta);

			await _hubConnection.InvokeAsync("RespuestaConsultarRdaControl", clientId, payload);
		}
		catch (Exception ex)
		{
			var payload = JsonConvert.SerializeObject(new
			{
				items = new object[] { },
				error = ex.Message
			});

			await _hubConnection.InvokeAsync("RespuestaConsultarRdaControl", clientId, payload);
		}
	}

	public async Task ReenviarRda(string clientId, int idRda)
	{
		try
		{
			var rdaOrquestador = new RdaOrquestadorService(_configuration);
			var resultado = await rdaOrquestador.EnviarPorIdAsync(idRda);
			var payload = JsonConvert.SerializeObject(resultado);

			await _hubConnection.InvokeAsync("RespuestaReenviarRda", clientId, payload);
		}
		catch (Exception ex)
		{
			var payload = JsonConvert.SerializeObject(new
			{
				ok = false,
				idRda = idRda,
				estado = "ERROR",
				mensaje = ex.Message
			});

			await _hubConnection.InvokeAsync("RespuestaReenviarRda", clientId, payload);
		}
	}

	public async Task RegenerarRda(string clientId, int idRda)
	{
		try
		{
			var rdaOrquestador = new RdaOrquestadorService(_configuration);
			var resultado = await rdaOrquestador.RegenerarPorIdAsync(idRda, false);
			var payload = JsonConvert.SerializeObject(resultado);

			await _hubConnection.InvokeAsync("RespuestaRegenerarRda", clientId, payload);
		}
		catch (Exception ex)
		{
			var payload = JsonConvert.SerializeObject(new
			{
				ok = false,
				idRda = idRda,
				estado = "ERROR",
				mensaje = ex.Message
			});

			await _hubConnection.InvokeAsync("RespuestaRegenerarRda", clientId, payload);
		}
	}

	public async Task ConsultarDetalleRda(string clientId, int idRda)
	{
		try
		{
			var service = new RdaDetalleConsultaService();
			var detalle = await service.ConsultarPorIdAsync(idRda);
			var payload = JsonConvert.SerializeObject(detalle);

			await _hubConnection.InvokeAsync("RespuestaConsultarDetalleRda", clientId, payload);
		}
		catch (Exception ex)
		{
			var payload = JsonConvert.SerializeObject(new
			{
				ok = false,
				mensaje = ex.Message
			});

			await _hubConnection.InvokeAsync("RespuestaConsultarDetalleRda", clientId, payload);
		}
	}

	public async Task ConsultarHistorialRda(string clientId, int idRda)
	{
		try
		{
			var service = new RdaHistorialConsultaService();
			var respuesta = await service.ConsultarPorDocumentoAsync(idRda);
			var payload = JsonConvert.SerializeObject(respuesta);

			await _hubConnection.InvokeAsync("RespuestaConsultarHistorialRda", clientId, payload);
		}
		catch (Exception ex)
		{
			var payload = JsonConvert.SerializeObject(new
			{
				ok = false,
				mensaje = ex.Message,
				items = new object[] { }
			});

			await _hubConnection.InvokeAsync("RespuestaConsultarHistorialRda", clientId, payload);
		}
	}

	public async Task ReenviarRdaLote(string clientId, string idsJson)
	{
		var settings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		async Task EmitirProgresoAsync(RdaProcesoMasivoProgressDto p)
		{
			try
			{
				p.Accion = "REENVIO_MASIVO";
				var json = JsonConvert.SerializeObject(p, settings);
				await _hubConnection.InvokeAsync("ProgresoRda", clientId, json);
			}
			catch
			{
				// no tumbar proceso por progreso
			}
		}

		try
		{
			var ids = string.IsNullOrWhiteSpace(idsJson)
				? new List<int>()
				: JsonConvert.DeserializeObject<List<int>>(idsJson) ?? new List<int>();

			ids = ids
				.Where(x => x > 0)
				.Distinct()
				.ToList();

			var total = ids.Count;
			var procesadas = 0;
			var exitosas = 0;
			var fallidas = 0;
			var resultados = new List<RdaProcesoResultado>();

			await EmitirProgresoAsync(new RdaProcesoMasivoProgressDto
			{
				Total = total,
				Procesadas = 0,
				Exitosas = 0,
				Fallidas = 0,
				Mensaje = "Iniciando reenvío masivo...",
				UltimoDocumento = null,
				EnCurso = true
			});

			var rdaOrquestador = new RdaOrquestadorService(_configuration);

			foreach (var idRda in ids)
			{
				RdaProcesoResultado resultado;

				try
				{
					resultado = await rdaOrquestador.EnviarPorIdAsync(idRda);
				}
				catch (Exception ex)
				{
					resultado = new RdaProcesoResultado
					{
						Ok = false,
						IdRda = idRda,
						Estado = "ERROR",
						Mensaje = ex.Message
					};
				}

				resultados.Add(resultado);

				procesadas++;
				if (resultado.Ok) exitosas++;
				else fallidas++;

				await EmitirProgresoAsync(new RdaProcesoMasivoProgressDto
				{
					Total = total,
					Procesadas = procesadas,
					Exitosas = exitosas,
					Fallidas = fallidas,
					Mensaje = resultado.Ok
						? $"Reenviado documento {idRda}"
						: $"Falló reenvío documento {idRda}: {resultado.Mensaje}",
					UltimoDocumento = idRda,
					EnCurso = procesadas < total
				});
			}

			var respuesta = new RdaLoteResultadoDto
			{
				Ok = fallidas == 0,
				Accion = "REENVIO_MASIVO",
				Total = total,
				Procesadas = procesadas,
				Exitosas = exitosas,
				Fallidas = fallidas,
				Mensaje = $"Reenvío masivo finalizado. Exitosas: {exitosas}. Fallidas: {fallidas}.",
				Resultados = resultados
			};

			var payload = JsonConvert.SerializeObject(respuesta, settings);
			await _hubConnection.InvokeAsync("RespuestaReenviarRdaLote", clientId, payload);
		}
		catch (Exception ex)
		{
			var respuesta = new RdaLoteResultadoDto
			{
				Ok = false,
				Accion = "REENVIO_MASIVO",
				Total = 0,
				Procesadas = 0,
				Exitosas = 0,
				Fallidas = 0,
				Mensaje = ex.Message,
				Resultados = new List<RdaProcesoResultado>()
			};

			var payload = JsonConvert.SerializeObject(respuesta, settings);
			await _hubConnection.InvokeAsync("RespuestaReenviarRdaLote", clientId, payload);
		}
	}

	public async Task RegenerarRdaLote(string clientId, string idsJson)
	{
		var settings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		async Task EmitirProgresoAsync(RdaProcesoMasivoProgressDto p)
		{
			try
			{
				p.Accion = "REGENERACION_MASIVA";
				var json = JsonConvert.SerializeObject(p, settings);
				await _hubConnection.InvokeAsync("ProgresoRda", clientId, json);
			}
			catch
			{
				// no tumbar proceso por progreso
			}
		}

		try
		{
			var ids = string.IsNullOrWhiteSpace(idsJson)
				? new List<int>()
				: JsonConvert.DeserializeObject<List<int>>(idsJson) ?? new List<int>();

			ids = ids
				.Where(x => x > 0)
				.Distinct()
				.ToList();

			var total = ids.Count;
			var procesadas = 0;
			var exitosas = 0;
			var fallidas = 0;
			var resultados = new List<RdaProcesoResultado>();

			await EmitirProgresoAsync(new RdaProcesoMasivoProgressDto
			{
				Total = total,
				Procesadas = 0,
				Exitosas = 0,
				Fallidas = 0,
				Mensaje = "Iniciando regeneración masiva...",
				UltimoDocumento = null,
				EnCurso = true
			});

			var rdaOrquestador = new RdaOrquestadorService(_configuration);

			foreach (var idRda in ids)
			{
				RdaProcesoResultado resultado;

				try
				{
					resultado = await rdaOrquestador.RegenerarPorIdAsync(idRda, false);
				}
				catch (Exception ex)
				{
					resultado = new RdaProcesoResultado
					{
						Ok = false,
						IdRda = idRda,
						Estado = "ERROR",
						Mensaje = ex.Message
					};
				}

				resultados.Add(resultado);

				procesadas++;
				if (resultado.Ok) exitosas++;
				else fallidas++;

				await EmitirProgresoAsync(new RdaProcesoMasivoProgressDto
				{
					Total = total,
					Procesadas = procesadas,
					Exitosas = exitosas,
					Fallidas = fallidas,
					Mensaje = resultado.Ok
						? $"Regenerado documento {idRda}"
						: $"Falló regeneración documento {idRda}: {resultado.Mensaje}",
					UltimoDocumento = idRda,
					EnCurso = procesadas < total
				});
			}

			var respuesta = new RdaLoteResultadoDto
			{
				Ok = fallidas == 0,
				Accion = "REGENERACION_MASIVA",
				Total = total,
				Procesadas = procesadas,
				Exitosas = exitosas,
				Fallidas = fallidas,
				Mensaje = $"Regeneración masiva finalizada. Exitosas: {exitosas}. Fallidas: {fallidas}.",
				Resultados = resultados
			};

			var payload = JsonConvert.SerializeObject(respuesta, settings);
			await _hubConnection.InvokeAsync("RespuestaRegenerarRdaLote", clientId, payload);
		}
		catch (Exception ex)
		{
			var respuesta = new RdaLoteResultadoDto
			{
				Ok = false,
				Accion = "REGENERACION_MASIVA",
				Total = 0,
				Procesadas = 0,
				Exitosas = 0,
				Fallidas = 0,
				Mensaje = ex.Message,
				Resultados = new List<RdaProcesoResultado>()
			};

			var payload = JsonConvert.SerializeObject(respuesta, settings);
			await _hubConnection.InvokeAsync("RespuestaRegenerarRdaLote", clientId, payload);
		}
	}
	//--------------------------------------Consultar Interoperabilidad------------------------------------//
	public async Task ConsultarPacienteInteroperabilidadExacto(string clientId, string filtroJson)
	{
		try
		{
			var filtro = string.IsNullOrWhiteSpace(filtroJson)
				? new ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro()
				: JsonConvert.DeserializeObject<ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro>(filtroJson)
				  ?? new ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro();

			var service = new InteroperabilidadConsultaPacienteService(_configuration);
			var respuesta = await service.ConsultarPacienteExactoAsync(filtro);

			var payload = JsonConvert.SerializeObject(respuesta);
			await _hubConnection.InvokeAsync("RespuestaConsultarPacienteInteroperabilidadExacto", clientId, payload);
		}
		catch (Exception ex)
		{
			var payload = JsonConvert.SerializeObject(new
			{
				ok = false,
				mensaje = ex.Message,
				paciente = (object)null
			});

			await _hubConnection.InvokeAsync("RespuestaConsultarPacienteInteroperabilidadExacto", clientId, payload);
		}
	}

	public async Task ConsultarPacienteInteroperabilidadSimilar(string clientId, string filtroJson)
	{
		try
		{
			var filtro = string.IsNullOrWhiteSpace(filtroJson)
				? new ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro()
				: JsonConvert.DeserializeObject<ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro>(filtroJson)
				  ?? new ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro();

			var service = new InteroperabilidadConsultaPacienteService(_configuration);
			var respuesta = await service.ConsultarPacienteSimilarAsync(filtro);

			var payload = JsonConvert.SerializeObject(respuesta);
			await _hubConnection.InvokeAsync("RespuestaConsultarPacienteInteroperabilidadSimilar", clientId, payload);
		}
		catch (Exception ex)
		{
			var payload = JsonConvert.SerializeObject(new
			{
				ok = false,
				mensaje = ex.Message,
				items = new object[] { }
			});

			await _hubConnection.InvokeAsync("RespuestaConsultarPacienteInteroperabilidadSimilar", clientId, payload);
		}
	}

	public async Task ConsultarRdaPacienteInteroperabilidad(string clientId, string filtroJson)
	{
		try
		{
			var filtro = string.IsNullOrWhiteSpace(filtroJson)
				? new ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro()
				: JsonConvert.DeserializeObject<ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro>(filtroJson)
				  ?? new ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro();

			var service = new InteroperabilidadConsultaPacienteService(_configuration);
			var respuesta = await service.ConsultarRdaPacienteAsync(filtro);

			var payload = JsonConvert.SerializeObject(respuesta);
			await _hubConnection.InvokeAsync("RespuestaConsultarRdaPacienteInteroperabilidad", clientId, payload);
		}
		catch (Exception ex)
		{
			var payload = JsonConvert.SerializeObject(new
			{
				ok = false,
				mensaje = ex.Message,
				items = new object[] { }
			});

			await _hubConnection.InvokeAsync("RespuestaConsultarRdaPacienteInteroperabilidad", clientId, payload);
		}
	}

	public async Task ConsultarEncuentrosPacienteInteroperabilidad(string clientId, string filtroJson)
	{
		try
		{
			var filtro = string.IsNullOrWhiteSpace(filtroJson)
				? new ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro()
				: JsonConvert.DeserializeObject<ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro>(filtroJson)
				  ?? new ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.InteroperabilidadPacienteFiltro();

			var service = new InteroperabilidadConsultaPacienteService(_configuration);

			var respuesta = await service.ConsultarEncuentrosClinicosAsync(filtro);

			var payload = JsonConvert.SerializeObject(respuesta);
			await _hubConnection.InvokeAsync("RespuestaConsultarEncuentrosPacienteInteroperabilidad", clientId, payload);
		}
		catch (Exception ex)
		{
			var payload = JsonConvert.SerializeObject(new
			{
				ok = false,
				mensaje = ex.Message,
				items = new object[] { }
			});

			await _hubConnection.InvokeAsync("RespuestaConsultarEncuentrosPacienteInteroperabilidad", clientId, payload);
		}
	}
	//------------------------------------------------------------------------------------------------//





	public async Task ObtenerFacturasCreadas(string clientId, string Factura)
    {
        try
        {
			// Instanciar el repositorio
			
			var repo = new ServicioRydentLocal.LogicaDelNegocio.Facturatech.Adicionales_Abonos_Dian();
			// Obtener las facturas pendientes
			var facturasCreadas = repo.ListarFacturasCreadas(Factura);

            // Serializar a JSON
            var jsonFacturasCreadas = JsonConvert.SerializeObject(facturasCreadas);

			//comprimir respuesta
			var jsonFacturasCreadasComprimido = ArchivosHelper.CompressString(jsonFacturasCreadas);

			// Enviar respuesta a través de SignalR
			await _hubConnection.InvokeAsync("RespuestaObtenerFacturasCreadas", clientId, jsonFacturasCreadasComprimido);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener facturas pendientes: {ex.Message}");
        }
    }

	public async Task ObtenerFacturasPendientes(string clientId)
	{
		try
		{
			// Instanciar el repositorio
			var repo = new ServicioRydentLocal.LogicaDelNegocio.Facturatech.Adicionales_Abonos_Dian();

			// Obtener las facturas pendientes
			var facturasPendientes = repo.listarFacturasPendientes();

			// Serializar a JSON
			var jsonFacturasPendientes = JsonConvert.SerializeObject(facturasPendientes);

            //comprimir respuesta
            var jsonFacturasPendientesComprimido = ArchivosHelper.CompressString(jsonFacturasPendientes);

			// Enviar respuesta a través de SignalR
			await _hubConnection.InvokeAsync("RespuestaObtenerFacturasPendientes", clientId, jsonFacturasPendientesComprimido);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error al obtener facturas pendientes: {ex.Message}");
		}
	}

	/*public async Task PresentarFacturasEnDian(string clienteIdDestino, string payloadJson, CancellationToken ct = default)
	{
		PresentarFacturasPayload payload;
		try
		{
			payload = JsonConvert.DeserializeObject<PresentarFacturasPayload>(payloadJson) ?? new PresentarFacturasPayload();
		}
		catch (Exception ex)
		{
			await EmitResumenAsync(clienteIdDestino, new ResumenPresentacionLote
			{
				total = 0,
				ok = 0,
				fail = 1,
				resultados = new List<ResultadoPresentacionItem>
			{
				new ResultadoPresentacionItem { ok = false, mensaje = "Payload inválido: " + ex.Message }
			}
			}, ct);
			return;
		}

		var resultados = new List<ResultadoPresentacionItem>();
		int okCount = 0;
		int processed = 0;
		int total = payload.items?.Count ?? 0;

		// 1) Determinar la operación de salud (SS_SIN_APORTE, SS_RECAUDO, etc.)
		string opString = "";
		if (payload.items != null && payload.items.Count > 0)
		{
			opString = (payload.items[0].operation ?? "").Trim();
		}

		HealthOperationType operationType;
		switch (opString.ToUpperInvariant())
		{
			case "SS_RECAUDO":
				operationType = HealthOperationType.Recaudo;
				break;
			case "SS_CUFE":
				operationType = HealthOperationType.Cufe;
				break;
			case "SS_REPORTE":
				operationType = HealthOperationType.Reporte;
				break;
			case "SS_SIN_APORTE":
			default:
				operationType = HealthOperationType.SinAporte;
				opString = "SS_SIN_APORTE"; // normalizamos por si venía vacío
				break;
		}

		using var scope = _scopeFactory.CreateScope();
		var repo = scope.ServiceProvider.GetRequiredService<FacturacionSaludRepository>();

		foreach (var item in payload.items)
		{
			ct.ThrowIfCancellationRequested();

			// Si tipoFactura viene null, asumimos 1 (factura normal)
			int tipoFactura = item.tipoFactura ?? 1;

			var resItem = new ResultadoPresentacionItem
			{
				idRelacion = item.idRelacion,
				factura = item.factura,
				codigoPrestador = item.codigoPrestadorPPAL
			};

			try
			{
				// 2) Construir DTO para la API intermedia según operación y tipoFactura
				var dto = await repo.BuildHealthInvoiceDtoAsync(
					item.idRelacion,
					tipoFactura,
					operationType,
					ct);

				var bodyJson = JsonConvert.SerializeObject(dto);

				// 3) Invocar API intermedia (mapeando DataicoResponse si viene)
				(bool ok, string mensaje, string? externalId) =
					await _api.PostHealthInvoiceAsync(item.codigoPrestadorPPAL, bodyJson, ct);

				resItem.ok = ok;
				resItem.mensaje = ok ? "ENVIADA" : mensaje;
				resItem.externalId = externalId;
				if (ok) okCount++;

				// 4) Si fue OK y tenemos UUID, marcar TRANSACCIONID en la base
				if (ok && !string.IsNullOrWhiteSpace(externalId))
				{
					await repo.MarcarTransaccionIdAsync(
						item.idRelacion,
						tipoFactura,
						externalId!,
						ct
					);
				}
			}
			catch (Exception ex)
			{
				resItem.ok = false;
				resItem.mensaje = "Excepción: " + ex.Message;
			}

			resultados.Add(resItem);
			processed++;

			// 5) Progreso agregado cada 10 (o al terminar)
			if (processed % 10 == 0 || processed == total)
			{
				await EmitProgresoAsync(clienteIdDestino, new
				{
					aggregate = true,   // bandera para entender que es parcial
					processed,
					total,
					ok = okCount,
					fail = processed - okCount
				}, ct);
			}
		}

		// 6) Resumen final
		await EmitResumenAsync(clienteIdDestino, new ResumenPresentacionLote
		{
			total = resultados.Count,
			ok = okCount,
			fail = resultados.Count - okCount,
			resultados = resultados
		}, ct);
	}*/

	public async Task PresentarFacturasEnDian(string clienteIdDestino, string payloadJson, CancellationToken ct = default)
	{
		PresentarFacturasPayload payload;
		try
		{
			payload = JsonConvert.DeserializeObject<PresentarFacturasPayload>(payloadJson) ?? new PresentarFacturasPayload();
		}
		catch (Exception ex)
		{
			await EmitResumenAsync(clienteIdDestino, new ResumenPresentacionLote
			{
				total = 0,
				ok = 0,
				fail = 1,
				resultados = new List<ResultadoPresentacionItem>
			{
				new ResultadoPresentacionItem { ok = false, mensaje = "Payload inválido: " + ex.Message }
			}
			}, ct);
			return;
		}

		var resultados = new List<ResultadoPresentacionItem>();
		int okCount = 0;
		int procesadas = 0;
		int total = payload.items?.Count ?? 0;
        int? idSede = payload.sedeId;

		// =========================
		// ✅ Determinar operationType SIEMPRE
		// =========================
		string opString = (payload.items != null && payload.items.Count > 0)
			? (payload.items[0].operation ?? "").Trim()
			: "";

		HealthOperationType operationType;
		switch (opString.ToUpperInvariant())
		{
			case "SS_RECAUDO":
				operationType = HealthOperationType.Recaudo;
				break;
			case "SS_CUFE":
				operationType = HealthOperationType.Cufe;
				break;
			case "SS_REPORTE":
				operationType = HealthOperationType.Reporte;
				break;
			case "SS_SIN_APORTE":
			default:
				operationType = HealthOperationType.SinAporte;
				opString = "SS_SIN_APORTE";
				break;
		}

		var settings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		// =========================
		// ✅ Frecuencia (cantidad + tiempo)
		// =========================
		int cada = total <= 200 ? 10 : total <= 1000 ? 25 : 50;

		var sw = Stopwatch.StartNew();
		long lastMs = -999999;

		async Task EmitirProgresoAsync(string? ultimoDoc, string mensaje, string? lastExternalId = null, bool force = false)
		{
			var nowMs = sw.ElapsedMilliseconds;

			bool porTiempo = (nowMs - lastMs) >= 700;
			bool porCantidad = (procesadas == 1) || (procesadas % cada == 0) || (procesadas == total);

			if (!force && !porTiempo && !porCantidad) return;

			lastMs = nowMs;

			var dto = new
			{
				accion = "PRESENTAR_DIAN",
				total = total,
				procesadas = procesadas,
				exitosas = okCount,
				fallidas = procesadas - okCount,
				ultimoDocumento = ultimoDoc,
				mensaje = mensaje,
				lastExternalId = lastExternalId
			};

			var json = JsonConvert.SerializeObject(dto, settings);

			// Hub: RespuestaProgresoPresentacion -> Front: ProgresoPresentacionFactura
			await _hubConnection.InvokeAsync("RespuestaProgresoPresentacion", clienteIdDestino, json, ct);
		}

		// ✅ Progreso inicial
		await EmitirProgresoAsync(null, "Iniciando presentación DIAN...", force: true);

		using var scope = _scopeFactory.CreateScope();
		var repo = scope.ServiceProvider.GetRequiredService<FacturacionSaludRepository>();

		foreach (var item in payload.items ?? new List<PresentarFacturaItem>())
		{
			ct.ThrowIfCancellationRequested();

			int tipoFactura = item.tipoFactura ?? 1;

			var resItem = new ResultadoPresentacionItem
			{
				idRelacion = item.idRelacion,
				factura = item.factura,
				codigoPrestador = item.codigoPrestadorPPAL
			};

			// ✅ NO usamos item.numeroFactura porque puede no tener getter
			string ultimoDoc = item.factura ?? item.idRelacion.ToString();
			string? externalId = null;

			try
			{
				// 1) Construir DTO
				var dto = await repo.BuildHealthInvoiceDtoAsync(
					item.idRelacion,
					tipoFactura,
					operationType,
					ct);

				var bodyJson = JsonConvert.SerializeObject(dto);

				// 2) Enviar
				(bool ok, string mensaje, string? extId) =
					await _api.PostHealthInvoiceAsync(item.codigoPrestadorPPAL, bodyJson, idSede, ct);

				externalId = extId;

				resItem.ok = ok;
				resItem.mensaje = ok ? "ENVIADA" : mensaje;
				resItem.externalId = extId;

				if (ok) okCount++;

				// 3) Guardar UUID si OK
				if (ok && !string.IsNullOrWhiteSpace(extId))
				{
					await repo.MarcarTransaccionIdAsync(item.idRelacion, tipoFactura, extId!, ct);
				}
			}
			catch (Exception ex)
			{
				resItem.ok = false;
				resItem.mensaje = "Excepción: " + ex.Message;
			}

			resultados.Add(resItem);
			procesadas++;

			// ✅ Progreso en caliente (acumulado)
			await EmitirProgresoAsync(
				ultimoDoc,
				resItem.ok ? "Presentando DIAN: OK" : "Presentando DIAN: FAIL",
				lastExternalId: externalId
			);
		}

		// ✅ Progreso final
		await EmitirProgresoAsync(null, "Finalizando presentación DIAN...", force: true);

		// ✅ Resumen final
		await EmitResumenAsync(clienteIdDestino, new ResumenPresentacionLote
		{
			total = resultados.Count,
			ok = okCount,
			fail = resultados.Count - okCount,
			resultados = resultados
		}, ct);
	}

	// Helpers (sin cambios)
	/*private async Task EmitProgresoAsync(string clienteIdDestino, object progresoObj, CancellationToken ct)
	{
		var progresoJson = JsonConvert.SerializeObject(progresoObj);
		await _hubConnection.InvokeAsync("RespuestaProgresoPresentacion", clienteIdDestino, progresoJson, ct);
	}*/

	private async Task EmitResumenAsync(string clienteIdDestino, ResumenPresentacionLote resumen, CancellationToken ct)
	{
		var settings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		var resumenJson = JsonConvert.SerializeObject(resumen, settings);
		await _hubConnection.InvokeAsync("RespuestaPresentarFacturasEnDian", clienteIdDestino, resumenJson, ct);
	}

	public async Task DescargarJsonFacturaPendiente(string clienteIdDestino, string payloadJson, CancellationToken ct = default)
	{
		PresentarFacturasPayload payload;

		try
		{
			payload = JsonConvert.DeserializeObject<PresentarFacturasPayload>(payloadJson)
					  ?? new PresentarFacturasPayload();
		}
		catch (Exception ex)
		{
			await _hubConnection.InvokeAsync(
				"RespuestaDescargarJsonFacturaPendiente",
				clienteIdDestino,
				JsonConvert.SerializeObject(new { error = "Payload inválido", detail = ex.Message }),
				ct
			);
			return;
		}

		// ✅ En este caso es 1 factura (individual)
		var item = payload.items?.FirstOrDefault();
		if (item == null)
		{
			await _hubConnection.InvokeAsync(
				"RespuestaDescargarJsonFacturaPendiente",
				clienteIdDestino,
				JsonConvert.SerializeObject(new { error = "No hay items en payload" }),
				ct
			);
			return;
		}

		int tipoFactura = item.tipoFactura ?? 1;

		// Determinar operationType igual que ya lo haces
		string opString = (item.operation ?? "").Trim();
		HealthOperationType operationType = opString.ToUpperInvariant() switch
		{
			"SS_RECAUDO" => HealthOperationType.Recaudo,
			"SS_CUFE" => HealthOperationType.Cufe,
			"SS_REPORTE" => HealthOperationType.Reporte,
			_ => HealthOperationType.SinAporte
		};

		using var scope = _scopeFactory.CreateScope();
		var repo = scope.ServiceProvider.GetRequiredService<FacturacionSaludRepository>();

		try
		{
			var dto = await repo.BuildHealthInvoiceDtoAsync(item.idRelacion, tipoFactura, operationType, ct);

			// ✅ mismo JSON que enviarías a Dataico
			var json = JsonConvert.SerializeObject(dto, new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				Formatting = Formatting.Indented // bonito para “ver antes”
			});

			await _hubConnection.InvokeAsync(
				"RespuestaDescargarJsonFacturaPendiente",
				clienteIdDestino,
				json,
				ct
			);
		}
		catch (Exception ex)
		{
			await _hubConnection.InvokeAsync(
				"RespuestaDescargarJsonFacturaPendiente",
				clienteIdDestino,
				JsonConvert.SerializeObject(new { error = "Error construyendo JSON", detail = ex.Message }),
				ct
			);
		}
	}







	public async Task GuardarDatosEvolucion(string clientId, string datosEvolucion)
    {
        try
        {
            var objEvolucion = JsonConvert.DeserializeObject<RespuestaEvolucionPacienteModel>(datosEvolucion);
            var objEvolucionServicios = new TEVOLUCIONServicios();
            var objFirma = new TFIRMAServicios();
            var archivosHelper = new ArchivosHelper();
            int idFirma = 0;
            if ((!string.IsNullOrEmpty(objEvolucion.imgFirmaPaciente) || !string.IsNullOrEmpty(objEvolucion.imgFirmaDoctor)))
            {
                var firma = new TFIRMA();
                firma.FECHA = DateTime.Now.Date.ToString("dd/MM/yyyy");
                firma.HORA = DateTime.Now.Date.ToString("hh:mm tt");
                byte[] firmaPaciente = archivosHelper.CrearImagenConBase64(objEvolucion.imgFirmaPaciente, objEvolucion.imgFirmaDoctor);
                if (firmaPaciente != null)
                {
                    firma.FIRMA = archivosHelper.ConvertirPNGaJPEG(firmaPaciente);
                }
                
                idFirma = await objFirma.Agregar(firma);
                if (idFirma > 0)
                {
                    objEvolucion.evolucion.FIRMA = idFirma;
                }
            }
            var resultado = await objEvolucionServicios.Agregar(objEvolucion.evolucion);
            await _hubConnection.InvokeAsync("RespuestaGuardarDatosEvolucion", clientId, resultado.ToString());
        }
        catch (Exception e)
        {
            throw;
        }
        
    }

	/*public async Task GuardarDatosPersonales(string clientId, string datosPersonales)
    {
        try
        {
            var objDatosPersonalesCompletos = JsonConvert.DeserializeObject<RespuestaDatosPersonales>(datosPersonales);
            var objDatosPersonales = objDatosPersonalesCompletos.datosPersonales;
            var objFotoPaciente = objDatosPersonalesCompletos.strFotoFrontal;
            var objDatosPersonalesServicios = new DatosPersonalesServicios(); 
            var objFotosFrontalesServicios = new TFOTOSFRONTALESServicios();
            var objTratamientosServicios = new TTRATAMIENTOServicios();
            var archivosHelper = new ArchivosHelper();
            var resultado = await objDatosPersonalesServicios.Agregar(objDatosPersonales);
            // Verificar si la imagen es válida antes de convertir
            if (!string.IsNullOrEmpty(objFotoPaciente) && EsBase64Valido(objFotoPaciente))
            {
                var fotoPaciente = new TFOTOSFRONTALES
                {
                    IDANAMNESIS = resultado,
                    FOTOFRENTE = Convert.FromBase64String(objFotoPaciente)
                };
                await objFotosFrontalesServicios.Agregar(fotoPaciente);
            }
            // meter datos para inicializar tratamiento

            if (resultado > 0)
            {
                var tratamientoPaciente = new TTRATAMIENTO
                {
                    IDTRATAMIENTO = resultado,
                    FECHA = objDatosPersonales.FECHA_INGRESO_DATE ?? DateTime.Now.Date,
                    NUMTRAT = 1,
                    ID_DOCTOR = objDatosPersonales.COD_DOCTOR
                };
                await objTratamientosServicios.Agregar(tratamientoPaciente);
            }

            await _hubConnection.InvokeAsync("RespuestaGuardarDatosPersonales", clientId, resultado.ToString());
        }
        catch (Exception e)
        {
            throw;
        }

    }*/

	public async Task GuardarDatosPersonales(string clientId, string datosPersonales)
	{
		try
		{
			var objDatosPersonalesCompletos =
				JsonConvert.DeserializeObject<RespuestaDatosPersonales>(datosPersonales);

			var objDatosPersonales = objDatosPersonalesCompletos?.datosPersonales;
			var objFotoPaciente = objDatosPersonalesCompletos?.strFotoFrontal;

			if (objDatosPersonales == null)
				throw new Exception("JSON inválido: datosPersonales viene null");

			var objDatosPersonalesServicios = new DatosPersonalesServicios();
			var objFotosFrontalesServicios = new TFOTOSFRONTALESServicios();
			var objTratamientosServicios = new TTRATAMIENTOServicios();

			// ✅ AQUÍ ES DONDE ESTÁ FALLANDO (y lo vamos a capturar bien)
			var resultado = await objDatosPersonalesServicios.Agregar(objDatosPersonales);

			// Foto (si llega)
			if (!string.IsNullOrEmpty(objFotoPaciente) && EsBase64Valido(objFotoPaciente))
			{
				var fotoPaciente = new TFOTOSFRONTALES
				{
					IDANAMNESIS = resultado,
					FOTOFRENTE = Convert.FromBase64String(objFotoPaciente)
				};
				await objFotosFrontalesServicios.Agregar(fotoPaciente);
			}

			// Tratamiento
			if (resultado > 0)
			{
				var tratamientoPaciente = new TTRATAMIENTO
				{
					IDTRATAMIENTO = resultado, // OJO: revisa si IDTRATAMIENTO es identity/PK independiente
					FECHA = objDatosPersonales.FECHA_INGRESO_DATE ?? DateTime.Now.Date,
					NUMTRAT = 1,
					ID_DOCTOR = objDatosPersonales.COD_DOCTOR
				};

				await objTratamientosServicios.Agregar(tratamientoPaciente);
			}

			await _hubConnection.InvokeAsync(
				"RespuestaGuardarDatosPersonales",
				clientId,
				JsonConvert.SerializeObject(new { IDANAMNESIS = resultado })
				//resultado.ToString()
			);
		}
		catch (Exception ex)
		{
			// ✅ Captura más “real” posible
			string msg = ex.Message;

			// EF Core: DbUpdateException trae el inner con el error real de BD
			if (ex is DbUpdateException dbEx)
				msg = dbEx.InnerException?.Message ?? dbEx.Message;

			// Si viene envuelto (muy común)
			if (ex.InnerException != null)
				msg = ex.InnerException.Message;

			// 🔥 CAPTURA ESPECÍFICA POR TIPO (opcional pero útil)
			// Firebird
			// if (ex is FbException fbEx)
			//     msg = $"Firebird ({fbEx.ErrorCode}): {fbEx.Message}";

			// SQL Server
			// if (ex is SqlException sqlEx)
			//     msg = $"SQL Server ({sqlEx.Number}): {sqlEx.Message}";

			// ✅ Log en servidor (para que quede evidencia completa)
			Console.Error.WriteLine("==== ERROR GuardarDatosPersonales ====");
			Console.Error.WriteLine($"clientId: {clientId}");
			Console.Error.WriteLine($"msg: {msg}");
			Console.Error.WriteLine(ex.ToString()); // stacktrace completo
			Console.Error.WriteLine("======================================");

			// ✅ Mándalo al front
			await _hubConnection.InvokeAsync("ErrorConexion", clientId, msg);

			// re-lanza
			throw;
		}
	}

	// Método para validar Base64 antes de convertir
	private bool EsBase64Valido(string base64String)
    {
        Span<byte> buffer = new Span<byte>(new byte[base64String.Length]);
        return Convert.TryFromBase64String(base64String, buffer, out _);
    }

    public async Task EditarDatosPersonales(string clientId, string datosPersonales)
    {
        try
        {
            var objDatosPersonalesCompletos = JsonConvert.DeserializeObject<RespuestaDatosPersonales>(datosPersonales);
            var objDatosPersonales = objDatosPersonalesCompletos.datosPersonales;
            var objFotoPaciente = objDatosPersonalesCompletos.strFotoFrontal;

            var objDatosPersonalesServicios = new DatosPersonalesServicios();
            var objFotosFrontalesServicios = new TFOTOSFRONTALESServicios();

            // Verificar si el paciente existe antes de actualizar
            var pacienteExistente = await objDatosPersonalesServicios.ConsultarPorId(objDatosPersonales.IDANAMNESIS);
            if (pacienteExistente == null)
            {
                await _hubConnection.InvokeAsync("ErrorEdicionDatosPersonales", clientId, "Paciente no encontrado");
                return;
            }

            // Actualizar datos personales
            var resultado = await objDatosPersonalesServicios.Editar(objDatosPersonales.IDANAMNESIS, objDatosPersonales);

            // Si hay una nueva imagen, actualizarla
            if (!string.IsNullOrEmpty(objFotoPaciente) && EsBase64Valido(objFotoPaciente))
            {
                var fotoPaciente = new TFOTOSFRONTALES
                {
                    IDANAMNESIS = objDatosPersonales.IDANAMNESIS,
                    FOTOFRENTE = Convert.FromBase64String(objFotoPaciente)
                };

                await objFotosFrontalesServicios.Editar(fotoPaciente);
            }

            await _hubConnection.InvokeAsync("RespuestaEditarDatosPersonales", clientId, resultado.ToString());
        }
        catch (Exception e)
        {
            await _hubConnection.InvokeAsync("ErrorEdicionDatosPersonales", clientId, e.Message);
        }
    }
        
        
        public async Task EditarAntecedentes(string clientId, string antecedentesPaciente)
        {
            try
            {
                var objAntecedentes = JsonConvert.DeserializeObject<Antecedentes>(antecedentesPaciente);
            
               
                var objDatosPersonalesServicios = new DatosPersonalesServicios();
                var objAntecedentesPacienteServicios = new AntecedentesServicios();

                // Verificar si el paciente existe antes de actualizar
                var pacienteExistente = await objDatosPersonalesServicios.ConsultarPorId(objAntecedentes.IDANAMNESIS);
                if (pacienteExistente == null)
                {
                    await _hubConnection.InvokeAsync("ErrorEdicionDatosPersonales", clientId, "Paciente no encontrado");
                    return;
                }

                // Actualizar datos personales
                var resultado = await objAntecedentesPacienteServicios.Editar(objAntecedentes.IDANAMNESIS, objAntecedentes);

                

                await _hubConnection.InvokeAsync("RespuestaEditarAntecedentes", clientId, resultado.ToString());
            }
            catch (Exception e)
            {
                await _hubConnection.InvokeAsync("ErrorEdicionDatosPersonales", clientId, e.Message);
            }
        }





        public async Task<RespuestaConsultarPorDiaYPorUnidadModel> ConsultarPorDiaYPorUnidad(string clientId, int silla, DateTime fecha)
        {
            var objHorariosAgenda = new THORARIOSAGENDAServicios();
            var objRespuestaConsultarPorDiaYPorUnidad = new P_AGENDA1();
            var objRespuestaConsultarHorariosSilla = await objHorariosAgenda.ConsultarPorId(silla); 
            var sillaStr=silla.ToString();
       
            var intervalo = objRespuestaConsultarHorariosSilla.INTERVALO;
            using (var _dbcontext = new AppDbContext())
            {
                if (intervalo == null || intervalo < 0)
                {
                    intervalo = 15;

                }
                var obj = await _dbcontext.P_AGENDA1(sillaStr, fecha.Date, "1", objRespuestaConsultarHorariosSilla.HORAINICIAL, objRespuestaConsultarHorariosSilla.HORAFINAL, intervalo ?? 0, "", "");
                var modelo = new RespuestaConsultarPorDiaYPorUnidadModel();
               //---------------------------validar si el dia seleccoionado es festivo----------------------//
                var Festivo = new TFESTIVOSServicios();
                var objRespuestaBuscarFestivo = await Festivo.ConsultarPorFecha(fecha.Date);
                modelo.lstP_AGENDA1 = obj;
                if (objRespuestaBuscarFestivo !=  null && objRespuestaBuscarFestivo.FECHA > DateTime.MinValue)
                {
                    modelo.esFestivo = true;
                }
                else
                {
                    modelo.esFestivo = false;
                }
                //-----------------------------------------------------------------------------------------------//
                modelo.terminoRefrescar = true;
                var modeloSerializado = JsonConvert.SerializeObject(modelo);
                var modeloSerializadoComprimido = ArchivosHelper.CompressString(modeloSerializado);
                await _hubConnection.InvokeAsync("RespuestaObtenerConsultaPorDiaYPorUnidad", clientId, modeloSerializadoComprimido);
                return modelo == null ? new RespuestaConsultarPorDiaYPorUnidadModel() : modelo;
            
            
            }
        }

    
    private async Task<string> validacionesExclullentes(string clientId, string datosAgenda)
    {
        var mensaje = "ACA VA MENSAJE SI VALIDACION NO PASO";
        //validar doctor no tenga cita a la misma hora en otra silla
        //validar que la agenda tenga espacio
        return mensaje ;
    }

    private void respuestaCrearCitaVacia(string clientId, List<ConfirmacionesPedidasModel> lstRespuestaConfirmacionesPedidas)
    {
        var objResp = new RespuestaConsultarPorDiaYPorUnidadModel();
        objResp.lstConfirmacionesPedidas = lstRespuestaConfirmacionesPedidas;
        var objRespSerializado = JsonConvert.SerializeObject(objResp);
        var objRespSerializadoComprimido = ArchivosHelper.CompressString(objRespSerializado);
        _hubConnection.InvokeAsync("RespuestaAgendarCita", clientId, objRespSerializadoComprimido);
    }

    private string BuscarHoraFinal(string horaCita, string duracion)
    {
        var horaFinal = AgregarMinutosAHora(horaCita, duracion);
        return horaFinal;
    }

    private string AgregarMinutosAHora(string hora, string minutos)
    {
        // Convierte la hora y los minutos a un TimeSpan
        TimeSpan horaCita = TimeSpan.Parse(hora);
        int duracion = int.Parse(minutos);

        // Agrega la duración a la hora de la cita
        TimeSpan horaFinal = horaCita.Add(TimeSpan.FromMinutes(duracion));

        // Convierte la hora final de vuelta a una cadena de hora
        string horaFinalString = horaFinal.ToString(@"hh\:mm");

        return horaFinalString;
    }
    public async Task RealizarAccionesEnCitaAgendada(string clientId, string datosRealizarAccionAgenda)
    {
        var objHistorial = new THISTORIALServicios();
        var objDetallesCitasServicios = new TDETALLECITASServicios();
        var objFrasesParaAgendar = new T_FRASE_XEVOLUCIONServicios();
        var respuesta = false;
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new TimeSpanConverter());
        var objDatosRealizarAccionCitaAgendada = JsonConvert.DeserializeObject<List<RespuestaAccionesEnCitaAgendada>>(datosRealizarAccionAgenda, settings);
        if (objDatosRealizarAccionCitaAgendada != null && objDatosRealizarAccionCitaAgendada.Count() > 0)
        {
            if (objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "BORRAR").Any())
            {
                var borrarCita = objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "BORRAR").FirstOrDefault();
                if (borrarCita.aceptado)
                {
                    var cita = await objDetallesCitasServicios.ConsultarPorFechaSillaHora(borrarCita.fecha.Date, borrarCita.silla, borrarCita.hora);
                    //Consultar idAnamnesis por idAnamnesisTexto
                    var objAnamnesis = new TANAMNESISServicios();
                    var objIdAnamnesis = await objAnamnesis.ConsultarPorIdTextoElIdAnamnesis(cita[0].ID);
                    //------------------------------------------------------------------//
                    if (cita != null && cita.Count() > 0)
                    {
                        respuesta = await objDetallesCitasServicios.Borrar(borrarCita.fecha.Date, borrarCita.silla, borrarCita.hora);
                        if (respuesta)
                        {
                            var objCitasBorradas = new TCITASBORRADASServicios();
                            await objCitasBorradas.Agregar(new TCITASBORRADAS() { FECHA = borrarCita.fecha.Date, SILLA = borrarCita.silla, HORA = borrarCita.hora, NOMBRE = cita[0].NOMBRE, USUARIO = borrarCita.quienLoHace, FECHASUCESO = DateTime.Now.Date });
                            var mensaje = "Cita borrada de " + cita[0].NOMBRE + " el " + DateTime.Now.Date.ToString("dd/MM/yyyy") + " a las " + DateTime.Now.TimeOfDay.ToString() + "estaba programada para" + cita[0].FECHA + "a las" + cita[0].HORA + "en la silla" + cita[0].SILLA;
                            await objHistorial.Agregar(new THISTORIAL() { FECHA = DateTime.Now.Date, HORA = DateTime.Now.TimeOfDay, USUARIO = borrarCita.quienLoHace, IDANAMNESIS = objIdAnamnesis, DESCRIPCION = mensaje });
                        }
                    }
                }
            }
            if (objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "CANCELARCITA").Any())
            {
                var cancelarCita = objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "CANCELARCITA").FirstOrDefault();
                if (cancelarCita.aceptado)
                {
                    var cita = await objDetallesCitasServicios.ConsultarPorFechaSillaHora(cancelarCita.fecha.Date, cancelarCita.silla, cancelarCita.hora);
                    //Consultar idAnamnesis por idAnamnesisTexto
                    var objAnamnesis = new TANAMNESISServicios();
                    var objIdAnamnesis = await objAnamnesis.ConsultarPorIdTextoElIdAnamnesis(cita[0].ID);
                    //------------------------------------------------------------------//
                    if (cita != null && cita.Count() > 0)
                    {
                        respuesta = await objDetallesCitasServicios.Borrar(cancelarCita.fecha.Date, cancelarCita.silla, cancelarCita.hora);
                        if (respuesta)
                        {
                            var objCitasCanceladas = new TCITASCANCELADASServicios();
                            var citaCancelada = new TCITASCANCELADAS() { FECHA = cancelarCita.fecha.Date, SILLA = cancelarCita.silla, HORA = cancelarCita.hora, NOMBRE = cita[0].NOMBRE, USUARIO = cancelarCita.quienLoHace, MOTIVO_CANCELA = cancelarCita.mensaje };
                            await objCitasCanceladas.Agregar(citaCancelada);
                            var mensaje = "Cita cancelada de " + cita[0].NOMBRE + " el " + DateTime.Now.Date.ToString("dd/MM/yyyy") + " a las " + DateTime.Now.TimeOfDay.ToString() + "estaba programada para" + cita[0].FECHA + "a las" + cita[0].HORA + "en la silla" + cita[0].SILLA;
                            await objHistorial.Agregar(new THISTORIAL() { FECHA = DateTime.Now.Date, HORA = DateTime.Now.TimeOfDay, USUARIO = cancelarCita.quienLoHace, IDANAMNESIS = objIdAnamnesis, DESCRIPCION = mensaje });
                        }
                    }
                }
            }
            else if (objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "CONFIRMAR").Any())
            {
                var confirmarCita = objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "CONFIRMAR").FirstOrDefault();
                if (confirmarCita.aceptado)
                {
                    var cita = await objDetallesCitasServicios.ConsultarPorFechaSillaHora(confirmarCita.fecha.Date, confirmarCita.silla, confirmarCita.hora);
                    
                    if (cita != null && cita.Count() > 0)
                    {
                        cita[0].CONFIRMAR = "SI";
                        cita[0].OBSERVACIONES = confirmarCita.respuesta;
                        //Consultar idAnamnesis por idAnamnesisTexto
                        var objAnamnesis = new TANAMNESISServicios();
                        var objIdAnamnesis = await objAnamnesis.ConsultarPorIdTextoElIdAnamnesis(cita[0].ID);
                        //------------------------------------------------------------------//
                        if (objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "CONFIRMAR_ALARMA_AGENDA").Any())
                        {
                            var alarmarCita = objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "CONFIRMAR_ALARMA_AGENDA").FirstOrDefault();
                            if (alarmarCita.aceptado)
                            {
                                cita[0].ALARMAR = "SI";
                            }
                        }
                        respuesta = await objDetallesCitasServicios.Editar(confirmarCita.fecha.Date, confirmarCita.silla, confirmarCita.hora, cita[0]);
                        if (respuesta)
                        {
                            var mensaje = "Cita confirmada de " + cita[0].NOMBRE + " el " + DateTime.Now.Date.ToString("dd/MM/yyyy") + " a las " + DateTime.Now.TimeOfDay.ToString() + "estaba programada para" + cita[0].FECHA + "a las" + cita[0].HORA + "en la silla" + cita[0].SILLA;
                            await objHistorial.Agregar(new THISTORIAL() { FECHA = DateTime.Now.Date, HORA = DateTime.Now.TimeOfDay, USUARIO = confirmarCita.quienLoHace, IDANAMNESIS = objIdAnamnesis, DESCRIPCION = mensaje }); 
                        }

                    }
                }
            }
            else if (objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "SINCONFIRMAR").Any())
            {
                var citaSinConfirmar = objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "SINCONFIRMAR").FirstOrDefault();
                if (citaSinConfirmar.aceptado)
                {
                    var cita = await objDetallesCitasServicios.ConsultarPorFechaSillaHora(citaSinConfirmar.fecha.Date, citaSinConfirmar.silla, citaSinConfirmar.hora);
                    
                    if (cita != null && cita.Count() > 0)
                    {
                        cita[0].CONFIRMAR = "";
                        //cita[0].OBSERVACIONES = citaSinConfirmar.respuesta;
                        respuesta = await objDetallesCitasServicios.Editar(citaSinConfirmar.fecha.Date, citaSinConfirmar.silla, citaSinConfirmar.hora, cita[0]);
                    }
                }
            }
            else if (objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "NOASISTIO").Any())
            {
                var noAsistio = objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "NOASISTIO").FirstOrDefault();
                if (noAsistio.aceptado)
                {
                    var cita = await objDetallesCitasServicios.ConsultarPorFechaSillaHora(noAsistio.fecha.Date, noAsistio.silla, noAsistio.hora);
                    //Consultar idAnamnesis por idAnamnesisTexto
                    var objAnamnesis = new TANAMNESISServicios();
                    var objIdAnamnesis = await objAnamnesis.ConsultarPorIdTextoElIdAnamnesis(cita[0].ID);
                    //------------------------------------------------------------------//
                    if (cita != null && cita.Count() > 0)
                    {
                        cita[0].ASISTENCIA = "NO";
                        respuesta = await objDetallesCitasServicios.Editar(noAsistio.fecha.Date, noAsistio.silla, noAsistio.hora, cita[0]);
                        if (respuesta)
                        {
                            var mensaje = "Cita no asistida de " + cita[0].NOMBRE + " el " + DateTime.Now.Date.ToString("dd/MM/yyyy") + " a las " + DateTime.Now.TimeOfDay.ToString() + "estaba programada para" + cita[0].FECHA + "a las" + cita[0].HORA + "en la silla" + cita[0].SILLA;
                            await objHistorial.Agregar(new THISTORIAL() { FECHA = DateTime.Now.Date, HORA = DateTime.Now.TimeOfDay, USUARIO = noAsistio.quienLoHace, IDANAMNESIS = objIdAnamnesis, DESCRIPCION = mensaje });
                        }
                        if (noAsistio.respuesta != null && noAsistio.respuesta!="" && cita[0].ID != null && cita[0].ID != "")
                        {
                            var objEvolucionPaciente = new TEVOLUCIONServicios();
                            var objEvolucion = new TEVOLUCION();
                            objEvolucion.IDEVOLUSECUND = objIdAnamnesis;
                            objEvolucion.FECHA = noAsistio.fecha.Date;
                            objEvolucion.HORA = noAsistio.hora;
                            objEvolucion.EVOLUCION = noAsistio.respuesta;
                            objEvolucion.DOCTOR = cita[0].DOCTOR;
                            await objEvolucionPaciente.Agregar(objEvolucion);
                            var mensaje ="se realiza evolucion desde agenda por inasistencia del Paciente " + cita[0].NOMBRE + "que estaba programada para el dia " + cita[0].FECHA.Value.Date + " a las " + cita[0].HORA + "esto se hizo el dia" + DateTime.Now.Date + "a las" + DateTime.Now.TimeOfDay + "y lo hizo" + noAsistio.quienLoHace;
                        }
                    }
                }
            }
            else if (objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "ASISTIO").Any())
            {
                var asistio = objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "ASISTIO").FirstOrDefault();
                if (asistio.aceptado)
                {
                    var cita = await objDetallesCitasServicios.ConsultarPorFechaSillaHora(asistio.fecha.Date, asistio.silla, asistio.hora);
                    if (cita != null && cita.Count() > 0)
                    {
                        cita[0].ASISTENCIA = "SI";
                        //cita[0].HORA_LLEGADA_CITA= cita[0].HORA_LLEGADA_CITA = DateTime.Now.TimeOfDay;
                        respuesta = await objDetallesCitasServicios.Editar(asistio.fecha.Date, asistio.silla, asistio.hora, cita[0]);
                        if (cita[0].ID != null && cita[0].ID != "")
                        {
                            try
                            {
                                //Consultar idAnamnesis por idAnamnesisTexto
                                var objAnamnesis = new TANAMNESISServicios();
                                var objIdAnamnesis = await objAnamnesis.ConsultarPorIdTextoElIdAnamnesis(cita[0].ID);
                                //------------------------------------------------------------------//
                                var objEvolucionPaciente = new TEVOLUCIONServicios();
                                var existeEvolucion = await objEvolucionPaciente.ConsultarPorAnamnesisFechaYHora(objIdAnamnesis, asistio.fecha.Date, asistio.hora);
                                if (existeEvolucion != null)
                                {
                                    if (existeEvolucion.IDEVOLUCION.HasValue)
                                    {
                                        await objEvolucionPaciente.Borrar(existeEvolucion.IDEVOLUCION.Value);
                                    }
                                }
                            }
                            catch (Exception e)
                            {

                                throw;
                            }
                            
                            
                        }
                    }
                }
            }
            else if (objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "QUITARASISTENCIA").Any())
            {
                var quitarAsistencia = objDatosRealizarAccionCitaAgendada.Where(x => x.tipoAccion == "QUITARASISTENCIA").FirstOrDefault();
                if (quitarAsistencia.aceptado)
                {
                    var cita = await objDetallesCitasServicios.ConsultarPorFechaSillaHora(quitarAsistencia.fecha.Date, quitarAsistencia.silla, quitarAsistencia.hora);
                    if (cita != null && cita.Count() > 0)
                    {
                        cita[0].ASISTENCIA = "";
                        //cita[0].HORA_LLEGADA_CITA = null;
                        
                        respuesta = await objDetallesCitasServicios.Editar(quitarAsistencia.fecha.Date, quitarAsistencia.silla, quitarAsistencia.hora, cita[0]);
                        if (cita[0].ID != null && cita[0].ID != "")
                        {
                            //Consultar idAnamnesis por idAnamnesisTexto
                            var objAnamnesis = new TANAMNESISServicios();
                            var objIdAnamnesis = await objAnamnesis.ConsultarPorIdTextoElIdAnamnesis(cita[0].ID);
                            //------------------------------------------------------------------//

                            var objEvolucionPaciente = new TEVOLUCIONServicios();
                            var existeEvolucion = await objEvolucionPaciente.ConsultarPorAnamnesisFechaYHora(objIdAnamnesis, quitarAsistencia.fecha.Date, quitarAsistencia.hora);
                            if (existeEvolucion != null)
                            {
                                if (existeEvolucion.IDEVOLUCION.HasValue)
                                {
                                    await objEvolucionPaciente.Borrar(existeEvolucion.IDEVOLUCION.Value);
                                }
                            }
                        }
                    }
                }
            }



			/*else if (objDatosRealizarAccionCitaAgendada.Any(x => x.tipoAccion == "RECORDARCITA"))
            {
                var enviarMensajeRecordarCita = objDatosRealizarAccionCitaAgendada.First(x => x.tipoAccion == "RECORDARCITA");

                if (enviarMensajeRecordarCita.aceptado)
                {
                    var servicioWhatsApp = new WhatsAppService();

                    try
                    {
                        // Consultar las citas de forma asíncrona
                        var listadoCitasParaEnviarMensaje = await objDetallesCitasServicios
                            .ConsultarPorFechaySilla(enviarMensajeRecordarCita.fecha.Date, enviarMensajeRecordarCita.silla);

                        if (listadoCitasParaEnviarMensaje == null || !listadoCitasParaEnviarMensaje.Any())
                        {
                            Console.WriteLine("No hay citas para enviar recordatorios.");
                            return;
                        }

                        // Recorrer cada cita secuencialmente
                        for (int i = 0; i < listadoCitasParaEnviarMensaje.Count; i++)
                        {
                            var cita = listadoCitasParaEnviarMensaje[i];

                            // Validar si ya se envió el mensaje (COLOR ya es "VERDE")
                            if (cita.CEDULA == "SI")
                            {
                                Console.WriteLine($"Mensaje ya enviado para la cita con ID: {cita.ID}. Se omite el envío.");
                                continue; // Pasar a la siguiente cita
                            }

                            var haciaNumero = ValidarYAgregarPrefijo(cita.TELEFONO);
                            if (string.IsNullOrWhiteSpace(haciaNumero))
                            {
                                haciaNumero = ValidarYAgregarPrefijo(cita.CELULAR);
                            }

                            var fraseRecordatorio = await objFrasesParaAgendar.ConsultarPorTipo(5);
                            var templateNombre  = "";

                            if (fraseRecordatorio != null)
                            {
                                templateNombre = fraseRecordatorio.CONTENIDO; 
                            }
                            else
                            {
                                templateNombre = "Hola {0}, te recordamos que tu cita está agendada para el dia {1} a las {2} con {3} .";
                            }  
                            var parametros = new List<string>
                            {
                                cita.NOMBRE,
                                cita.FECHA?.ToString("dd/MM/yyyy"), // Formato día/mes/año
                                cita.HORA.HasValue ? DateTime.Today.Add(cita.HORA.Value)
                                .ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture): "Hora no especificada",
                                cita.DOCTOR
                            };

                            if (!string.IsNullOrWhiteSpace(haciaNumero))
                            {
                                var resultado = await servicioWhatsApp.EnviarMensaje(haciaNumero, templateNombre, parametros);

                                if (!resultado)
                                {
                                    Console.WriteLine($"Error al enviar el mensaje para la cita con ID: {cita.ID}");
                                }
                                else
                                {
                                    Console.WriteLine($"Mensaje enviado correctamente a {cita.NOMBRE}.");

                                    // Actualizar el campo CEDULA a "SI"
                                    var respuestaEdicion = await objDetallesCitasServicios.ActualizarCampo(
                                        cita.FECHA.Value.Date,
                                        cita.SILLA ?? 0,
                                        cita.HORA.Value,
                                        "SI"
                                    );

                                    if (!respuestaEdicion)
                                    {
                                        Console.WriteLine($"Error al actualizar el color de la cita con ID: {cita.ID}");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Datos incompletos para enviar mensaje a {cita.NOMBRE}.");
                            }
                        }

                        respuesta = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al procesar los mensajes: {ex.Message}");
                        respuesta = false;
                    }
                }
            }*/
			else if (objDatosRealizarAccionCitaAgendada.Any(x => x.tipoAccion == "RECORDARCITA"))
			{
				var enviarMensajeRecordarCita = objDatosRealizarAccionCitaAgendada.First(x => x.tipoAccion == "RECORDARCITA");

				if (enviarMensajeRecordarCita.aceptado)
				{
					var servicioWhatsApp = new WhatsAppService();

					try
					{
						// Consultar las citas de forma asíncrona
						var listadoCitasParaEnviarMensaje = await objDetallesCitasServicios
							.ConsultarPorFechaySilla(enviarMensajeRecordarCita.fecha.Date, enviarMensajeRecordarCita.silla);

						if (listadoCitasParaEnviarMensaje == null || !listadoCitasParaEnviarMensaje.Any())
						{
							Console.WriteLine("No hay citas para enviar recordatorios.");
							respuesta = true; // no es error
						}
						else
						{
							// 🔥 IMPORTANTE:
							// - WhatsApp:UseQueue = true  => ENCOLA (no envía aquí, no marca SI aquí)
							// - WhatsApp:UseQueue = false => ENVÍA DIRECTO y marca SI aquí

							bool useQueue = _configuration?.GetValue<bool>("WhatsApp:UseQueue") ?? false;
							bool useRandomTemplate = _configuration?.GetValue<bool>("WhatsApp:UseRandomReminderTemplate") ?? false;

							// ✅ Si NO es random, trae la frase 1 sola vez (evita consultar BD por cada cita)
							string templateDesdeBd = "Hola {0}, te recordamos que tu cita está agendada para el día {1} a las {2} con {3}.";
							if (!useRandomTemplate)
							{
								var fraseRecordatorio = await objFrasesParaAgendar.ConsultarPorTipo(5);
								if (!string.IsNullOrWhiteSpace(fraseRecordatorio?.CONTENIDO))
									templateDesdeBd = fraseRecordatorio.CONTENIDO;
							}

							for (int i = 0; i < listadoCitasParaEnviarMensaje.Count; i++)
							{
								var cita = listadoCitasParaEnviarMensaje[i];

								// Si ya se envió => omitir
								if (cita.CEDULA == "SI")
								{
									Console.WriteLine($"Mensaje ya enviado para la cita con ID: {cita.ID}. Se omite el envío.");
									continue;
								}

								var haciaNumero = ValidarYAgregarPrefijo(cita.TELEFONO);
								if (string.IsNullOrWhiteSpace(haciaNumero))
									haciaNumero = ValidarYAgregarPrefijo(cita.CELULAR);

								if (string.IsNullOrWhiteSpace(haciaNumero))
								{
									Console.WriteLine($"Datos incompletos para enviar mensaje a {cita.NOMBRE}.");
									continue;
								}

								// ✅ Plantilla:
								// - Random: cada cita elige una diferente (DEBE ir en el for)
								// - BD: usa la misma para todas (ya cargada fuera del for)
								string templateNombre = useRandomTemplate
									? TemplateAleatorioRecordar()
									: templateDesdeBd;

								var parametros = new List<string>
		                        {
			                        cita.NOMBRE,
			                        cita.FECHA?.ToString("dd/MM/yyyy") ?? "",
			                        cita.HORA.HasValue
				                        ? DateTime.Today.Add(cita.HORA.Value).ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture)
				                        : "Hora no especificada",
			                        cita.DOCTOR ?? ""
		                        };

								if (useQueue)
								{
									// ✅ Encolar (y NO marcar SI aquí)
									var encolado = await EnviarWhatsAppSeguro(
										servicioWhatsApp,
										haciaNumero,
										templateNombre,
										parametros,
										"RECORDAR",
										fecha: cita.FECHA?.Date,
										silla: cita.SILLA,
										hora: cita.HORA
									);

									if (!encolado)
										Console.WriteLine($"No se pudo encolar el recordatorio para cita ID: {cita.ID}");
									else
										Console.WriteLine($"Recordatorio ENCOLADO para {cita.NOMBRE} (ID: {cita.ID}).");
								}
								else
								{
									// ✅ Modo antiguo: enviar directo y marcar SI
									var enviado = await servicioWhatsApp.EnviarMensaje(haciaNumero, templateNombre, parametros);

									if (!enviado)
									{
										Console.WriteLine($"Error al enviar el mensaje para la cita con ID: {cita.ID}");
									}
									else
									{
										Console.WriteLine($"Mensaje enviado correctamente a {cita.NOMBRE}.");

										var respuestaEdicion = await objDetallesCitasServicios.ActualizarCampo(
											cita.FECHA!.Value.Date,
											cita.SILLA ?? 0,
											cita.HORA!.Value,
											"SI"
										);

										if (!respuestaEdicion)
											Console.WriteLine($"Error al actualizar el estado (CEDULA) de la cita con ID: {cita.ID}");
									}
								}
							}

							respuesta = true;
						}

					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error al procesar los mensajes: {ex.Message}");
						respuesta = false;
					}
				}
			}

		}
		var respuestaSerializada = JsonConvert.SerializeObject(respuesta);
        var respuestaSerializadaComprimida = ArchivosHelper.CompressString(respuestaSerializada);
        await _hubConnection.InvokeAsync("RespuestaRealizarAccionesEnCitaAgendada", clientId, respuestaSerializadaComprimida);
    }



	public static string ValidarYAgregarPrefijo(string numero)
	{
		if (string.IsNullOrWhiteSpace(numero))
			return null;

		var regex = new Regex(@"^3\d{9}$");

		numero = numero.Trim().Replace(" ", "").Replace("-", "");

		if (numero.StartsWith("+57"))
		{
			var numeroSinPrefijo = numero.Substring(3);
			return regex.IsMatch(numeroSinPrefijo) ? numero : null;
		}
		else if (numero.StartsWith("57"))
		{
			var numeroSinPrefijo = numero.Substring(2);
			if (regex.IsMatch(numeroSinPrefijo))
				return $"+{numero}";
		}
		else if (regex.IsMatch(numero))
		{
			return $"+57{numero}";
		}

		return null;
	}

	public async Task ObtenerValidacionesAgenda(string clientId, string datosAgenda)
    {
        
        try
        {
            var exigeRevisarCronograma = false;
            var objDetallesCitasServicios = new TDETALLECITASServicios();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TimeSpanConverter());
            var objAgenda = JsonConvert.DeserializeObject<RespuestaConsultarPorDiaYPorUnidadModel>(datosAgenda, settings);
            var editar = objAgenda != null && objAgenda.detalleCitaEditar != null && objAgenda.detalleCitaEditar.FECHA != null && objAgenda.detalleCitaEditar.SILLA != null && objAgenda.detalleCitaEditar.HORA != null;
            var fecha = objAgenda.lstDetallaCitas[0].FECHA;
            var nombre = objAgenda.lstDetallaCitas[0].NOMBRE;
            var historia = objAgenda.lstDetallaCitas[0].ID;
            var horaEditar = objAgenda.lstDetallaCitas[0].HORA;
            

            if (objAgenda.lstConfirmacionesPedidas != null && objAgenda.lstConfirmacionesPedidas.Count() > 0)
            {
                var lstConfirmacionesPedidas = objAgenda.lstConfirmacionesPedidas;
                List<ConfirmacionesPedidasModel> lstRespuestaConfirmacionesPedidas = new List<ConfirmacionesPedidasModel>();

                if (lstConfirmacionesPedidas.Where(x => x.nombreConfirmacion == "QDoctoresConCitaOtraUnidad").Any())
                {
                    bool estaRepetido = false;
                    var duracion = Convert.ToInt32(objAgenda.lstDetallaCitas[0].DURACION);
                    var horaCita = objAgenda.lstDetallaCitas[0].HORA;
                    var horaFinal = BuscarHoraFinal(horaCita.ToString(), duracion.ToString());
                    var horaFinalTs = TimeSpan.Parse(horaFinal);
                    var doctor = objAgenda.lstDetallaCitas[0].DOCTOR;
                    var silla = objAgenda.lstDetallaCitas[0].SILLA;
                    
                    //hacer consulta de hora incial de la agenda y hora final de la agenda
                    var objHorariosAgenda = new THORARIOSAGENDAServicios();
                    var objRespuestaConsultarHorariosSilla = await objHorariosAgenda.ConsultarPorId(objAgenda.lstDetallaCitas[0].SILLA??0);
                    var hayEspacio = await BuscarEspacioAgenda(objAgenda.lstDetallaCitas[0].SILLA.ToString(), objAgenda.lstDetallaCitas[0].FECHA.Value, "1", objRespuestaConsultarHorariosSilla.HORAINICIAL.ToString(), objRespuestaConsultarHorariosSilla.HORAFINAL.ToString(), 15, "", "", objAgenda.lstDetallaCitas[0].HORA.ToString(), BuscarHoraFinal(objAgenda.lstDetallaCitas[0].HORA.ToString(), objAgenda.lstDetallaCitas[0].DURACION.ToString()), objAgenda.detalleCitaEditar);
                    if (!hayEspacio)
                    {
                        lstRespuestaConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel()
                        {
                            mensaje = "Este espacio ya esta ocupado por otra cita",
                            nombreConfirmacion = "NO_HAY_ESPACIO",
                            pedirConfirmar = false,
                            esMensajeRestrictivo = true
                        });
                        respuestaCrearCitaVacia(clientId, lstRespuestaConfirmacionesPedidas);
                        return;
                    }

                    //aca hacemos el query para ver si el doctor tiene cita en otra unidad
                    if (fecha.HasValue && horaCita.HasValue)
                    {
                        estaRepetido = await objDetallesCitasServicios.ConsultarDoctoresConCitaOtraUnidad(doctor, fecha.Value, horaCita.Value, horaFinalTs, objAgenda.detalleCitaEditar);
                    }

                    if (estaRepetido)
                    {
                        lstRespuestaConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel()
                        {
                            mensaje = "El doctor tiene una cita en otra unidad a la misma hora, No se puede dar la cita",
                            nombreConfirmacion = "QDoctoresConCitaOtraUnidad",
                            pedirConfirmar = false,
                            esMensajeRestrictivo = true
                        });
                        respuestaCrearCitaVacia(clientId, lstRespuestaConfirmacionesPedidas);
                        return; // preguntar si este return nos saca de la funcion
                    }
                }
                if (lstConfirmacionesPedidas.Where(x => x.nombreConfirmacion == "CITA_REPETIDA").Any())
                {
                    bool estaRepetido = false;
                    var resultado = await objDetallesCitasServicios.ConsultarPacienteConCitaRepetida(nombre, fecha.Value, historia);
                    if (editar)
                    {
                        //revisar porfavor
                        if (resultado != null)
                        {
                            var resultadoEditar =  resultado?.Where(x =>x.FECHA != fecha && x.HORA != horaEditar);
                        }
                    }
                    else//se agrego esto porque siempre asi se estyuviera editando se iba a la validacion de si el paciente tenia cita repetida
                    {
                        if (resultado != null && resultado.Count > 0)
                        {
                            lstRespuestaConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel()
                            {
                                mensaje = "El paciente ya tiene cita asignada el dia "+ fecha.Value.Date.ToString("dd/MM/yyyy") + " desea continuar asignando ésta cita?",
                                nombreConfirmacion = "CITA_REPETIDA",
                                pedirConfirmar = true,
                                esMensajeRestrictivo = false
                            });
                        }
                    }
                    
                }
                if (!editar)
                {
                    if (lstConfirmacionesPedidas.Where(x => x.nombreConfirmacion == "PROXIMA_CITA_ASUNTO").Any())
                    {
                        var idHistoria = objAgenda.lstDetallaCitas[0].ID;
                        if (idHistoria != null)
                        {
                            var objAnamnesis = new TANAMNESISServicios();
                            var objAnamnesisModel = await objAnamnesis.ConsultarPorIdTexto(idHistoria);
                            if (objAnamnesisModel != null && objAnamnesisModel.IDANAMNESIS > 0)
                            {
                                var objEvolucion = new TEVOLUCIONServicios();
                                var objEvolucionModel = await objEvolucion.ConsultarUltimaEvolucion(objAnamnesisModel.IDANAMNESIS);
                                if (objEvolucionModel != null)
                                {
                                    objAgenda.lstDetallaCitas[0].ASUNTO = objEvolucionModel.PROXIMA_CITAstr;
                                }
                            }
                        }
                    }
                }
                

                
                // A partir de aca ya no hay validaciones ya entramos a guardar la cita
                if (lstRespuestaConfirmacionesPedidas.Any())
                {
                    objAgenda.lstConfirmacionesPedidas = lstRespuestaConfirmacionesPedidas;
                    var objAgendaSerializado = JsonConvert.SerializeObject(objAgenda);
                    var objAgendaSerializadoComprimido = ArchivosHelper.CompressString(objAgendaSerializado);
                    _hubConnection.InvokeAsync("RespuestaAgendarCita", clientId, objAgendaSerializadoComprimido);
                }
                else
                {
                    objAgenda.lstConfirmacionesPedidas = new List<ConfirmacionesPedidasModel>();
                    if (editar)
                    {
                        await EditarDatosAgenda(clientId, objAgenda);
                    }
                    else
                    {
                        await GuardarDatosAgenda(clientId, objAgenda);
                       
                    }

                }
            }
            
            else
            {
                if (editar)
                {
                    await EditarDatosAgenda(clientId, objAgenda);
                }
                else
                {
                    await GuardarDatosAgenda(clientId, objAgenda);
                }
            }
        }
        catch (Exception e)
        {

            throw;
        }

        
    }

    

    public async Task EditarDatosAgenda(string clientId, RespuestaConsultarPorDiaYPorUnidadModel objAgenda)
    {
        try
        {
            var objTHistorialServicios = new THISTORIALServicios();
            var objTCitasServicios = new TCITASServicios();
            var objDetalleCitasServicios = new TDETALLECITASServicios();
            var objFrasesParaAgendar = new T_FRASE_XEVOLUCIONServicios();
            var objDetalleCitasEditar = objAgenda.detalleCitaEditar;
            var objDetalleCitas= objAgenda.lstDetallaCitas[0];
            
            if (objDetalleCitasEditar.SILLA != null && objDetalleCitasEditar.FECHA != null && objDetalleCitasEditar.HORA != null)
            {
                var existeAgenda = await objTCitasServicios.ConsultarPorId(objDetalleCitas.SILLA ?? 0, objDetalleCitas.FECHA ?? DateTime.MinValue);
                if (existeAgenda == null || existeAgenda.SILLA <= 0)
                {
                    var objCita = new TCITAS();
                    objCita.SILLA = objDetalleCitas.SILLA ?? 0;
                    objCita.FECHA = objDetalleCitas.FECHA ?? DateTime.MinValue;
                    objCita.FECHA_TEXTO = objAgenda.citas.FECHA_TEXTO; //Toca consultar el intervalo segun la silla y ponerlo;
                    await objTCitasServicios.Agregar(objCita);
                }
                await objDetalleCitasServicios.Editar(objDetalleCitasEditar.FECHA??DateTime.Today, objDetalleCitasEditar.SILLA??0, objDetalleCitasEditar.HORA??TimeSpan.Zero, objDetalleCitas);
                var mensajeDescripcion = "Se edito la cita de " + objDetalleCitasEditar.NOMBRE + "que estaba en la silla " + objDetalleCitasEditar.SILLA + " el dia " + objDetalleCitasEditar.FECHA.Value.Date + " a las " + objDetalleCitas.HORA + " y se programo para el dia " +
                    objDetalleCitas.FECHA.Value.Date + " a las " + objDetalleCitas.HORA + "en la silla" + objDetalleCitas.SILLA;
                var objAnamnesis = new TANAMNESISServicios();
                var obdIdAnamnesis = await objAnamnesis.ConsultarPorIdTexto(objDetalleCitas.ID);
                var objTHistorial = new THISTORIAL() { DESCRIPCION = mensajeDescripcion, FECHA = DateTime.Now.Date, HORA = DateTime.Now.TimeOfDay, USUARIO = objDetalleCitas.ID, IDANAMNESIS = obdIdAnamnesis.IDANAMNESIS };
                await objTHistorialServicios.Agregar(objTHistorial);
                if (objDetalleCitasEditar.FECHA != objDetalleCitas.FECHA || objDetalleCitasEditar.HORA != objDetalleCitas.HORA)
                {
                    //---------------------enviamos mensaje-------------------------------------------------//
                    //Enviar mensaje cita ha sido asignada para el dia a la hora
                    var servicioWhatsApp = new WhatsAppService();
                    var haciaNumero = ValidarYAgregarPrefijo(objDetalleCitas.TELEFONO);
                    if (string.IsNullOrWhiteSpace(haciaNumero))
                    {
                        haciaNumero = ValidarYAgregarPrefijo(objDetalleCitas.CELULAR);
                    }


                    var fraseRecordatorio = await objFrasesParaAgendar.ConsultarPorTipo(6);
                    var templateNombre = "";

                    if (fraseRecordatorio != null)
                    {
                        templateNombre = fraseRecordatorio.CONTENIDO;
                    }
                    else
                    {
                        templateNombre = "Hola {0}, tu cita ha sido reagendada para el dia {1} a las {2} con {3} .";
                    }
                    var parametros = new List<string>
                    {
                        objDetalleCitas.NOMBRE,
                        objDetalleCitas.FECHA?.ToString("dd/MM/yyyy"), // Formato día/mes/año
                        objDetalleCitas.HORA.HasValue ? DateTime.Today.Add(objDetalleCitas.HORA.Value)
                        .ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture): "Hora no especificada",
                        //objAgenda.lstDetallaCitas[0].HORA?.ToString(@"hh\:mm"),      // Formato de hora "hh:mm"
                        objDetalleCitas.DOCTOR
                    };

                    if (!string.IsNullOrWhiteSpace(haciaNumero))
                    {
						//var resultado = await servicioWhatsApp.EnviarMensajeCitaAgenda(haciaNumero, templateNombre, parametros);
						var resultado = await EnviarWhatsAppSeguro(servicioWhatsApp, haciaNumero, templateNombre, parametros, "AGENDA");

						if (!resultado)
                        {
                            Console.WriteLine($"Error al enviar el mensaje para la cita con ID: {objDetalleCitas.ID}");
                        }
                        else
                        {
                            Console.WriteLine($"Mensaje enviado correctamente a {objDetalleCitas.NOMBRE}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Datos incompletos para enviar mensaje a {objDetalleCitas.NOMBRE}.");
                    }
                    //--------------------------------------------------------------------------------------//
                }




                var objResp = new RespuestaConsultarPorDiaYPorUnidadModel();
                objResp.lstConfirmacionesPedidas = new List<ConfirmacionesPedidasModel>();
                objResp.lstConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel() { mensaje = "Cita editada correctamente", nombreConfirmacion = "CITA_GUARDADA", pedirConfirmar = false, esMensajeRestrictivo = false });
                var objRespSerializado = JsonConvert.SerializeObject(objResp);
                var objRespSerializadoComprimido = ArchivosHelper.CompressString(objRespSerializado);
                await _hubConnection.InvokeAsync("RespuestaAgendarCita", clientId, objRespSerializadoComprimido);
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    

    public async Task GuardarDatosAgenda(string clientId, RespuestaConsultarPorDiaYPorUnidadModel objAgenda)
    {
        try
        {
            var objTHistorialServicios = new THISTORIALServicios();
            var objTCitasServicios = new TCITASServicios();
            var objDetalleCitasServicios = new TDETALLECITASServicios();
            var objFrasesParaAgendar = new T_FRASE_XEVOLUCIONServicios();
            using (var _dbcontext = new AppDbContext())
            {
                objAgenda.lstDetallaCitas[0].IDCONSECUTIVO = int.Parse(await _dbcontext.CONSULTAR_GENERADOR("GEN_DETALLECITAS"));
            }
            
            var objDetalleCitas = objAgenda.lstDetallaCitas[0];
            //objDetalleCitas.NOMBRE = ConvertToISO8859_1(objDetalleCitas.NOMBRE);
            //objDetalleCitas.DOCTOR = ConvertToISO8859_1(objDetalleCitas.DOCTOR);
            //objDetalleCitas.ASUNTO = ConvertToISO8859_1(objDetalleCitas.ASUNTO);

            //-----------------Aca deben ir validaciones----------------------//
            if (objDetalleCitas.SILLA != null && objDetalleCitas.FECHA != null)
            {
                var existeAgenda = await objTCitasServicios.ConsultarPorId(objDetalleCitas.SILLA??0, objDetalleCitas.FECHA??DateTime.MinValue);
                if (existeAgenda == null || existeAgenda.SILLA <= 0)
                {
                    var objCita = new TCITAS();
                    objCita.SILLA = objDetalleCitas.SILLA??0;
                    objCita.FECHA = objDetalleCitas.FECHA??DateTime.MinValue;
                    objCita.FECHA_TEXTO = objAgenda.citas.FECHA_TEXTO; //Toca consultar el intervalo segun la silla y ponerlo;
                    await objTCitasServicios.Agregar(objCita);
                }
                await objDetalleCitasServicios.Agregar(objDetalleCitas);
                var mensajeDescripcion = "Se agrego la cita de " + objDetalleCitas.NOMBRE + "en la silla " + objDetalleCitas.SILLA + " el dia " + objDetalleCitas.FECHA.Value.Date + " a las " + objDetalleCitas.HORA;
                var objAnamnesis = new TANAMNESISServicios();
                var objAnamnesisModel = await objAnamnesis.ConsultarPorIdTexto(objDetalleCitas.ID);
                var objTHistorial = new THISTORIAL() { DESCRIPCION = mensajeDescripcion, FECHA = DateTime.Now.Date, HORA = DateTime.Now.TimeOfDay, USUARIO = objDetalleCitas.ID, IDANAMNESIS = objAnamnesisModel.IDANAMNESIS };
                await objTHistorialServicios.Agregar(objTHistorial);
                var objResp = new RespuestaConsultarPorDiaYPorUnidadModel();
                objResp.lstConfirmacionesPedidas = new List<ConfirmacionesPedidasModel>();
                var objMora = new P_CONSULTAR_MORA_ID_TEXTO();
                using (var _dbcontext = new AppDbContext())
                {
                    objMora = await _dbcontext.P_CONSULTAR_MORA_ID_TEXTO(objDetalleCitas.ID);
                }
                if (objMora != null && objMora.MORA > 0)
                {
                    
                    //string formattedNumber = String.Format("{0:0.}", objMora.MORA);
                    //Console.WriteLine(formattedNumber); // Salida: 1235
                    objResp.lstConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel() { mensaje = "El paciente tiene una mora de " + objMora.MORA?.ToString("N0", new CultureInfo("es-ES")), nombreConfirmacion = "MORA", pedirConfirmar = false, esMensajeRestrictivo = false });
                }
                objResp.lstConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel() { mensaje = "Cita guardada correctamente", nombreConfirmacion = "CITA_GUARDADA", pedirConfirmar = false, esMensajeRestrictivo = false });
                
                //---------------------enviamos mensaje-------------------------------------------------//
                //Enviar mensaje cita ha sido asignada para el dia a la hora
                var servicioWhatsApp = new WhatsAppService();
                var haciaNumero = ValidarYAgregarPrefijo(objAgenda.lstDetallaCitas[0].TELEFONO);
                if (string.IsNullOrWhiteSpace(haciaNumero))
                {
                    haciaNumero = ValidarYAgregarPrefijo(objAgenda.lstDetallaCitas[0].CELULAR);
                }

                
                var fraseRecordatorio = await objFrasesParaAgendar.ConsultarPorTipo(6);
                var templateNombre = "";

                if (fraseRecordatorio != null)
                {
                    templateNombre = fraseRecordatorio.CONTENIDO;
                }
                else
                {
                    templateNombre = "Hola {0}, tu cita ha sido agendada para el dia {1} a las {2} con {3} .";
                }
                var parametros = new List<string>
                {
                    objAgenda.lstDetallaCitas[0].NOMBRE,
                    objAgenda.lstDetallaCitas[0].FECHA?.ToString("dd/MM/yyyy"), // Formato día/mes/año
                    objAgenda.lstDetallaCitas[0].HORA.HasValue ? DateTime.Today.Add(objAgenda.lstDetallaCitas[0].HORA.Value)
                    .ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture): "Hora no especificada",
                    //objAgenda.lstDetallaCitas[0].HORA?.ToString(@"hh\:mm"),      // Formato de hora "hh:mm"
                    objAgenda.lstDetallaCitas[0].DOCTOR
                };

                if (!string.IsNullOrWhiteSpace(haciaNumero))
                {
					//var resultado = await servicioWhatsApp.EnviarMensajeCitaAgenda(haciaNumero, templateNombre, parametros);
					var resultado = await EnviarWhatsAppSeguro(servicioWhatsApp, haciaNumero, templateNombre, parametros, "AGENDA");
					if (!resultado)
                    {
                        Console.WriteLine($"Error al enviar el mensaje para la cita con ID: {objAgenda.lstDetallaCitas[0].ID}");
                    }
                    else
                    {
                        Console.WriteLine($"Mensaje enviado correctamente a {objAgenda.lstDetallaCitas[0].NOMBRE}.");
                    }
                }
                else
                {
                    Console.WriteLine($"Datos incompletos para enviar mensaje a {objAgenda.lstDetallaCitas[0].NOMBRE}.");
                }
                //--------------------------------------------------------------------------------------//
                var objRespSerializado = JsonConvert.SerializeObject(objResp);
                var objRespSerializadoComprimido = ArchivosHelper.CompressString(objRespSerializado);
                await _hubConnection.InvokeAsync("RespuestaAgendarCita", clientId, objRespSerializadoComprimido);

            }
            
        }
        catch (Exception e)
        {
            var objResp = new RespuestaConsultarPorDiaYPorUnidadModel();
            objResp.lstConfirmacionesPedidas = new List<ConfirmacionesPedidasModel>();
            objResp.lstConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel() { mensaje = "No se puedo guardar la cita: "+e.Message, nombreConfirmacion = "CITA_GUARDADA", pedirConfirmar = false, esMensajeRestrictivo = true });
            var objRespSerializado = JsonConvert.SerializeObject(objResp);
            var objRespSerializadoComprimido = ArchivosHelper.CompressString(objRespSerializado);
            await _hubConnection.InvokeAsync("RespuestaAgendarCita", clientId, objRespSerializadoComprimido);
        }
    }

    public async Task<bool> BuscarEspacioAgenda(string IN_SILLA, DateTime IN_FECHA, string IN_TIPO, string HORAINI, string HORAFIN, int INTERVALO, string PARARINI, string PARARFIN, string h1, string h2, TDETALLECITAS? citaEditar = null)
    {
        //var time1 = TimeSpan.Parse(h1);
        //var time2 = TimeSpan.Parse(h2);

        using (var _dbcontext = new AppDbContext())
        {
            try
            {
                var lstAgendaDelDiaPorFecha = await _dbcontext.P_AGENDA1(IN_SILLA, IN_FECHA.Date, "1", HORAINI, HORAFIN, INTERVALO, "", "");
                var lstAgendaDelDiaPorFechaFiltrado = lstAgendaDelDiaPorFecha.Where(x => x.OUT_HORA >= TimeSpan.Parse(h1) && x.OUT_HORA <= TimeSpan.Parse(h2).Subtract(TimeSpan.FromMinutes(1)) && !string.IsNullOrEmpty(x.OUT_NOMBRE));
                if (citaEditar != null && citaEditar.FECHA != null && citaEditar.SILLA != null && citaEditar?.HORA != null)
                {
                    var lstAgendaDelDiaPorFechaFiltradoMenosElQueSeEdita = lstAgendaDelDiaPorFechaFiltrado.Where(x => x.OUT_HORA_CITA != citaEditar.HORA && x.OUT_NOMBRE != citaEditar.NOMBRE);
                    return lstAgendaDelDiaPorFechaFiltradoMenosElQueSeEdita.Count() == 0;
                }
                else
                {
                    return lstAgendaDelDiaPorFechaFiltrado.Count() == 0;
                }
                
            }
            catch (Exception e)
            {

                return false;
            }
            
        }
    }


	private async Task ProcesarRdaDesdeRipsAsync(DatosGuardarRips objRips)
	{
		try
		{
			var rdaContext = RdaGeneracionContextMapper.FromDatosGuardarRips(objRips);
			var validacionRda = RdaGeneracionContextValidator.Validate(rdaContext);

			var objHistorialServicios = new THISTORIALServicios();

			if (!validacionRda.Ok)
			{
				await objHistorialServicios.Agregar(new THISTORIAL()
				{
					FECHA = DateTime.Now.Date,
					HORA = DateTime.Now.TimeOfDay,
					USUARIO = "",
					DESCRIPCION = "RDA contexto inválido: " + validacionRda.Error
				});
				return;
			}

			await objHistorialServicios.Agregar(new THISTORIAL()
			{
				FECHA = DateTime.Now.Date,
				HORA = DateTime.Now.TimeOfDay,
				USUARIO = "",
				DESCRIPCION =
					$"RDA contexto listo. IdAnamnesis={rdaContext.IdAnamnesis}, " +
					$"IdDoctor={rdaContext.IdDoctor}, " +
					$"FechaConsulta={rdaContext.FechaConsulta:yyyy-MM-dd}, " +
					$"CodigoConsulta={rdaContext.CodigoConsulta}, " +
					$"CodigoDiagnosticoPrincipal={rdaContext.CodigoDiagnosticoPrincipal}, " +
					$"CodigoProcedimiento={rdaContext.CodigoProcedimiento}, " +
					$"Factura={rdaContext.Factura}"
			});

			var documentoInternoService = new RdaDocumentoInternoService();
			var documentoInterno = await documentoInternoService.ConstruirDesdeContexto(rdaContext);

			var consultaDocumento = documentoInterno.Documento?.Consulta ?? new RdaConsultaSource();
			var encounterDocumento = consultaDocumento.Encounter ?? new RdaEncounterSource();
			var antecedentesDocumento = consultaDocumento.Antecedentes ?? new RdaAntecedentesSource();
			var diagnosticoDocumento = consultaDocumento.Diagnostico ?? new RdaDiagnosticoSource();
			var procedimientoDocumento = consultaDocumento.Procedimiento ?? new RdaProcedimientoSource();
			var prestadorDocumento = documentoInterno.Documento?.Prestador ?? new RdaPrestadorSource();

			await objHistorialServicios.Agregar(new THISTORIAL()
			{
				FECHA = DateTime.Now.Date,
				HORA = DateTime.Now.TimeOfDay,
				USUARIO = "",
				DESCRIPCION =
					$"RDA documento consolidado. " +
					$"Paciente={encounterDocumento.Nombres} {encounterDocumento.Apellidos}, " +
					$"Doc={encounterDocumento.TipoDocumento} {encounterDocumento.NumeroDocumento}, " +
					$"Prestador={prestadorDocumento.NombrePrestador}, " +
					$"Doctor={prestadorDocumento.NombreDoctor}, " +
					$"Consulta={encounterDocumento.CodigoConsulta}, " +
					$"Dx={encounterDocumento.CodigoDiagnosticoPrincipal}, " +
					$"Proc={encounterDocumento.CodigoProcedimiento}, " +
					$"Motivo={antecedentesDocumento.MotivoConsulta}, " +
					$"Dx1={diagnosticoDocumento.Diagnostico1}, " +
					$"ProcReal={procedimientoDocumento.CodigoProcedimiento}"
			});

			var jsonRdaInterno = JsonConvert.SerializeObject(documentoInterno, Formatting.Indented);

			var bundleFhir = RdaFhirConsultaMapper.Map(documentoInterno);
			var jsonFhir = JsonConvert.SerializeObject(bundleFhir, Formatting.Indented);

			var validacionFhir = RdaFhirConsultaValidator.Validate(bundleFhir);

			if (!validacionFhir.Ok)
			{
				await objHistorialServicios.Agregar(new THISTORIAL()
				{
					FECHA = DateTime.Now.Date,
					HORA = DateTime.Now.TimeOfDay,
					USUARIO = "",
					DESCRIPCION = "RDA FHIR inválido: " + validacionFhir.Error
				});
				return;
			}

			await objHistorialServicios.Agregar(new THISTORIAL()
			{
				FECHA = DateTime.Now.Date,
				HORA = DateTime.Now.TimeOfDay,
				USUARIO = "",
				DESCRIPCION =
					$"RDA FHIR válido. " +
					$"Tipo={documentoInterno.TipoDocumento}, " +
					$"Paciente={encounterDocumento.Nombres} {encounterDocumento.Apellidos}, " +
					$"Prestador={prestadorDocumento.NombrePrestador}, " +
					$"LongitudInterno={jsonRdaInterno.Length}, " +
					$"LongitudFhir={jsonFhir.Length}"
			});

			var options = new RdaOptions();
			_configuration.GetSection("Interoperabilidad:Rda").Bind(options);

			var snapshotInterno = options.GuardarSnapshot ? jsonRdaInterno : null;

			var objRdaDocumentoServicios = new TRDADOCUMENTOServicios();

			var idEvolucionRda = encounterDocumento.IdEvolucion > 0
				? encounterDocumento.IdEvolucion
				: (int?)null;

			var existente = await objRdaDocumentoServicios.ConsultarPorAnamnesisYEvolucion(
				rdaContext.IdAnamnesis,
				idEvolucionRda,
				documentoInterno.TipoDocumento);

			int idRdaFinal;

			if (existente.ID > 0)
			{
				existente.FECHA_ATENCION = encounterDocumento.FechaConsulta;
				existente.ESTADO = "GENERADO";
				existente.JSON_RDAstr = jsonFhir;
				existente.JSON_SNAPSHOTstr = snapshotInterno;
				existente.MENSAJE_ERROR = null;
				existente.FECHA_GENERACION = DateTime.Now;
				existente.FECHA_ENVIO = null;
				existente.INTENTOS = 0;
				existente.CODIGO_HTTP = null;
				existente.RESPUESTA_APIstr = null;
				existente.REQUEST_APIstr = null;

				var actualizado = await objRdaDocumentoServicios.Editar(existente.ID, existente);
				if (!actualizado)
				{
					await objHistorialServicios.Agregar(new THISTORIAL()
					{
						FECHA = DateTime.Now.Date,
						HORA = DateTime.Now.TimeOfDay,
						USUARIO = "",
						DESCRIPCION =
							$"Error actualizando RDA en TRDA_DOCUMENTO. " +
							$"IdRda={existente.ID}, " +
							$"IdAnamnesis={rdaContext.IdAnamnesis}"
					});
					return;
				}

				idRdaFinal = existente.ID;

				await objHistorialServicios.Agregar(new THISTORIAL()
				{
					FECHA = DateTime.Now.Date,
					HORA = DateTime.Now.TimeOfDay,
					USUARIO = "",
					DESCRIPCION =
						$"RDA actualizado en TRDA_DOCUMENTO. " +
						$"IdRda={existente.ID}, " +
						$"Actualizado=SI, " +
						$"IdAnamnesis={rdaContext.IdAnamnesis}, " +
						$"Estado=GENERADO, " +
						$"Formato=FHIR_BUNDLE, " +
						$"Snapshot={(snapshotInterno != null ? "SI" : "NO")}"
				});
			}
			else
			{
				var entidadRda = new TRDA_DOCUMENTO
				{
					IDANAMNESIS = rdaContext.IdAnamnesis,
					IDEVOLUCION = idEvolucionRda,
					FECHA_ATENCION = encounterDocumento.FechaConsulta,
					TIPO_DOCUMENTO = documentoInterno.TipoDocumento,
					ESTADO = "GENERADO",
					JSON_RDAstr = jsonFhir,
					JSON_SNAPSHOTstr = snapshotInterno,
					MENSAJE_ERROR = null,
					FECHA_GENERACION = DateTime.Now,
					FECHA_ENVIO = null,
					INTENTOS = 0,
					CODIGO_HTTP = null,
					RESPUESTA_APIstr = null,
					REQUEST_APIstr = null
				};

				idRdaFinal = await objRdaDocumentoServicios.Agregar(entidadRda);

				if (idRdaFinal <= 0)
				{
					await objHistorialServicios.Agregar(new THISTORIAL()
					{
						FECHA = DateTime.Now.Date,
						HORA = DateTime.Now.TimeOfDay,
						USUARIO = "",
						DESCRIPCION =
							$"Error guardando RDA en TRDA_DOCUMENTO. " +
							$"IdAnamnesis={rdaContext.IdAnamnesis}"
					});
					return;
				}

				await objHistorialServicios.Agregar(new THISTORIAL()
				{
					FECHA = DateTime.Now.Date,
					HORA = DateTime.Now.TimeOfDay,
					USUARIO = "",
					DESCRIPCION =
						$"RDA guardado en TRDA_DOCUMENTO. " +
						$"IdRda={idRdaFinal}, " +
						$"IdAnamnesis={rdaContext.IdAnamnesis}, " +
						$"Estado=GENERADO, " +
						$"Formato=FHIR_BUNDLE, " +
						$"Snapshot={(snapshotInterno != null ? "SI" : "NO")}"
				});
			}

			if (options.Enabled && options.EnviarAutomaticamente && idRdaFinal > 0)
			{
				var envioService = new RdaEnvioService(_configuration);
				var envioResultado = await envioService.EnviarDocumentoPorIdAsync(idRdaFinal);

				await objHistorialServicios.Agregar(new THISTORIAL()
				{
					FECHA = DateTime.Now.Date,
					HORA = DateTime.Now.TimeOfDay,
					USUARIO = "",
					DESCRIPCION =
						$"RDA intento de envío. " +
						$"IdRda={idRdaFinal}, " +
						$"Ok={(envioResultado.Ok ? "SI" : "NO")}, " +
						$"CodigoHttp={envioResultado.CodigoHttp}, " +
						$"Mensaje={envioResultado.Mensaje}"
				});
			}
		}
		catch (Exception ex)
		{
			var objHistorialServicios = new THISTORIALServicios();
			await objHistorialServicios.Agregar(new THISTORIAL()
			{
				FECHA = DateTime.Now.Date,
				HORA = DateTime.Now.TimeOfDay,
				USUARIO = "",
				DESCRIPCION = "Error procesando RDA desde RIPS: " + ex.Message
			});
		}
	}




	public async Task BorrarDatosAgenda(string clientId, string datosAgenda)
    {

    }



    private async Task<string> RetornarFotoEnBase64ConPrefijo(int idFirma, int tipo)
    {
        var archivosHelper = new ArchivosHelper();
        var objFirma = new TFIRMAServicios();
        var resultadoFirma = await objFirma.ConsultarPorId(idFirma);
        if (tipo == 1)
        {
            var recorteFirmaPaciente = archivosHelper.recortarImganFromBytes(resultadoFirma.FIRMA, new Rectangle(0, 0, 1364, 225));
            var imagenReducida = archivosHelper.ReducirTamañoImagen(recorteFirmaPaciente, 30, 10);
            return archivosHelper.obtenerBase64ConPrefijo(imagenReducida);
        }
        else
        {
            var recorteFirmaDoctor = archivosHelper.recortarImganFromBytes(resultadoFirma.FIRMA, new Rectangle(0, (482 - 215), 1364, 215));
            var imagenReducida = archivosHelper.ReducirTamañoImagen(recorteFirmaDoctor, 30, 10);
            return archivosHelper.obtenerBase64ConPrefijo(imagenReducida);
        }   
       
    }


    private void AutenticarPinDeRydent(string pin)
    {
        //var resultadoAnamenseis = await objAname.ConsultarPorId(8703);
        //resultadoAnamenseis.NOMBRES = "JUAN PEREZ1";
        //await objAname.Editar(8703, resultadoAnamenseis);
    }

    private void ProcessResults<T>(List<T>  results)
    {
        foreach (var obj in results)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        }
    }

    private void ProcessResults<T>(T results)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(results));
    }

    public class TimeSpanConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
        }

        public override object? ReadJson(
            Newtonsoft.Json.JsonReader reader,
            Type objectType,
            object? existingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.Value == null)
                return objectType == typeof(TimeSpan) ? TimeSpan.Zero : null;

            if (reader.Value is DateTime dt)
                return dt.TimeOfDay;

            if (reader.Value is TimeSpan ts)
                return ts;

            if (reader.Value is string s && TimeSpan.TryParse(s, out var parsed))
                return parsed;

            // fallback
            return objectType == typeof(TimeSpan) ? TimeSpan.Zero : null;
        }

        public override void WriteJson(
            Newtonsoft.Json.JsonWriter writer,
            object? value,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value == null) { writer.WriteNull(); return; }
            writer.WriteValue(((TimeSpan)value).ToString());
        }
    }


	public static class JsonHelper
	{
		public static readonly JsonSerializerOptions Options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
		{
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true
		};
	}

}

