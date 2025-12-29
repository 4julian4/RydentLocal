using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
	public class PrepararNuevoEstadoCuentaRequest
	{
		public int PacienteId { get; set; }   // ID (Modulo.IdPaciente)
		public int DoctorId { get; set; }     // idDoctor
	}

}
