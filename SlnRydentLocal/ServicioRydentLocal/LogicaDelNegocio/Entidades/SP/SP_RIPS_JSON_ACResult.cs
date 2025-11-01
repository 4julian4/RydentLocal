using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
    public class SP_RIPS_JSON_ACResult
    {
        public string? NUMERO_FACTURA { get; set; }
        public string? CODIGO_PRESTADOR { get; set; }
        public string? DOCUMENTO_IDENTIDAD { get; set; }
        public string? IDANAMNESIS_TEXTO { get; set; }
        public DateTime? FECHA_TRAT { get; set; }
        public string? NUMERO_AUTORIZACION { get; set; }
        public string? CODIGO_CONSULTA { get; set; }
        public string? CODIGO_FINALIDAD { get; set; }
        public string? CODIGO_CAUSA { get; set; }
        public string? DIAGNOSTICO_1 { get; set; }
        public string? DIAGNOSTICO_2 { get; set; }
        public string? DIAGNOSTICO_3 { get; set; }
        public string? DIAGNOSTICO_4 { get; set; }
        public string? CODIGO_TIPO_DIAGNOSTICO { get; set; }
        public string? VALOR_CONSULTA { get; set; }
        public string? VALOR_CUOTA { get; set; }
        public string? VALOR_NETO { get; set; }
        public string? MODALIDADGRUPOSERVICIOTECSAL { get; set; }
        public string? GRUPOSERVICIOS { get; set; }
        public string? CODSERVICIO { get; set; }
        public string? FINALIDADTECNOLOGIASALUD { get; set; }
        public string? CONCEPTORECAUDO { get; set; }
        public string? NUMFEVPAGOMODERADOR { get; set; }
        [Key]
        public int? CONSECUTIVO { get; set; }
    }
}
