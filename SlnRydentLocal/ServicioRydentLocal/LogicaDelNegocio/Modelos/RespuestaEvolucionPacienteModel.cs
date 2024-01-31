using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class RespuestaEvolucionPacienteModel
    {
        public TEVOLUCION evolucion { set; get; }
        public string imgFirma { set; get; }
    }
}
