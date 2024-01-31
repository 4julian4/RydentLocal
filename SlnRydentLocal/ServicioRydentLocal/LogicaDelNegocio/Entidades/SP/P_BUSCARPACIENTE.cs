using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
    [Keyless]
    public class P_BUSCARPACIENTE
    {
        public int IDANAMNESIS { set; get; }
        public string NOMBRE_PACIENTE { set; get; }
        public string IDANAMNESISTEXTO { set; get; }
        public string NUMDOCUMENTO { set; get; }
        public string DOCTOR { set; get; }
        public string PERFIL { set; get; }
        public string NUMAFILIACION { set; get; }
        public string TELEFONO { set; get; }
    }
}
