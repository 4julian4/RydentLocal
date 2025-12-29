using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Facturatech;
using ServicioRydentLocal.LogicaDelNegocio.Helpers;
using ServicioRydentLocal.LogicaDelNegocio.Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	}
}
