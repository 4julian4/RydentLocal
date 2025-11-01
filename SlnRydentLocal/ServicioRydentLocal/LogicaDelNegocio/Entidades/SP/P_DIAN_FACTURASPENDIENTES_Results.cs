using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
    public class P_DIAN_FACTURASPENDIENTES_Results
    {
        public int IDRESOLUCION_DIAN { get; set; }
        public string PRESTADOR { get; set; }
        public string CODIGO_PRESTADOR { get; set; }
        public string NOMBRE_PACIENTE { get; set; }
        public string DOCUMENTO { get; set; }
        public string TIPO_DOCUMENTO { get; set; }
        public string FACTURA { get; set; }
        public decimal VALOR_TOTAL { get; set; }
        public DateTime FECHA { get; set; }
        [Key]
        public int IDRELACION { get; set; }
        public int TIPOFACTURA { get; set; }
        public string DOCUMENTO_RESPONS { get; set; }
        public string NOMBRE_RESPONS { get; set; }
        public int TIENERESPONSABLE { get; set; } = 0;
        public string ERRORES { get; set; }
    }
}
