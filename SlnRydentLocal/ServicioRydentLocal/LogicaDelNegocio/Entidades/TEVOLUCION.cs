using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
    [Table("TEVOLUCION")]
    public class TEVOLUCION
    {
        [Key]
        public int? IDEVOLUCION { set; get; }
        public int? IDEVOLUSECUND { set; get; }
        public byte[]? PROXIMA_CITA { set; get; }

        [NotMapped]
        public string? PROXIMA_CITAstr { 
            set
            {
                if (value != null)
                {
                    PROXIMA_CITA = Encoding.ASCII.GetBytes(value);
                }
            }
            get
            {
                if (PROXIMA_CITA != null)
                {
                    return Encoding.ASCII.GetString(PROXIMA_CITA);
                }
                return null;
            }   
        }
        public DateTime? FECHA_PROX_CITA { set; get; }
        public string? FECHA_ORDEN { set; get; }
        public byte[]? ENTRADA { set; get; }
        [NotMapped]
        public string? ENTRADAstr
        {
            set
            {
                if (value != null)
                {
                    ENTRADA = Encoding.ASCII.GetBytes(value);
                }
            }
            get
            {
                if (ENTRADA != null)
                {
                    return Encoding.ASCII.GetString(ENTRADA);
                }
                return null;
            }   
        }
        
        public byte[]? SALIDA { set; get; }
        [NotMapped]
        public string? SALIDAstr
        {
            set
            {
                if (value != null)
                {
                    SALIDA = Encoding.ASCII.GetBytes(value);
                }
            }
            get
            {
                if (SALIDA != null)
                {
                    return Encoding.ASCII.GetString(SALIDA);
                }
                return null;
            }
        }

        public DateTime? FECHA { set; get; }
        public TimeSpan? HORA { set; get; }
        public string? DOCTOR { set; get; }
        public int? FIRMA { set; get; }
        public string? COMPLICACION { set; get; }
        public TimeSpan? HORA_FIN { set; get; }
        public string? COLOR { set; get; }
        public string? NOTA { set; get; }
        public string? EVOLUCION { set; get; }
        public string? URGENCIAS { set; get; }
        public TimeSpan? HORA_LLEGADA { set; get; }
    }
}


