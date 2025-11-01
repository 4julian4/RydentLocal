using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TINFORMACIONREPORTES")]
    public class TINFORMACIONREPORTES
    {
        [Key]
        public int ID { get; set; }
        public string? NOMBRE { get; set; }
        public string? DIRECCION { get; set; }
        public string? TELEFONO { get; set; }
        public string? ESPECIALIDAD { get; set; }
        public string? OBSERVACIONES { get; set; }
        public string? CIUDAD { get; set; }
        public string? RUTA_IMAGEN { get; set; }
        public int? IDRESOLUCION_DIAN { get; set; }
        public string? NIT { get; set; }
        public string? CODIGO_PRESTADOR { get; set; }
        public string? NOTA_PRESUPUESTO { get; set; }
        public string? NOTA_FACTURA { get; set; }
        public string? NOTA_RECIBO { get; set; }
        public string? NOTA_CXC { get; set; }
        public string? DESCRIPCION_FACTURA { get; set; }
        public string? NOTA_CONTROL_PAGOS { get; set; }
        public string? TIPO_ID { get; set; }
        public string? NUMERO_CONTRATO { get; set; }
        public string? PLAN_BENEFICIOS { get; set; }
        public string? NUMERO_POLIZA { get; set; }
        public string? CODIGO_ENTIDAD { get; set; }
        public string? NUMDOCUMENTOIDOBLIGADO { get; set; }
        public string? ENTIDADXDEFECTO { get; set; }
        public string? IDSISPRO { get; set; }
        public string? PASSISPRO { get; set; }
        public Int16? CONFACTURA { get; set; }
    }
}