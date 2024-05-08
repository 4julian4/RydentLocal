using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class RespuestaBusquedaCitasPacienteModel
    {
        public string? ID { set; get; }
        public DateTime? FECHA_CITA { set; get; }
        public TimeSpan? HORA_CITA { set; get; }
        public string? SILLA_CITA { set; get; }
        public string? NOMBRE_PACIENTE { set; get; }
        public string? NUMDOCUMENTO { set; get; }
        public string? DOCTOR { set; get; }
        public string? TELEFONO_PACIENTE { set; get; }
        public string? OBSERVACIONES { set; get; }
        public string? ASUNTO { set; get; }
        public int? IDCONSECUTIVO { set; get; }
    }
}
