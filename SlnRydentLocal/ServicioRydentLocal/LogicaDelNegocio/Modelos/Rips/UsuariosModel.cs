using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
    public class UsuariosModel
    {
        public string tipoDocumentoIdentificacion { get; set; }
        public string numDocumentoIdentificacion { get; set; }
        public string? tipoUsuario { get; set; }
        public string? fechaNacimiento { get; set; }
        public string codSexo { get; set; }
        public string codPaisResidencia { get; set; }
        public string? codMunicipioResidencia { get; set; }
        public string? codZonaTerritorialResidencia { get; set; }
        public string incapacidad { get; set; }
        public int consecutivo { get; set; }
        public string? codPaisOrigen { get; set; }
        public ServiciosModel servicios { get; set; }
    }
}
