using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
	[Table("TCODIGOS_CONSLUTAS_TEMP")]
	public class TCODIGOS_CONSLUTAS_TEMP
	{
		[Key]
		[Column("CODIGO")]
		[StringLength(8)]
		public string CODIGO { get; set; } = string.Empty;

		[Column("NOMBRE")]
		[StringLength(300)]
		public string? NOMBRE { get; set; }

		[Column("COSTO")]
		public int? COSTO { get; set; }
	}
}