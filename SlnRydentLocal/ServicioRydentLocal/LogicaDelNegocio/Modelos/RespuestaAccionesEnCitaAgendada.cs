using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class RespuestaAccionesEnCitaAgendada
    {
        public int silla { get; set; }
        public DateTime fecha { get; set; }
        public TimeSpan hora { get; set; }
        public string tipoAccion { get; set; }
        public bool aceptado { get; set; }
        public string respuesta { get; set; }
        public string quienLoHace { get; set; }
        public string mensaje { get; set; }
    }
}
