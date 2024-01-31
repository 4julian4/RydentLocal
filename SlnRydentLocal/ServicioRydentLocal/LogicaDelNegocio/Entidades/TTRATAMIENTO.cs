using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TTRATAMIENTO")]
    public class TTRATAMIENTO
    {
        public int IDTRATAMIENTO { set; get; }
        public DateTime FECHA { set; get; }
        public int? NUMTRAT { set; get; }
        public int? ID_DOCTOR { set; get; }
        public string? CHAR_SALDO_PLAN_TRAT { set; get; }
        public float? VALOR { set; get; }
        public float? DESCUENTO { set; get; }
        public string? DESCUENTO_CHAR { set; get; }
        public string? DESCUENTO_PORC { set; get; }
        public string? DOCTOR { set; get; }
        public string? AUDITADO { set; get; }
        public DateTime? FECHA_ { set; get; }
        public int? ID { set; get; }
        public string? CATEGORIA_PACIENTE { set; get; }
        public int? ID_CATEGORIA_PACIENTE { set; get; }
        public string? CAMARA_PUVA { set; get; }
        public string? SOLOLECTURA { set; get; }
        public DateTime? FECHA_INICIO { set; get; }
        public DateTime? FECHA_TERMINAR { set; get; }
        public DateTime? FECHA_TERMINO { set; get; }
        public int? IDALUMNO { set; get; }
        public string? IDMATERIA { set; get; }
        public string? ESTADO { set; get; }
    }
}
