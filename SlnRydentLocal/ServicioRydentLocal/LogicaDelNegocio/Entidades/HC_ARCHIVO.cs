using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("HC_ARCHIVO")]
    public class HC_ARCHIVO
    {
        public int ID { get; set; }
        public string DATOS { get; set; }
        public string NOMBRE { get; set; }
        public DateTime FECHA { get; set; }
        public string NOMBRE_FUENTE { get; set; }
        public int ID_ANAMNESIS { get; set; }
        public int? ID_HC_FOTOS { get; set; }
        public string? ID_EXTERNA { get; set; }
        public string EXT { get; set; }
        public string PREFIJO { get; set; }
        public string ARCHIVO_PREFIJO { get { return PREFIJO + DATOS; } }
    }
}