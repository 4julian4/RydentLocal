using System;
using Newtonsoft.Json;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;

namespace ServicioRydentLocal.LogicaDelNegocio.Servicios.Interoperabilidad
{
	public class RdaPacienteSnapshotReader
	{
		public RdaPacienteSnapshotInterno? Leer(string? jsonSnapshot)
		{
			if (string.IsNullOrWhiteSpace(jsonSnapshot))
				return null;

			try
			{
				return JsonConvert.DeserializeObject<RdaPacienteSnapshotInterno>(jsonSnapshot);
			}
			catch
			{
				return null;
			}
		}

		public int? ExtraerIdDoctor(string? jsonSnapshot)
		{
			var snapshot = Leer(jsonSnapshot);
			return snapshot?.Documento?.Prestador?.IdDoctor;
		}

		public DateTime? ExtraerFechaAtencion(string? jsonSnapshot)
		{
			var snapshot = Leer(jsonSnapshot);

			if (snapshot?.Documento?.Consulta?.Encounter?.FechaConsulta != null)
				return snapshot.Documento.Consulta.Encounter.FechaConsulta.Value.DateTime;

			return null;
		}
	}
}