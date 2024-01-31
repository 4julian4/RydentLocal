using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TCONFIGURACIONCALENDARIO")]
    public class TCONFIGURACIONCALENDARIO
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { set; get; }
        public string? GRUPO { set; get; }
        public string? VALOR { set; get; }
        public string? VALORSECUNDARIO { set; get; }
        public string? NOMBRE { set; get; }
        public string? IDENUMERADOR { set; get; }
    }
}
