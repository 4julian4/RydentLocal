using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("HC_ARCHIVO_SA")]
    public class HC_ARCHIVO_SA
    {
        [Key]
        public int ID { get; set; }
        public string DATOS { get; set; }
        public string NOMBRE { get; set; }
        public DateTime FECHA { get; set; }
        public string NOMBRE_FUENTE { get; set; }
        public int ID_ANAMNESIS { get; set; }
        public int ID_HC_FOTOS { get; set; } = 0;
        public string ID_EXTERNA { get; set; }
    }
}