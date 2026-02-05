using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Facturatech;
using ServicioRydentLocal.LogicaDelNegocio.Helpers;
using ServicioRydentLocal.LogicaDelNegocio.Repositorio;
using ServicioRydentLocal.LogicaDelNegocio.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Facturatech
{
    public class LNDianGeneral
    {
        public MemoryStream FacturaXId(int id)
        {
            using (var dbContext = new AppDbContext())
            {
                var _Adicionales_Abonos_Dian = new Adicionales_Abonos_Dian().listarXid(id);
                //if (!_Adicionales_Abonos_Dian.Any())
				if (!(_Adicionales_Abonos_Dian.Count() > 0))
				{
                    return null;
                }
                var P_AdicionalAbono = _Adicionales_Abonos_Dian.FirstOrDefault();
                var factura = P_AdicionalAbono.factura ?? "";
                var prefijo = P_AdicionalAbono.prefijo ?? "";
                if (string.IsNullOrEmpty(factura))
                {
                    return null;
                }
                return new FacturaTec().ConsultarFactura(prefijo, factura.Replace(prefijo, ""), P_AdicionalAbono);
            }
        }


        

       /* public string FacturaXMLBase64XId(int id)
        {
            using (var dbContext = new AppDbContext())
            {
                var P_AdicionalAbono = new Adicionales_Abonos_Dian()
                                            .listarXid(id)
                                            .FirstOrDefault();

                if (P_AdicionalAbono == null || string.IsNullOrEmpty(P_AdicionalAbono.factura))
                {
                    return null;
                }

                return new FacturaTec().DescargarXMLFactura(
                    P_AdicionalAbono.prefijo ?? "",
                    P_AdicionalAbono.factura.Replace(P_AdicionalAbono.prefijo ?? "", ""),
                    P_AdicionalAbono
                );
            }
        }*/


		public string FacturaXMLBase64XId(int id)
		{
			var _Adicionales_Abonos_Dian = new Adicionales_Abonos_Dian().listarXid(id);
			if (!(_Adicionales_Abonos_Dian.Count() > 0))
			{
				return null;
			}
			var P_AdicionalAbono = _Adicionales_Abonos_Dian.FirstOrDefault();
			var factura = P_AdicionalAbono.factura ?? "";
			var prefijo = P_AdicionalAbono.prefijo ?? "";
			if (string.IsNullOrEmpty(factura))
			{
				return null;
			}
			return new FacturaTec().DescargarXMLFactura(prefijo, factura.Replace(prefijo, ""), P_AdicionalAbono);
		}

		public string FacturaBase64XId(int id)
		{
			var _Adicionales_Abonos_Dian = new Adicionales_Abonos_Dian().listarXid(id);
			if (!(_Adicionales_Abonos_Dian.Count() > 0))
			{
				return null;
			}
			var P_AdicionalAbono = _Adicionales_Abonos_Dian.FirstOrDefault();
			var factura = P_AdicionalAbono.factura ?? "";
			var prefijo = P_AdicionalAbono.prefijo ?? "";
			if (string.IsNullOrEmpty(factura))
			{
				return null;
			}
			return new FacturaTec().DescargarBase64Factura(prefijo, factura.Replace(prefijo, ""), P_AdicionalAbono);
		}

		public MemoryStream FacturaCXCXId(int id)
		{
			var _Adicionales_Abonos_Dian = new Adicionales_Abonos_Dian().listarCXCXid(id);
			if (!(_Adicionales_Abonos_Dian.Count() > 0))
			{
				return null;
			}
			var P_AdicionalAbono = _Adicionales_Abonos_Dian.FirstOrDefault();
			//var xml = new FacturaTec().formatoFactura(adicionalAbono);
			var factura = P_AdicionalAbono.factura ?? "";
			var prefijo = P_AdicionalAbono.prefijo ?? "";
			if (string.IsNullOrEmpty(factura))
			{
				return null;
			}
			//var _Adicionales_AbonosEditar = new LNAdicionales_Abonos().Consultar(id);
			//var _Resolucion_Dian_Otros = new RepResolucion_Dian_Otros().Obtener(_Adicionales_AbonosEditar.idResoliucionDian);
			return new FacturaTec().ConsultarFactura(prefijo, factura.Replace(prefijo, ""), P_AdicionalAbono);
		}

		public NegMensaje GenerarFactura(P_Adicionales_Abonos_Dian adicionalAbonoAbonoConsulta, List<T_ADICIONALES_ABONOS_MOTIVOS> lstDetalles)
		{
			NegMensaje mensaje = new NegMensaje();
			if (string.IsNullOrEmpty(adicionalAbonoAbonoConsulta.transaccionId) && adicionalAbonoAbonoConsulta.idResoliucionDian > 0)
			{
				var objFacturatecLN = new FacturaTec();
				//var _Resolucion_Dian_Otros = new RepResolucion_Dian_Otros().Obtener(adicionalAbonoAbonoConsulta.idResoliucionDian);
				var xml = objFacturatecLN.formatoFactura(adicionalAbonoAbonoConsulta, lstDetalles);
				mensaje = objFacturatecLN.GenerarFactura(xml, adicionalAbonoAbonoConsulta);
				mensaje.id = adicionalAbonoAbonoConsulta.idRelacion.ToString();
				if (mensaje.error != "")
				{
					return mensaje;
				}
				if (mensaje.Status == Enumeraciones.ResupestasFacturatech.Estados.Firmada)
				{
					new LNAdicionales_Abonos().EditarTransaccionId(adicionalAbonoAbonoConsulta.idRelacion, mensaje.transaccionID);
				}
			}
			return mensaje;
		}

		public MemoryStream FacturaXML(int id)
		{
			var objAADian = new LNAdicionales_Abonos_Dian();

			var _Adicionales_AbonosEditar = new LNAdicionales_Abonos().Consultar(id);
			var listDetalleAbono = objAADian.ListAdicionalesAbonosMotivosXIdRelacion(id);

			var _Adicionales_Abonos_Dian = new LNAdicionales_Abonos_Dian().listarXid(id);
			var adicionalAbonoDianConsulta = _Adicionales_Abonos_Dian.FirstOrDefault();
			var factura = adicionalAbonoDianConsulta.factura ?? "";
			var prefijo = adicionalAbonoDianConsulta.prefijo ?? "";
			if (string.IsNullOrEmpty(factura))
			{
				return null;
			}
			if (_Adicionales_AbonosEditar.idResoliucionDian > 0)
			{
				MemoryStream archivoStream = new MemoryStream();
				string base64Result = Convert.ToBase64String(new FacturaTec().formatoFactura(adicionalAbonoDianConsulta, listDetalleAbono));
				var s = TransferenciaArchivos.Base64ToStream(base64Result);
				s.CopyTo(archivoStream);

				archivoStream.Flush(); //Always catches me out
				archivoStream.Position = 0; //Not sure if this is required
				return archivoStream;
			}
			else
			{
				return null;
			}
		}


		public MemoryStream FacturaCXCXML(int id)
		{

			var objAADian = new LNAdicionales_Abonos_Dian();
			var listDetalleAbono = objAADian.ListAdicionalesAbonosMotivosXIdRelacion(0);
			var _Adicionales_Abonos_Dian = new LNAdicionales_Abonos_Dian().listarCXCXid(id);
			var adicionalAbonoDianConsulta = _Adicionales_Abonos_Dian.FirstOrDefault();
			var factura = adicionalAbonoDianConsulta.factura ?? "";
			var prefijo = adicionalAbonoDianConsulta.prefijo ?? "";
			if (string.IsNullOrEmpty(factura))
			{
				return null;
			}
			if (adicionalAbonoDianConsulta.idResoliucionDian > 0)
			{
				MemoryStream archivoStream = new MemoryStream();
				string base64Result = Convert.ToBase64String(new FacturaTec().formatoFactura(adicionalAbonoDianConsulta, listDetalleAbono));
				var s = TransferenciaArchivos.Base64ToStream(base64Result);
				s.CopyTo(archivoStream);

				archivoStream.Flush(); //Always catches me out
				archivoStream.Position = 0; //Not sure if this is required
				return archivoStream;
			}
			else
			{
				return null;
			}
		}

		public InfoFactura GenerarFactura(int id)
		{
			InfoFactura infofac = new InfoFactura();
			//var _Adicionales_AbonosEditar = new LNAdicionales_Abonos().Consultar(id);
			var objAADian = new LNAdicionales_Abonos_Dian();


			var _Adicionales_Abonos_Dian = objAADian.listarXid(id);
			var listDetalleAbono = objAADian.ListAdicionalesAbonosMotivosXIdRelacion(id);

			var P_AdicionalAbonoDianConsulta = _Adicionales_Abonos_Dian.FirstOrDefault();
			var factura = P_AdicionalAbonoDianConsulta.factura ?? "";
			var prefijo = P_AdicionalAbonoDianConsulta.prefijo ?? "";
			if (string.IsNullOrEmpty(factura))
			{
				infofac.Mensaje.error = "No hay Factura asociada";
				return infofac;
			}

			infofac.Mensaje = GenerarFactura(P_AdicionalAbonoDianConsulta, listDetalleAbono);

			if (!string.IsNullOrEmpty(infofac.Mensaje.error))
			{
				return infofac;
			}

			infofac.PDFStream = new FacturaTec().ConsultarFactura(prefijo, factura.Replace(prefijo, ""), P_AdicionalAbonoDianConsulta);
			if (infofac.PDFStream != null)
			{
				return infofac;
			}
			else
			{
				infofac.Mensaje.error = "La factura está pendiente de aprobación";
				return infofac;
			}

		}




		public InfoFactura GenerarFacturaCXC(int id)
		{
			InfoFactura infofac = new InfoFactura();
			//var _Adicionales_AbonosEditar = new LNAdicionales_Abonos().Consultar(id);

			var _Adicionales_Abonos_Dian = new LNAdicionales_Abonos_Dian().listarCXCXid(id);
			var P_AdicionalAbonoDianConsulta = _Adicionales_Abonos_Dian.FirstOrDefault();
			var factura = P_AdicionalAbonoDianConsulta.factura ?? "";
			var prefijo = P_AdicionalAbonoDianConsulta.prefijo ?? "";
			if (string.IsNullOrEmpty(factura))
			{
				infofac.Mensaje.error = "No hay Factura asociada";
				return infofac;
			}
			infofac.Mensaje = GenerarFacturaCXC(P_AdicionalAbonoDianConsulta);
			if (!string.IsNullOrEmpty(infofac.Mensaje.error))
			{
				return infofac;
			}

			infofac.PDFStream = new FacturaTec().ConsultarFactura(prefijo, factura.Replace(prefijo, ""), P_AdicionalAbonoDianConsulta);
			if (infofac.PDFStream != null)
			{
				return infofac;
			}
			else
			{
				infofac.Mensaje.error = "La factura está pendiente de aprobación";
				return infofac;
			}

		}
		public NegMensaje GenerarFacturaCXC(P_Adicionales_Abonos_Dian adicionalAbonoAbonoConsulta)
		{
			NegMensaje mensaje = new NegMensaje();
			if (string.IsNullOrEmpty(adicionalAbonoAbonoConsulta.transaccionId) && adicionalAbonoAbonoConsulta.idResoliucionDian > 0)
			{
				var objAADian = new LNAdicionales_Abonos_Dian();
				var listDetalleAbono = objAADian.ListAdicionalesAbonosMotivosXIdRelacion(0);
				var xml = new FacturaTec().formatoFactura(adicionalAbonoAbonoConsulta, listDetalleAbono);
				mensaje = new FacturaTec().GenerarFacturaCXC(xml, adicionalAbonoAbonoConsulta);
				mensaje.id = adicionalAbonoAbonoConsulta.idRelacion.ToString();
				if (mensaje.error != "")
				{
					return mensaje;
				}
				if (mensaje.Status == Enumeraciones.ResupestasFacturatech.Estados.Firmada)
				{
					new LNAdicionales_Abonos().EditarCXCTransaccionId(adicionalAbonoAbonoConsulta.idRelacion, mensaje.transaccionID);
				}
			}
			return mensaje;
		}

		public string FacturaXMLBase64XId_FacturaTec(int id)
		{
			var adicionales = new Adicionales_Abonos_Dian().listarXid(id);
			if (!(adicionales.Count() > 0)) return null;

			var p = adicionales.FirstOrDefault();
			var factura = p.factura ?? "";
			var prefijo = p.prefijo ?? "";

			if (string.IsNullOrEmpty(factura)) return null;

			return new FacturaTec().DescargarXMLFactura(prefijo, factura.Replace(prefijo, ""), p);
		}

		// Reutilizamos un HttpClient único (importante si vas a pedir muchos XML)
		private static readonly HttpClient _http = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(60)
		};

		/// <summary>
		/// Descarga el XML de la factura desde DATAICO (vía API intermedia)
		/// y lo retorna como Base64 para anexarlo en xmlFevFile.
		/// 
		/// Requiere:
		/// - T_ADICIONALES_ABONOS.TRANSACCIONID = UUID Dataico
		/// - TINFORMACIONREPORTES.CODIGO_PRESTADOR_PPAL (tenantCode)
		/// - Variable de entorno: API_INTERMEDIA_BASE_URL (ej: https://localhost:7226)
		/// </summary>
		public string? FacturaXMLBase64XId_Dataico(int id)
		{
			try
			{
				using (var dbContext = new AppDbContext())
				{
					var abono = dbContext.T_ADICIONALES_ABONOS
						.AsNoTracking()
						.Where(x => x.IDRELACION == id)
						.OrderByDescending(x => x.FECHA)
						.ThenByDescending(x => x.HORA)
						.FirstOrDefault();

					if (abono == null) return null;

					var uuid = (abono.TRANSACCIONID ?? string.Empty).Trim();
					if (string.IsNullOrWhiteSpace(uuid)) return null;

					var doctorId = abono.RECIBIDO_POR.HasValue ? abono.RECIBIDO_POR.Value : abono.IDDOCTOR;
					if (!doctorId.HasValue) return null;

					var idReporte = dbContext.TDATOSDOCTORES
						.AsNoTracking()
						.Where(d => d.ID == doctorId.Value)
						.Select(d => d.IDREPORTE)
						.FirstOrDefault();

					if (idReporte <= 0) return null;

					var infoReporte = dbContext.TINFORMACIONREPORTES
						.AsNoTracking()
						.FirstOrDefault(r => r.ID == idReporte);

					if (infoReporte == null) return null;

					var tenantCode = (infoReporte.CODIGO_PRESTADOR_PPAL ?? infoReporte.CODIGO_PRESTADOR ?? string.Empty).Trim();
					if (string.IsNullOrWhiteSpace(tenantCode)) return null;

					var baseUrl = GetApiIntermediaBaseUrl();
					if (string.IsNullOrWhiteSpace(baseUrl)) return null;

					// baseUrl ya viene normalizado a ".../api"
					var url = $"{baseUrl.TrimEnd('/')}/fes/documents/{Uri.EscapeDataString(uuid)}/xml";

					using var req = new HttpRequestMessage(HttpMethod.Get, url);
					req.Headers.TryAddWithoutValidation("X-Tenant-Code", tenantCode);

					using var res = _http.Send(req); // si tu _http es HttpClient y esto compila en tu versión
					if (!res.IsSuccessStatusCode) return null;

					var bytes = res.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
					if (bytes == null || bytes.Length == 0) return null;

					return Convert.ToBase64String(bytes);
				}
			}
			catch
			{
				return null;
			}
		}


		private static string? GetApiIntermediaBaseUrl()
		{
			// 1) Prioridad: variable de entorno
			var env = Environment.GetEnvironmentVariable("API_INTERMEDIA_BASE_URL");
			if (!string.IsNullOrWhiteSpace(env))
				return NormalizeApiBase(env);

			env = Environment.GetEnvironmentVariable("API_INTERMEDIA_URL");
			if (!string.IsNullOrWhiteSpace(env))
				return NormalizeApiBase(env);

			// 2) Fallback: appsettings.json del worker
			//    (en un Worker, AppContext.BaseDirectory suele ser donde queda el exe + appsettings)
			var environment =
				Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
				?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

			var cfg = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
				.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
				.AddEnvironmentVariables()
				.Build();

			var fromSettings = cfg["ApiIntermedia:BaseUrl"]; // <- tu "https://localhost:7226/api"
			if (!string.IsNullOrWhiteSpace(fromSettings))
				return NormalizeApiBase(fromSettings);

			return null;
		}

		private static string NormalizeApiBase(string baseUrl)
		{
			baseUrl = baseUrl.Trim().TrimEnd('/');

			// Queremos que la base final quede ".../api" (una sola vez)
			if (baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
				return baseUrl;

			return baseUrl + "/api";
		}


	}
}
