using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
	[Table("TDATOSDOCTORES")]
	public class TDATOSDOCTORES
	{
		[Key]
		public int ID { set; get; }
		public string? NOMBRE { set; get; }
		public string? TELEFONO { set; get; }
		public string? DIRECCION { set; get; }
		public string? ESPECIALIDAD { set; get; }
		public string? OBSERVACIONES { set; get; }
		public string? CELULAR { set; get; }
		public string? CLAVE { set; get; }
		public string? TP { set; get; }
		public string? CODIGO_DOCTOR { set; get; }
		public string? RUTA_IMAGEN { set; get; }
		public int? IDREPORTE { set; get; }
		public int? IDESPECIALIDAD { set; get; }
		public string? COLOR { set; get; }
		public int? IDPERFIL { set; get; }
		public string? USUARIO { set; get; }
		public string? CENTROCOSTOS { set; get; }
		public string? ESTADO { set; get; }
		public int? PORCENTAJE { set; get; }

		public string? TIPO_ID { set; get; }
		public string? NUMERO_DOCUMENTO { set; get; }
		public int? CODSERVICIO { set; get; }
	}
}