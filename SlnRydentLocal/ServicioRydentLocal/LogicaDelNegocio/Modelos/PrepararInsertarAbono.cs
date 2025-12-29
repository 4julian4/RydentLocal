using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{

	// -------------------------
	// 1) PREPARAR INSERTAR ABONO
	// -------------------------
	public sealed class PrepararInsertarAbonoRequest
	{
		public int IdPaciente { get; set; }
		public int Fase { get; set; }

		/// <summary>
		/// Doctor del tratamiento (equivalente a "idDoctor" que usa Estado de Cuenta).
		/// </summary>
		public int IdDoctorTratante { get; set; }

		/// <summary>
		/// Usuario actual (para reglas tipo "RECIBIDO_POR_SEGUN_USUARIO", "FIRMA_SEGUN_USUARIO", etc.).
		/// </summary>
		public string? UsuarioActual { get; set; }

		/// <summary>
		/// Nombre del doctor seleccionado en UI (si aplica) o el doctor por defecto (DoctorSeleccionado).
		/// </summary>
		public int? IdDoctorSeleccionadoUi { get; set; }
	}

	public sealed class PrepararInsertarAbonoResponse
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }

		// Contexto
		public int IdPaciente { get; set; }
		public int Fase { get; set; }
		public int IdDoctorTratante { get; set; }

		// Estado de cuenta (lo mínimo para decisiones UI tipo "pago superior a deuda")
		public double MoraTotal { get; set; }
		public double ValorAFacturar { get; set; }

		// Fecha
		public string FechaHoy { get; set; } = "";           // "yyyy-MM-dd"
		public string? UltimaFechaAbono { get; set; }        // "yyyy-MM-dd" o null

		// Config/Reglas (banderas que en Delphi salen de consultarConfirmacion/personalizaciones)
		public AbonoUiRulesDto Rules { get; set; } = new();

		// Datos para combos/listas
		public List<DoctorItemDto> DoctoresRecibidoPor { get; set; } = new List<DoctorItemDto>();
		public int? IdRecibidoPorPorDefecto { get; set; }     // Delphi: RECIBIDO_POR_SEGUN_USUARIO + TClave.idDoctor
		public bool RecibidoPorHabilitado { get; set; }       // Delphi: cbRecibidoPor.Enabled := False cuando aplica

		public List<string> NombresRecibe { get; set; } = new List<string>();
		public string? NombreRecibePorDefecto { get; set; }

		public List<MotivoItemDto> Motivos { get; set; } = new List<MotivoItemDto>();
		public List<string> CodigosConcepto { get; set; } = new List<string>();

		// Sugeridos
		public string? ReciboSugerido { get; set; }  // Delphi: P_MAX_RECIBOS_CON_LETRAS
		public string? FacturaSugerida { get; set; } // Delphi: P_MAX_CON_LETRAS
		public int? IdResolucionDian { get; set; }   // Delphi: QAbonoResolucionDian -> IDRESOLUCION

		// Catálogos opcionales
		public List<double> ValoresIvaPermitidos { get; set; } = new List<double>();
	}

	public sealed class AbonoUiRulesDto
	{
		// Delphi: FECHAS_FIJAS_RECIBOS (habilita DatePicker)
		public bool PermiteCambiarFechaAbono { get; set; }

		// Delphi: RECIBOS_OCULTOS_CUENTAS (si es 0, oculta recibo)
		public bool MostrarCampoRecibo { get; set; }

		// Delphi: CONTROL_RECIBOS_FACUTAS (si es 0, readonly)
		public bool PermiteEditarFacturaYRecibo { get; set; }

		// Delphi: FACTURAS_RECIBOS_DECIMALES (formato)
		public bool UsaDecimalesEnValores { get; set; }

		// Delphi: LISTADO_DOCTOR_BLANCO (si es 0, no permite blanco)
		public bool PermiteRecibidoPorEnBlanco { get; set; }

		// Delphi: RECIBIDO_POR_SEGUN_USUARIO (si es 0, fija recibido_por)
		public bool RecibidoPorSegunUsuario { get; set; }

		// Delphi: RECIBO_MANUAL (personalización)
		public bool ReciboManual { get; set; }

		// Delphi: MOTIVO_EGRESOS_INGRESOS (si es 0 usa tabla motivos; si no usa lista rápida)
		public bool UsaCatalogoMotivos { get; set; }

		// Delphi: FIRMA_PAGOS, FIRMA_SEGUN_USUARIO (si luego lo metemos)
		public bool PermiteFirmaPagos { get; set; }
		public bool FirmaSegunUsuario { get; set; }

		// Delphi: CONTROL_RECIBOS_FACUTAS + tipoFacturacion(idDoctor)=3 (usa CBFactura)
		public int TipoFacturacion { get; set; } // por ejemplo 3
	}

	public sealed class DoctorItemDto
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = "";
	}

	public sealed class MotivoItemDto
	{
		public int? Id { get; set; }              // si viene de tabla motivos
		public string Nombre { get; set; } = "";  // EmotivoAbono.Text
		public string? Codigo { get; set; }       // CBCodigoConcepto.Text
		public double? Valor { get; set; }
		public int? Iva { get; set; }
	}

	// -------------------------
	// 2) INSERTAR ABONO (GUARDAR)
	// -------------------------
	public sealed class InsertarAbonoRequest
	{
		public int IdPaciente { get; set; }
		public int Fase { get; set; }
		public int IdDoctorTratante { get; set; }

		/// <summary>
		/// Doctor "recibido por" (cbRecibidoPor). En Delphi se guarda en RECIBIDO_POR.
		/// </summary>
		public int? IdRecibidoPor { get; set; }

		/// <summary>
		/// Fecha del abono (DTPFechaAbono).
		/// </summary>
		public string FechaAbono { get; set; } = ""; // "yyyy-MM-dd"

		/// <summary>
		/// Recibo visible/usable (ERecibo). Si está oculto puede ir "".
		/// </summary>
		public string? Recibo { get; set; }

		/// <summary>
		/// Recibo relacionado (Delphi: RECIBO_RELACIONADO := ERecibo.Text)
		/// </summary>
		public string? ReciboRelacionado { get; set; }

		/// <summary>
		/// Factura (CBFactura o eFactura según tipoFacturacion).
		/// </summary>
		public string? Factura { get; set; }

		/// <summary>
		/// Motivo / descripción principal (EmotivoAbono.Text)
		/// </summary>
		public string? Descripcion { get; set; }

		/// <summary>
		/// Código de concepto (CBCodigoConcepto.Text)
		/// </summary>
		public string? CodigoConcepto { get; set; }

		/// <summary>
		/// IVA (Delphi: CBIvaIncluido + CBIva)
		/// En Delphi: si "incluido" checked => CONIVA=0 y VALORIVA = CBIva
		/// </summary>
		public bool IvaIncluido { get; set; }
		public double? ValorIva { get; set; }

		/// <summary>
		/// Nombre recibe (CBNombreRecibe.Text)
		/// </summary>
		public string? NombreRecibe { get; set; }

		/// <summary>
		/// Delphi: PagoTercero (default 1, en recaudos lo ponen 0)
		/// </summary>
		public int PagoTercero { get; set; } = 1;

		/// <summary>
		/// Si en Delphi hubiese que insertar también la factura (TIPO=4) antes del abono (TIPO=1)
		/// </summary>
		public bool InsertarFacturaSiAplica { get; set; }

		public double? ValorFactura { get; set; } // Delphi: EValorFactura si se inserta factura

		/// <summary>
		/// ✅ Detalle de conceptos para insertar en T_ADICIONALES_ABONOS_MOTIVOS
		/// </summary>
		public List<AbonoConceptoDetalleDto> ConceptosDetalle { get; set; } = new List<AbonoConceptoDetalleDto>();

		/// <summary>
		/// Formas de pago (T_ABONOS_TIPO_PAGO) - Delphi marca varios checks.
		/// </summary>
		public List<AbonoTipoPagoDto> TiposPago { get; set; } = new List<AbonoTipoPagoDto>();


		// Si luego metemos firma:
		public int? IdFirma { get; set; } // Delphi: FIRMA (-1 si no)
	}

	public sealed class AbonoTipoPagoDto
	{
		/// <summary>
		/// EFECTIVO / CONSIGNACION / TARJETA DE CREDITO / TARJETA DEBITO / CHEQUE / BONO / SISTECREDITO
		/// </summary>
		public string TipoDePago { get; set; } = "";

		public double Valor { get; set; }

		/// <summary>
		/// Descripción asociada por tipo de pago (EDescripcion1..7)
		/// </summary>
		public string? Descripcion { get; set; }

		/// <summary>
		/// Número (ENro1..7) - en DB es VARCHAR
		/// </summary>
		public string? Numero { get; set; }

		/// <summary>
		/// Fecha texto (EFecha1..7) - en DB es VARCHAR
		/// </summary>
		public string? FechaTexto { get; set; }
	}


	public sealed class AbonoConceptoDetalleDto
	{
		public string Codigo { get; set; } = "";
		public string Descripcion { get; set; } = "";

		/// <summary>
		/// Valor unitario del concepto (sin puntos).
		/// </summary>
		public double Valor { get; set; }

		/// <summary>
		/// Cantidad (>= 1)
		/// </summary>
		public int Cantidad { get; set; }

		/// <summary>
		/// Si el valor ya incluye IVA (en UI: "IVA incluido").
		/// </summary>
		public bool IvaIncluido { get; set; }

		/// <summary>
		/// Porcentaje de IVA (0, 4, 6, 12, 16, 19...)
		/// </summary>
		public double PorcentajeIva { get; set; }
	}


	public sealed class InsertarAbonoResponse
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }

		/// <summary>
		/// IDs de relación/identificador generados en T_ADICIONALES_ABONOS.
		/// </summary>
		public int? IdRelacion { get; set; }
		public int? Identificador { get; set; }

		public string? ReciboUsado { get; set; }
		public string? FacturaUsada { get; set; }

		/// <summary>
		/// Por si el Worker ajusta consecutivos por concurrencia (igual que Delphi revalida con MAX).
		/// </summary>
		public bool AjustoConsecutivos { get; set; }

		/// <summary>
		/// Resumen actualizado para refrescar UI de estado de cuenta.
		/// </summary>
		public double? MoraTotalActualizada { get; set; }
	}

}
