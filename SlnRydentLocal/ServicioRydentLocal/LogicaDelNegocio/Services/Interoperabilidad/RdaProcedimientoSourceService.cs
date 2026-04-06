using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaProcedimientoSourceService : IRdaProcedimientoSourceService
	{
		public RdaProcedimientoSourceService()
		{
		}

		public async Task<RdaProcedimientoSource> ConsultarPorAnamnesis(int idAnamnesis)
		{
			if (idAnamnesis <= 0)
				return new RdaProcedimientoSource();

			using (var db = new AppDbContext())
			{
				var obj = await db.T_RIPS_PROCEDIMIENTOS
					.AsNoTracking()
					.Where(x => x.IDANAMNESIS == idAnamnesis)
					.OrderByDescending(x => x.FECHAPROCEDIMIENTO)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();

				if (obj == null)
					return new RdaProcedimientoSource();

				var catalogoLookup = new RdaCatalogoLookupService();

				var codigoProcedimiento = !string.IsNullOrWhiteSpace(obj.CODIGOPROCEDIMIENTO)
					? obj.CODIGOPROCEDIMIENTO.Trim()
					: null;

				var dxPrincipal = !string.IsNullOrWhiteSpace(obj.DXPRINCIPAL)
					? obj.DXPRINCIPAL.Trim()
					: null;

				var dxRelacionado = !string.IsNullOrWhiteSpace(obj.DXRELACIONADO)
					? obj.DXRELACIONADO.Trim()
					: null;

				var codigoEntidad = !string.IsNullOrWhiteSpace(obj.CODIGOENTIDAD)
					? obj.CODIGOENTIDAD.Trim()
					: null;

				return new RdaProcedimientoSource
				{
					IdAnamnesis = idAnamnesis,
					NumeroAutorizacion = obj.NUMEROAUTORIZACION,

					CodigoProcedimiento = codigoProcedimiento,
					NombreProcedimiento = await catalogoLookup.ConsultarNombreProcedimientoAsync(codigoProcedimiento),

					AmbitoRealizacion = obj.AMBITOREALIZACION,
					FinalidadProcedimiento = obj.FINALIDADPROCEDIMIENTI,
					PersonalQueAtiende = obj.PERSONALQUEATIENDE,

					DxPrincipal = dxPrincipal,
					NombreDxPrincipal = await catalogoLookup.ConsultarNombreDiagnosticoAsync(dxPrincipal),

					DxRelacionado = dxRelacionado,
					NombreDxRelacionado = await catalogoLookup.ConsultarNombreDiagnosticoAsync(dxRelacionado),

					Complicacion = obj.COMPLICACION,
					FormaRealizacionActoQuir = obj.FORMAREALIZACIONACTOQUIR,
					ValorProcedimiento = obj.VALORPROCEDIMIENTO,

					CodigoEntidad = codigoEntidad,
					NombreEntidad = await catalogoLookup.ConsultarNombreEpsAsync(codigoEntidad),

					Extranjero = obj.EXTRANJERO,
					Pais = obj.PAIS
				};
			}
		}
	}

	public interface IRdaProcedimientoSourceService
	{
		Task<RdaProcedimientoSource> ConsultarPorAnamnesis(int idAnamnesis);
	}
}