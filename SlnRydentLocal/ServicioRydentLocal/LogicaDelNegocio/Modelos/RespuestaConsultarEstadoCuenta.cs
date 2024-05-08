using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class RespuestaConsultarEstadoCuenta
    {
        public int? ID { get; set; } //IDAnamnesis
        public int? FASE { get; set; }
        public int? IDDOCTOR { get; set; }
        public List<int>? lstFases { get; set; }//
        public List<P_CONSULTAR_ESTACUENTA>? P_CONSULTAR_ESTACUENTA { get; set; }
        public List<P_CONSULTAR_ESTACUENTAPACIENTE>? P_CONSULTAR_ESTACUENTAPACIENTE { get; set; }
        public List<RespuestaSaldoPorDoctor>? RespuestaSaldoPorDoctor { get; set; }
        public int? CONSECUTIVO { get; set; }
        public string? DESCRIPCION { get; set; }
        public decimal? costoTratamiento { get; set; }
        public decimal? pagosRealizados { get; set; }
        public decimal? descuentos { get; set; }
        public decimal? restante { get; set; }
        public decimal? ultimoAbono { get; set; }
        public decimal? saldoTotal { get; set; }
        public decimal? ideales { get; set; }
        public decimal? realizados { get; set; }
        public DateTime? fechaInicial { get; set; }
        public bool? tratamientoSinFinanciar{ get; set; }
        public bool? mensajeSinTratamiento { get; set; }

    }
}
   
