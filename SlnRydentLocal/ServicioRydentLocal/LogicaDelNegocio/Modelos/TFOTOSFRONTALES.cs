using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class TFOTOSFRONTALES
    {
        [Key]
        public int IDANAMNESIS { get; set; }
        public byte[] FOTOFRENTE { get; set; }

    }
    
}
