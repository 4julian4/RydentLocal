using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
	public class P_ADICIONALES_ABONOS_MOTIVOS_D_Result
	{
		[Key]
		public int ID { get; set; }
		public int? IDRELACION_OUT { get; set; }
		public string? DESCRIPCION { get; set; }
		public string? CODIGO { get; set; }
		public decimal? VALOR { get; set; }
		public decimal? VALORIVA { get; set; }
		public decimal? PORCENTAJEIVA { get; set; }
		public int? CANTIDAD { get; set; }
	}
}
