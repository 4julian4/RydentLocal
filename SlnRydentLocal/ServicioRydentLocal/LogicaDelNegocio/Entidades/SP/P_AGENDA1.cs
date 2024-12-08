using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
    [Keyless]
    public class P_AGENDA1
    {
        public TimeSpan OUT_HORA { set; get; }
        public string? OUT_CODIGO { set; get; }
        public string? OUT_TELEFONO { set; get; }
        public string? OUT_NOMBRE { set; get; }
        public string? OUT_ASUNTO { set; get; }
        public string? OUT_ASISTENCIA { set; get; }
        public string? OUT_DURACION { set; get; }
        public DateTime? OUT_FECHA { set; get; }
        public string? OUT_SILLA { set; get; }
        public string? OUT_HORAINI { set; get; }
        public string? OUT_HORAFIN { set; get; }
        public int? OUT_INTERVALO { set; get; }
        public string? OUT_PARARINI { set; get; }
        public string? OUT_PARARFIN { set; get; }
        public string? OUT_TIPO { set; get; }
        public string? OUT_DOCTOR { set; get; }
        public string? OUT_CONFIRMAR { set; get; }
        public string? OUT_ABONO { set; get; }
        public int? OUT_RETARDO { set; get; }
        public string? OUT_ID { set; get; }
        public string? OUT_OBSERVACIONES { set; get; }
        public string? OUT_NRO_AFILIACION { set; get; }
        public int? OUT_IDCONSECUTIVO { set; get; }
        public DateTime? OUT_FECHA_OBSERVACION { set; get; }
        public TimeSpan? OUT_HORA_OBSERVACION { set; get; }
        public TimeSpan? OUT_HORA_LLEGADA { set; get; }
        public TimeSpan? OUT_HORA_INGRESO { set; get; }
        public TimeSpan? OUT_HORA_SALIDA { set; get; }
        public string? OUT_COLOR { set; get; }
        public string? OUT_ALARMAR { set; get; }
        public string? OUT_ANESTECIA { set; get; }
        public string? OUT_CEDULA { set; get; }
        public string? OUT_REFERIDO_POR { set; get; }
        public string? OUT_E_MAIL_RESP { set; get; }
        public string? OUT_CONVENIO { set; get; }
        public string? OUT_CRONOGRAMA { set; get; }
        public TimeSpan? OUT_HORA_ATENCION { set; get; }
        public string? OUT__NOMBRE { set; get; }
        public string? OUT_ASIGNADO_POR { set; get; }
        public TimeSpan? OUT_HORA_CITA { set; get; }
        public string? OUT_NOTA_IMPORTANTE { set; get; }
        public DateTime? OUT_FECHA_SUCESO { set; get; }
        public string? OUT_CELULAR { set; get; }
        public string? OUT_IDCALENDARIO { set; get; }
    }
}
 
