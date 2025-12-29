using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Facturatech
{
    public class NegMensaje
    {
        public string id { set; get; }
        public string Status { get; set; }
        public string MensajeCorrecto { get; set; }
        public string error { get; set; }
        public string transaccionID { get; set; }
    }
}
