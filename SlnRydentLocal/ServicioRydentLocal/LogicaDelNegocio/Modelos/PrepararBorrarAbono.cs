using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
	// -------------------------
	// 1) PREPARAR BORRAR ABONO
	// -------------------------
	public sealed class PrepararBorrarAbonoRequest
	{
		/// <summary>Paciente (Delphi: Modulo.IdPaciente)</summary>
		public int IdPaciente { get; set; }

		/// <summary>Fase seleccionada (Delphi: CBFases.Text)</summary>
		public int Fase { get; set; }

		/// <summary>Doctor del tratamiento (Delphi: Modulo.consultarIdDoctor(DoctorSeleccionado))</summary>
		public int IdDoctorTratante { get; set; }

		public int IdRelacion { get; set; }


		/// <summary>
		/// Identificador del movimiento a borrar (Delphi: QP_Consultar_Cuenta.identificador)
		/// Este es el "paquete" de ese abono/adicional.
		/// </summary>
		public int Identificador { get; set; }

		/// <summary>
		/// Usuario actual (para validar permisos si lo haces en backend).
		/// </summary>
		public string? UsuarioActual { get; set; }
	}  

public sealed class PrepararBorrarAbonoResponse
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }

		// Eco de contexto
		public int IdPaciente { get; set; }
		public int Fase { get; set; }
		public int IdDoctorTratante { get; set; }
		public int Identificador { get; set; }

		/// <summary>
		/// ✅ Lista de relaciones encontradas dentro de ese Identificador.
		/// (Puede ser 1, pero lo dejamos listo para múltiples por seguridad)
		/// </summary>
		public List<int> IdRelaciones { get; set; } = new();

		/// <summary>
		/// Texto que el front puede mostrar como resumen antes de borrar (similar a Delphi armando "tipo")
		/// </summary>
		public string? ResumenParaConfirmar { get; set; }

		/// <summary>
		/// Delphi pide motivo siempre (ventana Motivo).
		/// </summary>
		public bool RequiereMotivo { get; set; } = true;
	}

	// -------------------------
	// 2) BORRAR ABONO (EJECUTAR)
	// -------------------------
	public sealed class BorrarAbonoRequest
	{
		public int IdPaciente { get; set; }
		public int Fase { get; set; }
		public int IdDoctorTratante { get; set; }
		public int Identificador { get; set; }

		/// <summary>
		/// Motivo escrito por el usuario (Delphi: MotivoBorrar.MMotivo.Text)
		/// </summary>
		public string Motivo { get; set; } = "";

		/// <summary>
		/// Usuario actual (por auditoría o validación de permisos)
		/// </summary>
		public string? UsuarioActual { get; set; }
	}

	public sealed class BorrarAbonoResponse
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }

		/// <summary>
		/// Cuántos registros de T_ADICIONALES_ABONOS se borraron (por Identificador)
		/// </summary>
		public int RegistrosBorrados { get; set; }

		/// <summary>
		/// Mora total actualizada para refrescar UI (opcional)
		/// </summary>
		public double? MoraTotalActualizada { get; set; }
	}
}
