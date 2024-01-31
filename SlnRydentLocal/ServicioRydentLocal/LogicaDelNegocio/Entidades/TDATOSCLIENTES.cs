using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TDATOSCLIENTES")]
    public class TDATOSCLIENTES
    {
        [Key]
        public string NOMBRE { set; get; }
        public string? DIRECCION { set; get; }
        public string? TELEFONO { set; get; }
        public string? PROFESION { set; get; }
        public string? CIUDAD { set; get; }
        public string? RYDENT { set; get; }
        public string? ENTRADA { set; get; }
        public string? SERVIDOR { set; get; }
        public string? TP { set; get; }
        public string? CLAVE { set; get; }
        public string? PRIORIDAD { set; get; }
        public string? IP { set; get; }
        public string? CODIGO_PRESTADOR { set; get; }
        public string? NOMBRE_SEDE { set; get; }
        public int? CODIGO_SEDE { set; get; }
        public string? NUEVO { set; get; }
        public DateTime? FECHA_AUTORIZADA { set; get; }
        public string? DESCRIPCION { set; get; }
        public string? TIPO_ID { set; get; }
        public string? NUMERO_ID { set; get; }
        public string? RAZON { set; get; }
        public string? NUMERO_CONTRATO { set; get; }
        public string? PLAN_BENEFICIOS { set; get; }
        public string? NUMERO_POLIZA { set; get; }
    }
}

