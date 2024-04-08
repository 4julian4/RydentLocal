using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class ConfirmacionesPedidasModel
    {
        public string mensaje { get; set; }
        public string nombreConfirmacion { get; set; }
        public bool esMensajeRestrictivo { get; set; } = false;
        public bool pedirConfirmar { get; set; } = false;
    }
}
