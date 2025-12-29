using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
	public class PrepararNuevoEstadoCuentaResponse
	{
		// Defaults para UI
		public int FaseSugerida { get; set; }                 // ultimaFase + 1 (o 1)
		public int TipoFacturacionDoctor { get; set; }        // 1 / 2 / 3 (como Delphi)

		// Etiqueta y valor sugerido de “factura / compromiso”
		public string LabelDocumento { get; set; } = "";      // "N° Factura" o "Compromiso de Compraventa"
		public string? DocumentoSugerido { get; set; }        // si TipoFacturacionDoctor==2 => QMaxFactura (P_MAX_CON_LETRAS)

		// Convenio sugerido (para preseleccionar en el combo)
		public int? ConvenioSugeridoId { get; set; }          // desde QLocalizarConvenio o lógica equivalente

		// Reglas de cálculo/formato
		public bool UsaDecimalesEnFacturas { get; set; }      // confirmación FACTURAS_RECIBOS_DECIMALES (0=>decimales, 1=>redondeo)

		// Defaults típicos (para evitar “hardcode” en front si quieres)
		public int IntervaloDefaultDias { get; set; } = 30;   // Delphi fuerza 30 si vacío
		public int NumeroCuotasDefault { get; set; } = 1;     // Delphi fuerza 1 si vacío

		// Flags de personalización
		public bool PermiteFacturaVieja { get; set; }         // Consultarpersonalizaciones('FACTURA VIEJA')

		// Para manejo de errores
		public bool Ok { get; set; } = true;
		public string? Mensaje { get; set; }
	}

}
