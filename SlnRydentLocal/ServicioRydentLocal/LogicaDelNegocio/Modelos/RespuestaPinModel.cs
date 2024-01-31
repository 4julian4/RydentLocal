using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class RespuestaPinModel
    {
        public TCLAVE clave { set; get; }=new TCLAVE();
        public List<ListadoItemModel> lstDoctores { set; get; } = new List<ListadoItemModel>();
    }
}
