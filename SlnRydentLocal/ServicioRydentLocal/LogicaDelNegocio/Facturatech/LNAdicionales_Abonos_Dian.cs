using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Facturatech
{
	public class LNAdicionales_Abonos_Dian
	{
		public List<P_Adicionales_Abonos_Dian> listarXid(int PIDRELACION)
		{
			return new Adicionales_Abonos_Dian().listarXid(PIDRELACION);
		}
		public List<T_ADICIONALES_ABONOS_MOTIVOS> ListAdicionalesAbonosMotivosXIdRelacion(int PIDRELACION)
		{
			return new Adicionales_Abonos_Dian().ListAdicionalesAbonosMotivosXIdRelacion(PIDRELACION);
		}
		public List<P_Adicionales_Abonos_Dian> listarCXCXid(int PIDRELACION)
		{
			return new Adicionales_Abonos_Dian().listarCXCXid(PIDRELACION);
		}
		public List<EntSPDianFacturasPendientes> listarFacturasPendientes()
		{
			return new Adicionales_Abonos_Dian().listarFacturasPendientes();
		}
		public List<EntSPDianFacturasCreadas> ListarFacturasCreadas(string Factura)
		{
			return new Adicionales_Abonos_Dian().ListarFacturasCreadas(Factura);
		}
	}
}
