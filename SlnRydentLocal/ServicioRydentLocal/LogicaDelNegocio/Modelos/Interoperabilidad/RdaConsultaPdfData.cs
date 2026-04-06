namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaConsultaPdfData
	{
		// =========================
		// IDENTIFICACION DEL ENCUENTRO
		// =========================
		public string? Paciente { get; set; }
		public string? TipoDocumento { get; set; }
		public string? Documento { get; set; }
		public string? Doctor { get; set; }
		public string? Prestador { get; set; }

		public string? FechaAtencion { get; set; }
		public string? Factura { get; set; }
		public string? NumeroAutorizacion { get; set; }

		public string? CodigoConsulta { get; set; }
		public string? NombreConsulta { get; set; }
		public string? CausaAtencion { get; set; }
		public string? TipoDiagnostico { get; set; }

		// =========================
		// ASEGURAMIENTO
		// =========================
		public string? EntidadResponsable { get; set; }
		public string? CodigoEntidad { get; set; }
		public string? TipoAfiliacion { get; set; }
		public string? NumeroAfiliacion { get; set; }

		// =========================
		// UBICACION / CONTACTO
		// =========================
		public string? Ciudad { get; set; }
		public string? Departamento { get; set; }
		public string? Direccion { get; set; }
		public string? Telefono { get; set; }
		public string? Celular { get; set; }

		// =========================
		// DIAGNOSTICOS
		// =========================
		public string? DiagnosticoPrincipal { get; set; }
		public string? Diagnostico2 { get; set; }
		public string? Diagnostico3 { get; set; }
		public string? Diagnostico4 { get; set; }

		// =========================
		// DATOS CLINICOS
		// =========================
		public string? MotivoConsulta { get; set; }
		public string? EnfermedadActual { get; set; }
		public string? Alergias { get; set; }
		public string? Medicamentos { get; set; }
		public string? FactoresRiesgo { get; set; }
		public string? EnfermedadesPrevias { get; set; }
		public string? Cirugias { get; set; }
		public string? RevisionSistemas { get; set; }
		public string? Observaciones { get; set; }

		// =========================
		// PROCEDIMIENTO
		// =========================
		public string? ProcedimientoCodigo { get; set; }
		public string? ProcedimientoNombre { get; set; }
		public string? ProcedimientoDxPrincipal { get; set; }
		public string? ProcedimientoDxRelacionado { get; set; }
		public string? Ambito { get; set; }
		public string? FinalidadProcedimiento { get; set; }
		public string? PersonalAtiende { get; set; }
		public string? Complicacion { get; set; }
		public string? FormaActoQuir { get; set; }
		public string? ValorProcedimiento { get; set; }

		public string? EntidadProcedimiento { get; set; }
		public string? Extranjero { get; set; }
		public string? Pais { get; set; }
	}
}