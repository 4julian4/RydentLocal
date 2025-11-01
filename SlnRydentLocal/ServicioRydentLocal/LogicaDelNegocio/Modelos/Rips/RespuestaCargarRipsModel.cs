using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
    public class RespuestaCargarRipsModel
    {
        public bool? resultState { get; set; }
        public int? procesoId { get; set; }
        public string? numFactura { get; set; }
        public string? codigoUnicoValidacion { get; set; }
        public string? codigoUnicoValidacionToShow { get; set; }
        public DateTime? fechaRadicacion { get; set; }
        public string? rutaArchivos { get; set; }
        public List<ResultadosValidacionModel> resultadosValidacion { get; set; }
    }

    public class ResultadosValidacionModel
    {
        public string? Clase { get; set; }
        public string? Codigo { get; set; }
        public string? Descripcion { get; set; }
        public string? Observaciones { get; set; }
        public string? PathFuente { get; set; }
        public string? Fuente { get; set; }
    }
}
