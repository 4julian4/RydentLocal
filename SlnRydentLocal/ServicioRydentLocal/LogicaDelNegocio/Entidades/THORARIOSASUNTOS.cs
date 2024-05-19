using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Keyless]
    public class THORARIOSASUNTOS
    {
        public string? SILLAS { get; set; }
        public string? ASUNTO { get; set; }
        public int? REPETICION { get; set; }
        public int? TIEMPO { get; set; }
        public string? INTERVALOS { get; set; }
    }
}
