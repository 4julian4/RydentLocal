using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaPrestadorSourceService : IRdaPrestadorSourceService
	{
		public RdaPrestadorSourceService()
		{
		}

		public async Task<RdaPrestadorSource> ConsultarPorDoctor(int? idDoctor)
		{
			if (!idDoctor.HasValue || idDoctor.Value <= 0)
				return new RdaPrestadorSource();

			using (var _dbcontext = new AppDbContext())
			{
				var obj = await (
					from d in _dbcontext.TDATOSDOCTORES
					join i in _dbcontext.TINFORMACIONREPORTES on d.IDREPORTE equals i.ID into gj
					from i in gj.DefaultIfEmpty()
					where d.ID == idDoctor.Value
					select new RdaPrestadorSource
					{
						CodigoPrestador = i != null ? i.CODIGO_PRESTADOR_PPAL : null,
						NombrePrestador = i != null ? i.NOMBRE : null,
						NitPrestador = i != null ? i.NIT : null,
						TipoDocumentoPrestador = i != null ? i.TIPO_ID : null,
						DireccionPrestador = i != null ? i.DIRECCION : null,
						TelefonoPrestador = i != null ? i.TELEFONO : null,
						CiudadPrestador = i != null ? i.CIUDAD : null,

						IdDoctor = d.ID,
						IdInformacionReporte = i != null ? i.ID : (int?)null,
						NombreDoctor = d.NOMBRE,
						TipoDocumentoDoctor = d.TIPO_ID,
						NumeroDocumentoDoctor = d.NUMERO_DOCUMENTO,
						EspecialidadDoctor = d.ESPECIALIDAD
					}
				).AsNoTracking().FirstOrDefaultAsync();

				return obj ?? new RdaPrestadorSource();
			}
		}
	}

	public interface IRdaPrestadorSourceService
	{
		Task<RdaPrestadorSource> ConsultarPorDoctor(int? idDoctor);
	}
}