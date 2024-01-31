using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("UNIDADREVISADA")]
    public class UNIDADREVISADA
    {
        [Key] 
        public Int32 IDUNIDADREVISADA { get; set; }
        public Int32? IDSILLA { get; set; }
        public DateTime? FECHA { get; set; }
        public TimeSpan? HORA { get; set; }
    }
}
