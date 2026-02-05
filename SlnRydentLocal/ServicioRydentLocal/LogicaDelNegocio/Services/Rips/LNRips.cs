using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Facturatech;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace ServicioRydentLocal.LogicaDelNegocio.Services.Rips
{
    public class LNRips
    {
        private string urlApiBase;
        private readonly IConfiguration _configuration;
        protected readonly AppDbContext _dbcontext;
        public LNRips(IConfiguration configuration)
        {
            _configuration = configuration;
            urlApiBase = _configuration.GetValue<string>("RipsDocker:urlApiBase");
        }
        public List<TDATOSDOCTORES> ListarDoctores()
        {
            using (var _dbcontext = new AppDbContext())
            {
                return _dbcontext.TDATOSDOCTORES.ToList();
            }
        }
        public List<TINFORMACIONREPORTES> ListarInformacionReportes()
        {
            using (var _dbcontext = new AppDbContext())
            {
                return _dbcontext.TINFORMACIONREPORTES.ToList();
            }
        }
        public TINFORMACIONREPORTES InformacionReportesXId(int id)
        {
            using (var _dbcontext = new AppDbContext())
            {
                return _dbcontext.TINFORMACIONREPORTES.Where(x => x.ID == id).FirstOrDefault();
            }
        }

        public async Task<TINFORMACIONREPORTES> InformacionReportesXIdAsync(int id)
        {
            using (var _dbcontext = new AppDbContext())
            {
                return await _dbcontext.TINFORMACIONREPORTES
                                       .Where(x => x.ID == id)
                                       .FirstOrDefaultAsync();
            }
        }

        public string obtenerTokenRips(TINFORMACIONREPORTES infoReportes)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            HttpClient client = new HttpClient(handler);
            string res = string.Empty;
            string url = $"{urlApiBase}/auth/LoginSISPRO";
            var objConsultar = new SolicitarTokenModel();
            objConsultar.persona.identificacion.tipo = infoReportes.TIPO_ID;
            objConsultar.persona.identificacion.numero = infoReportes.IDSISPRO;
            objConsultar.nit = infoReportes.NUMDOCUMENTOIDOBLIGADO;
            objConsultar.clave = infoReportes.PASSISPRO;
            string json = System.Text.Json.JsonSerializer.Serialize(objConsultar);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = client.PostAsync(url, content);
            response.Wait();
            var respuesta = response.Result;
            if (respuesta.IsSuccessStatusCode)
            {
                var resBody = respuesta.Content.ReadAsStringAsync();
                resBody.Wait();
                if (!string.IsNullOrEmpty(resBody.Result))
                {
                    RespuestaSolicitarTokenModel resultado = JsonSerializer.Deserialize<RespuestaSolicitarTokenModel>(resBody.Result);
                    return resultado.token;
                }
            }
            return res;
        }

		/*public async Task<List<RespuestaCargarRipsModel>> CargarRipsSinFacturaAsync(
	        List<transaccionModel> lstRips,
	        string token,
	        bool conFactura,
	        Func<ProgresoRipsDto, Task>? reportProgress = null,
	        CancellationToken ct = default)
		{
			var respuestaLista = new List<RespuestaCargarRipsModel>();

			var handler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
			};

			using var client = new HttpClient(handler);

			var url = conFactura
				? $"{urlApiBase}/PaquetesFevRips/CargarFevRips"
				: $"{urlApiBase}/PaquetesFevRips/CargarRipsSinFactura";

			client.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

			// ✅ Para no estar creando esto por cada item
			var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

			// ✅ Si conFactura, crea el helper una sola vez
			LNDianGeneral? fac = conFactura ? new LNDianGeneral() : null;

			int total = lstRips?.Count ?? 0;
			int procesadas = 0;
			int ok = 0;
			int fail = 0;

			// ✅ Frecuencia “segura” para SignalR (por cantidad)
			int cada = total <= 200 ? 10 : total <= 1000 ? 25 : 50;

			// ✅ Frecuencia “segura” por TIEMPO (evita spam si todo responde muy rápido)
			var sw = Stopwatch.StartNew();
			long lastMs = -999999;

			async Task EmitirProgresoAsync(string? ultimoDoc, string mensaje, bool force = false)
			{
				if (reportProgress == null) return;

				// manda si:
				// - force (inicio/fin)
				// - o cada N items
				// - o si pasó >= 700ms desde el último envío
				var nowMs = sw.ElapsedMilliseconds;
				bool porTiempo = (nowMs - lastMs) >= 700;
				bool porCantidad = (procesadas == 1) || (procesadas % cada == 0) || (procesadas == total);

				if (force || porCantidad || porTiempo)
				{
					lastMs = nowMs;

					var payload = new ProgresoRipsDto
					{
						Accion = "PRESENTAR",
						Total = total,
						Procesadas = procesadas,
						Exitosas = ok,
						Fallidas = fail,
						UltimoDocumento = ultimoDoc,
						Mensaje = mensaje
					};

					await reportProgress(payload);
				}
			}

			// ✅ Progreso inicial
			await EmitirProgresoAsync(null, "Iniciando presentación...", force: true);

			foreach (var objRIPS in lstRips)
			{
				ct.ThrowIfCancellationRequested();
				procesadas++;

				// Para mostrar “último doc”
				var ultimoDoc = objRIPS?.rips?.numFactura ?? objRIPS?.rips?.numNota;

				try
				{
					if (!conFactura)
					{
						objRIPS.rips.numNota = objRIPS.rips.numFactura;
						objRIPS.rips.tipoNota = "RS";
						objRIPS.rips.numFactura = null;
					}
					else
					{
						// ⚠️ OJO: esto puede ser pesado si el XML se consulta por red por cada item.
						// Si FacturaBase64XId hace HTTP, considera cache o reuso interno.
						objRIPS.xmlFevFile = fac!.FacturaBase64XId(objRIPS.rips.idRelacion ?? 0);
					}

					string json = System.Text.Json.JsonSerializer.Serialize(objRIPS);
					using var content = new StringContent(json, Encoding.UTF8, "application/json");

					using var httpResp = await client.PostAsync(url, content, ct);
					var body = await httpResp.Content.ReadAsStringAsync(ct);

					if (!string.IsNullOrWhiteSpace(body))
					{
						var resultado = JsonSerializer.Deserialize<RespuestaCargarRipsModel>(body, opcionesJson);

						if (resultado != null)
						{
							respuestaLista.Add(resultado);

							// Cuenta OK / FAIL con tolerancia
							if (resultado.resultState == true) ok++;
							else fail++;
						}
						else
						{
							fail++;
						}
					}
					else
					{
						fail++;
					}
				}
				catch
				{
					fail++;
					// opcional: podrías agregar un “resultado” de error para que el frontend lo vea en el JSON final
				}

				// ✅ Progreso en caliente
				await EmitirProgresoAsync(ultimoDoc, "Enviando a SISPRO...");
			}

			// ✅ Progreso final
			await EmitirProgresoAsync(null, "Finalizando...", force: true);

			return respuestaLista;
		}*/

		// ✅ CARGAR/PRESENTAR: aquí se toma la decisión FACTURATEC vs DATAICO usando proveedorFe
		//    + progreso real (cargando XML / enviando) sin estallar SignalR (throttle por cantidad y tiempo).
		public async Task<List<RespuestaCargarRipsModel>> CargarRipsSinFacturaAsync(
			List<transaccionModel> lstRips,
			string token,
			bool conFactura,
			string proveedorFe,
			Func<ProgresoRipsDto, Task>? reportProgress = null,
			CancellationToken ct = default)
		{
			var respuestaLista = new List<RespuestaCargarRipsModel>();

			var handler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
			};

			using var client = new HttpClient(handler);

			var url = conFactura
				? $"{urlApiBase}/PaquetesFevRips/CargarFevRips"
				: $"{urlApiBase}/PaquetesFevRips/CargarRipsSinFactura";

			client.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

			var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

			// ✅ Helper una sola vez si se requiere factura
			LNDianGeneral? fac = conFactura ? new LNDianGeneral() : null;

			var proveedor = (proveedorFe ?? "").Trim().ToUpperInvariant();

			int total = lstRips?.Count ?? 0;
			int procesadas = 0;
			int ok = 0;
			int fail = 0;

			// ✅ Frecuencia “segura” para SignalR (por cantidad)
			int cada = total <= 200 ? 10 : total <= 1000 ? 25 : 50;

			// ✅ Frecuencia “segura” por TIEMPO (evita spam si todo responde muy rápido)
			var sw = Stopwatch.StartNew();
			long lastMs = -999999;

			async Task EmitirProgresoAsync(string? ultimoDoc, string mensaje, bool force = false)
			{
				if (reportProgress == null) return;

				var nowMs = sw.ElapsedMilliseconds;
				bool porTiempo = (nowMs - lastMs) >= 700;
				bool porCantidad = (procesadas == 1) || (procesadas % cada == 0) || (procesadas == total);

				if (force || porCantidad || porTiempo)
				{
					lastMs = nowMs;

					var payload = new ProgresoRipsDto
					{
						Accion = "PRESENTAR",
						Total = total,
						Procesadas = procesadas,
						Exitosas = ok,
						Fallidas = fail,
						UltimoDocumento = ultimoDoc,
						Mensaje = mensaje
					};

					await reportProgress(payload);
				}
			}

			// ✅ Progreso inicial
			await EmitirProgresoAsync(null, "Iniciando presentación...", force: true);

			foreach (var objRIPS in lstRips)
			{
				ct.ThrowIfCancellationRequested();
				procesadas++;

				var ultimoDoc = objRIPS?.rips?.numFactura ?? objRIPS?.rips?.numNota;

				try
				{
					if (!conFactura)
					{
						objRIPS.rips.numNota = objRIPS.rips.numFactura;
						objRIPS.rips.tipoNota = "RS";
						objRIPS.rips.numFactura = null;
						objRIPS.xmlFevFile = null;
					}
					else
					{
						// ✅ Cargar XML SOLO si hace falta (evita doble carga si ya viene en objRIPS)
						if (string.IsNullOrWhiteSpace(objRIPS.xmlFevFile))
						{
							await EmitirProgresoAsync(ultimoDoc, "Cargando XML de la factura...");

							var idRel = objRIPS.rips.idRelacion ?? 0;

							if (proveedor == "FACTURATEC")
							{
								objRIPS.xmlFevFile = fac!.FacturaXMLBase64XId_FacturaTec(idRel);
							}
							else if (proveedor == "DATAICO")
							{
								objRIPS.xmlFevFile = fac!.FacturaXMLBase64XId_Dataico(idRel);
							}
							else
							{
								objRIPS.xmlFevFile = null;
							}
						}
					}

					// ✅ Enviar
					await EmitirProgresoAsync(ultimoDoc, "Enviando a SISPRO...");

					string json = System.Text.Json.JsonSerializer.Serialize(objRIPS);
					using var content = new StringContent(json, Encoding.UTF8, "application/json");

					using var httpResp = await client.PostAsync(url, content, ct);
					var body = await httpResp.Content.ReadAsStringAsync(ct);

					if (!string.IsNullOrWhiteSpace(body))
					{
						var resultado = JsonSerializer.Deserialize<RespuestaCargarRipsModel>(body, opcionesJson);

						if (resultado != null)
						{
							respuestaLista.Add(resultado);

							if (resultado.resultState == true) ok++;
							else fail++;
						}
						else
						{
							fail++;
						}
					}
					else
					{
						fail++;
					}
				}
				catch
				{
					fail++;
				}

				// ✅ Progreso en caliente (con contadores actualizados)
				await EmitirProgresoAsync(ultimoDoc, $"Procesando... ({procesadas}/{total})");
			}

			// ✅ Progreso final
			await EmitirProgresoAsync(null, "Finalizando...", force: true);

			return respuestaLista;
		}



		public List<RespuestaCargarRipsModel> CargarRipsSinFactura(List<transaccionModel> lstRips, string token, bool conFactura)
        {
            var respuestaCargarRipsSinFactura = new List<RespuestaCargarRipsModel>();
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            HttpClient client = new HttpClient(handler);
            string res = string.Empty;
            string url = conFactura ? $"{urlApiBase}/PaquetesFevRips/CargarFevRips" : $"{urlApiBase}/PaquetesFevRips/CargarRipsSinFactura";
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            foreach (var objRIPS in lstRips)
            {
                if (!conFactura)
                {
                    objRIPS.rips.numNota = objRIPS.rips.numFactura;
                    objRIPS.rips.tipoNota = "RS";
                    objRIPS.rips.numFactura = null;

                }
                else
                {
                    var fac = new LNDianGeneral();
                    //objRIPS.xmlFevFile = fac.FacturaXMLBase64XId(objRIPS.rips.idRelacion ?? 0);
					objRIPS.xmlFevFile = fac.FacturaBase64XId(objRIPS.rips.idRelacion ?? 0);

				}
                string json = System.Text.Json.JsonSerializer.Serialize(objRIPS);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync(url, content);

                response.Wait();
                var respuesta = response.Result;

                var resBody = respuesta.Content.ReadAsStringAsync();
                resBody.Wait();
                if (!string.IsNullOrEmpty(resBody.Result))
                {
                    var opciones = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    RespuestaCargarRipsModel resultado = JsonSerializer.Deserialize<RespuestaCargarRipsModel>(resBody.Result, opciones);

                    respuestaCargarRipsSinFactura.Add(resultado);
                }

            }
            return respuestaCargarRipsSinFactura;
        }


		/* public List<transaccionModel> MapearRipsSinFactura(List<transaccionModel> lstRips, bool conFactura, string proveedorFe)
         {
			var proveedor = (proveedorFe ?? "").Trim().ToUpperInvariant();
			foreach (var objRIPS in lstRips)
             {
                 if (!conFactura)
                 {
                     objRIPS.rips.numNota = objRIPS.rips.numFactura;
                     objRIPS.rips.tipoNota = "RS";
                     objRIPS.rips.numFactura = null;

                 }
                 else
                 {
                     var fac = new LNDianGeneral();
                     objRIPS.xmlFevFile = fac.FacturaXMLBase64XId(objRIPS.rips.idRelacion ?? 0);

                 }

             }
             return lstRips;
         }*/

		public List<transaccionModel> MapearRipsSinFactura(
	        List<transaccionModel> lstRips,
	        bool conFactura,
	        string proveedorFe)
		{
			var proveedor = (proveedorFe ?? "").Trim().ToUpperInvariant();

			// ✅ Instancia una sola vez (no dentro del foreach)
			var fac = new LNDianGeneral();

			foreach (var objRIPS in lstRips)
			{
				if (!conFactura)
				{
					objRIPS.rips.numNota = objRIPS.rips.numFactura;
					objRIPS.rips.tipoNota = "RS";
					objRIPS.rips.numFactura = null;
					objRIPS.xmlFevFile = null; // coherente: no hay factura/FEV
				}
				else
				{
					var idRel = objRIPS.rips.idRelacion ?? 0;

					// ✅ Elegir según proveedor
					if (proveedor == "FACTURATEC")
					{
						objRIPS.xmlFevFile = fac.FacturaXMLBase64XId_FacturaTec(idRel);
					}
					else if (proveedor == "DATAICO")
					{
						objRIPS.xmlFevFile = fac.FacturaXMLBase64XId_Dataico(idRel);
					}
					else
					{
						// si viene vacío o no reconocido
						objRIPS.xmlFevFile = null;
					}
				}
			}

			return lstRips;
		}

		public List<transaccionModel> MapearRipsSinFacturaConProgreso(
			List<transaccionModel> lstRips,
			bool conFactura,
			string proveedorFe,
			Func<ProgresoRipsDto, Task>? reportProgress = null,
			CancellationToken ct = default)
		{
			var proveedor = (proveedorFe ?? "").Trim().ToUpperInvariant();
			var fac = new LNDianGeneral();

			int total = lstRips?.Count ?? 0;
			int procesadas = 0;
			int ok = 0;
			int fail = 0;

			// throttle (igual idea que en Presentar)
			int cada = total <= 200 ? 10 : total <= 1000 ? 25 : 50;

			var sw = Stopwatch.StartNew();
			long lastMs = -999999;

			void Emitir(string? ultimoDoc, string mensaje, bool force = false)
			{
				if (reportProgress == null) return;

				var nowMs = sw.ElapsedMilliseconds;
				bool porTiempo = (nowMs - lastMs) >= 700;
				bool porCantidad = (procesadas == 1) || (procesadas % cada == 0) || (procesadas == total);

				if (force || porCantidad || porTiempo)
				{
					lastMs = nowMs;

					_ = reportProgress(new ProgresoRipsDto
					{
						Accion = "GENERAR",
						Total = total,
						Procesadas = procesadas,
						Exitosas = ok,
						Fallidas = fail,
						UltimoDocumento = ultimoDoc,
						Mensaje = mensaje
					});
				}
			}

			Emitir(null, "Preparando...", force: true);

			foreach (var objRIPS in lstRips)
			{
				ct.ThrowIfCancellationRequested();
				procesadas++;

				var ultimoDoc = objRIPS?.rips?.numFactura ?? objRIPS?.rips?.numNota;

				try
				{
					if (!conFactura)
					{
						objRIPS.rips.numNota = objRIPS.rips.numFactura;
						objRIPS.rips.tipoNota = "RS";
						objRIPS.rips.numFactura = null;
						objRIPS.xmlFevFile = null;

						ok++;
						Emitir(ultimoDoc, "Preparando RIPS sin factura...");
					}
					else
					{
						var idRel = objRIPS.rips.idRelacion ?? 0;

						if (proveedor == "FACTURATEC")
							objRIPS.xmlFevFile = fac.FacturaXMLBase64XId_FacturaTec(idRel);
						else if (proveedor == "DATAICO")
							objRIPS.xmlFevFile = fac.FacturaXMLBase64XId_Dataico(idRel);
						else
							objRIPS.xmlFevFile = null;

						ok++;
						Emitir(ultimoDoc, "Cargando XML de facturas...");
					}
				}
				catch
				{
					fail++;
					objRIPS.xmlFevFile = null;
					Emitir(ultimoDoc, "Error cargando XML (se continúa)...");
				}
			}

			Emitir(null, "Preparación finalizada.", force: true);

			return lstRips;
		}



		public List<transaccionModel> MapearRipsSinFacturaAsync(List<transaccionModel> lstRips, bool conFactura)
        {
            if (conFactura)
            {
                var fac = new LNDianGeneral(); // Instancia única para evitar múltiples creaciones
                foreach (var objRIPS in lstRips)
                {
                    objRIPS.xmlFevFile = fac.FacturaXMLBase64XId(objRIPS.rips.idRelacion ?? 0);
                }
            }
            else
            {
                foreach (var objRIPS in lstRips)
                {
                    objRIPS.rips.numNota = objRIPS.rips.numFactura;
                    objRIPS.rips.tipoNota = "RS";
                    objRIPS.rips.numFactura = null;
                }
            }
            return lstRips;
        }

        /*public async Task<List<transaccionModel>> MapearRipsSinFactura(List<transaccionModel> lstRips, bool conFactura)
        {
            if (conFactura)
            {
                var fac = new LNDianGeneral(); // Instancia única para evitar múltiples creaciones

                // Crear una lista de tareas para descargar todas las facturas en paralelo
                var tareas = lstRips.Select(async objRIPS =>
                {
                    objRIPS.xmlFevFile = await fac.FacturaXMLBase64XId(objRIPS.rips.idRelacion ?? 0);
                }).ToList();

                // Esperar que todas las descargas terminen
                await Task.WhenAll(tareas);
            }
            else
            {
                foreach (var objRIPS in lstRips)
                {
                    objRIPS.rips.numNota = objRIPS.rips.numFactura;
                    objRIPS.rips.tipoNota = "RS";
                    objRIPS.rips.numFactura = null;
                }
            }
            return lstRips;
        }*/







        public List<transaccionModel> ConsultarRips(DateTime FECHAINI, DateTime FECHAFIN, string EPS, string FACTURA, int IDREPORTE, int IDDOCTOR, string EXTRANJERO)
         {
             using (var _dbcontext = new AppDbContext())
             {
                 var lstTransaccion = new List<transaccionModel>();
                 var acModel = _dbcontext.SP_RIPS_JSON_AC(FECHAINI, FECHAFIN, EPS, FACTURA, IDREPORTE, IDDOCTOR, EXTRANJERO);
                 var usModel = _dbcontext.SP_RIPS_JSON_US(FECHAINI, FECHAFIN, EPS, FACTURA, IDREPORTE, IDDOCTOR, EXTRANJERO);

                 var apModel = _dbcontext.SP_RIPS_JSON_AP(FECHAINI, FECHAFIN, EPS, FACTURA, IDREPORTE, IDDOCTOR, EXTRANJERO);
                 var lstFacturas = usModel.Select(x => x.NUMERO_FACTURA).Distinct().ToList();
                 if (usModel != null && usModel.Any() && lstFacturas.Any())
                 {
                     foreach (var factura in lstFacturas.OrderBy(x => x))
                     {

                         var modelo = new transaccionModel();
                         modelo.rips = new transaccionRipsModel();

                         modelo.rips.numFactura = factura;
                         var usMOdelXFactura = usModel.Where(y => y.NUMERO_FACTURA == factura).ToList();
                         modelo.rips.numDocumentoIdObligado = usMOdelXFactura.FirstOrDefault().NUMDOCUMENTOIDOBLIGADO;
                         modelo.rips.idRelacion = usMOdelXFactura.FirstOrDefault().IDRELACION;
                         modelo.rips.usuarios = usMOdelXFactura.ConvertAll(x => new UsuariosModel()
                         {
                             codMunicipioResidencia = x.CODIGO_CIUDAD,
                             codPaisOrigen = x.CODPAISORIGEN,
                             codPaisResidencia = x.CODPAISRESIDENCIA,
                             codSexo = x.SEXO,
                             codZonaTerritorialResidencia = x.ZONA_RESIDENCIAL,
                             consecutivo = x.CONSECUTIVO ?? 0,
                             fechaNacimiento = x.FECHANACIMIENTO,
                             incapacidad = x.INCAPACIDAD,
                             numDocumentoIdentificacion = x.IDANAMNESIS_TEXTO,
                             tipoDocumentoIdentificacion = x.DOCUMENTO_IDENTIDAD,
                             tipoUsuario = x.TIPOUSUARIO,
                             servicios = new ServiciosModel()
                             {
                                // consultas = lstAC(acModel.Where(y => y.NUMERO_FACTURA == factura).OrderBy(x => x.CONSECUTIVO).ToList()),
                                 //procedimientos = lstAP(apModel.Where(y => y.NUMERO_FACTURA == factura).OrderBy(x => x.CONSECUTIVO).ToList())

								 consultas = lstAC(acModel.Where(y => y.NUMERO_FACTURA == factura && y.IDANAMNESIS_TEXTO == x.IDANAMNESIS_TEXTO).OrderBy(x => x.CONSECUTIVO).ToList()),
								 procedimientos = lstAP(apModel.Where(y => y.NUMERO_FACTURA == factura && y.IDANAMNESIS_TEXTO == x.IDANAMNESIS_TEXTO).OrderBy(x => x.CONSECUTIVO).ToList())
							 }
                         });
                         lstTransaccion.Add(modelo);
                     }
					ReasignarConsecutivos(lstTransaccion);
				}
                 return lstTransaccion;
             }

         }

		public void ReasignarConsecutivos(List<transaccionModel> lstTransaccion)
		{

			// Recorrer todas las transacciones
			foreach (var transaccion in lstTransaccion)
			{
				int consecutivoUsuario = 1; // Contador global de usuarios
				if (transaccion.rips?.usuarios != null)
				{
					foreach (var usuario in transaccion.rips.usuarios)
					{
						// Asignar consecutivo global al usuario
						usuario.consecutivo = consecutivoUsuario++;

						// Reasignar consecutivos de consultas si existen
						if (usuario.servicios?.consultas != null)
						{
							int consecutivoConsulta = 1;
							foreach (var consulta in usuario.servicios.consultas)
							{
								consulta.consecutivo = consecutivoConsulta++;
							}
						}

						// Reasignar consecutivos de procedimientos si existen
						if (usuario.servicios?.procedimientos != null)
						{
							int consecutivoProcedimiento = 1;
							foreach (var procedimiento in usuario.servicios.procedimientos)
							{
								procedimiento.consecutivo = consecutivoProcedimiento++;
							}
						}
					}
				}
			}
		}


		private List<ConsultasModel> lstAC(List<SP_RIPS_JSON_ACResult> lstAC)
        {
            var modeloReturn = new List<ConsultasModel>();
            if (lstAC.Any())
            {
                modeloReturn = lstAC.ConvertAll(x => new ConsultasModel()
                {
                    causaMotivoAtencion = x.CODIGO_CAUSA,
                    codConsulta = x.CODIGO_CONSULTA,
                    codDiagnosticoPrincipal = x.DIAGNOSTICO_1 == "" ? null : x.DIAGNOSTICO_1,
                    codDiagnosticoRelacionado1 = x.DIAGNOSTICO_2 == "" ? null : x.DIAGNOSTICO_2,
                    codDiagnosticoRelacionado2 = x.DIAGNOSTICO_3 == "" ? null : x.DIAGNOSTICO_3,
                    codDiagnosticoRelacionado3 = x.DIAGNOSTICO_4 == "" ? null : x.DIAGNOSTICO_4,
                    codPrestador = x.CODIGO_PRESTADOR,
                    codServicio = x.CODSERVICIO != null ? Convert.ToInt32(x.CODSERVICIO ?? "0") : 0,
                    conceptoRecaudo = x.CONCEPTORECAUDO,
                    consecutivo = x.CONSECUTIVO ?? 0,
                    fechaInicioAtencion = (x.FECHA_TRAT ?? DateTime.MinValue).ToString("yyyy-MM-dd HH:mm"),
                    finalidadTecnologiaSalud = x.FINALIDADTECNOLOGIASALUD,
                    grupoServicios = x.GRUPOSERVICIOS,
                    modalidadGrupoServicioTecSal = x.MODALIDADGRUPOSERVICIOTECSAL,
                    numDocumentoIdentificacion = x.IDANAMNESIS_TEXTO,
                    numFEVPagoModerador = x.NUMFEVPAGOMODERADOR,
                    tipoDiagnosticoPrincipal = x.CODIGO_TIPO_DIAGNOSTICO,
                    tipoDocumentoIdentificacion = x.DOCUMENTO_IDENTIDAD,
                    valorPagoModerador = Convert.ToInt64(x.VALOR_CUOTA),
                    vrServicio = Convert.ToInt64(x.VALOR_CONSULTA)
                });

            }
            return modeloReturn;
        }



        private List<ProcedimientosModel> lstAP(List<SP_RIPS_JSON_APResult> lstAP)
        {
            var modeloReturn = new List<ProcedimientosModel>();
            if (lstAP.Any())
            {
                modeloReturn = lstAP.ConvertAll(x => new ProcedimientosModel()
                {
                    codComplicacion = x.DIAGNOSTICO3,
                    codDiagnosticoRelacionado = x.DIAGNOSTICO2,
                    codDiagnosticoPrincipal = x.DIAGNOSTICO1,
                    codProcedimiento = x.CODIGO_PROCEDIMIENTO,
                    idMIPRES = x.IDMIPRES,
                    numAutorizacion = x.NUMERO_AUTORIZACION,
                    codPrestador = x.CODIGO_PRESTADOR,
                    codServicio = x.CODSERVICIO != null ? Convert.ToInt32(x.CODSERVICIO ?? "0") : 0,
                    conceptoRecaudo = x.CONCEPTORECAUDO,
                    consecutivo = x.CONSECUTIVO ?? 0,
                    fechaInicioAtencion = (x.FECHAR_REALIZACION ?? DateTime.MinValue).ToString("yyyy-MM-dd HH:mm"),
                    finalidadTecnologiaSalud = x.FINALIDADTECNOLOGIASALUD,
                    grupoServicios = x.GRUPOSERVICIOS,
                    modalidadGrupoServicioTecSal = x.MODALIDADGRUPOSERVICIOTECSAL,
                    numDocumentoIdentificacion = x.IDANAMNESIS_TEXTO,
                    numFEVPagoModerador = x.NUMFEVPAGOMODERADOR,

                    tipoDocumentoIdentificacion = x.DOCUMENTO_IDENTIDAD,
                    valorPagoModerador = Convert.ToInt64(x.VALORPAGOMODERADOR),
                    viaIngresoServicioSalud = x.VIAINGRESOSERVICIOSALUD,
                    vrServicio = Convert.ToInt64(x.COSTO)
                });

            }
            return modeloReturn;
        }
    }
}
