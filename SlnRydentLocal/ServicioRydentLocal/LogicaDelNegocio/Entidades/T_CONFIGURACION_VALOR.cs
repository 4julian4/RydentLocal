using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("T_CONFIGURACION_VALOR")]
    public class T_CONFIGURACION_VALOR
    {
        [Key]
        public int ID { get; set; }
        public string? NOMBRE { get; set; }
        public string? VALOR { get; set; }
        public string? GRUPO { get; set; }
        public string? SUBGRUPO { get; set; }
    }
}
