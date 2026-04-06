using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;
using System;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaRipsRebuildService : IRdaRipsRebuildService
	{
		public async Task<DatosGuardarRips?> ReconstruirDesdeAnamnesisAsync(int idAnamnesis, string? facturaReferencia = null)
		{
			if (idAnamnesis <= 0)
				return null;

			var objRipsDxServicios = new T_RIPS_DXServicios();
			var objRipsProcedimientosServicios = new T_RIPS_PROCEDIMIENTOSServicios();

			var ripsDx = await objRipsDxServicios.ConsultarUltimoPorAnamnesis(idAnamnesis);
			if (ripsDx == null)
				return null;

			T_RIPS_PROCEDIMIENTOS? ripsProc = null;

			if (!string.IsNullOrWhiteSpace(facturaReferencia))
			{
				ripsProc = await objRipsProcedimientosServicios
					.ConsultarUltimoPorAnamnesisYFactura(idAnamnesis, facturaReferencia);
			}

			if (ripsProc == null)
			{
				ripsProc = await objRipsProcedimientosServicios.ConsultarUltimoPorAnamnesis(idAnamnesis);
			}

			return MapearModelo(ripsDx, ripsProc);
		}

		public async Task<DatosGuardarRips?> ReconstruirExactoAsync(
			int idAnamnesis,
			DateTime? fechaAtencion,
			TimeSpan? horaAtencion,
			string? facturaReferencia = null,
			int? idEvolucion = null)
		{
			if (idAnamnesis <= 0 || !fechaAtencion.HasValue)
				return null;

			var objRipsDxServicios = new T_RIPS_DXServicios();
			var objRipsProcedimientosServicios = new T_RIPS_PROCEDIMIENTOSServicios();

			// =========================
			// 1. Buscar DX exacto por anamnesis + fecha + hora
			// =========================
			var ripsDx = await objRipsDxServicios.ConsultarExactoAsync(
				idAnamnesis,
				fechaAtencion.Value,
				horaAtencion,
				facturaReferencia);

			// Fallbacks controlados
			if (ripsDx == null && !string.IsNullOrWhiteSpace(facturaReferencia))
			{
				ripsDx = await objRipsDxServicios.ConsultarPorAnamnesisFechaYFacturaAsync(
					idAnamnesis,
					fechaAtencion.Value,
					facturaReferencia);
			}

			if (ripsDx == null)
			{
				ripsDx = await objRipsDxServicios.ConsultarPorAnamnesisYFechaAsync(
					idAnamnesis,
					fechaAtencion.Value);
			}

			if (ripsDx == null)
				return null;

			// =========================
			// 2. Buscar procedimiento exacto por anamnesis + fecha + hora
			// =========================
			T_RIPS_PROCEDIMIENTOS? ripsProc = await objRipsProcedimientosServicios.ConsultarExactoAsync(
				idAnamnesis,
				fechaAtencion.Value,
				horaAtencion,
				!string.IsNullOrWhiteSpace(facturaReferencia) ? facturaReferencia : ripsDx.FACTURA);

			// Fallbacks controlados
			if (ripsProc == null && !string.IsNullOrWhiteSpace(facturaReferencia))
			{
				ripsProc = await objRipsProcedimientosServicios.ConsultarPorAnamnesisFechaYFacturaAsync(
					idAnamnesis,
					fechaAtencion.Value,
					facturaReferencia);
			}

			if (ripsProc == null && !string.IsNullOrWhiteSpace(ripsDx.FACTURA))
			{
				ripsProc = await objRipsProcedimientosServicios.ConsultarPorAnamnesisFechaYFacturaAsync(
					idAnamnesis,
					fechaAtencion.Value,
					ripsDx.FACTURA);
			}

			if (ripsProc == null)
			{
				ripsProc = await objRipsProcedimientosServicios.ConsultarPorAnamnesisYFechaAsync(
					idAnamnesis,
					fechaAtencion.Value);
			}

			var model = MapearModelo(ripsDx, ripsProc);

			// Blindaje final: prevalece el documento RDA solicitado
			model.FECHACONSULTA = fechaAtencion;
			model.HORALOTE = horaAtencion;

			if (!string.IsNullOrWhiteSpace(facturaReferencia))
				model.FACTURA = facturaReferencia;

			return model;
		}

		private static DatosGuardarRips MapearModelo(T_RIPS_DX ripsDx, T_RIPS_PROCEDIMIENTOS? ripsProc)
		{
			var model = new DatosGuardarRips
			{
				IDANAMNESIS = ripsDx.IDANAMNESIS,
				IDDOCTOR = ripsDx.IDDOCTOR,
				FACTURA = ripsDx.FACTURA,
				FECHACONSULTA = ripsDx.FECHACONSULTA,
				HORALOTE = ripsDx.HORA,
				CODIGOENTIDAD = ripsDx.CODIGOENTIDAD,
				NUMEROAUTORIZACION = ripsDx.NUMEROAUTORIZACION,

				CODIGOCONSULTA = ripsDx.CODIGOCONSULTA,
				FINALIDADCONSULTA = ripsDx.FINALIDADCONSULTA,
				CAUSAEXTERNA = ripsDx.CAUSAEXTERNA,
				CODIGODIAGNOSTICOPRINCIPAL = ripsDx.DX1,
				CODIGODIAGNOSTICO2 = ripsDx.DX2,
				CODIGODIAGNOSTICO3 = ripsDx.DX3,
				CODIGODIAGNOSTICO4 = ripsDx.DX4,
				TIPODIAGNOSTICO = ripsDx.TIPODIAGNOSTICO,
				VALORCONSULTA = ripsDx.VALORCONSULTA,
				VALORCUOTAMODERADORA = ripsDx.VALORCUOTAMODERADORA,
				VALORNETO = ripsDx.VALORNETO,
				EXTRANJERO = ripsDx.EXTRANJERO,
				PAIS = ripsDx.PAIS
			};

			if (ripsProc != null)
			{
				model.CODIGOPROCEDIMIENTO = ripsProc.CODIGOPROCEDIMIENTO;
				model.FINALIDADPROCEDIMIENTI = ripsProc.FINALIDADPROCEDIMIENTI;
				model.AMBITOREALIZACION = ripsProc.AMBITOREALIZACION;
				model.PERSONALQUEATIENDE = ripsProc.PERSONALQUEATIENDE;
				model.DXPRINCIPAL = ripsProc.DXPRINCIPAL;
				model.DXRELACIONADO = ripsProc.DXRELACIONADO;
				model.COMPLICACION = ripsProc.COMPLICACION;
				model.FORMAREALIZACIONACTOQUIR = ripsProc.FORMAREALIZACIONACTOQUIR;
				model.VALORPROCEDIMIENTO = ripsProc.VALORPROCEDIMIENTO;

				if (string.IsNullOrWhiteSpace(model.FACTURA))
					model.FACTURA = ripsProc.FACTURA;

				if (!model.IDDOCTOR.HasValue || model.IDDOCTOR.Value <= 0)
					model.IDDOCTOR = ripsProc.IDDOCTOR;

				if (string.IsNullOrWhiteSpace(model.CODIGOENTIDAD))
					model.CODIGOENTIDAD = ripsProc.CODIGOENTIDAD;

				if (!model.FECHACONSULTA.HasValue)
					model.FECHACONSULTA = ripsProc.FECHAPROCEDIMIENTO;

				if (!model.HORALOTE.HasValue)
					model.HORALOTE = ripsProc.HORA;

				if (string.IsNullOrWhiteSpace(model.NUMEROAUTORIZACION))
					model.NUMEROAUTORIZACION = ripsProc.NUMEROAUTORIZACION;

				if (string.IsNullOrWhiteSpace(model.EXTRANJERO))
					model.EXTRANJERO = ripsProc.EXTRANJERO;

				if (string.IsNullOrWhiteSpace(model.PAIS))
					model.PAIS = ripsProc.PAIS;

				if (string.IsNullOrWhiteSpace(model.DXPRINCIPAL))
					model.DXPRINCIPAL = model.CODIGODIAGNOSTICOPRINCIPAL;
			}

			return model;
		}
	}

	public interface IRdaRipsRebuildService
	{
		Task<DatosGuardarRips?> ReconstruirDesdeAnamnesisAsync(int idAnamnesis, string? facturaReferencia = null);

		Task<DatosGuardarRips?> ReconstruirExactoAsync(
			int idAnamnesis,
			DateTime? fechaAtencion,
			TimeSpan? horaAtencion,
			string? facturaReferencia = null,
			int? idEvolucion = null);
	}
}