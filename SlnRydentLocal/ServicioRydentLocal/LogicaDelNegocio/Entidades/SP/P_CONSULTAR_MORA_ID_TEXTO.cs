using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
    [Keyless]
    public class P_CONSULTAR_MORA_ID_TEXTO
    {
        public  double? MORA { get; set; }
        public  double? CARTERA { get; set; }
    }
}
