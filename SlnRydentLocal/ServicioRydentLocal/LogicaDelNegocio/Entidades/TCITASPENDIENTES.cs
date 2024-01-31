using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TCITASPENDIENTES")]
    public class TCITASPENDIENTES
    {
        [Key]
        public int ID { set; get; }
        public string? IDCALENDARIO { set; get; }
        public DateTime? FECHA { set; get; }
        public TimeSpan? HORA { set; get; }
        public string? NOMBRE { set; get; }
        public string? ASUNTO { set; get; }
        public int? SILLA { set; get; }
        public string? DURACION { set; get; }
    }
}
