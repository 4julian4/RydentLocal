using System;
using System.Collections.Generic;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public class InteroperabilidadPacienteFiltro
	{
		public string TipoDocumento { get; set; } = "";
		public string NumeroDocumento { get; set; } = "";
		public string Humanuser { get; set; } = "";
		public string IdDoctor { get; set; } = "";
	}

	public class InteroperabilidadPacienteFiltroConsulta
	{
		public string TipoDocumento { get; set; } = "";
		public string NumeroDocumento { get; set; } = "";
		public string Humanuser { get; set; } = "";
	}

	public class InteroperabilidadPacienteResumen
	{
		public bool Encontrado { get; set; }
		public bool Exacto { get; set; }
		public bool Multiple { get; set; }

		public string IdExterno { get; set; }
		public string TipoDocumento { get; set; }
		public string NumeroDocumento { get; set; }

		public string Nombres { get; set; }
		public string Apellidos { get; set; }
		public string NombreCompleto { get; set; }

		public string Sexo { get; set; }
		public string FechaNacimiento { get; set; }

		public string Telefono { get; set; }
		public string Celular { get; set; }
		public string Email { get; set; }

		public string Direccion { get; set; }
		public string CiudadCodigo { get; set; }
		public string CiudadNombre { get; set; }
		public string DepartamentoCodigo { get; set; }
		public string DepartamentoNombre { get; set; }
		public string ZonaResidencial { get; set; }

		public string EpsCodigo { get; set; }
		public string EpsNombre { get; set; }
		public string Afiliacion { get; set; }

		public string RawJson { get; set; }
	}

	public class InteroperabilidadPacienteSimilarItem
	{
		public string IdExterno { get; set; }
		public string TipoDocumento { get; set; }
		public string NumeroDocumento { get; set; }
		public string Nombres { get; set; }
		public string Apellidos { get; set; }
		public string NombreCompleto { get; set; }
		public string Sexo { get; set; }
		public string FechaNacimiento { get; set; }
		public string CiudadNombre { get; set; }
		public string EpsNombre { get; set; }
		public string RawJson { get; set; }
	}

	public class InteroperabilidadRdaPacienteItem
	{
		public string IdDocumento { get; set; }
		public string Fecha { get; set; }
		public string TipoDocumento { get; set; }
		public string Titulo { get; set; }
		public string Prestador { get; set; }
		public string Autor { get; set; }
		public string RawJson { get; set; }
	}

	public class InteroperabilidadEncuentroItem
	{
		public string IdEncuentro { get; set; }
		public string FechaInicio { get; set; }
		public string FechaFin { get; set; }
		public string Clase { get; set; }
		public string Modalidad { get; set; }
		public string Prestador { get; set; }
		public string Doctor { get; set; }
		public string DiagnosticoPrincipal { get; set; }
		public string CausaAtencion { get; set; }
		public string RawJson { get; set; }
	}

	public class InteroperabilidadConsultaPacienteRespuesta
	{
		public bool Ok { get; set; }
		public string Mensaje { get; set; }
		public InteroperabilidadPacienteResumen Paciente { get; set; }
	}

	public class InteroperabilidadConsultaPacienteSimilarRespuesta
	{
		public bool Ok { get; set; }
		public string Mensaje { get; set; }
		public List<InteroperabilidadPacienteSimilarItem> Items { get; set; } = new List<InteroperabilidadPacienteSimilarItem>();
	}

	public class InteroperabilidadConsultaRdaPacienteRespuesta
	{
		public bool Ok { get; set; }
		public string Mensaje { get; set; }
		public List<InteroperabilidadRdaPacienteItem> Items { get; set; } = new List<InteroperabilidadRdaPacienteItem>();
	}

	public class InteroperabilidadConsultaEncuentrosRespuesta
	{
		public bool Ok { get; set; }
		public string Mensaje { get; set; }
		public List<InteroperabilidadEncuentroItem> Items { get; set; } = new List<InteroperabilidadEncuentroItem>();
	}
}