using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
    public class SP_RIPS_JSON_USResult
    {
        public string? DOCUMENTO_IDENTIDAD { get; set; }
        public string? IDANAMNESIS_TEXTO { get; set; }
        public string? CODIGO_PRESTADOR { get; set; }
        public string? TIPO_PACIENTE { get; set; }
        public string? APELLIDOS { get; set; }
        public string? APELLIDO_SEG { get; set; }
        public string? NOMBRES { get; set; }
        public string? NOMBRE_SEG { get; set; }
        public string? EDAD { get; set; }
        public string? TIPO_EDAD { get; set; }
        public string? SEXO { get; set; }
        public string? CODIGO_DEPARTAMENTO { get; set; }
        public string? CODIGO_CIUDAD { get; set; }
        public string? ZONA_RESIDENCIAL { get; set; }
        public string? FEV { get; set; }
        public string? TIPOUSUARIO { get; set; }
        public string? FECHANACIMIENTO { get; set; }
        public string? CODPAISRESIDENCIA { get; set; }
        public string? INCAPACIDAD { get; set; }
        public string? CODPAISORIGEN { get; set; }
        [Key]
        public int? CONSECUTIVO { get; set; }
        public string? NUMERO_FACTURA { get; set; }
        public string? NUMDOCUMENTOIDOBLIGADO { get; set; }
        public int? IDRELACION { get; set; }
    }
}
