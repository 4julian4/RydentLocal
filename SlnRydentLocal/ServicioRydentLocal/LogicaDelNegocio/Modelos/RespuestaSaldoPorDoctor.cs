using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    [Keyless]
    public class RespuestaSaldoPorDoctor
    {
        public string? DOCTOR { get; set; } 
        public decimal? VALOR_TOTAL { get; set; }
        public decimal? ABONOS { get; set; }
    }
}
