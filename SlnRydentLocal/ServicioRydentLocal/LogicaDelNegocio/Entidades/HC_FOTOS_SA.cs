using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("HC_FOTOS_SA")]
    public class HC_FOTOS_SA
    {
        public int ID { get; set; }
        public Stream FOTO { get; set; }
        public string NOMBRE { get; set; }
        public int PERSONA { get; set; }
        public DateTime FECHA { get; set; }
        public int CATEGORIA { get; set; }
        public int IDDOCTOR { get; set; }
    }
}