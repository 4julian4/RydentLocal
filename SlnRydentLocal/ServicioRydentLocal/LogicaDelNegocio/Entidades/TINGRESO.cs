using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TINGRESO")]
    public class TINGRESO
    {
        [Key]
        public int ID { get; set; }
        public DateTime? FECHA { get; set; }
        public string? DETALLE_INGRESO { get; set; }
        public double? VALOR_INGRESO { get; set; }
        public int? EMPRESA { get; set; }
        public int IDINFORMACIONREPORTE { get; set; }
        public int? IDCUENTAXCOBRAR { get; set; }
        public int? NUMERO { get; set; }
        public string? TIPODEPAGO { get; set; }
        public string? CAJA { get; set; }
        public string? MOTIVO { get; set; }
        public int? IDFIRMA { get; set; }
        public string? FACTURA { get; set; }
        public string? RECIBO { get; set; }
        public string? CODIGO { get; set; }
        public int? IDRESOLUCIONDIAN { get; set; }
        public int? IVA { get; set; }
    }
    
}
