using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.TablasFraccionadas.TAnamnesis
{
    
    public class DatosPersonales : TAnamnesisBase
    {
        [Key]
        public int IDANAMNESIS { set; get; }
        public string? IDANAMNESIS_TEXTO { set; get; }
        public string? IMPORTANTE { set; get; }
        public string? NOTA_IMPORTANTE { set; get; }
        public long? COMPARACION { set; get; }
        public string? FECHA_INGRESO { set; get; }
        public DateTime? FECHA_INGRESO_DATE { set; get; }
        public TimeSpan? HORA_INGRESO { set; get; }
        public string? NOMBRES { set; get; }
        public string? APELLIDOS { set; get; }
        public string? NOMBRE_PACIENTE { get; set; }
        public string? FECHAN_DIA { get; set; }
        public string? FECHAN_MES { set; get; }
        public string? FECHAN_ANO { set; get; }
        public string? DOCUMENTO_IDENTIDAD { set; get; }
        public string? SEXO { set; get; }
        public string? EDAD { set; get; }
        public string? EDADMES { set; get; }
        public string? DIRECCION_PACIENTE { set; get; }
        public string? TELF_P { set; get; }
        public string? TELF_P_OTRO { set; get; }
        public string? CELULAR_P { set; get; }
        public string? NOMBRE_RESPONS { set; get; }
        public string? DIRECCION_RESPONSABLE { set; get; }
        public string? TELF_RESP { set; get; }
        public string? TELF_OF_RESP { set; get; }
        public string? CELULAR_RESPONSABLE { set; get; }
        public int? BEEPER_RESPONSABLE { set; get; }
        public int? COD_BEEPR_RESP { set; get; }
        public string? E_MAIL_RESP { set; get; }
        public string? REFERIDO_POR { set; get; }
        public string? NRO_AFILIACION { set; get; }
        public string? CONVENIO { set; get; }
        public string? ESTADO_TRATAMIENTO { set; get; }
        public string? TIPO_PACIENTE { set; get; }
        public string? CEDULA_NUMERO { set; get; }
        public string? ESTADOCIVIL { set; get; }
        public string? PARENTESCO { set; get; }
        public string? NIVELESCOLAR { set; get; } 
        public string? ZONA_RECIDENCIAL { set; get; }
        public string? PARENTESCO_RESPONSABLE { set; get; }
        public string? DOMICILIO { set; get; }
        public string? EMERGENCIA { set; get; }
        public string? ACOMPANATE_TEL { set; get; }
        public string? ACOMPANATE { set; get; }
        public string? BARRIO { set; get; }
        public string? LUGAR { set; get; }
        public string? DOCUMENTO_RESPONS { set; get; }
        public string? ACTIVIDAD_ECONOMICA { set; get; }
        public string? ESTRATO { set; get; }
        public string? LUGAR_NACIMIENTO { set; get; }
        public string? CODIGO_CIUDAD { set; get; }
        public string? CODIGO_DEPARTAMENTO { set; get; }
        public string? CODIGO_EPS { set; get; }
        public string? CODIGO_EPS_LISTADO { set; get; }
        public int? NUMERO_TTITULAR { set; get; }
        public string? NOMBREPADRE { set; get; }
        public string? TELEFONOPADRE { set; get; }
        public string? NOMBRE_MADRE { set; get; }
        public string? TELEFONOMADRE { set; get; }
        public string? CEL_PADRE { set; get; }
        public string? CEL_MADRE { set; get; }
        public string? OCUPACION_PADRE { set; get; }
        public string? OCUPACION_MADRE { set; get; }
        public string? NUMEROHERMANOS { set; get; }
        public string? RELACIONPADRES { set; get; }
        public int ACTIVO { set; get; }
        public int? IDREFERIDOPOR { set; get; }
        public int? COD_DOCTOR { set; get; }
        public string? DOCTOR { set; get; }


    }
}
