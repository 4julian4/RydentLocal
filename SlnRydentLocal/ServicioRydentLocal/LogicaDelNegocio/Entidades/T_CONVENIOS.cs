using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
	[Table("T_CONVENIOS")]
	public class T_CONVENIOS
	{
		[Key]
		[Column("ID")]
		public int ID { get; set; }

		[Column("NOMBRE")]
		[MaxLength(200)]
		public string? NOMBRE { get; set; }

		[Column("DESCUENTO")]
		[MaxLength(20)]
		public string? DESCUENTO { get; set; }

		[Column("FACTURASPENDIENTE")]
		[MaxLength(2)]
		public string? FACTURASPENDIENTE { get; set; }

		[Column("TELEFONO")]
		[MaxLength(100)]
		public string? TELEFONO { get; set; }

		[Column("DIRECCION")]
		[MaxLength(150)]
		public string? DIRECCION { get; set; }

		[Column("CORREO")]
		[MaxLength(150)]
		public string? CORREO { get; set; }

		// Firebird: BLOB SUB_TYPE 1 = texto
		[Column("OBSERVACIONES")]
		public string? OBSERVACIONES { get; set; }

		[Column("CONTACTO")]
		[MaxLength(150)]
		public string? CONTACTO { get; set; }

		[Column("CARGO")]
		[MaxLength(150)]
		public string? CARGO { get; set; }

		[Column("NIT")]
		[MaxLength(100)]
		public string? NIT { get; set; }

		[Column("EMAIL")]
		[MaxLength(100)]
		public string? EMAIL { get; set; }

		[Column("CIUDAD")]
		[MaxLength(100)]
		public string? CIUDAD { get; set; }

		[Column("MISION")]
		[MaxLength(100)]
		public string? MISION { get; set; }

		[Column("NIVELRIESGO")]
		[MaxLength(5)]
		public string? NIVELRIESGO { get; set; }
	}
}



