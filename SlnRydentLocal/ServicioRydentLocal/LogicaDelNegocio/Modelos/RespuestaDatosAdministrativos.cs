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
        public int? pacientesNuevos { get; set; }
        public int? tratamientosActivos { get; set; }
        public int? pacientesAbonaron { get; set; }
        public int? pacientesMora { get; set; }
        public int? pacientesNoAsistieron { get; set; }
        public int? citasCanceladas { get; set; }
        public int? pacientesInicianTratamiento { get; set; }
        public decimal? carteraTotal { get; set; }
        public decimal? moraTotal { get; set; }
        public decimal? totalIngresos { get; set; }
        public decimal? totalEgresos { get; set; }
        public decimal? totalCaja { get; set; }
    }
}
