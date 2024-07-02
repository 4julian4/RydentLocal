using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class RespuestaDatosAdministrativos
    {
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaFin { get; set; }
        public int? pacientesAsistieron { get; set; }
        public int? totalPacientesNuevos { get; set; }
        public int? tratamientosActivos { get; set; }
        public int? pacientesAbonaron { get; set; }
        public int? pacientesMora { get; set; }
        public int? pacientesNoAsistieron { get; set; }
        public int? citasCanceladas { get; set; }
        public int? pacientesInicianTratamiento { get; set; }
        public decimal? totalCartera { get; set; }
        public decimal? moraTotal { get; set; }
        public decimal? totalAbonos { get; set; }
        public decimal? totalIngresos { get; set; }
        public decimal? totalEgresos { get; set; }
        public decimal? totalCaja { get; set; }
        public List<PacientesNuevos> lstPacientesNuevos { get; set; }



        public class PacientesNuevos
        {
            public DateTime? FECHA_INGRESO_DATE { get; set; }
            public string SEXO { get; set; }
        }

    }

    
}
