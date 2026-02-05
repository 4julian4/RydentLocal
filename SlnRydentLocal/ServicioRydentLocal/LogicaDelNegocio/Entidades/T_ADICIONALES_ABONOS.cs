using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("T_ADICIONALES_ABONOS")]
    public class T_ADICIONALES_ABONOS
    {
        public int ID { set; get; }
        public int? FASE { set; get; }
        public int? TIPO { set; get; }
        public decimal? VALOR { set; get; }
        public DateTime? FECHA { set; get; }
        public string? DESCRIPCION { set; get; }
        public string? RECIBO { set; get; }
        public int? IDENTIFICADOR { set; get; }
        public TimeSpan? HORA { set; get; }
        public string? IDODONTOLOGO { set; get; }
        public int? IDDOCTOR { set; get; }
        [Key]
        public int? IDRELACION { set; get; }
        public int? IDRESOLIUCIONDIAN { set; get; }
        public int? DETALLE_TTO { set; get; }
        public string? FACTURA { set; get; }
        public int? RECIBIDO_POR { set; get; }
        public int? FIRMA { set; get; }
        public int? CONIVA { set; get; }
        public decimal? VALORIVA { set; get; }
        public int? PAGOTERCERO { set; get; }
        public string? NOMBRE_RECIBE { set; get; }
        public string? CODIGO_DESCRIPCION { set; get; }
        public string? CONSECUTIVO_NOTACREDITO { set; get; }
        public string? NC_ELABORADO_POR { set; get; }
        public string? NC_APROBADO_POR { set; get; }
        public string? RECIBO_RELACIONADO { set; get; }
        public string? TRANSACCIONID { set; get; }
        public int? COPAGO { set; get; }
        public string? CONCEPTOCOPAGO { set; get; }
		public string? COPAGOFACTURARELACIONADA { set; get; }
		public string? COBERTURA { set; get; }
		public string? NUMERO_POLIZA { set; get; }
		public string? TIPOOPERACION { set; get; }
		//public DateTime? FECHASUCESO { set; get; }
        //public string? MOTIVO { set; get; }
    }
}



 