using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    public class T_RIPS_DX
    {
        [Key]
        public int ID { get; set; }
        public int? IDANAMNESIS { get; set; }
        public int? IDDOCTOR { get; set; }
        public string? FACTURA { get; set; }
        public string? CODIGOPRESTADOR { get; set; }
        public string? TIPOIDENTIFICACION { get; set; }
        public string? IDENTIFICACION { get; set; }
        public DateTime? FECHACONSULTA { get; set; }
        public string? NUMEROAUTORIZACION { get; set; }
        public string? CODIGOCONSULTA { get; set; }
        public string? FINALIDADCONSULTA { get; set; }
        public string? CAUSAEXTERNA { get; set; }
        public string? DX1 { get; set; }
        public string? DX2 { get; set; }
        public string? DX3 { get; set; }
        public string? DX4 { get; set; }
        public string? TIPODIAGNOSTICO { get; set; }
        public double? VALORCONSULTA { get; set; }
        public double? VALORCUOTAMODERADORA { get; set; }
        public double? VALORNETO { get; set; }
        public string? CODIGOENTIDAD { get; set; }
        public string? EXTRANJERO { get; set; }
        public string? PAIS { get; set; }
    }
}
