using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TCONFIGURACIONES_RYDENT")]
    public class TCONFIGURACIONES_RYDENT
    {
        [Key]
        public int ID { get; set; }
        public string? NOMBRE { get; set; }
        public int? PERMISO { get; set; } = 0;
    }
}
