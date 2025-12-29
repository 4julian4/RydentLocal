using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Facturatech
{
	public class LNAdicionales_Abonos
	{
		public bool EditarTransaccionId(int idRelacion, string transaccionId)
		{
			bool respuesta = false;
			respuesta = new RepAdicionales_Abonos().EditarTransaccionId(idRelacion, transaccionId);
			return respuesta;
		}
		public bool EditarCXCTransaccionId(int idRelacion, string transaccionId)
		{
			bool respuesta = false;
			respuesta = new RepAdicionales_Abonos().EditarCXCTransaccionId(idRelacion, transaccionId);
			return respuesta;
		}
		public EntAdicionales_Abonos Consultar(int idRelacion)
		{
			return new RepAdicionales_Abonos().Consultar(idRelacion);
		}
	}
}
