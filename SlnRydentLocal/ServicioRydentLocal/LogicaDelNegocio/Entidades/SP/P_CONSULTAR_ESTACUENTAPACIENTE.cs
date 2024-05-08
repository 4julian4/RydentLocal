using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
    [Keyless]
    public class P_CONSULTAR_ESTACUENTAPACIENTE
    {
        
        public DateTime? FECHA { get; set; }
        public double? ABONO { get; set; }
        public double? MORA_ACTUAL { get; set; }
        public double? MORATOTAL { get; set; }
        public double? VALOR_TRATAMIENTO { get; set; }
        public string? NUMERO_HISTORIA { get; set; }
        public DateTime? FECHA_INICIO { get; set; }
        public string? NOMBRE_PACIENTE { get; set; }
        public string? TELEFONO { get; set; }
        public int? FASE { get; set; }
        public string? NOMBRE_DOCTOR { get; set; }
    }
 }
