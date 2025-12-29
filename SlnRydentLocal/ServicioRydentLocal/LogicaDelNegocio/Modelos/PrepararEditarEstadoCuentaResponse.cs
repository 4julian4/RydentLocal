using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
	public class PrepararEditarEstadoCuentaResponse
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }

		// Datos para precargar el diálogo (como Delphi)
		public DateTime? FechaInicio { get; set; }
		public double? ValorTratamiento { get; set; }
		public double? ValorCuotaInicial { get; set; }
		public int? NumeroCuotas { get; set; }
		public double? ValorCuota { get; set; }

		public int? IntervaloTiempo { get; set; }
		public int? NumeroCuotaIni { get; set; }
		public int? IntervaloIni { get; set; }

		public string? Descripcion { get; set; }
		public string? Documento { get; set; }      // FACTURA o Compromiso
		public string? Observaciones { get; set; }  // blob -> string

		public int? ConvenioId { get; set; }

		// Para el label como Delphi
		public int TipoFacturacionDoctor { get; set; } // 2=factura, 1/3=compromiso
		public string LabelDocumento { get; set; } = "";
	}
}
