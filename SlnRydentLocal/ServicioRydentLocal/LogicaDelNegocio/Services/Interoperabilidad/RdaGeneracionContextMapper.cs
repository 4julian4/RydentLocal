using ServicioRydentLocal.LogicaDelNegocio.Modelos;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public static class RdaGeneracionContextMapper
	{
		public static RdaGeneracionContext FromDatosGuardarRips(DatosGuardarRips source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			return new RdaGeneracionContext
			{
				IdAnamnesis = source.IDANAMNESIS ?? 0,
				IdDoctor = source.IDDOCTOR,
				FechaConsulta = source.FECHACONSULTA,
				HoraConsulta = source.HORALOTE,
				Factura = source.FACTURA,
				NumeroAutorizacion = source.NUMEROAUTORIZACION,
				CodigoConsulta = source.CODIGOCONSULTA,
				CodigoDiagnosticoPrincipal = source.CODIGODIAGNOSTICOPRINCIPAL,
				CodigoProcedimiento = source.CODIGOPROCEDIMIENTO
			};
		}
	}
}