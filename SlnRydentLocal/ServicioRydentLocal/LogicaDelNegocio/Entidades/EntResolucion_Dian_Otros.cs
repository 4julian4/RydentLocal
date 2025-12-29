using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
	public class EntResolucion_Dian_Otros
	{
		public int id { get; set; }
		public int idResolucion_Dian { get; set; }
		public string tipoPersona { get; set; }
		public string identificadorFiscal { get; set; }
		public string regimen { get; set; }
		public string pais { get; set; }
		public string codigoPais { get; set; }
		public string departamento { get; set; }
		public string codigoDepartamento { get; set; }
		public string ciudad { get; set; }
		public string codigoCiudad { get; set; }
		public string obligacionContribuyente { get; set; }
		public string matriculaMercantil { get; set; }
		public string correo { get; set; }
		public string codigoMoneda { get; set; }
		public string porcentajeIva { get; set; }
		public DateTime fechaResolucionFin { get; set; }
		public string numeroAutorizacionFacturacion { get; set; }
		public string FTUsuario { get; set; }
		public string FTPswWebService { get; set; }
		public string FTRutaWebService { get; set; }
		public string FTAmbienteProduccion { get; set; }
		public string CodigoImpuesto { get; set; }
		public string NombreImpuesto { get; set; }
	}
}
