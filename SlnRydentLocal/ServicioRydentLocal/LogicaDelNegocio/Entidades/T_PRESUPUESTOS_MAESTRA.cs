using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    public class T_PRESUPUESTOS_MAESTRA
    {
        [Key]
        public int ID { get; set; }
        public string? NOMBRE { get; set; }
        public DateTime? FECHA { get; set; }
        public int? ID_PLAN_TTO { get; set; }
        public int? ID_ANAMNESIS { get; set; }
        public int? NUM_PRESUPUESTO { get; set; }
        public decimal? COSTO { get; set; }
        public decimal? CUOTA_INICIAL { get; set; }
        public int? INTERVALO { get; set; }
        public int? NUMERO_CUOTAS { get; set; }
        public decimal? VALOR_CUOTA { get; set; }
        public string? DESCRIPCION { get; set; }
        public string? OBSERVACIONES { get; set; }
        public decimal? DESCUENTO_PORCENTAJE { get; set; }
    }
    
}
