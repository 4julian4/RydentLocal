using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	public class TCODIGOS_CONSLUTAS_TEMPServicios : ITCODIGOS_CONSLUTAS_TEMPServicios
	{
		public TCODIGOS_CONSLUTAS_TEMPServicios()
		{
		}

		public async Task<List<TCODIGOS_CONSLUTAS_TEMP>> ConsultarTodos()
		{
			using (var db = new AppDbContext())
			{
				var items = await db.TCODIGOS_CONSLUTAS_TEMP
					.AsNoTracking()
					.OrderBy(x => x.CODIGO)
					.ToListAsync();

				return items ?? new List<TCODIGOS_CONSLUTAS_TEMP>();
			}
		}

		public async Task<TCODIGOS_CONSLUTAS_TEMP?> ConsultarPorCodigo(string? codigo)
		{
			if (string.IsNullOrWhiteSpace(codigo))
				return null;

			var codigoNormalizado = codigo.Trim().ToUpperInvariant();

			using (var db = new AppDbContext())
			{
				return await db.TCODIGOS_CONSLUTAS_TEMP
					.AsNoTracking()
					.FirstOrDefaultAsync(x => x.CODIGO != null && x.CODIGO.ToUpper() == codigoNormalizado);
			}
		}

		public async Task<string?> ConsultarNombrePorCodigo(string? codigo)
		{
			if (string.IsNullOrWhiteSpace(codigo))
				return null;

			var codigoNormalizado = codigo.Trim().ToUpperInvariant();

			using (var db = new AppDbContext())
			{
				var item = await db.TCODIGOS_CONSLUTAS_TEMP
					.AsNoTracking()
					.Where(x => x.CODIGO != null && x.CODIGO.ToUpper() == codigoNormalizado)
					.Select(x => new
					{
						x.CODIGO,
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
				return await db.TCODIGOS_CONSLUTAS_TEMP
					.AsNoTracking()
					.CountAsync(x => x.CODIGO != null && x.CODIGO.ToUpper() == codigoNormalizado) > 0;
			}
		}

		public async Task<List<TCODIGOS_CONSLUTAS_TEMP>> BuscarPorNombre(string? texto, int maxRegistros = 20)
		{
			if (string.IsNullOrWhiteSpace(texto))
				return new List<TCODIGOS_CONSLUTAS_TEMP>();

			if (maxRegistros <= 0)
				maxRegistros = 20;

			var filtro = texto.Trim().ToUpperInvariant();

			using (var db = new AppDbContext())
			{
				var query = db.TCODIGOS_CONSLUTAS_TEMP
					.AsNoTracking()
					.Where(x =>
						(x.CODIGO != null && x.CODIGO.ToUpper().Contains(filtro)) ||
						(x.NOMBRE != null && x.NOMBRE.ToUpper().Contains(filtro)))
					.OrderBy(x => x.CODIGO)
					.Take(maxRegistros);

				return await query.ToListAsync();
			}
		}
	}

	public interface ITCODIGOS_CONSLUTAS_TEMPServicios
	{
		Task<List<TCODIGOS_CONSLUTAS_TEMP>> ConsultarTodos();
		Task<TCODIGOS_CONSLUTAS_TEMP?> ConsultarPorCodigo(string? codigo);
		Task<string?> ConsultarNombrePorCodigo(string? codigo);
		Task<bool> ExisteCodigo(string? codigo);
		Task<List<TCODIGOS_CONSLUTAS_TEMP>> BuscarPorNombre(string? texto, int maxRegistros = 20);
	}
}