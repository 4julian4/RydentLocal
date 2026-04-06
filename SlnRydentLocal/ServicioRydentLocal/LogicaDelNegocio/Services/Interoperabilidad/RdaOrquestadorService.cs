using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using ServicioRydentLocal.LogicaDelNegocio.Servicios.Interoperabilidad;
using System;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaOrquestadorService : IRdaOrquestadorService
	{
		private readonly IConfiguration _configuration;

		public RdaOrquestadorService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<RdaProcesoResultado> GenerarPacientePorAnamnesisAsync(
			int idAnamnesis,
			int? idDoctor = null,
			DateTime? fechaReferencia = null,
			bool intentarEnvioAutomatico = true)
		{
			try
			{
				if (idAnamnesis <= 0)
				{
					return new RdaProcesoResultado
					{
						Ok = false,
						Estado = "ERROR_CONTEXTO",
						Mensaje = "IdAnamnesis inválido."
					};
				}

				var context = new RdaGeneracionContext
				{
					IdAnamnesis = idAnamnesis,
					IdDoctor = idDoctor ?? 0,
					FechaConsulta = fechaReferencia ?? DateTime.Now.Date
				};

				var documentoInternoService = new RdaDocumentoInternoService();
				var documentoInterno = await documentoInternoService.ConstruirDesdeContexto(context);
				documentoInterno.TipoDocumento = "RDA_PACIENTE_INTERNO";

				var jsonInterno = RdaJsonSerializer.Serialize(documentoInterno);
				var bundleFhir = RdaFhirPacienteMapper.Map(documentoInterno);
				var jsonFhir = RdaJsonSerializer.Serialize(bundleFhir);

				var validacion = RdaFhirPacienteValidator.Validate(bundleFhir);
				if (!validacion.Ok)
				{
					return new RdaProcesoResultado
					{
						Ok = false,
						Estado = "ERROR_FHIR",
						Mensaje = validacion.Error
					};
				}

				var options = new RdaOptions();
				_configuration.GetSection("Interoperabilidad:Rda").Bind(options);

				var snapshotInterno = options.GuardarSnapshot ? jsonInterno : null;

				var encounterDocumento = documentoInterno.Documento?.Consulta?.Encounter ?? new RdaEncounterSource();
				var prestadorDocumento = documentoInterno.Documento?.Prestador ?? new RdaPrestadorSource();

				var objRdaDocumentoServicios = new TRDADOCUMENTOServicios();
				var existente = await objRdaDocumentoServicios.ConsultarPacienteExistente(idAnamnesis);

				int idRdaFinal;

				if (existente.ID > 0)
				{
					existente.IDEVOLUCION = null;
					existente.IDINFORMACIONREPORTE = prestadorDocumento.IdInformacionReporte;
					existente.FECHA_ATENCION = encounterDocumento.FechaConsulta ?? DateTime.Now.Date;
					existente.HORA_ATENCION = encounterDocumento.HoraConsulta;
					existente.FACTURA = encounterDocumento.Factura;
					existente.TIPO_DOCUMENTO = "RDA_PACIENTE_INTERNO";
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
						return new RdaProcesoResultado
						{
							Ok = false,
							Estado = "ERROR_BD",
							Mensaje = "No fue posible actualizar TRDA_DOCUMENTO paciente."
						};
					}

					idRdaFinal = existente.ID;
				}
				else
				{
					var entidadRda = new TRDA_DOCUMENTO
					{
						IDANAMNESIS = idAnamnesis,
						IDEVOLUCION = null,
						IDINFORMACIONREPORTE = prestadorDocumento.IdInformacionReporte,
						FECHA_ATENCION = encounterDocumento.FechaConsulta ?? DateTime.Now.Date,
						HORA_ATENCION = encounterDocumento.HoraConsulta,
						FACTURA = encounterDocumento.Factura,
						TIPO_DOCUMENTO = "RDA_PACIENTE_INTERNO",
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
						return new RdaProcesoResultado
						{
							Ok = false,
							Estado = "ERROR_BD",
							Mensaje = "No fue posible guardar TRDA_DOCUMENTO paciente."
						};
					}
				}

				if (options.Enabled && options.EnviarAutomaticamente && intentarEnvioAutomatico && idRdaFinal > 0)
				{
					var envio = await EnviarPorIdAsync(idRdaFinal);
					return new RdaProcesoResultado
					{
						Ok = envio.Ok,
						IdRda = idRdaFinal,
						Estado = envio.Ok ? "ENVIADO" : "ERROR_ENVIO",
						Mensaje = envio.Mensaje
					};
				}

				return new RdaProcesoResultado
				{
					Ok = true,
					IdRda = idRdaFinal,
					Estado = "GENERADO",
					Mensaje = "RDA paciente generado correctamente."
				};
			}
			catch (Exception ex)
			{
				return new RdaProcesoResultado
				{
					Ok = false,
					Estado = "ERROR",
					Mensaje = ex.Message
				};
			}
		}

		public async Task<RdaProcesoResultado> GenerarDesdeRipsAsync(DatosGuardarRips objRips, bool intentarEnvioAutomatico = true)
		{
			try
			{
				var rdaContext = RdaGeneracionContextMapper.FromDatosGuardarRips(objRips);
				var validacionRda = RdaGeneracionContextValidator.Validate(rdaContext);

				if (!validacionRda.Ok)
				{
					await RdaHistorialLogger.LogAsync(
						"RDA_GENERACION_ERROR",
						RdaHistorialLogger.TipoError,
						rdaContext?.IdAnamnesis ?? 0,
						null,
						"ERROR_CONTEXTO",
						"RDA_ORQUESTADOR",
						validacionRda.Error);

					return new RdaProcesoResultado
					{
						Ok = false,
						Mensaje = validacionRda.Error,
						Estado = "ERROR_CONTEXTO"
					};
				}

				var documentoInternoService = new RdaDocumentoInternoService();
				var documentoInterno = await documentoInternoService.ConstruirDesdeContexto(rdaContext);

				var consultaDocumento = documentoInterno.Documento?.Consulta ?? new RdaConsultaSource();
				var encounterDocumento = consultaDocumento.Encounter ?? new RdaEncounterSource();
				var prestadorDocumento = documentoInterno.Documento?.Prestador ?? new RdaPrestadorSource();

				var jsonRdaInterno = RdaJsonSerializer.Serialize(documentoInterno);
				var bundleFhir = RdaFhirConsultaMapper.Map(documentoInterno);
				var jsonFhir = RdaJsonSerializer.Serialize(bundleFhir);

				var validacionFhir = RdaFhirConsultaValidator.Validate(bundleFhir);
				if (!validacionFhir.Ok)
				{
					await RdaHistorialLogger.LogAsync(
						"RDA_FHIR_ERROR",
						RdaHistorialLogger.TipoError,
						rdaContext.IdAnamnesis,
						null,
						"ERROR_FHIR",
						"RDA_FHIR_VALIDATOR",
						validacionFhir.Error);

					return new RdaProcesoResultado
					{
						Ok = false,
						Mensaje = validacionFhir.Error,
						Estado = "ERROR_FHIR"
					};
				}

				var options = new RdaOptions();
				_configuration.GetSection("Interoperabilidad:Rda").Bind(options);

				var snapshotInterno = options.GuardarSnapshot ? jsonRdaInterno : null;
				var objRdaDocumentoServicios = new TRDADOCUMENTOServicios();

				var idEvolucionRda = encounterDocumento.IdEvolucion > 0
					? encounterDocumento.IdEvolucion
					: (int?)null;

				var existente = await objRdaDocumentoServicios.ConsultarConsultaExistente(
					rdaContext.IdAnamnesis,
					encounterDocumento.FechaConsulta,
					encounterDocumento.HoraConsulta,
					documentoInterno.TipoDocumento);

				int idRdaFinal;

				if (existente.ID > 0)
				{
					existente.IDEVOLUCION = idEvolucionRda;
					existente.IDINFORMACIONREPORTE = prestadorDocumento.IdInformacionReporte;
					existente.FECHA_ATENCION = encounterDocumento.FechaConsulta;
					existente.HORA_ATENCION = encounterDocumento.HoraConsulta;
					existente.FACTURA = encounterDocumento.Factura;
					existente.TIPO_DOCUMENTO = documentoInterno.TipoDocumento;
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
						return new RdaProcesoResultado
						{
							Ok = false,
							Mensaje = "No fue posible actualizar TRDA_DOCUMENTO.",
							Estado = "ERROR_BD"
						};
					}

					idRdaFinal = existente.ID;
				}
				else
				{
					var entidadRda = new TRDA_DOCUMENTO
					{
						IDANAMNESIS = rdaContext.IdAnamnesis,
						IDEVOLUCION = idEvolucionRda,
						IDINFORMACIONREPORTE = prestadorDocumento.IdInformacionReporte,
						FECHA_ATENCION = encounterDocumento.FechaConsulta,
						HORA_ATENCION = encounterDocumento.HoraConsulta,
						FACTURA = encounterDocumento.Factura,
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
						return new RdaProcesoResultado
						{
							Ok = false,
							Mensaje = "No fue posible guardar TRDA_DOCUMENTO.",
							Estado = "ERROR_BD"
						};
					}
				}

				if (options.Enabled && options.EnviarAutomaticamente && intentarEnvioAutomatico && idRdaFinal > 0)
				{
					var envio = await EnviarPorIdAsync(idRdaFinal);
					return new RdaProcesoResultado
					{
						Ok = envio.Ok,
						IdRda = idRdaFinal,
						Estado = envio.Ok ? "ENVIADO" : "ERROR_ENVIO",
						Mensaje = envio.Mensaje
					};
				}

				return new RdaProcesoResultado
				{
					Ok = true,
					IdRda = idRdaFinal,
					Estado = "GENERADO",
					Mensaje = "RDA generado correctamente."
				};
			}
			catch (Exception ex)
			{
				await RdaHistorialLogger.LogAsync(
					"RDA_GENERACION_ERROR",
					RdaHistorialLogger.TipoError,
					objRips?.IDANAMNESIS ?? 0,
					null,
					"ERROR",
					"RDA_ORQUESTADOR",
					ex.Message);

				return new RdaProcesoResultado
				{
					Ok = false,
					Mensaje = ex.Message,
					Estado = "ERROR"
				};
			}
		}

		public async Task<RdaProcesoResultado> EnviarPorIdAsync(int idRda)
		{
			var objRdaDocumentoServicios = new TRDADOCUMENTOServicios();
			var doc = await objRdaDocumentoServicios.ConsultarPorId(idRda);

			if (doc.ID <= 0)
			{
				return new RdaProcesoResultado
				{
					Ok = false,
					IdRda = idRda,
					Estado = "ERROR",
					Mensaje = "No existe el documento RDA."
				};
			}

			var envioService = new RdaEnvioService(_configuration);
			var envio = await envioService.EnviarDocumentoPorIdAsync(idRda);

			var docActualizado = await objRdaDocumentoServicios.ConsultarPorId(idRda);
			var estadoFinal = docActualizado.ID > 0
				? (docActualizado.ESTADO ?? (envio.Ok ? "ENVIADO" : "ERROR_ENVIO"))
				: (envio.Ok ? "ENVIADO" : "ERROR_ENVIO");

			return new RdaProcesoResultado
			{
				Ok = envio.Ok,
				IdRda = idRda,
				Estado = estadoFinal,
				Mensaje = envio.Mensaje
			};
		}

		public async Task<RdaProcesoResultado> RegenerarPorIdAsync(int idRda, bool intentarEnvioAutomatico = false)
		{
			var objRdaDocumentoServicios = new TRDADOCUMENTOServicios();
			var doc = await objRdaDocumentoServicios.ConsultarPorId(idRda);

			if (doc.ID <= 0)
			{
				return new RdaProcesoResultado
				{
					Ok = false,
					IdRda = idRda,
					Estado = "ERROR",
					Mensaje = "No existe el documento RDA."
				};
			}

			if (string.Equals(doc.ESTADO, "ENVIADO", StringComparison.OrdinalIgnoreCase))
			{
				return new RdaProcesoResultado
				{
					Ok = false,
					IdRda = idRda,
					Estado = "ENVIADO",
					Mensaje = "No se puede regenerar un RDA que ya fue enviado."
				};
			}

			if (string.Equals(doc.TIPO_DOCUMENTO, "RDA_PACIENTE_INTERNO", StringComparison.OrdinalIgnoreCase))
				return await RegenerarPacienteDesdeSnapshotAsync(doc, intentarEnvioAutomatico);

			var rebuildService = new RdaRipsRebuildService();

			var datosRips = await rebuildService.ReconstruirExactoAsync(
				doc.IDANAMNESIS,
				doc.FECHA_ATENCION,
				doc.HORA_ATENCION,
				!string.IsNullOrWhiteSpace(doc.FACTURA)
					? doc.FACTURA
					: (doc.JSON_SNAPSHOTstr != null ? ExtraerFacturaDesdeSnapshot(doc.JSON_SNAPSHOTstr) : null),
				doc.IDEVOLUCION);
			/*var datosRips = await rebuildService.ReconstruirDesdeAnamnesisAsync(
				doc.IDANAMNESIS,
				doc.JSON_SNAPSHOTstr != null ? ExtraerFacturaDesdeSnapshot(doc.JSON_SNAPSHOTstr) : null);*/

			if (datosRips == null)
			{
				return new RdaProcesoResultado
				{
					Ok = false,
					IdRda = idRda,
					Estado = "ERROR",
					Mensaje = "No se encontró base RIPS suficiente para regenerar el RDA."
				};
			}

			if (doc.HORA_ATENCION.HasValue)
				datosRips.HORALOTE = doc.HORA_ATENCION;

			if (!string.IsNullOrWhiteSpace(doc.FACTURA))
				datosRips.FACTURA = doc.FACTURA;

			return await GenerarDesdeRipsAsync(datosRips, intentarEnvioAutomatico);
		}

		private async Task<RdaProcesoResultado> RegenerarPacienteDesdeSnapshotAsync(
			TRDA_DOCUMENTO doc,
			bool intentarEnvioAutomatico)
		{
			var snapshotReader = new RdaPacienteSnapshotReader();
			var snapshot = snapshotReader.Leer(doc.JSON_SNAPSHOTstr);

			if (snapshot == null)
			{
				return new RdaProcesoResultado
				{
					Ok = false,
					IdRda = doc.ID,
					Estado = "ERROR",
					Mensaje = "No se pudo leer JSON_SNAPSHOT para regenerar RDA paciente."
				};
			}

			var idDoctor = snapshot.Documento?.Prestador?.IdDoctor;
			var fechaAtencion =
				snapshot.Documento?.Consulta?.Encounter?.FechaConsulta?.DateTime
				?? doc.FECHA_ATENCION;

			if (!idDoctor.HasValue || idDoctor.Value <= 0)
			{
				return new RdaProcesoResultado
				{
					Ok = false,
					IdRda = doc.ID,
					Estado = "ERROR",
					Mensaje = "JSON_SNAPSHOT no contiene idDoctor válido para regenerar RDA paciente."
				};
			}

			return await GenerarPacientePorAnamnesisAsync(
				doc.IDANAMNESIS,
				idDoctor,
				fechaAtencion,
				intentarEnvioAutomatico);
		}

		public async Task<int> EnviarPendientesAsync(int maxRegistros = 50)
		{
			var objRdaDocumentoServicios = new TRDADOCUMENTOServicios();
			var pendientes = await objRdaDocumentoServicios.ConsultarPendientesEnvio(maxRegistros);

			var enviadosOk = 0;

			if (pendientes != null)
			{
				foreach (var item in pendientes)
				{
					if (item == null || item.ID <= 0)
						continue;

					if (string.Equals(item.ESTADO, "NO_REINTENTAR", StringComparison.OrdinalIgnoreCase))
						continue;

					if (string.Equals(item.ESTADO, "ENVIADO", StringComparison.OrdinalIgnoreCase))
						continue;

					var resultado = await EnviarPorIdAsync(item.ID);
					if (resultado.Ok)
						enviadosOk++;
				}
			}

			return enviadosOk;
		}

		private static string? ExtraerFacturaDesdeSnapshot(string? jsonSnapshot)
		{
			if (string.IsNullOrWhiteSpace(jsonSnapshot))
				return null;

			try
			{
				var doc = JsonConvert.DeserializeObject<RdaDocumentoInterno>(jsonSnapshot);
				return doc?.Documento?.Consulta?.Encounter?.Factura;
			}
			catch
			{
				return null;
			}
		}
	}

	public interface IRdaOrquestadorService
	{
		Task<RdaProcesoResultado> GenerarPacientePorAnamnesisAsync(int idAnamnesis, int? idDoctor = null, DateTime? fechaReferencia = null, bool intentarEnvioAutomatico = true);
		Task<RdaProcesoResultado> GenerarDesdeRipsAsync(DatosGuardarRips objRips, bool intentarEnvioAutomatico = true);
		Task<RdaProcesoResultado> EnviarPorIdAsync(int idRda);
		Task<RdaProcesoResultado> RegenerarPorIdAsync(int idRda, bool intentarEnvioAutomatico = false);
		Task<int> EnviarPendientesAsync(int maxRegistros = 50);
	}
}