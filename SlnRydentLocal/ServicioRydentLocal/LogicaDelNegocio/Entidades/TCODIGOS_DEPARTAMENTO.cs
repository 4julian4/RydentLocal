using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    public class TCODIGOS_DEPARTAMENTO
    {
        [Key] 
        public string CODIGO_DEPARTAMENTO { get; set; }
        public string NOMBRE { get; set; }
    }
}
