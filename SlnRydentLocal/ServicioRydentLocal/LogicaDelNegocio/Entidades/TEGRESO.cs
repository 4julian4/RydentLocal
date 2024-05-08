using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    public class TEGRESO
    {
        public int ID;
        public DateTime? FECHA { get; set; }
        public string? DETALLE_EGRESO { get; set; }
        public double? VALOR_EGRESO { get; set; }
        public int? EMPRESA { get; set; }
        public int IDINFORMACIONREPORTE { get; set; }
        public int? IDCUENTAXPAGAR { get; set; }
        public int? NUMERO { get; set; }
        public string? TIPODEPAGO { get; set; }
        public string? CAJA { get; set; }
        public string? MOTIVO { get; set; }
        public int? IDFIRMA { get; set; }
        public string? CODIGO { get; set; }
        public double? IVA { get; set; }
        public string? FACTURA { get; set; }
        public double? RETENCION { get; set; }
    }
    
}
