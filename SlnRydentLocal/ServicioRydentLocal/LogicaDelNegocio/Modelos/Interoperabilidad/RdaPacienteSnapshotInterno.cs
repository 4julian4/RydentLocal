using System;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public class RdaPacienteSnapshotInterno
	{
		public string? TipoDocumento { get; set; }
		public DateTimeOffset? FechaGeneracion { get; set; }
		public RdaPacienteSnapshotDocumento? Documento { get; set; }
	}

	public class RdaPacienteSnapshotDocumento
	{
		public RdaPacienteSnapshotConsulta? Consulta { get; set; }
		public RdaPacienteSnapshotPrestador? Prestador { get; set; }
	}

	public class RdaPacienteSnapshotConsulta
	{
		public RdaPacienteSnapshotEncounter? Encounter { get; set; }
		public RdaPacienteSnapshotAntecedentes? Antecedentes { get; set; }
		public RdaPacienteSnapshotDiagnostico? Diagnostico { get; set; }
		public RdaPacienteSnapshotProcedimiento? Procedimiento { get; set; }
	}

	public class RdaPacienteSnapshotEncounter
	{
		public int IdAnamnesis { get; set; }
		public DateTimeOffset? FechaConsulta { get; set; }
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
		public string? CodigoEps { get; set; }
		public string? NombreEps { get; set; }
		public string? NroAfiliacion { get; set; }
		public string? TipoUsuario { get; set; }
	}

	public class RdaPacienteSnapshotAntecedentes
	{
		public int IdAnamnesis { get; set; }
		public string? MotivoConsulta { get; set; }
		public string? EnfermedadActual { get; set; }
		public string? EnfermedadesPreviasTexto { get; set; }
		public string? Diabetes { get; set; }
		public string? Gastricos { get; set; }
		public string? ObservacionesAntecedentes { get; set; }
	}

	public class RdaPacienteSnapshotDiagnostico
	{
	}

	public class RdaPacienteSnapshotProcedimiento
	{
		public int IdAnamnesis { get; set; }
		public string? NumeroAutorizacion { get; set; }
		public string? CodigoProcedimiento { get; set; }
		public string? NombreProcedimiento { get; set; }
		public string? AmbitoRealizacion { get; set; }
		public string? FinalidadProcedimiento { get; set; }
		public string? PersonalQueAtiende { get; set; }
		public string? DxPrincipal { get; set; }
		public string? NombreDxPrincipal { get; set; }
		public string? Complicacion { get; set; }
		public string? FormaRealizacionActoQuir { get; set; }
		public decimal? ValorProcedimiento { get; set; }
		public string? CodigoEntidad { get; set; }
		public string? NombreEntidad { get; set; }
		public string? Extranjero { get; set; }
		public string? Pais { get; set; }
	}

	public class RdaPacienteSnapshotPrestador
	{
		public string? CodigoPrestador { get; set; }
		public string? NombrePrestador { get; set; }
		public string? NitPrestador { get; set; }
		public string? TipoDocumentoPrestador { get; set; }
		public string? DireccionPrestador { get; set; }
		public string? TelefonoPrestador { get; set; }
		public string? CiudadPrestador { get; set; }
		public int? IdDoctor { get; set; }
		public string? NombreDoctor { get; set; }
		public string? TipoDocumentoDoctor { get; set; }
		public string? NumeroDocumentoDoctor { get; set; }
	}
}