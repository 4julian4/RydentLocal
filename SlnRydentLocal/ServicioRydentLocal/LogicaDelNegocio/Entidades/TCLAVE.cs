using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TCLAVE")]
    public class TCLAVE
    {
        [Key]
        public string CLAVE { set; get; }
        public string? USUARIO { set; get; }
        public string? CATEGORIA { set; get; }
        public int? ACCESO { set; get; }
        public int? COD_USUARIO { set; get; }
        public int? IDBOTONES { set; get; }
        public int? IDBODEGA { set; get; }
        public int? IDCATEGORIA { set; get; }
        public int? IDDOCTOR { set; get; }

    }
}
