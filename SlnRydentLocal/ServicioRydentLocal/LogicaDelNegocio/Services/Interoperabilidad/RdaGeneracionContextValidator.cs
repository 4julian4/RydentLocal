using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public static class RdaGeneracionContextValidator
	{
		public static RdaGeneracionContextValidationResult Validate(RdaGeneracionContext context)
		{
			if (context == null)
			{
				return new RdaGeneracionContextValidationResult
				{
					Ok = false,
					Error = "El contexto RDA es nulo."
				};
			}

			if (context.IdAnamnesis <= 0)
			{
				return new RdaGeneracionContextValidationResult
				{
					Ok = false,
					Error = "IdAnamnesis inválido."
				};
			}

			if (context.FechaConsulta == null)
			{
				return new RdaGeneracionContextValidationResult
				{
					Ok = false,
					Error = "FechaConsulta es obligatoria."
				};
			}

			return new RdaGeneracionContextValidationResult
			{
				Ok = true,
				Error = null
			};
		}
	}
}