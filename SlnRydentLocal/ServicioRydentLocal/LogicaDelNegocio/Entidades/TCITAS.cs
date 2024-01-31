using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TCITAS")]
    public class TCITAS
    {
        public int SILLA { set; get; }
        public string? FECHA_TEXTO { set; get; }
        public DateTime FECHA { set; get; }
        
    }
}
