using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("HC_FOTOS")]
    public class HC_FOTOS
    {
        public int ID { get; set; }
        public string FOTO { get; set; }
        public string NOMBRE { get; set; }
        public int PERSONA { get; set; }
        public DateTime FECHA { get; set; }
        public int CATEGORIA { get; set; }
        public int IDDOCTOR { get; set; }
        public string ARCHIVO { get; set; }
    }
}