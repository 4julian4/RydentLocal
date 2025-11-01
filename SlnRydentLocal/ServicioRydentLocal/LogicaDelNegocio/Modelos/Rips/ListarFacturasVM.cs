using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
    internal class ListarFacturasVM
    {
        public int idresolucion_dian { set; get; }

        public string Prestador { set; get; }

        public string codigo_prestador { set; get; }

        public string nombre_paciente { set; get; }

        public string documento { set; get; }

        public string tipo_documento { set; get; }

        public string factura { set; get; }

        public string valor_total { set; get; }

        public DateTime fecha { set; get; }
        public string idrelacion { set; get; }
        public List<EntSPDianFacturasPendientes> listarFacturasPendientes { set; get; } = new List<EntSPDianFacturasPendientes>();
        public List<EntSPDianFacturasCreadas> listarFacturasCreadas { set; get; } = new List<EntSPDianFacturasCreadas>();
    }
}
