﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TANAMNESIS")]
    public class TANAMNESIS
    {
        [Key]
        public int IDANAMNESIS { set; get; }
        public string? IDANAMNESIS_TEXTO { set; get; }
        public long? COMPARACION { set; get; }
        public string? FECHA_INGRESO { set; get; }
        public DateTime? FECHA_INGRESO_DATE { set; get; }
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
        public string? TRAT_ODONT_PREV_SI_NO { set; get; }
        public string? TRAT_ODONT_PREV_CUALES { set; get; }
        public string? PADC_ENFERM_SI_NO { set; get; }
        public string? PADC_ENFERM_CUALES { set; get; }
        public string? RECIBE_ALGUN_MEDIC_SI { set; get; }
        public string? RECIBE_ALGUN_MEDIC_CUAL { set; get; }
        public string? PROCED_QUIRUR_SI_NO { set; get; }
        public string? PROCED_QUIRUR_CUALES { set; get; }
        public string? REACC_ALERGIC_SI_NO { set; get; }
        public string? REACC_ALERGIC_CUALES { set; get; }
        public string? ANTECEDENTES_MEDICOS { set; get; }
        public string? ANTECEDENTES_ODONTOLO { set; get; }  
        public string? OBSERVACIONES { set; get; }
        public string? CEDULA_NUMERO { set; get; }
        public string? HIGIENEHORAL { set; get; }
        public string? FRECUENCICEPILLADO { set; get; }
        public string? USASEDADENTAL { set; get; }
        public string? ENFERMEDAD { set; get; }
        public string? CARIES { set; get; }
        public string? RECIDIVA { set; get; }
        public string? AMALGAMA { set; get; }
        public string? IONOMERO { set; get; }
        public string? NOMBREPADRE { set; get; }
        public string? TELEFONO_PADRE { set; get; }
        public string? TELEFONOPADRE { set; get; }
        public string? NOMBRE_MADRE { set; get; }
        public string? TELEFONO_MADRE { set; get; }
        public string? TELEFONOMADRE { set; get; }
        public string? NUMEROHERMANOS { set; get; }
        public string? RELACIONPADRES { set; get; }
        public string? ENFERMEDADESHERE { set; get; }
        public string? ESTADOCIVIL { set; get; }
        public string? NIVELESCOLAR { set; get; }
        public string? PESO { set; get; }
        public string? ALTURA { set; get; }
        public string? SALUDGENERAL { set; get; }
        public string? CARDIOP { set; get; }
        public string? PRESION { set; get; }
        public string? DIABETES { set; get; }
        public string? HEPATITIS { set; get; }
        public string? FIEBREREUMATICA { set; get; }
        public string? CONVULCIONES { set; get; }
        public string? AMIGDALITIS { set; get; }
        public string? ANEMIA { set; get; }
        public string? ASMA { set; get; }
        public string? TRAUMAFACIAL { set; get; }
        public string? ENFERMEDADESE { set; get; }
        public string? HERPES { set; get; }
        public string? ALERGIA { set; get; }
        public string? ESTATOMANDO { set; get; }
        public string? AESTADOH { set; get; }
        public string? AESTADOPORQUE { set; get; }
        public string? OTROS { set; get; }
        public string? HPORQUE { set; get; }
        public string? ALG_MEDIC_CUALES { set; get; }
        public string? A_ESTADO_HOSPIT_MOTIVO { set; get; }
        public string? TIPO_HISTORIA { set; get; }
        public string? HEMORRAGIAS { set; get; }
        public string? GASTRICOS { set; get; }
        public string? FUMA { set; get; }
        public string? EMBARAZO { set; get; }
        public string? RADIOTERAPIA { set; get; }
        public string? FOTOGRAFIA { set; get; }
        public string? ESTADOCIVILP { set; get; }
        public string? PROFESION { set; get; }
        public string? NOMBRE_PADRE { set; get; }
        public string? TEL_REMITIDOPOR { set; get; }
        public string? ODONTOLOGO { set; get; }
        public string? TEL_ODONTOLOGO { set; get; }
        public string? RECOMENDADOPOR { set; get; }
        public string? TEL_RECOMENDADOPOR { set; get; }
        public string? TRAUMATISMOS { set; get; }
        public string? EMPRESA { set; get; }
        public string? TIPO_HISTORIA_ORTODONCIA { set; get; }
        public string? TIPO_HISTORIA_ORTOPEDIA { set; get; }
        public string? TERMINOTRAT { set; get; }
        public string? DOCTOR { set; get; }
        public string? TIPO_PACIENTE { set; get; }
        public string? ESTADO_TRATAMIENTO { set; get; }
        public string? RH { set; get; }
        public string? CELULAR { set; get; }
        public string? BARRIO { set; get; }
        public string? TIPO_USUARIO { set; get; }
        public string? CODIGO_EPS { set; get; }
        public string? CODIGO_CIUDAD { set; get; }
        public string? CODIGO_DEPARTAMENTO { set; get; }
        public string? CODIGO_ZONA { set; get; }
        public string? APELLIDO_SEG { set; get; }
        public string? NOMBRE_SEG { set; get; }
        public string? ZONA_RECIDENCIAL { set; get; }
        public string? SIDA_SI_NO { set; get; }
        public string? TELEFONOACROM { set; get; }
        public string? CIRUGIAS { set; get; } 
        public string? OBS1 { set; get; }
        public string? OBS2 { set; get; }
        public string? OBS3 { set; get; }
        public string? OBS4 { set; get; }
        public string? OBS5 { set; get; }
        public string? OBS6 { set; get; }
        public string? OBS7 { set; get; }
        public string? OBS8 { set; get; }
        public string? OBS9 { set; get; }
        public string? OBS10 { set; get; }
        public string? OBS11 { set; get; }
        public string? OBS12 { set; get; }
        public string? OBS13 { set; get; }
        public string? OBS14 { set; get; }
        public string? OBS15 { set; get; }
        public string? CODIGO_EPS_LISTADO { set; get; }
        public DateTime? FECHA_AUTORIZADA { set; get; }
        public DateTime? FECHA_INGRESO_REAL { set; get; }
        public string? PARENTESCO { set; get; }
        public string? NUMERO_AUTORIZACION { set; get; }
        public DateTime? FECHA_NUMERO_AUTORIZACION { set; get; }
        public TimeSpan? HORA_INGRESO { set; get; }
        public string? EPS_TELEFONO { set; get; }
        public string? EMERGENCIA { set; get; }
        public string? ACOMPANATE { set; get; }
        public string? ACOMPANATE_TEL { set; get; }
        public string? PARENTESCO_RESPONSABLE { set; get; }
        public string? DOMICILIO { set; get; }
        public string? TITULAR { set; get; }
        public int? NUMERO_TTITULAR { set; get; }
        public string? FECHA_FINAL { set; get; }
        public string? NRO_AFILIACION { set; get; }
        public string? NIVEL_BENEFICIARIO { set; get; }
        public int? COD_DOCTOR { set; get; }
        public int ACTIVO { set; get; }
        public string? OBS_ANTESEDENTES { set; get; }
        public int? ATENDIDO_POR { set; get; }
        public string? CONVENIO { set; get; }
        public string? NOTA_IMPORTANTE { set; get; }
        public string? MEDICO { set; get; }
        public string? LUGAR { set; get; }
        public string? PROBLE_ESQUE_FACIALES_SN { set; get; }
        public string? PROBLE_ESQUE_FACIALES_OBS { set; get; }
        public string? TRAUMA_DENTAL_SN { set; get; }
        public string? TRAUMA_DENTAL_OBS { set; get; }
        public string? MENARCA_SN { set; get; }
        public string? MENARCA_OBS { set; get; }
        public string? MALARES { set; get; }
        public string? ASIMETRIAS_SN { set; get; }
        public string? ASIMETRIAS_OBS { set; get; }
        public string? IMPORTANTE { set; get; }
        public string? DOCUMENTO_RESPONS { set; get; }
        public string? ESTRATO { set; get; }
        public string? MOTIVO_DE_CONSULTA { set; get; }
        public string? TRAT_ODONT_PREV_CUALES_S { set; get; }
        public string? PADC_ENFERM_CUALES_S { set; get; }
        public string? A_ESTADO_HOSPIT_MOTIVO_S { set; get; }
        public string? RECIBE_ALGUN_MEDIC_CUAL_S { set; get; }
        public string? REACC_ALERGIC_CUALES_S { set; get; }
        public string? CIRUGIAS_S { set; get; }
        public string? CEL_PADRE { set; get; }
        public string? CEL_MADRE { set; get; }
        public string? ENFERMEDADESHERE_S { set; get; }
        public string? MEDICO_TEL { set; get; }
        public string? OCUPACION_PADRE { set; get; }
        public string? OCUPACION_MADRE { set; get; }
        public string? TRATAMIENTO_ORTODONCIA_S { set; get; }
        public string? TRATAMIENTO_ORTODONCIA { set; get; }
        public string? CIRUGIA_ORAL { set; get; }
        public string? CIRUGIA_ORAL_S { set; get; }
        public string? FOTOTERAPIA { set; get; }
        public string? FOTOTERAPIA_OBS { set; get; }
        public string? REVISION_SISTEMAS { set; get; }
        public string? ENFERMEDAD_TIROIDEA { set; get; }
        public string? ENFERMEDAD_TIROIDEA_SN { set; get; }
        public string? DISLIPIDEMIA { set; get; }
        public string? DISLIPIDEMIA_SN { set; get; }
        public string? OVARIO_POLIQUISTICO { set; get; }
        public string? OVARIO_POLIQUISTICO_SN { set; get; }
        public string? ENFERMEDAD_ACTUAL { set; get; }
        public string? EDAD_PADRE { set; get; }
        public string? EDAD_MADRE { set; get; }
        public string? ACTIVIDAD_ECONOMICA { set; get; }
        public string? ALTERACIONES_HEMATOLOGICAS { set; get; }
        public string? COMPLICACIONES_TTO { set; get; }
        public string? TRASTORNO_EMOCIONAL { set; get; }
        public string? LUGAR_NACIMIENTO { set; get; }
        public string? CORONARIA { set; get; }
        public string? CORONARIA_SN { set; get; }
        public int? IDREFERIDOPOR { set; get; }
        public string? NUM_INGRESO { set; get; }
        public string? RUTA_FOTOS { set; get; }
        public string? SERVIDOR { set; get; }
    }
}