using System;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaEncounterSource
	{
		public int IdAnamnesis { get; set; }
		public int IdEvolucion { get; set; }

		public DateTime? FechaConsulta { get; set; }
		public TimeSpan? HoraConsulta { get; set; }

		public string? TipoDocumento { get; set; }
		public string? NumeroDocumento { get; set; }

		public string? Nombres { get; set; }
		public string? Apellidos { get; set; }
		public DateTime? FechaNacimiento { get; set; }
		public string? Sexo { get; set; }

		public string? Direccion { get; set; }
		public string? Telefono { get; set; }
		public string? Celular { get; set; }

		public string? CodigoCiudad { get; set; }
		public string? CodigoDepartamento { get; set; }
		public string? NombreCiudad { get; set; }
		public string? NombreDepartamento { get; set; }
		public string? ZonaRecidencial { get; set; }

		public string? CodigoEps { get; set; }
		public string? NombreEps { get; set; }

		public string? NroAfiliacion { get; set; }
		public string? TipoUsuario { get; set; }

		public string? Doctor { get; set; }

		public string? CodigoConsulta { get; set; }
		public string? NombreConsulta { get; set; }

		public string? CodigoDiagnosticoPrincipal { get; set; }
		public string? NombreDiagnosticoPrincipal { get; set; }

		public string? CodigoProcedimiento { get; set; }
		public string? NombreProcedimiento { get; set; }

		public string? Factura { get; set; }

		public string? NotaEvolucion { get; set; }
	}
}