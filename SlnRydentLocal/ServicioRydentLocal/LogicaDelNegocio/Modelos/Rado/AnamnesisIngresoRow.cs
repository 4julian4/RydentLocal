using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rado
{
	[Keyless]
	public sealed class AnamnesisIngresoRow
	{
		[Column("Numero_documento")]
		public string? Numero_documento { get; set; }

		[Column("Tipo_documento")]
		public string? Tipo_documento { get; set; }

		[Column("PRIMER_NOMBRE")]
		public string? PRIMER_NOMBRE { get; set; }

		[Column("SEGUNDO_NOMBRE")]
		public string? SEGUNDO_NOMBRE { get; set; }

		[Column("PRIMER_APELLIDO")]
		public string? PRIMER_APELLIDO { get; set; }

		[Column("SEGUNDO_APELLIDO")]
		public string? SEGUNDO_APELLIDO { get; set; }

		[Column("APELLIDOS")]
		public string? APELLIDOS { get; set; }

		[Column("Fecha_nacimiento")]
		public string? Fecha_nacimiento { get; set; }

		[Column("Genero")]
		public string? Genero { get; set; }

		[Column("Indicativo_pais")]
		public string? Indicativo_pais { get; set; }

		[Column("Codigo_pais")]
		public string? Codigo_pais { get; set; }

		[Column("Codigo_departamento")]
		public string? Codigo_departamento { get; set; }

		[Column("Codigo_ciudad")]
		public string? Codigo_ciudad { get; set; }

		[Column("direccion")]
		public string? direccion { get; set; }

		[Column("telefono")]
		public string? telefono { get; set; }

		[Column("Correo_electronico")]
		public string? Correo_electronico { get; set; }

		[Column("Personal_codigo")]
		public string? Personal_codigo { get; set; }

		[Column("Personal")]
		public string? Personal { get; set; }

		[Column("ESPECIALIDAD")]
		public string? ESPECIALIDAD { get; set; }

		[Column("Entidad")]
		public string? Entidad { get; set; }

		[Column("Regimen")]
		public string? Regimen { get; set; }

		[Column("CentroAtencion")]
		public string? CentroAtencion { get; set; }

		[Column("Tipo_Estudio")]
		public string? Tipo_Estudio { get; set; }

		[Column("Codigo_Servicio")]
		public string? Codigo_Servicio { get; set; }

		[Column("Servicio_Ips")]
		public string? Servicio_Ips { get; set; }

		[Column("cantidad")]
		public int cantidad { get; set; }

		[Column("Fecha_Solicitud")]
		public string? Fecha_Solicitud { get; set; }

		[Column("Id_Orden")]
		public string? Id_Orden { get; set; }   // ✅ cambiado a string

		[Column("Id_paciente")]
		public int? Id_paciente { get; set; }

		[Column("Id_ingreso")]
		public long? Id_ingreso { get; set; }   // ✅ recomendado por si IDRELACION es grande

		[Column("ingreso")]
		public decimal? ingreso { get; set; }
	}
}
