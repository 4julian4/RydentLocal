using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TRESOLUCION_DIAN")]
    public class TRESOLUCION_DIAN
    {
        [Key]
        public int ID { set; get; }
        public string? DESCRIPCION { set; get; }
        public DateTime? FECHA { set; get; }
        public string? RESOLUCION { set; get; }
        public string? NUMERACION { set; get; }
        public int? RANGOINI { set; get; }
        public int? RANGOFIN { set; get; }
        public string? PREFIJO { set; get; }
        public int? TAMANNO { set; get; }
        public int? TIPO { set; get; }
        public int? ID_REEMPLAZA { set; get; }
    }
}
