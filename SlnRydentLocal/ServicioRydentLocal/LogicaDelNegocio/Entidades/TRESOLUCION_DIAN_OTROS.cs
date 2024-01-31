using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TRESOLUCION_DIAN_OTROS")]
    public class TRESOLUCION_DIAN_OTROS
    {
        [Key]
        public int ID { get; set; }
        public int IDRESOLUCION_DIAN { get; set; }
        public string?  TIPOPERSONA { get; set; }
        public string?  IDENTIFICADORFISCAL { get; set; }
        public string?  REGIMEN { get; set; }
        public string?  CODIGOIMPUESTO { get; set; }
        public string?  NOMBREIMPUESTO { get; set; }
        public string?  PAIS { get; set; }
        public string?  CODIGOPAIS { get; set; }
        public string?  DEPARTAMENTO { get; set; }
        public string?  CODIGODEPARTAMENTO { get; set; }
        public string?  CIUDAD { get; set; }
        public string?  CODIGOCIUDAD { get; set; }
        public string?  OBLIGACIONCONTRIBUYENTE { get; set; }
        public string?  MATRICULAMERCANTIL { get; set; }
        public string?  CORREO { get; set; }
        public string?  CODIGOMONEDA { get; set; }
        public string?  PORCENTAJEIVA { get; set; }
        public DateTime? FECHARESOLUCIONFIN { get; set; }
        public string?  NUMEROAUTORIZACIONFACTURACION { get; set; }
        public string?  FTUSUARIO { get; set; }
        public string?  FTPSWWEBSERVICE { get; set; }
        public string?  FTRUTAWEBSERVICE { get; set; }
        public string?  FTAMBIENTEPRODUCCION { get; set; }
    }
}


