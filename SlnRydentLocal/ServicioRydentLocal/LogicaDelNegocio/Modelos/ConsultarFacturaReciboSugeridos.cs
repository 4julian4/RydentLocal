using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
	public class ConsultarSugeridosAbonoRequest
	{
		public int IdPaciente { get; set; }
		public int Fase { get; set; }
		public int IdDoctorTratante { get; set; }
		public int IdDoctorSeleccionado { get; set; } // el “recibido por” seleccionado
	}

	public class ConsultarSugeridosAbonoResponse
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }

		public bool OcultarFactura { get; set; }
		public string? ReciboSugerido { get; set; }
		public string? FacturaSugerida { get; set; }
		public int? IdResolucionDian { get; set; }
	}
}
