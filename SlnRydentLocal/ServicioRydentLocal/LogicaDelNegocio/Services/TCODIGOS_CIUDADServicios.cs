using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	public class TCODIGOS_CIUDADServicios : ITCODIGOS_CIUDADServicios
	{
		public TCODIGOS_CIUDADServicios()
		{
		}

		public async Task<List<TCODIGOS_CIUDAD>> ConsultarTodos()
		{
			using (var db = new AppDbContext())
			{
				var items = await db.TCODIGOS_CIUDAD
					.AsNoTracking()
					.OrderBy(x => x.CODIGO_DEPARTAMENTO)
					.ThenBy(x => x.CODIGO_CIUDAD)
					.ToListAsync();

				return items ?? new List<TCODIGOS_CIUDAD>();
			}
		}

		public async Task<TCODIGOS_CIUDAD?> ConsultarPorCodigo(string? codigoDepartamento, string? codigoCiudad)
		{
			if (string.IsNullOrWhiteSpace(codigoDepartamento) || string.IsNullOrWhiteSpace(codigoCiudad))
				return null;

			var dep = codigoDepartamento.Trim();
			var ciudad = codigoCiudad.Trim();

			using (var db = new AppDbContext())
			{
				return await db.TCODIGOS_CIUDAD
					.AsNoTracking()
					.FirstOrDefaultAsync(x =>
						x.CODIGO_DEPARTAMENTO == dep &&
						x.CODIGO_CIUDAD == ciudad);
			}
		}

		public async Task<string?> ConsultarNombrePorCodigo(string? codigoDepartamento, string? codigoCiudad)
		{
			if (string.IsNullOrWhiteSpace(codigoDepartamento) || string.IsNullOrWhiteSpace(codigoCiudad))
				return null;

			var dep = codigoDepartamento.Trim();
			var ciudad = codigoCiudad.Trim();

			using (var db = new AppDbContext())
			{
				var item = await db.TCODIGOS_CIUDAD
					.AsNoTracking()
					.Where(x =>
						x.CODIGO_DEPARTAMENTO == dep &&
						x.CODIGO_CIUDAD == ciudad)
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

	public interface ITCODIGOS_CIUDADServicios
	{
		Task<List<TCODIGOS_CIUDAD>> ConsultarTodos();
		Task<TCODIGOS_CIUDAD?> ConsultarPorCodigo(string? codigoDepartamento, string? codigoCiudad);
		Task<string?> ConsultarNombrePorCodigo(string? codigoDepartamento, string? codigoCiudad);
	}
}