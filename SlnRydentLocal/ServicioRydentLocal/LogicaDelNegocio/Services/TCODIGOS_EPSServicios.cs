using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	public class TCODIGOS_EPSServicios : ITCODIGOS_EPSServicios
	{
		public TCODIGOS_EPSServicios()
		{
		}

		public async Task<List<TCODIGOS_EPS>> ConsultarTodos()
		{
			using (var db = new AppDbContext())
			{
				var obj = await db.TCODIGOS_EPS.AsNoTracking().ToListAsync();
				return obj ?? new List<TCODIGOS_EPS>();
			}
		}

		public async Task<string?> ConsultarNombrePorCodigo(string? codigo)
		{
			if (string.IsNullOrWhiteSpace(codigo))
				return null;

			var codigoNormalizado = codigo.Trim().ToUpperInvariant();

			using (var db = new AppDbContext())
			{
				var item = await db.TCODIGOS_EPS
					.AsNoTracking()
					.Where(x => x.CODIGO != null && x.CODIGO.ToUpper() == codigoNormalizado)
					.Select(x => new
					{
						x.NOMBRE
					})
					.FirstOrDefaultAsync();

				return string.IsNullOrWhiteSpace(item?.NOMBRE)
					? null
					: item.NOMBRE.Trim();
			}
		}
	}

	public interface ITCODIGOS_EPSServicios
	{
		Task<List<TCODIGOS_EPS>> ConsultarTodos();
		Task<string?> ConsultarNombrePorCodigo(string? codigo);
	}
}