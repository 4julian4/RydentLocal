using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class RespuestaConsultarPorDiaYPorUnidadModel
    {
        public TCITAS cita { set; get; } = new TCITAS();
        public List<TDETALLECITAS> lstDetallaCitas { get; set; } = new List<TDETALLECITAS>();
    }
}
