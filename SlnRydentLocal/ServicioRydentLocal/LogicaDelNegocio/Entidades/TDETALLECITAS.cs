using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TDETALLECITAS")]
    public class TDETALLECITAS
    {
        public string? ID { set; get; }
        public DateTime? FECHA { set; get; }
        public int? SILLA { set; get; }
        public string? NOMBRE { set; get; }
        public TimeSpan? HORA1 { set; get; }
        public TimeSpan? HORA { set; get; }
        public string? TELEFONO { set; get; }
        public string? OBSERVACIONES { set; get; }
        public string? CEDULA { set; get; }
        public string? ASISTENCIA { set; get; }
        public string? TIPO_CITA { set; get; }
        public string? ASUNTO { set; get; }
        public string? FECHA_TEXTO { set; get; }
        public string? DOCTOR { set; get; }
        public string? DURACION { set; get; }
        public string? CANCELADO { set; get; }
        public int? CASOS { set; get; }
        public string? ABONO { set; get; }
        public string? HORA_ING_CITA_TEXTO { set; get; }
        public string? CONFIRMAR { set; get; }
        public int? RETARDO { set; get; }
        public string? CITACONMORA { set; get; }
        public string? CITANUEVA { set; get; }
        public DateTime? FECHASUCESO { set; get; }
        public int? IDCONSECUTIVO { set; get; }
        public DateTime? FECHA_OBSERVACION { set; get; }
        public TimeSpan? HORA_OBSERVACION { set; get; }
        public string? CANCELADA_POR { set; get; }
        public TimeSpan? HORA_LLEGADA_CITA { set; get; }
        public TimeSpan? HORA_ING_CITA { set; get; }
        public TimeSpan? HORA_FIN_CITA { set; get; }
        public string? COLOR { set; get; }
        public string? ALARMAR { set; get; }
        public int? TARDANZA { set; get; }
        public string? ANESTECIA { set; get; }
        public TimeSpan? HORA_INI_ATENCION { set; get; }
        public string? CELULAR { set; get; }
        public string? MEDICAMENTOS { set; get; }
        public string? IDCALENDARIO { set; get; }
    }
}

