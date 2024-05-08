using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    public class T_DEFINICION_TRATAMIENTO
    {
        public int ID { get; set; }
        public int FASE { get; set; }
        public int IDDOCTOR { get; set; }
        public string? NUMERO_HISTORIA { get; set; }
        public double? VALOR_TRATAMIENTO { get; set; }
        public double? VALOR_CUOTA_INI { get; set; }
        public DateTime? FECHA_INICIO { get; set; }
        public int? NUMERO_CUOTA_INI { get; set; }
        public int? NUMERO_CUOTAS { get; set; }
        public double? VALOR_CUOTA { get; set; }
        public int? INTERVALO_TIEMPO { get; set; }
        public string? DESCRIPCION { get; set; }
        public string? FACTURA { get; set; }
        public int? INTERVALO_INI { get; set; }
        public byte[]? OBSERVACIONES { get; set; }
        public string? IDODONTOLOGO { get; set; }
        public int? RELACIONTRATAMIENTO { get; set; }
        public int? CONVENIO { get; set; }
        public int? IDPRESUPUESTOMAESTRA { get; set; }
        public short? FACTURAAUTOMATICA { get; set; }
        public int? CONSECUTIVO { get; set; }
        public short? VIEJO { get; set; }
    }
}
