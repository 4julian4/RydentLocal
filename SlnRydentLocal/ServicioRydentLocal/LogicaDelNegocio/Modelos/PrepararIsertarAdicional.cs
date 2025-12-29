using System;
using System.Collections.Generic;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
	

	// -------------------------
	// 1) PREPARAR INSERTAR ADICIONAL (TIPO=2)
	// -------------------------
	public sealed class PrepararInsertarAdicionalRequest
	{
		public int IdPaciente { get; set; }
		public int Fase { get; set; }
		public int IdDoctorTratante { get; set; }
		public string? UsuarioActual { get; set; }
		public int? IdDoctorSeleccionadoUi { get; set; }
	}

	public sealed class PrepararInsertarAdicionalResponse
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }

		public int IdPaciente { get; set; }
		public int Fase { get; set; }
		public int IdDoctorTratante { get; set; }

		public double MoraTotal { get; set; }
		public double ValorAFacturar { get; set; }

		public string FechaHoy { get; set; } = "";

		public AbonoUiRulesDto Rules { get; set; } = new();

		public List<DoctorItemDto> DoctoresRecibidoPor { get; set; } = new();
		public int? IdRecibidoPorPorDefecto { get; set; }
		public bool RecibidoPorHabilitado { get; set; }

		public List<string> NombresRecibe { get; set; } = new();
		public string? NombreRecibePorDefecto { get; set; }

		// ✅ Motivos del combo Delphi
		public List<MotivoItemDto> Motivos { get; set; } = new();
	}

	// -------------------------
	// 2) INSERTAR ADICIONALES (GUARDAR) - TIPO=2
	// -------------------------
	public sealed class AdicionalItemDto
	{
		/// <summary>Texto final que quedará en DESCRIPCION.</summary>
		public string Descripcion { get; set; } = "";

		/// <summary>Cantidad >= 1.</summary>
		public int Cantidad { get; set; } = 1;

		/// <summary>Valor unitario.</summary>
		public double ValorUnitario { get; set; }

		/// <summary>CODIGO_DESCRIPCION si aplica.</summary>
		public string? CodigoConcepto { get; set; }
	}

	public sealed class InsertarAdicionalRequest
	{
		public int IdPaciente { get; set; }
		public int Fase { get; set; }
		public int IdDoctorTratante { get; set; }

		public int? IdRecibidoPor { get; set; }

		public string Fecha { get; set; } = ""; // yyyy-MM-dd

		// ✅ Legacy (si el front aún manda 1 solo adicional)
		public string? Descripcion { get; set; }
		public string? CodigoConcepto { get; set; }
		public double Valor { get; set; } // total (legacy)

		// IVA global (como lo tienes hoy)
		public bool IvaIncluido { get; set; }
		public double? ValorIva { get; set; }

		public string? NombreRecibe { get; set; }
		public int PagoTercero { get; set; } = 1;
		public int? IdFirma { get; set; }

		/// <summary>✅ N líneas dinámicas.</summary>
		public List<AdicionalItemDto> Items { get; set; } = new();
	}

	public sealed class InsertarAdicionalItemResultDto
	{
		public int IdRelacion { get; set; }
		public int Identificador { get; set; }
		public string Descripcion { get; set; } = "";
		public double ValorTotal { get; set; }
	}

	public sealed class InsertarAdicionalResponse
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }

		public List<InsertarAdicionalItemResultDto> ItemsInsertados { get; set; } = new();

		public bool RelacionoAnticipos { get; set; }
		public double? RestanteTrasAnticipos { get; set; }

		public double? MoraTotalActualizada { get; set; }
	}
}
