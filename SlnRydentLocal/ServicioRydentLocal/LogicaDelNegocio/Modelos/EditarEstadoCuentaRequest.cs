using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
	public class EditarEstadoCuentaRequest
	{
		public int PacienteId { get; set; }
		public int DoctorId { get; set; }
		public int Fase { get; set; }

		public DateTime FechaInicio { get; set; }
		public double ValorTratamiento { get; set; }
		public double ValorCuotaInicial { get; set; }

		public int NumeroCuotas { get; set; }
		public double ValorCuota { get; set; }

		public int NumeroCuotaInicial { get; set; }
		public int IntervaloInicialDias { get; set; }
		public int IntervaloTiempoDias { get; set; }

		public string Descripcion { get; set; } = "";
		public string? Observaciones { get; set; }

		public string? Documento { get; set; } // FACTURA/compromiso
		public int ConvenioId { get; set; } = -1;

		public bool FacturaVieja { get; set; } // si lo usas
	}
}
