using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TPLANTRATAMIENTO")]
    public class TPLANTRATAMIENTO
    {
        public int ID { set; get; }
        public int IDPLANTRATAMIENTO { set; get; }
        public int INDEXPLANTRATSEC { set; get; }
        public DateTime FECHA_TRAT { set; get; }
        public string? DESCRIPCION { set; get; }
        public string? DIENTE { set; get; }
        public double? COSTO { set; get; }
        public DateTime? FECHA { set; get; }
        public string? COBRO { set; get; }
        public string? CHAR_COSTO { set; get; }
        public string? REALIZADO_SI_NO { set; get; }
        public double? TOTAL { set; get; }
        public string? CHAR_TOTAL { set; get; }
        public DateTime? TERMINACION { set; get; }
        public int? PLAZO { set; get; }
        public DateTime? FECHATRATAMIENTO { set; get; }
        public int? DURACION { set; get; }
        public string? CODIGO_PROCEDIMIENTO { set; get; }
        public string? NUMERO_FACTURA { set; get; }
        public string? NUMERO_AUTORIZACION { set; get; }
        public string? AMBITO_REALIZACION { set; get; }
        public string? FINALIDAD { set; get; }
        public string? PERSONAL { set; get; }
        public string? DIAGNOSTICO1 { set; get; }
        public string? DIAGNOSTICO2 { set; get; }
        public string? DIAGNOSTICO3 { set; get; }
        public string? REALIZACION_ACTO { set; get; }
        public DateTime? FECHAR_REALIZACION { set; get; }
        public string? EMBARAZO { set; get; }
        public int? CANTIDADTRATAMIENTOS { set; get; }
        public string? DOCTOR { set; get; }
        public string? NOMBRE_COD_PROCEDIMIENTO { set; get; }
        public string? CONTROLADO { set; get; }
        public string? PASAR_COBRAR { set; get; }
        public string? PAGO_REALIZADO { set; get; }
        public int? CANTIDAD { set; get; }
        public int ? IDCONVENIO { set; get; }
        public DateTime? FECHA_SUCESO { set; get; }
        public DateTime? HORA_SUCESO { set; get; }
        public int? IDPAQUETE { set; get; }
        public int? NUMSUPERFICIES { set; get; }
        public int? ID_ODONTOGRAMA_PROCEDIMIENTO { set; get; }
        public string? DESDEODONTOGRAMA { set; get; }
        public string? EPS { set; get; }
        public string? AUDITADA { set; get; }
        public string? CREADO_POR { set; get; }
        public int? CALIFICADO { set; get; }
        public string? CALIFICADO_POR { set; get; }
        public double? NOTA { set; get; }
        public double? COPAGO { set; get; }
        public int? IDFIRMA { set; get; }
        public string? DOCTOR_REALIZA { set; get; }
    }
}


  
  
  
 
  