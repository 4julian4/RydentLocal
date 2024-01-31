using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TCITASBORRADAS")]
    public class TCITASBORRADAS
    {
        public DateTime? FECHA { get; set; }
        public int? SILLA { get; set; }
        public TimeSpan? HORA { get; set; }
        public string? NOMBRE { get; set; }
        public string? USUARIO { get; set; }
        public string? IDCALENDARIO { get; set; }
        public DateTime? FECHASUCESO { get; set; }
        public int? IDCONSECUTIVO { get; set; }
    }
}
