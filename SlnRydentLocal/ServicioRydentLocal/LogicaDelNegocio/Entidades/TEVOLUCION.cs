using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TEVOLUCION")]
    public class TEVOLUCION
    {
        [Key]
        public int? IDEVOLUCION { set; get; }
        public int? IDEVOLUSECUND { set; get; }
        public string? PROXIMA_CITA { set; get; }
        public DateTime? FECHA_PROX_CITA { set; get; }
        public string? FECHA_ORDEN { set; get; }
        public string? ENTRADA { set; get; }
        public string? SALIDA { set; get; }
        public DateTime? FECHA { set; get; }
        public TimeSpan? HORA { set; get; }
        public string? DOCTOR { set; get; }
        public int? FIRMA { set; get; }
        public string? COMPLICACION { set; get; }
        public TimeSpan? HORA_FIN { set; get; }
        public string? COLOR { set; get; }
        public string? NOTA { set; get; }
        public string? EVOLUCION { set; get; }
        public string? URGENCIAS { set; get; }
        public TimeSpan? HORA_LLEGADA { set; get; }
    }
}


