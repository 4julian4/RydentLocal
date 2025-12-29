using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
	public class EntAdicionales_Abonos
	{
		public int id { set; get; }
		public int fase { set; get; }
		public int tipo { set; get; }
		public decimal valor { set; get; }
		public DateTime fecha { set; get; }
		public string descripcion { set; get; }
		public string recibo { set; get; }
		public int identificador { set; get; }
		public DateTime hora { set; get; }
		public string? idOdontologo { set; get; }
		public int idDoctor { set; get; }
		public int idRelacion { set; get; }
		public int idResoliucionDian { set; get; }
		public int detalle_Tto { set; get; }
		public string factura { set; get; }
		public int recibido_Por { set; get; }
		public int firma { set; get; }
		public int? conIva { set; get; }
		public decimal? valorIva { set; get; }
		public int pagoTercero { set; get; }
		public string nombre_Recibe { set; get; }
		public string codigo_Descripcion { set; get; }
		public string consecutivo_NotaCredito { set; get; }
		public string? nc_Elaborado_Por { set; get; }
		public string? nc_Aprobado_Por { set; get; }
		public string? recibo_Relacionado { set; get; }
		public string? transaccionId { set; get; }
	}
}
