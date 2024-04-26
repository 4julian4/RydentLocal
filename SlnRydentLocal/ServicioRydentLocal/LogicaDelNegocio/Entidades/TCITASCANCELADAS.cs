using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    public class TCITASCANCELADAS
    {
        public DateTime? FECHA { get; set; }
        public TimeSpan? HORA { get; set; }
        public string? NOMBRE { get; set; }
        public string? TELEFONO { get; set; }
        public string? ASUNTO { get; set; }
        public string? DOCTOR { get; set; }
        public string? DURACION { get; set; }
        public string? USUARIO { get; set; }
        public int? SILLA { get; set; }
        public string? MOTIVO_CANCELA { get; set; }
        public string? CANCELADO_POR { get; set; }
        public DateTime? FECHA_CANCELACION { get; set; }
        public int? ID { get; set; }
    }
}
