using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    public class THISTORIAL
    {
        public string DESCRIPCION { get; set; }
        public DateTime FECHA { get; set; }
        public TimeSpan HORA { get; set; }
        public string USUARIO { get; set; }
        public int IDANAMNESIS { get; set; }

    }
    
}
