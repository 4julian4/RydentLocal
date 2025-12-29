using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
	public class PrepararEditarEstadoCuentaRequest
	{
		public int PacienteId { get; set; }
		public int DoctorId { get; set; }
		public int Fase { get; set; }
	}
	
}
