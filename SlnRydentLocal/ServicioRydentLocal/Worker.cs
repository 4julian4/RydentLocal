
using AutoMapper.Execution;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Helpers;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;
using ServicioRydentLocal.LogicaDelNegocio.Services;
using ServicioRydentLocal.LogicaDelNegocio.Services.TAnamnesis;
using SixLabors.ImageSharp;
using System.Data;
using System.Globalization;
using System.Security.Principal;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private HubConnection _hubConnection;
    private readonly IConfiguration _configuration;

    // private readonly AppDbContext _dbContext;
    
    private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1); //esto es reciente 16/08/24





    // Constructor: Recibe una instancia de ILogger para realizar el registro de eventos.
    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        string url = _configuration.GetValue<string>("signalRServer:url");
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1) })
            .Build();

        _hubConnection.Reconnecting += error =>
        {
            _logger.LogWarning($"Reconnecting due to: {error?.Message}");  // Log de advertencia cuando se intenta reconectar
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            _logger.LogInformation($"Reconnected. Connection ID: {connectionId}");  // Log de informaci�n cuando la reconexi�n es exitosa
            return Task.CompletedTask;
        };

        _hubConnection.Closed += async error =>
        {
            _logger.LogError($"Connection closed due to: {error?.Message}");  // Log de error cuando la conexi�n se cierra
            await Task.Delay(TimeSpan.FromSeconds(15));  // Espera 15 segundos antes de intentar reconectar
            await StartConnectionAsync();  // Intenta reconectar
        };
    }

    // M�todo para iniciar la conexi�n a SignalR
    private async Task StartConnectionAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            try
            {
                await _hubConnection.StartAsync();
                _logger.LogInformation("Connected to SignalR hub.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error connecting to SignalR hub: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(15));
            }
        }
    }
   
    private async Task registrarSuscripciones()
    {
        _hubConnection.On<string, string>("ObtenerPin", async (clientId, pin) =>
        {
            await RecibirPinRydent(pin, clientId);
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

        _hubConnection.On<string, string>("GuardarDatosRips", async (clientId, datosRips) =>
        {
            await GuardarDatosRips(clientId, datosRips);
        });

        _hubConnection.On<string>("ObtenerCodigosEps", async (clientId) =>
        {
            await ObtenerCodigosEps(clientId);
        });

        _hubConnection.On<string, DateTime, DateTime>("ObtenerDatosAdministrativos", async (clientId, fechaInicio, fechaFin) =>
        {
            await ObtenerDatosAdministrativos(clientId, fechaInicio, fechaFin);
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
    }
    //----------------------Pasos:
    //1. Conectar con el servicio de SignalR usando la funcion ConnectToServer 
    //2. Recibir el pin de acceso de Rydent se usa el evento _hubConnection.On<string, string>("ObtenerPin"
    //3. Autenticar el pin de acceso de Rydent se usando await RecibirPinRydent(pin, clientId);
    //4. Enviar el pin de acceso de Rydent al servidor de Rydent
    // M�todo para conectar al servidor y registrar el equipo
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
                    await RegisterDeviceAsync(datosClientes.ENTRADA);
                }
                else
                {
                    _logger.LogWarning("No se pudo registrar el equipo porque la conexi�n a SignalR no est� establecida.");
                    await AttemptReconnectAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la conexi�n al servidor: {ex.Message}", ex);
        }
    }

    private async Task RegisterDeviceAsync(string entrada)
    {
        try
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("RegistrarEquipo", _hubConnection.ConnectionId, entrada);
                _logger.LogInformation("Equipo registrado con �xito.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al registrar el equipo: {ex.Message}", ex);
        }
    }

    private async Task AttemptReconnectAsync()
    {
        try
        {
            _logger.LogInformation("Intentando reconectar a SignalR...");
            await _hubConnection.StartAsync();
            _logger.LogInformation("Reconexi�n exitosa.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error durante la reconexi�n a SignalR: {ex.Message}", ex);
        }
    }


    // M�todo principal que se ejecuta en segundo plano
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartConnectionAsync();
        await registrarSuscripciones();

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Iniciando la consulta a la tabla");

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    if (_hubConnection.State != HubConnectionState.Connected)
                    {
                        await StartConnectionAsync();  // Intenta reconectar si la conexi�n est� ca�da
                    }

                    await ConnectToServer(scope.ServiceProvider);  // Conecta al servidor y registra el equipo

                    _logger.LogInformation("Consulta completada correctamente.");
                }
                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operaci�n cancelada.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error durante la ejecuci�n: {ex.Message}");
            }
        }
    }



    public async Task RecibirPinRydent(string pinacceso, string clientId)
    {
        var respuestaPin = new RespuestaPinModel();
       
        var objPINACCESO = new TCLAVEServicios();
        var objDOCTORES = new TDATOSDOCTORESServicios();
        var objEPS = new TCODIGOS_EPSServicios();
        var objPROCEDIMIENTOS = new TCODIGOS_PROCEDIMIENTOSServicios();
        var objCONSULTAS = new TCODIGOS_CONSLUTASServicios();
        var objDepartamentos = new TCODIGOS_DEPARTAMENTOServicios();
        var objCiudades = new TCODIGOS_CIUDADServicios();
        var objFrasesXEvolucion = new T_FRASE_XEVOLUCIONServicios();
        var objHorariosAgenda = new THORARIOSAGENDAServicios();
        var objHorariosAsuntos = new THORARIOSASUNTOSServicios();
        var objFestivos = new TFESTIVOSServicios();
        var objConfiguracionesRydent = new TCONFIGURACIONES_RYDENTServicios();
        var objTAnamnesisParaAgendayBuscadores = new TANAMNESISServicios();
        respuestaPin.clave = await objPINACCESO.ConsultarPorId(pinacceso);
        //esto se hace para evitar enviar la clave en el objeto respuestaPin el cual quedaria expuesto en el navegador web
        if (respuestaPin.clave.CLAVE != null)
        {
            respuestaPin.clave.CLAVE = "";
            respuestaPin.acceso = true;
            var listDoctores = await objDOCTORES.ConsultarTodos();
            var listEps = await objEPS.ConsultarTodos();
            var listProcedimientos = await objPROCEDIMIENTOS.ConsultarTodos();
            var listConsultas = await objCONSULTAS.ConsultarTodos();
            var listDepartamentos = await objDepartamentos.ConsultarTodos();
            var listCiudades = await objCiudades.ConsultarTodos();
            var lstFrasesXEvolucion = await objFrasesXEvolucion.ConsultarTodos();
            var listHorariosAgenda = await objHorariosAgenda.ConsultarTodos();
            var listHorariosAsuntos = await objHorariosAsuntos.ConsultarTodos();
            var listFestivos = await objFestivos.ConsultarTodos();
            var listConfiguracionesRydent = await objConfiguracionesRydent.ConsultarTodos();
            var listAnamnesisParaAgendayBuscadores = await objTAnamnesisParaAgendayBuscadores.ConsultarDatosPacientesParaCargarEnAgenda();
            if (listDoctores != null && listDoctores.Count() > 0)
            {
                respuestaPin.lstEps = listEps;
                respuestaPin.lstProcedimientos = listProcedimientos;
                respuestaPin.lstConsultas = listConsultas;
                respuestaPin.lstDepartamentos = listDepartamentos;
                respuestaPin.lstCiudades = listCiudades;
                respuestaPin.lstDoctores = listDoctores.ConvertAll(item => new ListadoItemModel() { id = item.ID.ToString(), nombre = (item.NOMBRE ?? "") });
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

    public async Task ConsultarPorIdDoctor(string clientId, int idDoctor)
    {
        var objDOCTORES = new TDATOSDOCTORESServicios();
        var objAname = new TANAMNESISServicios();
        var respuestaObtenerDoctor = new RespuestaObtenerDoctorModel();
        respuestaObtenerDoctor.doctor = await objDOCTORES.ConsultarPorId(idDoctor);
        respuestaObtenerDoctor.totalPacientes = await objAname.ConsultarTotalPacientesPorDoctor(idDoctor);
        await _hubConnection.InvokeAsync("RespuestaObtenerDoctor", clientId, JsonConvert.SerializeObject(respuestaObtenerDoctor));
    }

    public async Task ObtenerDoctorSiLoCambian(string clientId, int idDoctor)
    {
        var objDOCTORES = new TDATOSDOCTORESServicios();
        var objAname = new TANAMNESISServicios();
        var respuestaObtenerDoctor = new RespuestaObtenerDoctorModel();
        respuestaObtenerDoctor.doctor = await objDOCTORES.ConsultarPorId(idDoctor);
        respuestaObtenerDoctor.totalPacientes = await objAname.ConsultarTotalPacientesPorDoctor(idDoctor);
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

    public async Task ObtenerDatosAdministrativos (string clientId, DateTime fechaInicio, DateTime fechaFin)
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
        await _hubConnection.InvokeAsync("RespuestaObtenerDatosAdministrativos", clientId, JsonConvert.SerializeObject(respuestaDatosAdministrativos));
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




    public async Task ObtenerDatosEvolucion(string clientId, int idAnanesis)
    {
        var objEvolucion = new TEVOLUCIONServicios();
        var listEvolucion = await objEvolucion.ConsultarPorAnamnesis(idAnanesis);
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
        var objRips = JsonConvert.DeserializeObject<DatosGuardarRips>(datosRips);
        var objRipsDxServicios = new T_RIPS_DXServicios();
        var objRipsProcedimientosServicios = new T_RIPS_PROCEDIMIENTOSServicios();
        var datosGuardarRipsDx = new T_RIPS_DX();
        var objTAnamnesisServicios = new TANAMNESISServicios();
        var objTAnamnesis = new TANAMNESIS();
        objTAnamnesis = await objTAnamnesisServicios.ConsultarPorId(objRips.IDANAMNESIS ?? 0);
        
        var codigoEntidad = objRips.CODIGOENTIDAD;
        if (objRips.CODIGOENTIDAD == "")
        {
            codigoEntidad = "000000";
        }
        datosGuardarRipsDx.CODIGOENTIDAD = codigoEntidad;
        datosGuardarRipsDx.IDANAMNESIS = objRips.IDANAMNESIS;
        //ojo falta validar lo de la factura
        if (objRips.FACTURA == "AUTO")
        {
            using (var _dbcontext = new AppDbContext())
            {
                objRips.FACTURA = await _dbcontext.GEN_CONSECUTIVO_RIPS();
            }
        }
        
        datosGuardarRipsDx.FACTURA = string.IsNullOrEmpty(objRips.FACTURA) ? "PENDIENTE": objRips.FACTURA;
        datosGuardarRipsDx.IDDOCTOR = objRips.IDDOCTOR;

        var objTINFORMACIONREPORTESServicios = new TINFORMACIONREPORTESServicios();
        var codigoPrestador = await objTINFORMACIONREPORTESServicios.ConsultarCodigoPrestador(objRips.IDDOCTOR ?? 0);
        datosGuardarRipsDx.CODIGOPRESTADOR = string.IsNullOrEmpty(codigoPrestador) ? "000000" : codigoPrestador;
        
        
        datosGuardarRipsDx.TIPOIDENTIFICACION = objTAnamnesis.DOCUMENTO_IDENTIDAD;
        datosGuardarRipsDx.IDENTIFICACION = objTAnamnesis.CEDULA_NUMERO;
        datosGuardarRipsDx.FECHACONSULTA = objRips.FECHACONSULTA;
        datosGuardarRipsDx.NUMEROAUTORIZACION = objRips.NUMEROAUTORIZACION;
        datosGuardarRipsDx.CODIGOCONSULTA = objRips.CODIGOCONSULTA;
        datosGuardarRipsDx.FINALIDADCONSULTA = objRips.FINALIDADCONSULTA;
        datosGuardarRipsDx.CAUSAEXTERNA = objRips.CAUSAEXTERNA;
        datosGuardarRipsDx.DX1 = objRips.CODIGODIAGNOSTICOPRINCIPAL;
        datosGuardarRipsDx.TIPODIAGNOSTICO = objRips.TIPODIAGNOSTICO;
        datosGuardarRipsDx.VALORCONSULTA = objRips.VALORCONSULTA;
        datosGuardarRipsDx.VALORCUOTAMODERADORA = objRips.VALORCUOTAMODERADORA;
        datosGuardarRipsDx.VALORNETO = objRips.VALORNETO;

        var resultadoDx = await objRipsDxServicios.Agregar(datosGuardarRipsDx);
        var resultado = resultadoDx;
        if (resultadoDx)
        {
            var objHistorialServicios = new THISTORIALServicios();
            var mensaje = "Se agrega dx RIPS CORTO al paciente " + objTAnamnesis.IDANAMNESIS_TEXTO + " - " + objTAnamnesis.NOMBRE_PACIENTE;
            await objHistorialServicios.Agregar(new THISTORIAL() { FECHA = DateTime.Now.Date, HORA = DateTime.Now.TimeOfDay, USUARIO = "",DESCRIPCION = mensaje });
        }
        if (objRips.CODIGOPROCEDIMIENTO != null)
        {
            var datosGuardarRipsProcedimientos = new T_RIPS_PROCEDIMIENTOS();
            datosGuardarRipsProcedimientos.IDANAMNESIS = objRips.IDANAMNESIS;
            datosGuardarRipsProcedimientos.FACTURA = objRips.FACTURA;
            datosGuardarRipsProcedimientos.CODIGOENTIDAD = codigoEntidad;
            datosGuardarRipsProcedimientos.IDDOCTOR = objRips.IDDOCTOR;
            datosGuardarRipsProcedimientos.CODIGOPRESTADOR = codigoPrestador;
            datosGuardarRipsProcedimientos.TIPOIDENTIFICACION = objTAnamnesis.DOCUMENTO_IDENTIDAD;
            datosGuardarRipsProcedimientos.IDENTIFICACION = objTAnamnesis.CEDULA_NUMERO;
            datosGuardarRipsProcedimientos.FECHAPROCEDIMIENTO = objRips.FECHACONSULTA;
            datosGuardarRipsProcedimientos.NUMEROAUTORIZACION = objRips.NUMEROAUTORIZACION;
            datosGuardarRipsProcedimientos.CODIGOPROCEDIMIENTO = objRips.CODIGOPROCEDIMIENTO;
            datosGuardarRipsProcedimientos.FINALIDADPROCEDIMIENTI = objRips.FINALIDADPROCEDIMIENTI;
            datosGuardarRipsProcedimientos.AMBITOREALIZACION = objRips.AMBITOREALIZACION;
            datosGuardarRipsProcedimientos.PERSONALQUEATIENDE = objRips.PERSONALQUEATIENDE;
            datosGuardarRipsProcedimientos.DXPRINCIPAL = objRips.DXPRINCIPAL;
            datosGuardarRipsProcedimientos.DXRELACIONADO = objRips.DXRELACIONADO;
            datosGuardarRipsProcedimientos.COMPLICACION = objRips.COMPLICACION;
            datosGuardarRipsProcedimientos.FORMAREALIZACIONACTOQUIR = objRips.FORMAREALIZACIONACTOQUIR;
            datosGuardarRipsProcedimientos.VALORPROCEDIMIENTO = objRips.VALORPROCEDIMIENTO;
            datosGuardarRipsProcedimientos.EXTRANJERO = objRips.EXTRANJERO;
            datosGuardarRipsProcedimientos.PAIS = objRips.PAIS;
            var resultadoProcedimientos = await objRipsProcedimientosServicios.Agregar(datosGuardarRipsProcedimientos);
            if (resultadoProcedimientos)
            {
                var objHistorialServicios = new THISTORIALServicios();
                var mensaje = "Se agrega Procedimiento RIPS CORTO al paciente  " + objTAnamnesis.IDANAMNESIS_TEXTO + " - " + objTAnamnesis.NOMBRE_PACIENTE;
                await objHistorialServicios.Agregar(new THISTORIAL() { FECHA = DateTime.Now.Date, HORA = DateTime.Now.TimeOfDay, USUARIO = "", DESCRIPCION = mensaje });
            }
            resultado = resultadoProcedimientos;
        }
        await _hubConnection.InvokeAsync("RespuestaGuardarDatosRips", clientId, resultado);
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

        // Agrega la duraci�n a la hora de la cita
        TimeSpan horaFinal = horaCita.Add(TimeSpan.FromMinutes(duracion));

        // Convierte la hora final de vuelta a una cadena de hora
        string horaFinalString = horaFinal.ToString(@"hh\:mm");

        return horaFinalString;
    }
    public async Task RealizarAccionesEnCitaAgendada(string clientId, string datosRealizarAccionAgenda)
    {
        var objHistorial = new THISTORIALServicios();
        var objDetallesCitasServicios = new TDETALLECITASServicios();
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
            


            else if (objDatosRealizarAccionCitaAgendada.Any(x => x.tipoAccion == "RECORDARCITA"))
            {
                var enviarMensajeRecordarCita = objDatosRealizarAccionCitaAgendada.First(x => x.tipoAccion == "RECORDARCITA");

                if (enviarMensajeRecordarCita.aceptado)
                {
                    var servicioWhatsApp = new WhatsAppService();

                    try
                    {
                        // Consultar las citas de forma as�ncrona
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

                            // Validar si ya se envi� el mensaje (COLOR ya es "VERDE")
                            if (cita.CEDULA == "SI")
                            {
                                Console.WriteLine($"Mensaje ya enviado para la cita con ID: {cita.ID}. Se omite el env�o.");
                                continue; // Pasar a la siguiente cita
                            }

                            var haciaNumero = ValidarYAgregarPrefijo(cita.TELEFONO);
                            if (string.IsNullOrWhiteSpace(haciaNumero))
                            {
                                haciaNumero = ValidarYAgregarPrefijo(cita.CELULAR);
                            }

                            var templateNombre = "mi_plantilla";
                            var parametros = new List<string>
                            {
                                cita.NOMBRE,
                                cita.FECHA?.ToString("dd/MM/yyyy"), // Formato d�a/mes/a�o
                                cita.HORA?.ToString(@"hh\:mm")      // Formato de hora "hh:mm"
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

                                    // Actualizar el campo COLOR a "VERDE"
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
            }

        }
        var respuestaSerializada = JsonConvert.SerializeObject(respuesta);
        var respuestaSerializadaComprimida = ArchivosHelper.CompressString(respuestaSerializada);
        await _hubConnection.InvokeAsync("RespuestaRealizarAccionesEnCitaAgendada", clientId, respuestaSerializadaComprimida);
    }



    public static string ValidarYAgregarPrefijo(string numero)
    {
        // Define la expresi�n regular para n�meros celulares v�lidos en Colombia (10 d�gitos y empiezan con 3)
        var regex = new Regex(@"^3\d{9}$");

        // Remueve espacios o caracteres adicionales
        numero = numero.Trim().Replace(" ", "").Replace("-", "");

        // Caso 1: Si ya tiene el prefijo +57
        if (numero.StartsWith("+57"))
        {
            // Verifica que el resto del n�mero sea v�lido
            var numeroSinPrefijo = numero.Substring(3); // Remueve +57
            return regex.IsMatch(numeroSinPrefijo) ? numero : null;
        }
        // Caso 2: Si empieza con 57 pero no tiene el "+"
        else if (numero.StartsWith("57"))
        {
            var numeroSinPrefijo = numero.Substring(2); // Remueve 57
            if (regex.IsMatch(numeroSinPrefijo))
            {
                // Agrega el prefijo "+" si es v�lido
                return $"+{numero}";
            }
        }
        // Caso 3: N�mero sin prefijo 57
        else if (regex.IsMatch(numero))
        {
            // Agrega el prefijo +57 si es v�lido
            return $"+57{numero}";
        }

        // Si no es v�lido, retorna null
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
                                mensaje = "El paciente ya tiene cita asignada el dia "+ fecha.Value.Date.ToString("dd/MM/yyyy") + " desea continuar asignando �sta cita?",
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

                var templateNombre = "mi_plantilla";
                var parametros = new List<string>
                {
                    objAgenda.lstDetallaCitas[0].NOMBRE,
                    objAgenda.lstDetallaCitas[0].FECHA?.ToString("dd/MM/yyyy"), // Formato d�a/mes/a�o
                    objAgenda.lstDetallaCitas[0].HORA?.ToString(@"hh\:mm")      // Formato de hora "hh:mm"
                };

                if (!string.IsNullOrWhiteSpace(haciaNumero))
                {
                    var resultado = await servicioWhatsApp.EnviarMensajeCitaAgenda(haciaNumero, templateNombre, parametros);

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
            var imagenReducida = archivosHelper.ReducirTama�oImagen(recorteFirmaPaciente, 30, 10);
            return archivosHelper.obtenerBase64ConPrefijo(imagenReducida);
        }
        else
        {
            var recorteFirmaDoctor = archivosHelper.recortarImganFromBytes(resultadoFirma.FIRMA, new Rectangle(0, (482 - 215), 1364, 215));
            var imagenReducida = archivosHelper.ReducirTama�oImagen(recorteFirmaDoctor, 30, 10);
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

    public class TimeSpanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            TimeSpan tiempo;
            if (reader.Value == null)
            {
                return null;
            }
            else if (reader.Value is DateTime)
            {
                var dateTime = (DateTime)reader.Value;
                return dateTime.TimeOfDay;
            }
            else if (reader.Value is TimeSpan)
            {
                return (TimeSpan)reader.Value;
            }
            else if (TimeSpan.TryParse((string)reader.Value, out tiempo))
            {
                return tiempo;
            }
            else
            {
                var dateTime = (DateTime)reader.Value;
                return dateTime.TimeOfDay;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var timeSpan = (TimeSpan)value;
            writer.WriteValue(timeSpan.ToString());
        }
    }

}

