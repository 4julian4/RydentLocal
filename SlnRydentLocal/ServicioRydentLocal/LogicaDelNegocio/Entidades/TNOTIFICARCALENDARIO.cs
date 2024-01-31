using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TNOTIFICARCALENDARIO")]
    public class TNOTIFICARCALENDARIO
    {
        [Key]
        public Int32 ID { get; set; }
        public Int32? IDSILLA { get; set; }
        public DateTime? FECHA { get; set; }
        public TimeSpan? HORA { get; set; }
        public Int32? IDACCION { get; set; }
        public Int32? IDESTADO { get; set; }
    }
}
