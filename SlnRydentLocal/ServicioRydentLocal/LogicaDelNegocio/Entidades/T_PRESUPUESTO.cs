using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("T_PRESUPUESTO")]
    public class T_PRESUPUESTO
    {
        public int ID { get; set; }
        public int? ID_PLANTTO { get; set; }
        public decimal? COSTO { get; set; }
        public decimal? DESCUENTO { get; set; }
        public int? ID_MAESTRA { get; set; }
        public string? PROCEDIMIENTO { get; set; }
        public string? DIENTE { get; set; }
        public string? CODIGO_PROCEDIMIENTO { get; set; }
        public int? IDCONVENIO { get; set; }
        public int? IDREMITIDO_A { get; set; }
        public string? DOCTOR { get; set; }

    }
}


