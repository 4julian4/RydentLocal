using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaEvolucionSourceService : IRdaEvolucionSourceService
	{
		public RdaEvolucionSourceService()
		{
		}

		public async Task<RdaEvolucionSource> ConsultarPorPacienteYFecha(int idAnamnesis, DateTime fechaConsulta)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var fecha = fechaConsulta.Date;

				var obj = await _dbcontext.TEVOLUCION
					.AsNoTracking()
					.Where(x => x.IDEVOLUSECUND == idAnamnesis && x.FECHA == fecha)
					.OrderByDescending(x => x.HORA)
					.Select(x => new RdaEvolucionSource
					{
						IdEvolucion = x.IDEVOLUCION ?? 0,
						IdAnamnesis = x.IDEVOLUSECUND ?? 0,
						Fecha = x.FECHA,
						Hora = x.HORA,
						Doctor = x.DOCTOR,
						Nota = x.NOTA,
						Evolucion = x.EVOLUCION,
						Complicacion = x.COMPLICACION,
						FechaProximaCita = x.FECHA_PROX_CITA,
						ProximaCitaTexto = x.PROXIMA_CITAstr,
						Urgencias = x.URGENCIAS
					})
					.FirstOrDefaultAsync();

				return obj ?? new RdaEvolucionSource();
			}
		}
	}

	public interface IRdaEvolucionSourceService
	{
		Task<RdaEvolucionSource> ConsultarPorPacienteYFecha(int idAnamnesis, DateTime fechaConsulta);
	}
}