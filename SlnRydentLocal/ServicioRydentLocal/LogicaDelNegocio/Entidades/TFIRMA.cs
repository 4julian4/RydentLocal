using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("T_FIRMAS")]
    public class TFIRMA
    {
        [Key]
        public int ID { set; get; }
        public byte[] FIRMA { set; get; }
        public string FECHA { set; get; }
        public string HORA { set; get; }
        public string OBSERVACIONES { set; get; }
        public byte[] HUELLA { set; get; }

    }
}

