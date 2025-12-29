using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
    public class P_Adicionales_Abonos_Dian
    {
        public int idAnamnesis { get; set; }
        public decimal valor { get; set; }
        public string descripcion { get; set; } = "";
        public string factura { get; set; } = "";
        public DateTime fechaFactura { get; set; }
		public DateTime fechaFacturaIni { get; set; }
		public DateTime fechaResolucion { get; set; }
        public string numeracion { get; set; } = "";
        public string resolucion { get; set; } = "";
        public int rangoIni { get; set; }
        public int rangoFin { get; set; }
        public string prefijo { get; set; } = "";
        public int idDatosdoctores { get; set; }
        public string nombre { get; set; } = "";
        public string direccion { get; set; } = "";
        public string telefono { get; set; } = "";
        public string especialidad { get; set; } = "";
        public string ciudad { get; set; } = "";
        public string nota_Factura { get; set; } = "";
        public string idAnamnesis_Texto { get; set; } = "";
        public string nombre_Paciente { get; set; } = "";
        public string direccion_Paciente { get; set; } = "";
        public string telf_P { get; set; } = "";
        public string ruta_Imagen { get; set; } = "";
        public string nombres { get; set; } = "";
        public string apellidos { get; set; } = "";
        public string nit { get; set; } = "";
        public DateTime HoraFactura { get; set; }
        public string correo_Paciente { get; set; } = "";
        public decimal ValorIva { get; set; }
        public decimal PorcentajeIva { get; set; }
        public string codigoMoneda { get; set; } = "";
        public string FTAmbienteProduccion { get; set; } = "";
        public string tipoPersona { get; set; } = "";
        public string identificadorFiscal { get; set; } = "";
        public string regimen { get; set; } = "";
        public string codigoDepartamento { get; set; } = "";
        public string codigoCiudad { get; set; } = "";
        public string codigoPais { get; set; } = "";
        public string departamento { get; set; } = "";
        public string pais { get; set; } = "";
        public string obligacionContribuyente { get; set; } = "";
        public string matriculaMercantil { get; set; } = "";
        public string correo { get; set; } = "";
        public string CodigoImpuesto { get; set; } = "";
        public string NombreImpuesto { get; set; } = "";
        public string numeroAutorizacionFacturacion { get; set; } = "";
        public DateTime fechaResolucionFin { get; set; }
        public string FTRutaWebService { get; set; } = "";
        public string FTPswWebService { get; set; } = "";
        public string FTUsuario { get; set; } = "";
        public int idResoliucionDian { get; set; }
        public int idRelacion { get; set; }
        public string transaccionId { get; set; } = "";
        public string FormaPago { get; set; } = "";
        public string codigoDepartamento_Paciente { get; set; } = "";
        public string codigoCiudad_Paciente { get; set; } = "";
        public string codigoPais_Paciente { get; set; } = "";
        public string Ciudad_Paciente { get; set; } = "";
        public string departamento_Paciente { get; set; } = "";
        public string pais_Paciente { get; set; } = "";
        public string Tipo_Documento_Paciente { get; set; } = "";
        public string codigo_Prestador { get; set; } = "";
    }
}
