using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
    public class SP_RIPS_JSON_APResult
    {
        public string? NUMERO_FACTURA { get; set; }
        public string? CODIGO_PRESTADOR { get; set; }
        public string? DOCUMENTO_IDENTIDAD { get; set; }
        public string? IDANAMNESIS_TEXTO { get; set; }
        public DateTime? FECHAR_REALIZACION { get; set; }
        public string? NUMERO_AUTORIZACION { get; set; }
        public string? CODIGO_PROCEDIMIENTO { get; set; }
        public string? AMBITO_REALIZACION { get; set; }
        public string? FINALIDAD { get; set; }
        public string? PERSONAL { get; set; }
        public string? DIAGNOSTICO1 { get; set; }
        public string? DIAGNOSTICO2 { get; set; }
        public string? DIAGNOSTICO3 { get; set; }
        public string? REALIZACION_ACTO { get; set; }
        public string? COSTO { get; set; }
        public string? IDMIPRES { get; set; }
        public string? VIAINGRESOSERVICIOSALUD { get; set; }
        public string? MODALIDADGRUPOSERVICIOTECSAL { get; set; }
        public string? GRUPOSERVICIOS { get; set; }
        public string? CODSERVICIO { get; set; }
        public string? FINALIDADTECNOLOGIASALUD { get; set; }
        public string? CONCEPTORECAUDO { get; set; }
        public string? VALORPAGOMODERADOR { get; set; }
        public string? NUMFEVPAGOMODERADOR { get; set; }
        [Key]
        public int? CONSECUTIVO { get; set; }
    }
}
