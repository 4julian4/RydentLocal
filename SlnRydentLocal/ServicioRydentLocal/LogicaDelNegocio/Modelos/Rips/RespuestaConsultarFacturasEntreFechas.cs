using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
    public class RespuestaConsultarFacturasEntreFechas
    {
        public int? IDANAMNESIS { get; set; }
        public DateTime? FECHAINI { get; set; }
        public DateTime? FECHAFIN { get; set; }
        public DateTime? FECHA { get; set; }
        public string? FACTURA { get; set; }
        public string? DESCRIPCION { get; set; }
    }
}
