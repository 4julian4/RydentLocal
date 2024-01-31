using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class RespuestaObtenerDoctorModel
    {
        public TDATOSDOCTORES doctor { set; get; } = new TDATOSDOCTORES();
        public int totalPacientes { set; get; } = 0;
        public bool tieneAlarma { set; get; } = false;
    }
}
