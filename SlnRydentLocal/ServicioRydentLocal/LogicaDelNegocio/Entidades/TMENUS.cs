using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TMENUS")]
    public class TMENUS
    {
        [Key]
        public int ID { set; get; }
        public string NOMBRE { set; get; }
        public string? IDENUMARACION { set; get; }
        public Int16? ACTIVO { set; get; }
    }
}
