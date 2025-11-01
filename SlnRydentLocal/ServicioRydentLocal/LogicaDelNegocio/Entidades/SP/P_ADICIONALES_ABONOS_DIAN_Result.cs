using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
    public class P_ADICIONALES_ABONOS_DIAN_Result
    {
        [Key]
        public int IDANAMNESIS { get; set; }
        public decimal VALOR { get; set; }
        public string DESCRIPCION { get; set; } = "";
        public string FACTURA { get; set; } = "";
        public DateTime FECHAFACTURA { get; set; }
        public DateTime FECHARESOLUCION { get; set; }
        public string NUMERACION { get; set; } = "";
        public string RESOLUCION { get; set; } = "";
        public int RANGOINI { get; set; }
        public int RANGOFIN { get; set; }
        public string PREFIJO { get; set; } = "";
        public int IDDATOSDOCTORES { get; set; }
        public string NOMBRE { get; set; } = "";
        public string DIRECCION { get; set; } = "";
        public string TELEFONO { get; set; } = "";
        public string ESPECIALIDAD { get; set; } = "";
        public string CIUDAD { get; set; } = "";
        public string NOTA_FACTURA { get; set; } = "";
        public string IDANAMNESIS_TEXTO { get; set; } = "";
        public string NOMBRE_PACIENTE { get; set; } = "";
        public string DIRECCION_PACIENTE { get; set; } = "";
        public string TELF_P { get; set; } = "";
        public string RUTA_IMAGEN { get; set; } = "";
        public string NOMBRES { get; set; } = "";
        public string APELLIDOS { get; set; } = "";
        public string NIT { get; set; } = "";
        public DateTime HORAFACTURA { get; set; }
        public string CORREO_PACIENTE { get; set; } = "";
        public decimal VALORIVA { get; set; }
        public decimal PORCENTAJEIVA { get; set; }
        public string CODIGOMONEDA { get; set; } = "";
        public string FTAMBIENTEPRODUCCION { get; set; } = "";
        public string TIPOPERSONA { get; set; } = "";
        public string IDENTIFICADORFISCAL { get; set; } = "";
        public string REGIMEN { get; set; } = "";
        public string CODIGODEPARTAMENTO { get; set; } = "";
        public string CODIGOCIUDAD { get; set; } = "";
        public string CODIGOPAIS { get; set; } = "";
        public string DEPARTAMENTO { get; set; } = "";
        public string PAIS { get; set; } = "";
        public string OBLIGACIONCONTRIBUYENTE { get; set; } = "";
        public string MATRICULAMERCANTIL { get; set; } = "";
        public string CORREO { get; set; } = "";
        public string CODIGOIMPUESTO { get; set; } = "";
        public string NOMBREIMPUESTO { get; set; } = "";
        public string NUMEROAUTORIZACIONFACTURACION { get; set; } = "";
        public DateTime FECHARESOLUCIONFIN { get; set; }
        public string FTRUTAWEBSERVICE { get; set; } = "";
        public string FTPSWWEBSERVICE { get; set; } = "";
        public string FTUSUARIO { get; set; } = "";
        public int IDRESOLIUCIONDIAN { get; set; }
        public int IDRELACION { get; set; }
        public string TRANSACCIONID { get; set; } = "";
        public string FORMAPAGO { get; set; } = "";
        public string CODIGODEPARTAMENTO_PACIENTE { get; set; } = "";
        public string CODIGOCIUDAD_PACIENTE { get; set; } = "";
        public string CODIGOPAIS_PACIENTE { get; set; } = "";
        public string CIUDAD_PACIENTE { get; set; } = "";
        public string DEPARTAMENTO_PACIENTE { get; set; } = "";
        public string PAIS_PACIENTE { get; set; } = "";
        public string TIPO_DOCUMENTO_PACIENTE { get; set; } = "";
        public string CODIGO_PRESTADOR { get; set; } = "";
    }
}
