using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class RespuestaValidacionesAgendaModel
    {
        public string mensaje { get; set; }
        public bool respuesta { get; set; }
        public bool pedirConfirmacion { get; set; }=false;
        public List<ConfirmacionesPedidasModel> lstConfirmacionesPedidas { get; set; } = new List<ConfirmacionesPedidasModel>();
    }
}
