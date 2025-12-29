using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
    public class EntSPDianFacturasPendientes
    {
        public int idResolucion_Dian { get; set; }
        public string prestador { get; set; }
        public string codigo_Prestador { get; set; }
        public string codigo_Prestador_Ppal { get; set; }
        public string nombre_Paciente { get; set; }
        public string documento { get; set; }
        public string tipo_Documento { get; set; }
        public string factura { get; set; }
        public decimal valor_Total { get; set; }
        public DateTime fecha { get; set; }
        public int idRelacion { get; set; }
        public int tipoFactura { get; set; }
        public string tipoOperacion { get; set; }
        public string DOCUMENTO_RESPONS { get; set; }
        public string NOMBRE_RESPONS { get; set; }
        public int TIENERESPONSABLE { get; set; } = 0;
        public string ERRORES { get; set; }
    }
}
