using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    public class T_FRASE_XEVOLUCION
    {
        [Key]
        public int ID { get; set; }
        public string CONTENIDO { get; set; }
        public string TITULO { get; set; }
        public int TIPO { get; set; }
    }
}
