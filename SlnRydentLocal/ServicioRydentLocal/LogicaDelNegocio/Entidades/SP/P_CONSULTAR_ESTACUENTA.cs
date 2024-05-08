using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
    [Keyless]
    public class P_CONSULTAR_ESTACUENTA
    {
        public string? N_CUOTA { get; set; }
        public DateTime? FECHA { get; set; }
        public double? ABONO { get; set; }
        public double? ADICIONAL { get; set; }
        public double? MORA_ACTUAL { get; set; }
        public double? PARCIAL { get; set; }
        public double? MORATOTAL { get; set; }
        public double? VALOR_TRATAMIENTO { get; set; }
        public string? NUMERO_HISTORIA { get; set; }
        public double? DEBEABONAR { get; set; }
        public string? FACTURA { get; set; }
        public double? SALDO_PARCIAL { get; set; }
        public string? RECIBO { get; set; }
        public string? DESCRIPCION { get; set; }
        public string? DT_DESCRIPCION { get; set; }
        public byte[]? OBSERVACIONES { get; set; }
        public double? VALOR_CUOTA_INI { get; set; }
        public double? NUMERO_CUOTAS { get; set; }
        public double? VALOR_CUOTA { get; set; }
        public double? CUOTA_CUOTA_INI { get; set; }
        public int? NUMERO_CUOTA_INI { get; set; }
        public int? IDENTIFICADOR { get; set; }
        public int? IDRELACION { get; set; }
        public DateTime? FECHA_INICIO { get; set; }
        public int? RECIBIDO_POR { get; set; }
        public int? FIRMA { get; set; }
        public double? NOTACREDITO { get; set; }
        public string? RECIBIDO_X_NOMBRE { get; set; }
        public int? IDPRESUPUESTOMAESTRA { get; set; }
        public string? NOMBRE_RECIBE { get; set; }
        public double? VALORIVA { get; set; }
        public string? CODIGO_DESCRIPCION { get; set; }
        public double? VALOR_FACTURA { get; set; }
        public double? VALOR_A_FACTURAR { get; set; }
    }
}
