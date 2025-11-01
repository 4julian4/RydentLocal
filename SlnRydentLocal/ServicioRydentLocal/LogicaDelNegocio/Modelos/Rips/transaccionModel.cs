using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
    public class transaccionModel
    {
        public transaccionRipsModel rips { get; set; }
        public string? xmlFevFile { get; set; }
    }
    public class transaccionRipsModel
    {
        public string numDocumentoIdObligado { get; set; }
        public string numFactura { get; set; }
        public string? tipoNota { get; set; }
        public string? numNota { get; set; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? idRelacion { get; set; }
        public List<UsuariosModel> usuarios { get; set; } = new List<UsuariosModel>();
    }
}
