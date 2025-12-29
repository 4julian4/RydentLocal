using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
    internal class SolicitarTokenModel
    {
        public PersonaSolicitarTokenModel persona { get; set; } = new PersonaSolicitarTokenModel();
        public string nit { get; set; }
        public string clave { get; set; }
    }
    public class PersonaSolicitarTokenModel
    {
        public TipoIdentificacionModel identificacion { get; set; } = new TipoIdentificacionModel();
    }
    public class TipoIdentificacionModel
    {
        public string tipo { get; set; }
        public string numero { get; set; }
    }
    public class RespuestaSolicitarTokenModel
    {
        public string token { get; set; }
        public bool login { get; set; }
        public bool registrado { get; set; }
        public string errors { get; set; }
    }
}
