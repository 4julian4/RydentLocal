using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.TablasFraccionadas.TAnamnesis
{
   
    public class Antecedentes : TAnamnesisBase
    {
        [Key]
        public int IDANAMNESIS { set; get; }
        public string? IDANAMNESIS_TEXTO { set; get; }
        public string? IMPORTANTE { set; get; }
        public string? ENFERMEDAD_ACTUAL { set; get; }
        public string? MOTIVO_DE_CONSULTA { set; get; }
        public string? ENFERMEDADESHERE_S { set; get; }
        public string? TRAT_ODONT_PREV_CUALES { set; get; }
        public string? OBS_ANTESEDENTES { set; get; }
        public string? TRAUMA_DENTAL_SN { set; get; }
        public string? TRAUMA_DENTAL_OBS { set; get; }
        public string? TRAT_ODONT_PREV_CUALES_S { set; get; }
        public string? ENFERMEDADESHERE { set; get; }
        public string? TRATAMIENTO_ORTODONCIA { set; get; }
        public string? TRATAMIENTO_ORTODONCIA_S { set; get; }
        public string? CIRUGIA_ORAL { set; get; }
        public string? CIRUGIA_ORAL_S { set; get; }
        public string? PESO { set; get; }
        public string? ALTURA { set; get; }
        public string? SALUDGENERAL { set; get; }
        public string? RH { set; get; }
        public string? SIDA_SI_NO { set; get; }
        public string? MEDICO { set; get; }
        public string? MEDICO_TEL { set; get; }
        public string? PADC_ENFERM_CUALES { set; get; }
        public string? RECIBE_ALGUN_MEDIC_CUAL { set; get; }
        public string? REACC_ALERGIC_CUALES { set; get; }
        public string? A_ESTADO_HOSPIT_MOTIVO { set; get; }
        public string? CIRUGIAS { set; get; }
        public string? PADC_ENFERM_CUALES_S { set; get; }
        public string? A_ESTADO_HOSPIT_MOTIVO_S { set; get; }
        public string? RECIBE_ALGUN_MEDIC_CUAL_S { set; get; }
        public string? REACC_ALERGIC_CUALES_S { set; get; }
        public string? CIRUGIAS_S { set; get; }
        public string? CARDIOP { set; get; }
        public string? PRESION { set; get; }
        public string? DIABETES { set; get; }
        public string? AMIGDALITIS { set; get; }
        public string? ANEMIA { set; get; }
        public string? ASMA { set; get; }
        public string? TRAUMAFACIAL { set; get; }
        public string? ENFERMEDADESE { set; get; }
        public string? HERPES { set; get; }
        public string? ALERGIA { set; get; }
        public string? EMBARAZO { set; get; }
        public string? RADIOTERAPIA { set; get; }
        public string? HEMORRAGIAS { set; get; }
        public string? GASTRICOS { set; get; }
        public string? OBS2 { set; get; }
        public string? OBS3 { set; get; }
        public string? OBS4 { set; get; }
        public string? OBS5 { set; get; }
        public string? OBS6 { set; get; }
        public string? OBS7 { set; get; }
        public string? OBS9 { set; get; }
        public string? OBS8 { set; get; }
        public string? OBS10 { set; get; }
        public string? OBS11 { set; get; }
        public string? OBS12 { set; get; }
        public string? OBS13 { set; get; }
        public string? OBS14 { set; get; }
        public string? OBS15 { set; get; }
        public string? PROBLE_ESQUE_FACIALES_OBS { set; get; }
        public string? PROBLE_ESQUE_FACIALES_SN { set; get; }
        public string? MENARCA_SN { set; get; }
        public string? MENARCA_OBS { set; get; }
        public string? ASIMETRIAS_SN { set; get; }
        public string? ASIMETRIAS_OBS { set; get; }
        public string? HEPATITIS { set; get; }
        public string? FIEBREREUMATICA { set; get; }
        public string? CONVULCIONES { set; get; }
        public string? OTROS { set; get; }
        public string? FUMA { set; get; }
        public string? OBS1 { set; get; }
        public string? ALTERACIONES_HEMATOLOGICAS { set; get; }
        public string? COMPLICACIONES_TTO { set; get; }
        public string? TRASTORNO_EMOCIONAL { set; get; }
        public string? REVISION_SISTEMAS { set; get; }
        public string? FOTOTERAPIA { set; get; }
        public string? FOTOTERAPIA_OBS { set; get; }
        public string? ENFERMEDAD_TIROIDEA { set; get; }
        public string? ENFERMEDAD_TIROIDEA_SN { set; get; }
        public string? DISLIPIDEMIA { set; get; }
        public string? DISLIPIDEMIA_SN { set; get; }
        public string? OVARIO_POLIQUISTICO { set; get; }
        public string? OVARIO_POLIQUISTICO_SN { set; get; }
        public string? CORONARIA { set; get; }
        public string? CORONARIA_SN { set; get; }

    }
}
