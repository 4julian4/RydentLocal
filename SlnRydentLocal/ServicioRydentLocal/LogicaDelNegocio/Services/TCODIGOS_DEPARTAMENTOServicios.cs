using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	public class TCODIGOS_DEPARTAMENTOServicios : ITCODIGOS_DEPARTAMENTOServicios
	{
		public TCODIGOS_DEPARTAMENTOServicios()
		{
		}

		public async Task<List<TCODIGOS_DEPARTAMENTO>> ConsultarTodos()
		{
			using (var db = new AppDbContext())
			{
				var items = await db.TCODIGOS_DEPARTAMENTO
					.AsNoTracking()
					.OrderBy(x => x.CODIGO_DEPARTAMENTO)
					.ToListAsync();

				return items ?? new List<TCODIGOS_DEPARTAMENTO>();
			}
		}

		public async Task<TCODIGOS_DEPARTAMENTO?> ConsultarPorCodigo(string? codigoDepartamento)
		{
			if (string.IsNullOrWhiteSpace(codigoDepartamento))
				return null;

			var codigo = codigoDepartamento.Trim();

			using (var db = new AppDbContext())
			{
				return await db.TCODIGOS_DEPARTAMENTO
					.AsNoTracking()
					.FirstOrDefaultAsync(x => x.CODIGO_DEPARTAMENTO == codigo);
			}
		}

		public async Task<string?> ConsultarNombrePorCodigo(string? codigoDepartamento)
		{
			if (string.IsNullOrWhiteSpace(codigoDepartamento))
				return null;

			var codigo = codigoDepartamento.Trim();

			using (var db = new AppDbContext())
			{
				var item = await db.TCODIGOS_DEPARTAMENTO
					.AsNoTracking()
					.Where(x => x.CODIGO_DEPARTAMENTO == codigo)
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

	public interface ITCODIGOS_DEPARTAMENTOServicios
	{
		Task<List<TCODIGOS_DEPARTAMENTO>> ConsultarTodos();
		Task<TCODIGOS_DEPARTAMENTO?> ConsultarPorCodigo(string? codigoDepartamento);
		Task<string?> ConsultarNombrePorCodigo(string? codigoDepartamento);
	}
}