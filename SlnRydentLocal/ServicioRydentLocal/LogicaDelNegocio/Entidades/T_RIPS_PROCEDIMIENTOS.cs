using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    public class T_RIPS_PROCEDIMIENTOS
    {
        [Key]
        public int ID { get; set; }
        public int? IDANAMNESIS { get; set; }
        public int? IDDOCTOR { get; set; }
        public string? FACTURA { get; set; }
        public string? CODIGOPRESTADOR { get; set; }
        public string? TIPOIDENTIFICACION { get; set; }
        public string? IDENTIFICACION { get; set; }
        public DateTime? FECHAPROCEDIMIENTO { get; set; }
        public string? NUMEROAUTORIZACION { get; set; }
        public string? CODIGOPROCEDIMIENTO { get; set; }
        public string? AMBITOREALIZACION { get; set; }
        public string? FINALIDADPROCEDIMIENTI { get; set; }
        public string? PERSONALQUEATIENDE { get; set; }
        public string? DXPRINCIPAL { get; set; }
        public string? DXRELACIONADO { get; set; }
        public string? COMPLICACION { get; set; }
        public string? FORMAREALIZACIONACTOQUIR { get; set; }
        public double? VALORPROCEDIMIENTO { get; set; }
        public string? CODIGOENTIDAD { get; set; }
        public string? EXTRANJERO { get; set; }
        public string? PAIS { get; set; }
    }
}
