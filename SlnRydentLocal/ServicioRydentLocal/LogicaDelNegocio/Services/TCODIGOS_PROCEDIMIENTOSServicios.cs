using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	public class TCODIGOS_PROCEDIMIENTOSServicios : ITCODIGOS_PROCEDIMIENTOSServicios
	{
		public TCODIGOS_PROCEDIMIENTOSServicios()
		{
		}

		public async Task<List<TCODIGOS_PROCEDIMIENTOS>> ConsultarTodos()
		{
			using (var db = new AppDbContext())
			{
				var items = await db.TCODIGOS_PROCEDIMIENTOS
					.AsNoTracking()
					.OrderBy(x => x.CODIGO)
					.ToListAsync();

				return items ?? new List<TCODIGOS_PROCEDIMIENTOS>();
			}
		}

		public async Task<TCODIGOS_PROCEDIMIENTOS?> ConsultarPorCodigo(string? codigo)
		{
			if (string.IsNullOrWhiteSpace(codigo))
				return null;

			var codigoNormalizado = codigo.Trim().ToUpperInvariant();

			using (var db = new AppDbContext())
			{
				return await db.TCODIGOS_PROCEDIMIENTOS
					.AsNoTracking()
					.Where(x => x.CODIGO != null && x.CODIGO.ToUpper() == codigoNormalizado)
					.OrderBy(x => x.ID)
					.FirstOrDefaultAsync();
			}
		}

		public async Task<string?> ConsultarNombrePorCodigo(string? codigo)
		{
			if (string.IsNullOrWhiteSpace(codigo))
				return null;

			var codigoNormalizado = codigo.Trim().ToUpperInvariant();

			using (var db = new AppDbContext())
			{
				var item = await db.TCODIGOS_PROCEDIMIENTOS
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

		public async Task<bool> ExisteCodigo(string? codigo)
		{
			if (string.IsNullOrWhiteSpace(codigo))
				return false;

			var codigoNormalizado = codigo.Trim().ToUpperInvariant();

			using (var db = new AppDbContext())
			{
				return await db.TCODIGOS_PROCEDIMIENTOS
					.AsNoTracking()
					.CountAsync(x => x.CODIGO != null && x.CODIGO.ToUpper() == codigoNormalizado) > 0;
			}
		}
	}

	public interface ITCODIGOS_PROCEDIMIENTOSServicios
	{
		Task<List<TCODIGOS_PROCEDIMIENTOS>> ConsultarTodos();
		Task<TCODIGOS_PROCEDIMIENTOS?> ConsultarPorCodigo(string? codigo);
		Task<string?> ConsultarNombrePorCodigo(string? codigo);
		Task<bool> ExisteCodigo(string? codigo);
	}
}