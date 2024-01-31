using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TMENUSITEMS")]
    public class TMENUSITEMS
    {
        [Key]
        public int ID { set; get; }
        public int IDMENU { set; get; }
        public string? NOMBRE { set; get; }
        public string? IDENUMARACION { set; get; }
        public int? ORDEN { set; get; }
        public Int16? ACTIVO { set; get; }
        public string? VALOR { set; get; }
        public string? VALORSECUNDARIO { set; get; }
    }
}
