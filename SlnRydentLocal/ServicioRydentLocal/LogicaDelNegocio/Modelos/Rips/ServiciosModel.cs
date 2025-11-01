using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
    public class ServiciosModel
    {
        public List<ConsultasModel>? consultas { get; set; }
        public List<ProcedimientosModel>? procedimientos { get; set; }
    }
}
