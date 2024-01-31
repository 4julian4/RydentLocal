using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("T_CUENTASXCOBRAR")]
    public class T_CUENTASXCOBRAR
    {
        [Key]
        public int ID { get; set; }
        public DateTime? FECHA { get; set; }
        public string? DETALLE { get; set; }
        public int? IDEMPRESA { get; set; }
        public int? IDINFORMACIONREPORTE { get; set; }
        public DateTime? FECHAVENCE { get; set; }
        public double? VALOR { get; set; }
        public int? NUMERO { get; set; }
        public int? IDFIRMA { get; set; }
        public string? FACTURA { get; set; }

        public string? CONCEPTO { get; set; }
        public double? IVA { get; set; }
        public string? TRANSACCIONID { get; set; }
    }
}

