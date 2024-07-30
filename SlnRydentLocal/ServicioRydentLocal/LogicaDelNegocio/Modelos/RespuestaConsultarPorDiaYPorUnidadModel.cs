using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class RespuestaConsultarPorDiaYPorUnidadModel
    {
        public TCITAS citas { set; get; } = new TCITAS();
        public List<P_AGENDA1> lstP_AGENDA1 { get; set; } = new List<P_AGENDA1>();
        public List<TDETALLECITAS> lstDetallaCitas { get; set; } = new List<TDETALLECITAS>();
        public TDETALLECITAS? detalleCitaEditar { get; set; }
        public bool esFestivo { get; set; }=false;
        public List<ConfirmacionesPedidasModel>? lstConfirmacionesPedidas { get; set; }
        public bool terminoRefrescar { get; set; } = false;
    }
}
