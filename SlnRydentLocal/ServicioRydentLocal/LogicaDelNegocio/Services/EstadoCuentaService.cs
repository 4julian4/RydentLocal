using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	public interface IEstadoCuentaService
	{
		Task<PrepararNuevoEstadoCuentaResponse> PrepararNuevoAsync(PrepararNuevoEstadoCuentaRequest req);
		Task<CrearEstadoCuentaResponse> CrearAsync(CrearEstadoCuentaRequest req);
		Task<PrepararEditarEstadoCuentaResponse> PrepararEditarAsync(PrepararEditarEstadoCuentaRequest req);
		Task<EditarEstadoCuentaResponse> EditarAsync(EditarEstadoCuentaRequest req);
		Task<BorrarEstadoCuentaResponse> BorrarAsync(BorrarEstadoCuentaRequest req);
		Task<ConsultarSugeridosAbonoResponse> ConsultarSugeridosAbonoAsync(ConsultarSugeridosAbonoRequest req);
		Task<PrepararInsertarAbonoResponse> PrepararInsertarAbonoAsync(PrepararInsertarAbonoRequest req);
		Task<InsertarAbonoResponse> InsertarAbonoAsync(InsertarAbonoRequest req);

		Task<PrepararBorrarAbonoResponse> PrepararBorrarAbonoAsync(PrepararBorrarAbonoRequest req);
		Task<BorrarAbonoResponse> BorrarAbonoAsync(BorrarAbonoRequest req);

		Task<PrepararInsertarAdicionalResponse> PrepararInsertarAdicionalAsync(PrepararInsertarAdicionalRequest req);
		Task<InsertarAdicionalResponse> InsertarAdicionalAsync(InsertarAdicionalRequest req);

	}

	public class EstadoCuentaService : IEstadoCuentaService
	{
		public EstadoCuentaService()
		{
			// Constructor vacío (sin DI por tu contexto actual).
		}

		// =========================================================
		// PREPARAR NUEVO ESTADO CUENTA
		// =========================================================
		public async Task<PrepararNuevoEstadoCuentaResponse> PrepararNuevoAsync(PrepararNuevoEstadoCuentaRequest req)
		{
			var res = new PrepararNuevoEstadoCuentaResponse();

			try
			{
				using (var _db = new AppDbContext())
				{
					// 1) Fase sugerida
					var maxFase = await _db.T_DEFINICION_TRATAMIENTO
						.AsNoTracking()
						.Where(x => x.ID == req.PacienteId && x.IDDOCTOR == req.DoctorId)
						.MaxAsync(x => (int?)x.FASE) ?? 0;

					res.FaseSugerida = (maxFase <= 0) ? 1 : (maxFase + 1);

					// 2) Tipo facturación del doctor
					res.TipoFacturacionDoctor = await ObtenerTipoFacturacionDoctorAsync(req.DoctorId);

					// 3) Label
					res.LabelDocumento = (res.TipoFacturacionDoctor == 2)
						? "N° Factura"
						: "Compromiso de Compraventa";

					// 4) Documento sugerido (si aplica)
					if (res.TipoFacturacionDoctor == 2)
						res.DocumentoSugerido = await ObtenerFacturaSugeridaAsync(req.DoctorId);

					// 5) Convenio sugerido
					res.ConvenioSugeridoId = await _db.T_DEFINICION_TRATAMIENTO
						.AsNoTracking()
						.Where(x => x.ID == req.PacienteId && x.IDDOCTOR == req.DoctorId)
						.OrderByDescending(x => x.FASE)
						.Select(x => (int?)x.CONVENIO)
						.FirstOrDefaultAsync();

					if (res.ConvenioSugeridoId <= 0) res.ConvenioSugeridoId = null;

					// 6) Config decimales
					var confDec = await ObtenerConfirmacionIntAsync("FACTURAS_RECIBOS_DECIMALES");
					res.UsaDecimalesEnFacturas = (confDec == 0);

					// 7) Personalización factura vieja
					res.PermiteFacturaVieja = await ObtenerPersonalizacionBoolAsync("FACTURA VIEJA");
				}

				res.Ok = true;
				res.Mensaje = null;
				return res;
			}
			catch (Exception ex)
			{
				res.Ok = false;
				res.Mensaje = $"Error preparando datos: {ex.Message}";
				return res;
			}
		}

		// =========================================================
		// CREAR ESTADO CUENTA
		// =========================================================
		public async Task<CrearEstadoCuentaResponse> CrearAsync(CrearEstadoCuentaRequest req)
		{
			var res = new CrearEstadoCuentaResponse
			{
				PacienteId = req.PacienteId,
				DoctorId = req.DoctorId,
				Fase = req.Fase,
				Ok = false
			};

			try
			{
				using (var _db = new AppDbContext())
				{
					// Defaults estilo Delphi
					var numeroCuotaIni = (req.NumeroCuotaInicial <= 0) ? 1 : req.NumeroCuotaInicial;
					var intervaloTiempo = (req.IntervaloTiempoDias <= 0) ? 30 : req.IntervaloTiempoDias;
					var intervaloIni = (req.IntervaloInicialDias <= 0) ? intervaloTiempo : req.IntervaloInicialDias;
					var numeroCuotas = (req.NumeroCuotas <= 0) ? 1 : req.NumeroCuotas;

					// NumeroHistoria si no llega
					var numeroHistoria = req.NumeroHistoria;
					if (string.IsNullOrWhiteSpace(numeroHistoria))
					{
						numeroHistoria = await _db.TANAMNESIS
							.AsNoTracking()
							.Where(x => x.IDANAMNESIS == req.PacienteId)
							.Select(x => x.IDANAMNESIS_TEXTO)
							.FirstOrDefaultAsync();
					}
					numeroHistoria = numeroHistoria ?? "";

					// VIEJO
					short? viejoDb = (req.FacturaVieja == true) ? (short)1 : (short?)null;

					// Documento
					var documento = string.IsNullOrWhiteSpace(req.Documento) ? "0" : req.Documento.Trim();

					// OBSERVACIONES (BLOB)
					byte[]? obsBytes = null;
					if (!string.IsNullOrWhiteSpace(req.Observaciones))
						obsBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(req.Observaciones);

					await using var conn = _db.Database.GetDbConnection();
					if (conn.State != ConnectionState.Open)
						await conn.OpenAsync();

					await using var cmd = conn.CreateCommand();
					cmd.CommandText = @"
						INSERT INTO T_DEFINICION_TRATAMIENTO
						(
						  ID, FASE, IDDOCTOR, NUMERO_HISTORIA, VALOR_TRATAMIENTO, VALOR_CUOTA_INI,
						  FECHA_INICIO, NUMERO_CUOTA_INI, NUMERO_CUOTAS, VALOR_CUOTA, INTERVALO_TIEMPO,
						  DESCRIPCION, FACTURA, INTERVALO_INI, OBSERVACIONES, CONVENIO, IDPRESUPUESTOMAESTRA, VIEJO
						)
						VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
						RETURNING CONSECUTIVO;";

					void Add(object? v)
					{
						var p = cmd.CreateParameter();
						p.Value = v ?? DBNull.Value;
						cmd.Parameters.Add(p);
					}

					Add(req.PacienteId);
					Add(req.Fase);
					Add(req.DoctorId);
					Add(numeroHistoria);
					Add(req.ValorTratamiento);
					Add(req.ValorCuotaInicial);
					Add(req.FechaInicio.Date);
					Add(numeroCuotaIni);
					Add(numeroCuotas);
					Add(req.ValorCuota);
					Add(intervaloTiempo);
					Add(req.Descripcion);
					Add(documento);
					Add(intervaloIni);
					Add(obsBytes);
					Add(req.ConvenioId);
					Add(req.IdPresupuestoMaestra);
					Add(viejoDb);

					var result = await cmd.ExecuteScalarAsync();
					res.Consecutivo = (result == null || result == DBNull.Value) ? null : Convert.ToInt32(result);
				}

				res.Ok = true;
				res.Mensaje = "Estado de cuenta creado correctamente.";
				return res;
			}
			catch (Exception ex)
			{
				res.Ok = false;
				res.Mensaje = $"Error creando estado de cuenta: {ex.Message}";
				return res;
			}
		}

		// =========================================================
		// PREPARAR EDITAR
		// =========================================================
		public async Task<PrepararEditarEstadoCuentaResponse> PrepararEditarAsync(PrepararEditarEstadoCuentaRequest req)
		{
			var res = new PrepararEditarEstadoCuentaResponse { Ok = false };

			try
			{
				using (var _db = new AppDbContext())
				{
					var row = await _db.T_DEFINICION_TRATAMIENTO
						.AsNoTracking()
						.FirstOrDefaultAsync(x => x.ID == req.PacienteId && x.IDDOCTOR == req.DoctorId && x.FASE == req.Fase);

					if (row == null)
					{
						res.Mensaje = "No se encontró el estado de cuenta para editar.";
						return res;
					}

					// Tipo facturación + label
					res.TipoFacturacionDoctor = await ObtenerTipoFacturacionDoctorAsync(req.DoctorId);
					res.LabelDocumento = (res.TipoFacturacionDoctor == 2) ? "N° Factura" : "Compromiso de Compraventa";

					// Precarga de campos
					res.FechaInicio = row.FECHA_INICIO;
					res.ValorTratamiento = row.VALOR_TRATAMIENTO;
					res.ValorCuotaInicial = row.VALOR_CUOTA_INI;
					res.NumeroCuotas = row.NUMERO_CUOTAS;
					res.ValorCuota = row.VALOR_CUOTA;
					res.IntervaloTiempo = row.INTERVALO_TIEMPO;
					res.NumeroCuotaIni = row.NUMERO_CUOTA_INI;
					res.IntervaloIni = row.INTERVALO_INI;
					res.Descripcion = row.DESCRIPCION;
					res.Documento = row.FACTURA;
					res.Observaciones = BlobATexto(row.OBSERVACIONES);
					res.ConvenioId = row.CONVENIO;
				}

				res.Ok = true;
				return res;
			}
			catch (Exception ex)
			{
				res.Ok = false;
				res.Mensaje = $"Error preparando edición: {ex.Message}";
				return res;
			}
		}

		// =========================================================
		// EDITAR
		// =========================================================
		public async Task<EditarEstadoCuentaResponse> EditarAsync(EditarEstadoCuentaRequest req)
		{
			var res = new EditarEstadoCuentaResponse { Ok = false };

			try
			{
				using (var _db = new AppDbContext())
				{
					// Defaults estilo Delphi
					var numeroCuotaIni = (req.NumeroCuotaInicial <= 0) ? 1 : req.NumeroCuotaInicial;
					var intervaloTiempo = (req.IntervaloTiempoDias <= 0) ? 30 : req.IntervaloTiempoDias;
					var intervaloIni = (req.IntervaloInicialDias <= 0) ? intervaloTiempo : req.IntervaloInicialDias;
					var numeroCuotas = (req.NumeroCuotas <= 0) ? 1 : req.NumeroCuotas;

					var documento = string.IsNullOrWhiteSpace(req.Documento) ? "0" : req.Documento.Trim();
					var obsBytes = TextoABlob(req.Observaciones);
					short? viejoDb = req.FacturaVieja ? (short)1 : (short?)null;

					await _db.Database.ExecuteSqlInterpolatedAsync($@"
						UPDATE T_DEFINICION_TRATAMIENTO
						SET
						  VALOR_TRATAMIENTO = {req.ValorTratamiento},
						  VALOR_CUOTA_INI   = {req.ValorCuotaInicial},
						  NUMERO_CUOTAS     = {numeroCuotas},
						  VALOR_CUOTA       = {req.ValorCuota},
						  INTERVALO_TIEMPO  = {intervaloTiempo},
						  NUMERO_CUOTA_INI  = {numeroCuotaIni},
						  INTERVALO_INI     = {intervaloIni},
						  DESCRIPCION       = {req.Descripcion},
						  FACTURA           = {documento},
						  FECHA_INICIO      = {req.FechaInicio},
						  OBSERVACIONES     = {obsBytes},
						  CONVENIO          = {req.ConvenioId},
						  VIEJO             = {viejoDb}
						WHERE ID = {req.PacienteId} AND IDDOCTOR = {req.DoctorId} AND FASE = {req.Fase};
					");
				}

				res.Ok = true;
				res.Mensaje = "Estado de cuenta actualizado correctamente.";
				return res;
			}
			catch (Exception ex)
			{
				res.Ok = false;
				res.Mensaje = $"Error editando estado de cuenta: {ex.Message}";
				return res;
			}
		}

		// =========================================================
		// BORRAR
		// =========================================================
		public async Task<BorrarEstadoCuentaResponse> BorrarAsync(BorrarEstadoCuentaRequest req)
		{
			var res = new BorrarEstadoCuentaResponse { Ok = false };

			try
			{
				var tiene = await TieneAbonosOAdicionalesAsync(req.PacienteId, req.DoctorId, req.Fase);
				res.TieneAbonosOAdicionales = tiene;

				if (tiene)
				{
					res.Mensaje = "Este paciente aún tiene otros Abonos o Cobros. Debe borrarlos primero.";
					return res;
				}

				var filas = await BorrarDefinicionTratamientoAsync(req.PacienteId, req.DoctorId, req.Fase);

				if (filas <= 0)
				{
					res.Mensaje = "No se encontró el estado de cuenta para borrar (0 filas afectadas).";
					return res;
				}

				res.Ok = true;
				res.Mensaje = "Estado de cuenta borrado correctamente.";
				return res;
			}
			catch (Exception ex)
			{
				res.Mensaje = $"Error borrando estado de cuenta: {ex.Message}";
				return res;
			}
		}
		// =========================================================
		// CONSULTAR SUGERIDOS RECIBO Y FACTURA
		// =========================================================

		public async Task<ConsultarSugeridosAbonoResponse> ConsultarSugeridosAbonoAsync(ConsultarSugeridosAbonoRequest req)
		{
			using var _db = new AppDbContext();
			var con = (FbConnection)_db.Database.GetDbConnection();
			if (con.State != ConnectionState.Open) await con.OpenAsync();

			var ocultarFactura = await DebeOcultarFacturaAsync(
				con,
				req.IdPaciente,
				req.Fase,
				req.IdDoctorTratante
				);

			string? recibo = null;
			string? factura = null;
			int? idRes = null;

			if (req.IdDoctorSeleccionado > 0)
			{
				recibo = await ObtenerReciboSugeridoAsync(con, req.IdDoctorSeleccionado);

				if (!ocultarFactura)
				{
					var facInfo = await ObtenerFacturaSugeridaYResolucionAsync(con, req.IdDoctorSeleccionado);
					factura = facInfo.Factura;
					idRes = facInfo.IdResolucion;
				}
				else
				{
					factura = "";
					idRes = null;
				}
			}

			return new ConsultarSugeridosAbonoResponse
			{
				Ok = true,
				OcultarFactura = ocultarFactura,
				ReciboSugerido = recibo,
				FacturaSugerida = factura,
				IdResolucionDian = idRes
			};
		}

		// =========================================================
		// PREPARAR INSERTAR ABONO
		// =========================================================
		public async Task<PrepararInsertarAbonoResponse> PrepararInsertarAbonoAsync(PrepararInsertarAbonoRequest req)
		{
			using (var _db = new AppDbContext())
			{
				var con = (FbConnection)_db.Database.GetDbConnection();
				if (con.State != ConnectionState.Open)
					await con.OpenAsync();

				// Reglas (Delphi: consultarConfirmacion / personalizaciones)
				var rules = new AbonoUiRulesDto
				{
					PermiteCambiarFechaAbono = await ConsultarConfirmacionAsync(con, "FECHAS_FIJAS_RECIBOS") == 1,
					MostrarCampoRecibo = await ConsultarConfirmacionAsync(con, "RECIBOS_OCULTOS_CUENTAS") != 0,
					PermiteEditarFacturaYRecibo = await ConsultarConfirmacionAsync(con, "CONTROL_RECIBOS_FACUTAS") != 0,
					UsaDecimalesEnValores = await ConsultarConfirmacionAsync(con, "FACTURAS_RECIBOS_DECIMALES") == 0,
					PermiteRecibidoPorEnBlanco = await ConsultarConfirmacionAsync(con, "LISTADO_DOCTOR_BLANCO") == 0,

					// OJO: tu lógica original decía "== 0". La dejamos igual.
					RecibidoPorSegunUsuario = await ConsultarConfirmacionAsync(con, "RECIBIDO_POR_SEGUN_USUARIO") == 0,

					ReciboManual = await ConsultarPersonalizacionAsync(con, "RECIBO_MANUAL"),
					UsaCatalogoMotivos = await ConsultarConfirmacionAsync(con, "MOTIVO_EGRESOS_INGRESOS") == 0,
					PermiteFirmaPagos = await ConsultarConfirmacionAsync(con, "FIRMA_PAGOS") == 0,
					FirmaSegunUsuario = await ConsultarConfirmacionAsync(con, "FIRMA_SEGUN_USUARIO") == 0,

					// Tipo REAL (Delphi)
					TipoFacturacion = await TipoFacturacionAsync(con, req.IdDoctorTratante)
				};

				// Estado cuenta básico (mora/valor a facturar)
				var (moraTotal, valorAFacturar) = await ConsultarCuentaBasicaAsync(con, req.IdPaciente, req.Fase, req.IdDoctorTratante);

				// Última fecha abono (TIPO=1)
				var ultimaFechaAbono = await ConsultarUltimaFechaAbonoAsync(con, req.IdPaciente, req.Fase, req.IdDoctorTratante);

				// Doctores para combo Recibido Por
				var doctores = await CargarDoctoresAsync(con);

				int? idRecibidoPorDefecto = null;
				bool recibidoPorHabilitado = true;

				// 1) Caso Delphi: RECIBIDO_POR_SEGUN_USUARIO = 0 => usa TCLAVE.IDDOCTOR y bloquea combo
				if (rules.RecibidoPorSegunUsuario)
				{
					var idDoctorUsuario = await ObtenerIdDoctorPorUsuarioAsync(con, req.UsuarioActual ?? "");

					if (idDoctorUsuario.HasValue && idDoctorUsuario.Value > 0)
					{
						idRecibidoPorDefecto = idDoctorUsuario.Value;
						recibidoPorHabilitado = false;
					}
					else
					{
						// fallback: usa el doctor seleccionado en UI (si llegó)
						idRecibidoPorDefecto = (req.IdDoctorSeleccionadoUi.HasValue && req.IdDoctorSeleccionadoUi.Value > 0)
							? req.IdDoctorSeleccionadoUi.Value
							: (int?)null;

						recibidoPorHabilitado = true;
					}
				}
				else
				{
					// 2) Caso Delphi: si LISTADO_DOCTOR_BLANCO = 0 => se permite dejar en blanco
					if (rules.PermiteRecibidoPorEnBlanco)
					{
						idRecibidoPorDefecto = null; // Delphi: ItemIndex := -1
						recibidoPorHabilitado = true;
					}
					else
					{
						// si NO permite blanco, intenta UI; si no, luego forzamos el primero
						idRecibidoPorDefecto = (req.IdDoctorSeleccionadoUi.HasValue && req.IdDoctorSeleccionadoUi.Value > 0)
							? req.IdDoctorSeleccionadoUi.Value
							: (int?)null;

						recibidoPorHabilitado = true;
					}
				}

				// 3) Si NO permite blanco y quedó vacío, fuerza el primero del listado
				if (!rules.PermiteRecibidoPorEnBlanco && (!idRecibidoPorDefecto.HasValue || idRecibidoPorDefecto.Value <= 0))
				{
					var first = doctores.FirstOrDefault();
					if (first != null) idRecibidoPorDefecto = first.Id;
				}

				// Nombres recibe
				var nombresRecibe = await CargarNombresRecibeAsync(con);

				// Motivos / códigos
				var (motivos, codigos) = rules.UsaCatalogoMotivos
					? await CargarMotivosRapidosAsync(con)
					: await CargarMotivosDesdeCatalogoAsync(con);

				// Sugeridos (si ya hay recibidoPor)
				string? reciboSugerido = null;
				string? facturaSugerida = null;
				int? idResolucionDian = null;

				// ✅ Regla Delphi: decidir si “se debe ocultar factura” (solo a nivel backend)
				var ocultarFactura = await DebeOcultarFacturaAsync(con, req.IdPaciente, req.Fase, req.IdDoctorTratante);

				if (idRecibidoPorDefecto.HasValue && idRecibidoPorDefecto.Value > 0)
				{
					reciboSugerido = await ObtenerReciboSugeridoAsync(con, idRecibidoPorDefecto.Value);

					// Solo sugerimos factura si NO se debe ocultar
					if (!ocultarFactura)
					{
						var facInfo = await ObtenerFacturaSugeridaYResolucionAsync(con, idRecibidoPorDefecto.Value);
						facturaSugerida = facInfo.Factura;
						idResolucionDian = facInfo.IdResolucion;
					}
					else
					{
						facturaSugerida = "";     // para que al menos el front no muestre un consecutivo
						idResolucionDian = null;  // no hay resolución si no aplica factura
					}
				}

				return new PrepararInsertarAbonoResponse
				{
					Ok = true,
					Mensaje = null,

					IdPaciente = req.IdPaciente,
					Fase = req.Fase,
					IdDoctorTratante = req.IdDoctorTratante,

					MoraTotal = moraTotal,
					ValorAFacturar = valorAFacturar,

					FechaHoy = DateTime.Now.ToString("yyyy-MM-dd"),
					UltimaFechaAbono = ultimaFechaAbono?.ToString("yyyy-MM-dd"),

					Rules = rules,

					DoctoresRecibidoPor = doctores,
					IdRecibidoPorPorDefecto = idRecibidoPorDefecto,
					RecibidoPorHabilitado = recibidoPorHabilitado,

					NombresRecibe = nombresRecibe,
					NombreRecibePorDefecto = nombresRecibe.FirstOrDefault(),

					Motivos = motivos,
					CodigosConcepto = codigos,

					ReciboSugerido = reciboSugerido,
					FacturaSugerida = facturaSugerida,
					IdResolucionDian = idResolucionDian,

					ValoresIvaPermitidos = new List<double>()
				};
			}
		}

		// =========================================================
		// INSERTAR ABONO
		// =========================================================
		public async Task<InsertarAbonoResponse> InsertarAbonoAsync(InsertarAbonoRequest req)
		{
			if (req.IdPaciente <= 0) return new InsertarAbonoResponse { Ok = false, Mensaje = "IdPaciente inválido." };
			if (req.Fase <= 0) return new InsertarAbonoResponse { Ok = false, Mensaje = "Fase inválida." };
			if (req.IdDoctorTratante <= 0) return new InsertarAbonoResponse { Ok = false, Mensaje = "IdDoctorTratante inválido." };
			if (!req.IdRecibidoPor.HasValue || req.IdRecibidoPor.Value <= 0)
				return new InsertarAbonoResponse { Ok = false, Mensaje = "Debe seleccionar Recibido Por." };
			if (req.ConceptosDetalle == null || req.ConceptosDetalle.Count == 0)
				return new InsertarAbonoResponse { Ok = false, Mensaje = "Debe enviar al menos un concepto del abono." };


			var total = req.TiposPago?.Sum(x => x.Valor) ?? 0;
			if (total <= 0) return new InsertarAbonoResponse { Ok = false, Mensaje = "El total a abonar debe ser mayor a 0." };

			using (var _db = new AppDbContext())
			{
				var con = (FbConnection)_db.Database.GetDbConnection();
				if (con.State != ConnectionState.Open)
					await con.OpenAsync();

				using var tx = con.BeginTransaction(IsolationLevel.ReadCommitted);

				try
				{
					var tipoFacturacion = await TipoFacturacionAsync(con, req.IdDoctorTratante, tx);

					// ✅ Regla Delphi: decidir si debe ocultarse factura para este abono
					var ocultarFactura = await DebeOcultarFacturaAsync(con, req.IdPaciente, req.Fase, req.IdDoctorTratante, tx);

					// Si NO aplica factura, la vaciamos SIEMPRE (aunque el front la mande)
					if (ocultarFactura)
						req.Factura = "";

					bool ajusto = false;

					// Revalidar recibo si viene
					if (!string.IsNullOrWhiteSpace(req.Recibo))
					{
						var reciboActual = await ObtenerReciboSugeridoAsync(con, req.IdRecibidoPor.Value, tx);
						if (!string.Equals(req.Recibo?.Trim(), reciboActual?.Trim(), StringComparison.OrdinalIgnoreCase))
						{
							req.Recibo = reciboActual;
							ajusto = true;
						}
					}

					// Revalidar factura si viene (solo si aplica factura, y solo forzamos fuerte cuando tipoFacturacion=3)
					if (!ocultarFactura && !string.IsNullOrWhiteSpace(req.Factura))
					{
						var facInfo = await ObtenerFacturaSugeridaYResolucionAsync(con, req.IdRecibidoPor.Value, tx);
						if (!string.Equals(req.Factura?.Trim(), facInfo.Factura?.Trim(), StringComparison.OrdinalIgnoreCase))
						{
							if (tipoFacturacion == 3)
							{
								req.Factura = facInfo.Factura;
								ajusto = true;
							}
						}
					}

					// Resolución DIAN (solo si aplica factura)
					var idResolucionDian = -1;
					if (!ocultarFactura)
					{
						var facInfo2 = await ObtenerFacturaSugeridaYResolucionAsync(con, req.IdRecibidoPor.Value, tx);
						idResolucionDian = facInfo2.IdResolucion ?? -1;
					}

					// GEN_ID
					var idRelacion = await NextGenAsync(con, tx, "GEN_ADICIONALES_ABONOS");

					// Identificador = MAX + 1
					var identificador = await ObtenerSiguienteIdentificadorAsync(con, req.IdPaciente, req.Fase, req.IdDoctorTratante, tx);

					// Insertar ABONO (TIPO=1)
					await InsertarAdicionalAbonoAsync(con, tx,
						idPaciente: req.IdPaciente,
						fase: req.Fase,
						idDoctorTratante: req.IdDoctorTratante,
						tipo: 1,
						valor: total,
						fecha: ParseDate(req.FechaAbono),
						hora: DateTime.Now.TimeOfDay,
						recibo: req.Recibo ?? "",
						reciboRelacionado: req.ReciboRelacionado,
						factura: ocultarFactura ? "" : req.Factura,
						descripcion: req.Descripcion,
						codigoConcepto: req.CodigoConcepto,
						conIva: req.IvaIncluido ? (short)0 : (short?)null, // Delphi: si incluido => CONIVA=0
						valorIva: req.IvaIncluido ? req.ValorIva : null,
						idFirma: req.IdFirma ?? -1,
						recibidoPor: req.IdRecibidoPor.Value,
						nombreRecibe: req.NombreRecibe,
						pagoTercero: req.PagoTercero,
						idRelacion: idRelacion,
						idResolucionDian: idResolucionDian,
						consecutivoNotaCredito: 0,
						identificador: identificador
					);

					// =====================================================
					// ✅ INSERTAR CONCEPTOS (T_ADICIONALES_ABONOS_MOTIVOS)
					// =====================================================
					foreach (var c in req.ConceptosDetalle ?? new List<AbonoConceptoDetalleDto>())
					{
						if (string.IsNullOrWhiteSpace(c.Codigo) && string.IsNullOrWhiteSpace(c.Descripcion))
							continue;

						if (c.Valor <= 0) continue;
						if (c.Cantidad <= 0) continue;

						// ✅ Escritorio: guarda el PORCENTAJE en VALORIVA y deja PORCENTAJEIVA en NULL
						var porcentaje = c.IvaIncluido ? Math.Max(c.PorcentajeIva, 0) : 0;

						await InsertarAbonoMotivoAsync(con, tx,
							idRelacion: idRelacion,
							descripcion: c.Descripcion ?? "",
							codigo: c.Codigo ?? "",
							valorUnitario: c.Valor,
							cantidad: c.Cantidad,
							porcentajeIvaEnValorIva: porcentaje
						);

					}


					// Insertar tipos de pago
					foreach (var tp in req.TiposPago ?? new List<AbonoTipoPagoDto>())
					{
						if (tp.Valor <= 0) continue;

						await InsertarAbonoTipoPagoAsync(con, tx,
							valor: tp.Valor,
							descripcion: tp.Descripcion ?? "",
							tipoDePago: tp.TipoDePago,
							idRelacion: idRelacion,
							numero: tp.Numero ?? "",
							fechaTexto: tp.FechaTexto ?? ""
						);
					}

					// SP estilo Delphi
					await EjecutarIncrementarIdentificadorAsync(con, tx, req.IdPaciente, req.Fase, req.IdDoctorTratante);

					tx.Commit();

					// refrescar mora total
					var (moraTotal, _) = await ConsultarCuentaBasicaAsync(con, req.IdPaciente, req.Fase, req.IdDoctorTratante);

					return new InsertarAbonoResponse
					{
						Ok = true,
						Mensaje = null,

						IdRelacion = idRelacion,
						Identificador = identificador,

						ReciboUsado = req.Recibo,
						FacturaUsada = ocultarFactura ? "" : req.Factura,

						AjustoConsecutivos = ajusto,
						MoraTotalActualizada = moraTotal
					};
				}
				catch (Exception ex)
				{
					try { tx.Rollback(); } catch { }
					return new InsertarAbonoResponse { Ok = false, Mensaje = ex.Message };
				}
			}
		}

		// =========================================================
		// PREPARAR BORRAR ABONO
		// =========================================================
		public async Task<PrepararBorrarAbonoResponse> PrepararBorrarAbonoAsync(PrepararBorrarAbonoRequest req)
		{
			if (req.IdPaciente <= 0) return new PrepararBorrarAbonoResponse { Ok = false, Mensaje = "IdPaciente inválido." };
			if (req.Fase <= 0) return new PrepararBorrarAbonoResponse { Ok = false, Mensaje = "Fase inválida." };
			if (req.IdDoctorTratante <= 0) return new PrepararBorrarAbonoResponse { Ok = false, Mensaje = "IdDoctorTratante inválido." };
			if (req.Identificador <= 0) return new PrepararBorrarAbonoResponse { Ok = false, Mensaje = "Identificador inválido." };

			using (var _db = new AppDbContext())
			{
				var con = (FbConnection)_db.Database.GetDbConnection();
				if (con.State != ConnectionState.Open)
					await con.OpenAsync();

				// 🔎 Traemos las relaciones asociadas a ese IDENTIFICADOR.
				// Esto es CRÍTICO para borrar motivos/tipos de pago, igual que Delphi.
				var idRelaciones = await ObtenerIdRelacionesPorIdentificadorAsync(
					con, req.IdPaciente, req.Fase, req.IdDoctorTratante, req.Identificador, tx: null);

				if (idRelaciones.Count == 0)
					return new PrepararBorrarAbonoResponse { Ok = false, Mensaje = "No se encontró el abono (identificador) para borrar." };

				// Un resumen simple para mostrar en UI (sin enredos)
				var resumen = await ConstruirResumenBorrarAbonoAsync(
					con, req.IdPaciente, req.Fase, req.IdDoctorTratante, req.Identificador, tx: null);

				return new PrepararBorrarAbonoResponse
				{
					Ok = true,
					Mensaje = null,
					IdPaciente = req.IdPaciente,
					Fase = req.Fase,
					IdDoctorTratante = req.IdDoctorTratante,
					Identificador = req.Identificador,
					IdRelaciones = idRelaciones,
					ResumenParaConfirmar = resumen,
					RequiereMotivo = true
				};
			}
		}


		// =========================================================
		// BORRAR ABONO (DELPI STYLE)
		// =========================================================
		public async Task<BorrarAbonoResponse> BorrarAbonoAsync(BorrarAbonoRequest req)
		{
			if (req.IdPaciente <= 0) return new BorrarAbonoResponse { Ok = false, Mensaje = "IdPaciente inválido." };
			if (req.Fase <= 0) return new BorrarAbonoResponse { Ok = false, Mensaje = "Fase inválida." };
			if (req.IdDoctorTratante <= 0) return new BorrarAbonoResponse { Ok = false, Mensaje = "IdDoctorTratante inválido." };
			if (req.Identificador <= 0) return new BorrarAbonoResponse { Ok = false, Mensaje = "Identificador inválido." };

			// Delphi pide motivo: acá lo hacemos obligatorio
			if (string.IsNullOrWhiteSpace(req.Motivo))
				return new BorrarAbonoResponse { Ok = false, Mensaje = "Debe escribir un motivo para borrar." };

			// La tabla guarda MOTIVO VARCHAR(200)
			var motivo = req.Motivo.Trim();
			if (motivo.Length > 200) motivo = motivo.Substring(0, 200);

			using (var _db = new AppDbContext())
			{
				var con = (FbConnection)_db.Database.GetDbConnection();
				if (con.State != ConnectionState.Open)
					await con.OpenAsync();

				using var tx = con.BeginTransaction(IsolationLevel.ReadCommitted);

				try
				{
					// 1) Buscar IDRELACION(es) a borrar (por IDENTIFICADOR)
					var idRelaciones = await ObtenerIdRelacionesPorIdentificadorAsync(
						con, req.IdPaciente, req.Fase, req.IdDoctorTratante, req.Identificador, tx);

					if (idRelaciones.Count == 0)
						return new BorrarAbonoResponse { Ok = false, Mensaje = "No se encontró el abono (identificador) para borrar." };

					// 2) Copiar a tabla BORRAR (backup + motivo) - estilo Delphi
					//    (se copia TODO lo que exista en T_ADICIONALES_ABONOS para ese IDENTIFICADOR)
					var copiados = await CopiarAbonoABorrarAsync(
						con, tx, req.IdPaciente, req.Fase, req.IdDoctorTratante, req.Identificador, motivo);

					// 3) Borrar dependencias por IDRELACION
					//    - Motivos (conceptos)
					//    - Tipos de pago
					//    - XDR pagar (por IDABONO o IDADICIONAL)
					foreach (var idRel in idRelaciones)
					{
						await DeleteByIdRelacionAsync(con, tx, "T_ADICIONALES_ABONOS_MOTIVOS", "IDRELACION", idRel);
						await DeleteByIdRelacionAsync(con, tx, "T_ABONOS_TIPO_PAGO", "IDRELACION", idRel);

						// Esta tabla usa IDABONO e IDADICIONAL. Por seguridad borramos por ambos.
						await DeleteAbonosXdrPagarAsync(con, tx, idRel);
					}

					// 4) Borrar el/los registros principales por IDENTIFICADOR (todos los TIPOS que hayan quedado en ese paquete)
					var borrados = await BorrarAdicionalesPorIdentificadorAsync(
						con, tx, req.IdPaciente, req.Fase, req.IdDoctorTratante, req.Identificador);

					// 5) Recalcular/normalizar (Delphi ejecuta su lógica de actualización)
					await EjecutarActualizarEstadoCuentaAsync(con, tx, req.IdPaciente, req.Fase, req.IdDoctorTratante);

					tx.Commit();

					// 6) Refrescar mora total para UI
					var (moraTotal, _) = await ConsultarCuentaBasicaAsync(con, req.IdPaciente, req.Fase, req.IdDoctorTratante);

					return new BorrarAbonoResponse
					{
						Ok = true,
						Mensaje = null,
						RegistrosBorrados = borrados,
						MoraTotalActualizada = moraTotal
					};
				}
				catch (Exception ex)
				{
					try { tx.Rollback(); } catch { }
					return new BorrarAbonoResponse { Ok = false, Mensaje = ex.Message };
				}
			}
		}

		// =========================================================
		// PREPARAR INSERTAR ADICIONAL (TIPO=2) - 1:1 Delphi
		// =========================================================
		public async Task<PrepararInsertarAdicionalResponse> PrepararInsertarAdicionalAsync(PrepararInsertarAdicionalRequest req)
		{
			if (req.IdPaciente <= 0) return new PrepararInsertarAdicionalResponse { Ok = false, Mensaje = "IdPaciente inválido." };
			if (req.Fase <= 0) return new PrepararInsertarAdicionalResponse { Ok = false, Mensaje = "Fase inválida." };
			if (req.IdDoctorTratante <= 0) return new PrepararInsertarAdicionalResponse { Ok = false, Mensaje = "IdDoctorTratante inválido." };

			using (var _db = new AppDbContext())
			{
				var con = (FbConnection)_db.Database.GetDbConnection();
				if (con.State != ConnectionState.Open)
					await con.OpenAsync();

				// Reglas: reutilizamos AbonoUiRulesDto, PERO en adicionales NO hay recibo/factura.
				var rules = new AbonoUiRulesDto
				{
					PermiteCambiarFechaAbono = await ConsultarConfirmacionAsync(con, "FECHAS_FIJAS_RECIBOS") == 1,

					// ✅ En Delphi (Adicionales) no se usa Recibo/Factura en la UI.
					MostrarCampoRecibo = false,
					PermiteEditarFacturaYRecibo = false,
					ReciboManual = false,

					UsaDecimalesEnValores = await ConsultarConfirmacionAsync(con, "FACTURAS_RECIBOS_DECIMALES") == 0,
					PermiteRecibidoPorEnBlanco = await ConsultarConfirmacionAsync(con, "LISTADO_DOCTOR_BLANCO") == 0,

					RecibidoPorSegunUsuario = await ConsultarConfirmacionAsync(con, "RECIBIDO_POR_SEGUN_USUARIO") == 0,

					UsaCatalogoMotivos = await ConsultarConfirmacionAsync(con, "MOTIVO_EGRESOS_INGRESOS") == 0,
					PermiteFirmaPagos = await ConsultarConfirmacionAsync(con, "FIRMA_PAGOS") == 0,
					FirmaSegunUsuario = await ConsultarConfirmacionAsync(con, "FIRMA_SEGUN_USUARIO") == 0,

					TipoFacturacion = await TipoFacturacionAsync(con, req.IdDoctorTratante)
				};

				var (moraTotal, valorAFacturar) =
					await ConsultarCuentaBasicaAsync(con, req.IdPaciente, req.Fase, req.IdDoctorTratante);

				var doctores = await CargarDoctoresAsync(con);

				var motivos = await CargarMotivosAdicionalesAsync(con);

				int? idRecibidoPorDefecto = null;
				bool recibidoPorHabilitado = true;

				// 1) Delphi: RECIBIDO_POR_SEGUN_USUARIO=0 => usa TCLAVE.IDDOCTOR y bloquea combo
				if (rules.RecibidoPorSegunUsuario)
				{
					var idDoctorUsuario = await ObtenerIdDoctorPorUsuarioAsync(con, req.UsuarioActual ?? "");
					if (idDoctorUsuario.HasValue && idDoctorUsuario.Value > 0)
					{
						idRecibidoPorDefecto = idDoctorUsuario.Value;
						recibidoPorHabilitado = false;
					}
					else
					{
						idRecibidoPorDefecto = (req.IdDoctorSeleccionadoUi.HasValue && req.IdDoctorSeleccionadoUi.Value > 0)
							? req.IdDoctorSeleccionadoUi.Value
							: (int?)null;
						recibidoPorHabilitado = true;
					}
				}
				else
				{
					// 2) Delphi: si LISTADO_DOCTOR_BLANCO=0 => permite blanco (ItemIndex=-1)
					if (rules.PermiteRecibidoPorEnBlanco)
					{
						idRecibidoPorDefecto = null;
						recibidoPorHabilitado = true;
					}
					else
					{
						idRecibidoPorDefecto = (req.IdDoctorSeleccionadoUi.HasValue && req.IdDoctorSeleccionadoUi.Value > 0)
							? req.IdDoctorSeleccionadoUi.Value
							: (int?)null;
						recibidoPorHabilitado = true;
					}
				}

				// 3) Si NO permite blanco y quedó vacío, fuerza el primero
				if (!rules.PermiteRecibidoPorEnBlanco && (!idRecibidoPorDefecto.HasValue || idRecibidoPorDefecto.Value <= 0))
				{
					var first = doctores.FirstOrDefault();
					if (first != null) idRecibidoPorDefecto = first.Id;
				}

				var nombresRecibe = await CargarNombresRecibeAsync(con);

				return new PrepararInsertarAdicionalResponse
				{
					Ok = true,

					IdPaciente = req.IdPaciente,
					Fase = req.Fase,
					IdDoctorTratante = req.IdDoctorTratante,

					MoraTotal = moraTotal,
					ValorAFacturar = valorAFacturar,

					FechaHoy = DateTime.Now.ToString("yyyy-MM-dd"),

					Rules = rules,

					DoctoresRecibidoPor = doctores,
					Motivos = motivos,
					IdRecibidoPorPorDefecto = idRecibidoPorDefecto,
					RecibidoPorHabilitado = recibidoPorHabilitado,

					NombresRecibe = nombresRecibe,
					NombreRecibePorDefecto = nombresRecibe.FirstOrDefault()
				};
			}
		}


		// =========================================================
		// INSERTAR ADICIONALES (TIPO=2) - 1:1 Delphi
		// - Inserta N líneas como N filas en T_ADICIONALES_ABONOS
		// - Por cada fila: genera IDRELACION, asigna HORA distinta, TIPO=2, RECIBO/FACTURA=''
		// - Llama PagosConAticipos (tu método) por cada fila (condiciones internas Delphi)
		// - Luego ejecuta:
		//    P_INCREMENTAR_IDENTIFICADOR(id,fase,idDoctor)
		//    ACTUALIZARESTACUENTA(id,fase,idDoctor)
		// =========================================================
		public async Task<InsertarAdicionalResponse> InsertarAdicionalAsync(InsertarAdicionalRequest req)
		{
			if (req.IdPaciente <= 0) return new InsertarAdicionalResponse { Ok = false, Mensaje = "IdPaciente inválido." };
			if (req.Fase <= 0) return new InsertarAdicionalResponse { Ok = false, Mensaje = "Fase inválida." };
			if (req.IdDoctorTratante <= 0) return new InsertarAdicionalResponse { Ok = false, Mensaje = "IdDoctorTratante inválido." };

			if (!req.IdRecibidoPor.HasValue || req.IdRecibidoPor.Value <= 0)
				return new InsertarAdicionalResponse { Ok = false, Mensaje = "Debe seleccionar Recibido Por." };

			// ✅ Normalización: si vienen Items => N inserts
			// Si NO vienen Items pero viene legacy (Descripcion/Valor) => 1 insert (compatibilidad)
			var normalizedItems = (req.Items ?? new()).Where(x => x != null).ToList();

			if (normalizedItems.Count == 0)
			{
				// modo legacy: lo convertimos a un item
				if (!string.IsNullOrWhiteSpace(req.Descripcion) && req.Valor > 0)
				{
					normalizedItems.Add(new AdicionalItemDto
					{
						Descripcion = req.Descripcion!.Trim(),
						Cantidad = 1,
						ValorUnitario = req.Valor,
						CodigoConcepto = req.CodigoConcepto
					});
				}
				else
				{
					return new InsertarAdicionalResponse { Ok = false, Mensaje = "Debe agregar al menos un adicional." };
				}
			}

			// Validación fuerte (para que el front no mande basura)
			foreach (var it in normalizedItems)
			{
				if (string.IsNullOrWhiteSpace(it.Descripcion))
					return new InsertarAdicionalResponse { Ok = false, Mensaje = "Cada adicional debe tener descripción." };

				if (it.Cantidad <= 0) it.Cantidad = 1;

				if (it.ValorUnitario <= 0)
					return new InsertarAdicionalResponse
					{
						Ok = false,
						Mensaje = $"El valor del adicional '{it.Descripcion}' debe ser mayor a 0."
					};
			}

			using (var _db = new AppDbContext())
			{
				var con = (FbConnection)_db.Database.GetDbConnection();
				if (con.State != ConnectionState.Open)
					await con.OpenAsync();

				using var tx = con.BeginTransaction(IsolationLevel.ReadCommitted);

				try
				{
					var fecha = ParseDate(req.Fecha).Date;

					// Delphi: IdFirma si no aplica => -1
					var idFirma = (req.IdFirma.HasValue && req.IdFirma.Value > 0) ? req.IdFirma.Value : -1;

					// Para que queden ordenados por hora como Delphi
					var baseHora = DateTime.Now.TimeOfDay;
					var sec = 1;

					// Identificador inicial: MAX+1
					var nextIdentificador = await ObtenerSiguienteIdentificadorAsync(con, req.IdPaciente, req.Fase, req.IdDoctorTratante, tx);

					bool relacionoAnticiposAlguna = false;
					double restanteAcumulado = 0;

					var result = new InsertarAdicionalResponse { Ok = true };

					foreach (var it in normalizedItems)
					{
						var valorTotal = it.ValorUnitario * it.Cantidad;

						// IDRELACION
						var idRelacion = await NextGenAsync(con, tx, "GEN_ADICIONALES_ABONOS");

						// Identificador base (luego SP puede reajustar)
						var identificador = nextIdentificador++;

						await InsertarAdicionalAbonoAsync(
							con, tx,
							idPaciente: req.IdPaciente,
							fase: req.Fase,
							idDoctorTratante: req.IdDoctorTratante,
							tipo: 2,
							valor: valorTotal,
							fecha: fecha,
							hora: baseHora.Add(TimeSpan.FromSeconds(sec++)),
							recibo: "",                 // Delphi
							reciboRelacionado: "",      // Delphi
							factura: "",                // Delphi
							descripcion: it.Descripcion.Trim(),
							codigoConcepto: it.CodigoConcepto,
							conIva: req.IvaIncluido ? (short)0 : (short?)null,
							valorIva: req.IvaIncluido ? req.ValorIva : null,
							idFirma: idFirma,
							recibidoPor: req.IdRecibidoPor.Value,
							nombreRecibe: req.NombreRecibe,
							pagoTercero: req.PagoTercero,
							idRelacion: idRelacion,
							idResolucionDian: null, // adicional => null
							consecutivoNotaCredito: await ConsectutivoAdicionalAsync(con, tx, req.IdDoctorTratante),
							identificador: identificador
						);

						// ✅ Relacionar anticipos (tu misma lógica Delphi interna)
						// Si tu método ya valida RELACIONAR_ANTICIPOS + IDPRESUPUESTOMAESTRA, perfecto.
						var rel = await RelacionarAnticiposComoDelphiAsync(
							con, tx,
							costo: valorTotal,
							idPaciente: req.IdPaciente,
							fase: req.Fase,
							idDoctorTratante: req.IdDoctorTratante,
							idAdicionalRelacion: idRelacion
						);

						if (rel.Relaciono) relacionoAnticiposAlguna = true;
						restanteAcumulado += rel.Restante;

						result.ItemsInsertados.Add(new InsertarAdicionalItemResultDto
						{
							IdRelacion = idRelacion,
							Identificador = identificador,
							Descripcion = it.Descripcion.Trim(),
							ValorTotal = valorTotal
						});
					}

					// Post-proceso Delphi
					await EjecutarPIncrementarIdentificadorAsync(con, tx, req.IdPaciente, req.Fase, req.IdDoctorTratante);
					await EjecutarActualizarEstaCuentaAsync(con, tx, req.IdPaciente, req.Fase, req.IdDoctorTratante);

					// Ajustar identificadores reales (por si el SP reordenó)
					foreach (var x in result.ItemsInsertados)
					{
						x.Identificador = await ObtenerIdentificadorPorIdRelacionAsync(con, tx, x.IdRelacion);
					}

					tx.Commit();

					var (moraTotal, _) = await ConsultarCuentaBasicaAsync(con, req.IdPaciente, req.Fase, req.IdDoctorTratante);

					result.RelacionoAnticipos = relacionoAnticiposAlguna;
					result.RestanteTrasAnticipos = restanteAcumulado;
					result.MoraTotalActualizada = moraTotal;

					return result;
				}
				catch (Exception ex)
				{
					try { tx.Rollback(); } catch { }
					return new InsertarAdicionalResponse { Ok = false, Mensaje = ex.Message };
				}
			}
		}




		// =========================================================
		// HELPERS PRIVADOS
		// =========================================================
		

		private async Task<bool> TieneAbonosOAdicionalesAsync(int pacienteId, int doctorId, int fase)
		{
			using (var _db = new AppDbContext())
			{
				await using var conn = _db.Database.GetDbConnection();
				if (conn.State != ConnectionState.Open)
					await conn.OpenAsync();

				await using var cmd = conn.CreateCommand();
				cmd.CommandText = @"
					SELECT FIRST 1 1
					FROM T_ADICIONALES_ABONOS a
					WHERE a.ID = @id AND a.FASE = @fase AND a.IDDOCTOR = @doc
				";

				var p1 = cmd.CreateParameter();
				p1.ParameterName = "@id";
				p1.Value = pacienteId;
				cmd.Parameters.Add(p1);

				var p2 = cmd.CreateParameter();
				p2.ParameterName = "@fase";
				p2.Value = fase;
				cmd.Parameters.Add(p2);

				var p3 = cmd.CreateParameter();
				p3.ParameterName = "@doc";
				p3.Value = doctorId;
				cmd.Parameters.Add(p3);

				var result = await cmd.ExecuteScalarAsync();
				return result != null && result != DBNull.Value;
			}
		}

		private async Task<int> BorrarDefinicionTratamientoAsync(int pacienteId, int doctorId, int fase)
		{
			try
			{
				using (var _db = new AppDbContext())
				{
					await using var conn = _db.Database.GetDbConnection();
					if (conn.State != ConnectionState.Open)
						await conn.OpenAsync();

					await using var cmd = conn.CreateCommand();
					cmd.CommandText = @"
						DELETE FROM T_DEFINICION_TRATAMIENTO
						WHERE ID = @id AND IDDOCTOR = @doc AND FASE = @fase
					";

					var p1 = cmd.CreateParameter();
					p1.ParameterName = "@id";
					p1.Value = pacienteId;
					cmd.Parameters.Add(p1);

					var p2 = cmd.CreateParameter();
					p2.ParameterName = "@doc";
					p2.Value = doctorId;
					cmd.Parameters.Add(p2);

					var p3 = cmd.CreateParameter();
					p3.ParameterName = "@fase";
					p3.Value = fase;
					cmd.Parameters.Add(p3);

					return await cmd.ExecuteNonQueryAsync();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error borrando estado de cuenta: {ex.Message}", ex);
			}
		}

		private static DateTime ParseDate(string yyyyMMdd)
		{
			// Acepta "yyyy-MM-dd" o algo parseable
			if (DateTime.TryParse(yyyyMMdd, out var dt)) return dt.Date;

			return DateTime.ParseExact(
				yyyyMMdd,
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture
			).Date;
		}

		private async Task<int> ConsultarConfirmacionAsync(FbConnection con, string nombreConfig, FbTransaction? tx = null)
		{
			const string sql = @"
			SELECT COALESCE(c.PERMISO, 1)
			FROM TCONFIGURACIONES_RYDENT c
			WHERE UPPER(c.NOMBRE) = UPPER(@NOMBRE)
		";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@NOMBRE", nombreConfig);

			var o = await cmd.ExecuteScalarAsync();
			if (o == null || o == DBNull.Value) return 1;

			return Convert.ToInt32(o);
		}

		private async Task<bool> ConsultarPersonalizacionAsync(FbConnection con, string nombrePersonalizacion, FbTransaction? tx = null)
		{
			const string sql = @"
			SELECT COALESCE(p.PERMISO, 0)
			FROM T_PERSONALIZACIONES  p
			WHERE UPPER(p.NOMBRE) = UPPER(@NOMBRE)
		";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@NOMBRE", nombrePersonalizacion);

			var o = await cmd.ExecuteScalarAsync();
			if (o == null || o == DBNull.Value) return false;

			var txt = o.ToString()?.Trim()?.ToUpperInvariant();
			return txt == "1" || txt == "S" || txt == "SI" || txt == "TRUE";
		}
		private async Task<int> TipoFacturacionAsync(FbConnection con, int idDoctorTratante, FbTransaction? tx = null)
		{
			// Delphi:
			// SELECT FIRST 1 COALESCE(r.TIPO,1) ...
			const string sql = @"
			SELECT FIRST 1 COALESCE(r.TIPO, 1) AS TIPO
			FROM TDATOSDOCTORES d
			LEFT JOIN TINFORMACIONREPORTES i ON i.ID = d.IDREPORTE
			LEFT JOIN TRESOLUCION_DIAN r ON r.ID = i.IDRESOLUCION_DIAN
			WHERE d.ID = @doctor
		";

			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@doctor", idDoctorTratante);

			var o = await cmd.ExecuteScalarAsync();
			if (o == null || o == DBNull.Value) return 1;

			return Convert.ToInt32(o);
		}


		private async Task<bool> EsEstadoCuentaViejoAsync(
		FbConnection con, int idPaciente, int fase, int idDoctorTratante, FbTransaction? tx = null)
		{
			// Solo aplica si existe personalización FACTURA VIEJA
			var usa = await ConsultarPersonalizacionAsync(con, "FACTURA VIEJA", tx);
			if (!usa) return false;

			const string sql = @"
			SELECT COALESCE(d.VIEJO, 0) AS VIEJO
			FROM T_DEFINICION_TRATAMIENTO d
			WHERE d.ID = @ID AND d.FASE = @FASE AND d.IDDOCTOR = @IDDOCTOR
		";

			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);

			var o = await cmd.ExecuteScalarAsync();
			if (o == null || o == DBNull.Value) return false;

			return Convert.ToInt32(o) == 1;
		}

		private async Task<bool> DebeOcultarFacturaAsync(
		FbConnection con, int idPaciente, int fase, int idDoctorTratante, FbTransaction? tx = null)
		{
			var tipoFacturacion = await TipoFacturacionAsync(con, idDoctorTratante, tx);
			var confirmaFactura = await ConsultarConfirmacionAsync(con, "FACTURA_ESTADO_CUETA", tx);
			var esViejo = await EsEstadoCuentaViejoAsync(con, idPaciente, fase, idDoctorTratante, tx);

			return (tipoFacturacion == 2) || (confirmaFactura == 1) || esViejo;
		}

		private async Task<(double MoraTotal, double ValorAFacturar)> ConsultarCuentaBasicaAsync(
		FbConnection con, int idPaciente, int fase, int idDoctorTratante, FbTransaction? tx = null)
		{
			const string sql = @"SELECT * FROM P_CONSULTAR_ESTACUENTA(@ID, @FASE, @IDDOCTOR)";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);

			using var rd = await cmd.ExecuteReaderAsync();
			if (!await rd.ReadAsync()) return (0, 0);

			var mora = rd["MORATOTAL"] == DBNull.Value ? 0 : Convert.ToDouble(rd["MORATOTAL"]);
			var vaf = rd["VALOR_A_FACTURAR"] == DBNull.Value ? 0 : Convert.ToDouble(rd["VALOR_A_FACTURAR"]);

			return (mora, vaf);
		}

		private async Task<DateTime?> ConsultarUltimaFechaAbonoAsync(
			FbConnection con, int idPaciente, int fase, int idDoctorTratante, FbTransaction? tx = null)
		{
			const string sql = @"
			SELECT MAX(a.FECHA)
			FROM T_ADICIONALES_ABONOS a
			WHERE a.ID = @ID AND a.FASE = @FASE AND a.IDDOCTOR = @IDDOCTOR AND a.TIPO = 1
		";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);

			var o = await cmd.ExecuteScalarAsync();
			if (o == null || o == DBNull.Value) return null;

			return Convert.ToDateTime(o).Date;
		}

		private async Task<List<DoctorItemDto>> CargarDoctoresAsync(FbConnection con, FbTransaction? tx = null)
		{
			const string sql = @"SELECT d.ID, d.NOMBRE FROM TDATOSDOCTORES d ORDER BY d.NOMBRE";
			using var cmd = new FbCommand(sql, con, tx);
			using var rd = await cmd.ExecuteReaderAsync();

			var list = new List<DoctorItemDto>();
			while (await rd.ReadAsync())
			{
				list.Add(new DoctorItemDto
				{
					Id = Convert.ToInt32(rd["ID"]),
					Nombre = rd["NOMBRE"]?.ToString() ?? ""
				});
			}
			return list;
		}

		private async Task<List<string>> CargarNombresRecibeAsync(FbConnection con, FbTransaction? tx = null)
		{
			const string sql = @"
			SELECT FIRST 50 DISTINCT a.NOMBRE_RECIBE
			FROM T_ADICIONALES_ABONOS a
			WHERE a.NOMBRE_RECIBE IS NOT NULL AND TRIM(a.NOMBRE_RECIBE) <> ''
			ORDER BY a.NOMBRE_RECIBE
		";
			using var cmd = new FbCommand(sql, con, tx);
			using var rd = await cmd.ExecuteReaderAsync();

			var list = new List<string>();
			while (await rd.ReadAsync())
				list.Add(rd[0]?.ToString() ?? "");

			return list;
		}

		private async Task<(List<MotivoItemDto> Motivos, List<string> Codigos)> CargarMotivosDesdeCatalogoAsync(
			FbConnection con, FbTransaction? tx = null)
		{
			const string sql = @"
			SELECT m.ID, m.DESCRIPCION, m.CODIGO, m.VALOR
			FROM T_ADICIONALES_ABONOS_MOTIVOS m
			WHERE m.DESCRIPCION IS NOT NULL
			ORDER BY m.DESCRIPCION
		";

			using var cmd = new FbCommand(sql, con, tx);
			using var rd = await cmd.ExecuteReaderAsync();

			var motivos = new List<MotivoItemDto>();
			var codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var descripcionesVistas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			while (await rd.ReadAsync())
			{
				var descripcionRaw = rd["DESCRIPCION"] == DBNull.Value ? null : rd["DESCRIPCION"]?.ToString();
				var descripcion = (descripcionRaw ?? "").Trim();

				if (string.IsNullOrWhiteSpace(descripcion))
					continue;

				var descripcionKey = NormalizarTexto(descripcion);

				if (!descripcionesVistas.Add(descripcionKey))
					continue;

				var codigo = rd["CODIGO"] == DBNull.Value ? null : rd["CODIGO"]?.ToString();
				codigo = string.IsNullOrWhiteSpace(codigo) ? null : codigo.Trim();

				if (!string.IsNullOrWhiteSpace(codigo))
					codigos.Add(codigo);

				double? valor = null;
				if (rd["VALOR"] != DBNull.Value)
				{
					var raw = rd["VALOR"];

					if (raw is decimal dec) valor = (double)dec;
					else if (raw is double dbl) valor = dbl;
					else if (raw is float flt) valor = flt;
					else if (double.TryParse(raw.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
						valor = v;
				}

				motivos.Add(new MotivoItemDto
				{
					Id = rd["ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["ID"]),
					Nombre = descripcion,
					Codigo = codigo,
					Valor = valor
				});
			}

			return (motivos, codigos.OrderBy(x => x).ToList());
		}

		private static string NormalizarTexto(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return string.Empty;

			var parts = input
				.Trim()
				.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			return string.Join(' ', parts);
		}

		private async Task<(List<MotivoItemDto> Motivos, List<string> Codigos)> CargarMotivosRapidosAsync(FbConnection con, FbTransaction? tx = null)
		{
			const string sql = @"
			SELECT FIRST 50 DISTINCT a.DESCRIPCION, a.CODIGO_DESCRIPCION
			FROM T_ADICIONALES_ABONOS a
			WHERE a.DESCRIPCION IS NOT NULL AND TRIM(a.DESCRIPCION) <> ''
			ORDER BY a.DESCRIPCION
		";
			using var cmd = new FbCommand(sql, con, tx);
			using var rd = await cmd.ExecuteReaderAsync();

			var motivos = new List<MotivoItemDto>();
			var codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			while (await rd.ReadAsync())
			{
				var desc = rd["DESCRIPCION"]?.ToString() ?? "";
				var cod = rd["CODIGO_DESCRIPCION"]?.ToString();

				if (!string.IsNullOrWhiteSpace(cod)) codigos.Add(cod);

				motivos.Add(new MotivoItemDto
				{
					Id = null,
					Nombre = desc,
					Codigo = cod
				});
			}

			return (motivos, codigos.OrderBy(x => x).ToList());
		}


		private async Task<string?> ObtenerReciboSugeridoAsync(FbConnection con, int idDoctor, FbTransaction? tx = null)
		{
			const string sql = @"SELECT FIRST 1 r.RECIBO FROM P_MAX_RECIBOS_CON_LETRAS(@IDDOCTOR) r ORDER BY r.NUMERO DESC";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctor);

			var o = await cmd.ExecuteScalarAsync();
			return (o == null || o == DBNull.Value) ? null : o.ToString();
		}

		private async Task<(string? Factura, int? IdResolucion)> ObtenerFacturaSugeridaYResolucionAsync(FbConnection con, int idDoctor, FbTransaction? tx = null)
		{
			const string sql = @"SELECT FIRST 1 f.FACTURA, f.IDRESOLUCION FROM P_MAX_CON_LETRAS(@IDDOCTOR) f ORDER BY f.NUMERO DESC";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctor);

			using var rd = await cmd.ExecuteReaderAsync();
			if (!await rd.ReadAsync()) return (null, null);

			var factura = rd["FACTURA"] == DBNull.Value ? null : rd["FACTURA"]?.ToString();
			var idRes = rd["IDRESOLUCION"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["IDRESOLUCION"]);

			return (factura, idRes);
		}

		private async Task<int> NextGenAsync(FbConnection con, FbTransaction tx, string generatorName)
		{
			var sql = $"SELECT GEN_ID({generatorName}, 1) FROM RDB$DATABASE";
			using var cmd = new FbCommand(sql, con, tx);

			var o = await cmd.ExecuteScalarAsync();
			if (o == null || o == DBNull.Value)
				throw new Exception($"No se pudo generar ID con {generatorName}.");

			return Convert.ToInt32(o);
		}

		private async Task<int> ObtenerSiguienteIdentificadorAsync(FbConnection con, int idPaciente, int fase, int idDoctorTratante, FbTransaction tx)
		{
			const string sql = @"
			SELECT COALESCE(MAX(a.IDENTIFICADOR), 0)
			FROM T_ADICIONALES_ABONOS a
			WHERE a.ID = @ID AND a.FASE = @FASE AND a.IDDOCTOR = @IDDOCTOR
		";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);

			var o = await cmd.ExecuteScalarAsync();
			var max = (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o);

			return max + 1;
		}

		private async Task InsertarAdicionalAbonoAsync(
	FbConnection con, FbTransaction tx,
	int idPaciente, int fase, int idDoctorTratante,
	int tipo, double valor, DateTime fecha, TimeSpan hora,
	string recibo, string? reciboRelacionado, string? factura,
	string? descripcion, string? codigoConcepto,
	short? conIva, double? valorIva,
	int idFirma, int recibidoPor,
	string? nombreRecibe, int pagoTercero,
	int idRelacion, int? idResolucionDian,   // ✅ nullable
	int consecutivoNotaCredito,
	int identificador)
		{
			const string sql = @"
		INSERT INTO T_ADICIONALES_ABONOS
		(
			ID, FASE, TIPO, VALOR, FECHA, DESCRIPCION, RECIBO, IDENTIFICADOR, HORA,
			IDDOCTOR, IDRELACION, IDRESOLIUCIONDIAN, FACTURA,
			RECIBIDO_POR, FIRMA, CONIVA, VALORIVA, PAGOTERCERO, NOMBRE_RECIBE,
			CODIGO_DESCRIPCION, CONSECUTIVO_NOTACREDITO, RECIBO_RELACIONADO
		)
		VALUES
		(
			@ID, @FASE, @TIPO, @VALOR, @FECHA, @DESCRIPCION, @RECIBO, @IDENTIFICADOR, @HORA,
			@IDDOCTOR, @IDRELACION, @IDRESOLIUCIONDIAN, @FACTURA,
			@RECIBIDO_POR, @FIRMA, @CONIVA, @VALORIVA, @PAGOTERCERO, @NOMBRE_RECIBE,
			@CODIGO_DESCRIPCION, @CONSECUTIVO_NOTACREDITO, @RECIBO_RELACIONADO
		)
	";

			using var cmd = new FbCommand(sql, con, tx);

			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@TIPO", tipo);
			cmd.Parameters.AddWithValue("@VALOR", valor);
			cmd.Parameters.AddWithValue("@FECHA", fecha.Date);

			cmd.Parameters.AddWithValue("@DESCRIPCION", (object?)descripcion ?? "");
			cmd.Parameters.AddWithValue("@RECIBO", recibo ?? "");
			cmd.Parameters.AddWithValue("@IDENTIFICADOR", identificador);

			var pHora = cmd.Parameters.Add("@HORA", FbDbType.Time);
			pHora.Value = hora;

			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);
			cmd.Parameters.AddWithValue("@IDRELACION", idRelacion);

			// ✅ CORRECCIÓN: permitir NULL
			if (idResolucionDian.HasValue) cmd.Parameters.AddWithValue("@IDRESOLIUCIONDIAN", idResolucionDian.Value);
			else cmd.Parameters.Add("@IDRESOLIUCIONDIAN", FbDbType.Integer).Value = DBNull.Value;

			cmd.Parameters.AddWithValue("@FACTURA", (object?)factura ?? "");
			cmd.Parameters.AddWithValue("@RECIBIDO_POR", recibidoPor);
			cmd.Parameters.AddWithValue("@FIRMA", idFirma);

			if (conIva.HasValue) cmd.Parameters.AddWithValue("@CONIVA", conIva.Value);
			else cmd.Parameters.Add("@CONIVA", FbDbType.SmallInt).Value = DBNull.Value;

			if (valorIva.HasValue) cmd.Parameters.AddWithValue("@VALORIVA", valorIva.Value);
			else cmd.Parameters.Add("@VALORIVA", FbDbType.Double).Value = DBNull.Value;

			cmd.Parameters.AddWithValue("@PAGOTERCERO", pagoTercero);
			cmd.Parameters.AddWithValue("@NOMBRE_RECIBE", (object?)nombreRecibe ?? "");
			cmd.Parameters.AddWithValue("@CODIGO_DESCRIPCION", (object?)codigoConcepto ?? "");
			cmd.Parameters.AddWithValue("@CONSECUTIVO_NOTACREDITO", consecutivoNotaCredito);
			cmd.Parameters.AddWithValue("@RECIBO_RELACIONADO", (object?)reciboRelacionado ?? "");

			await cmd.ExecuteNonQueryAsync();
		}

		private async Task InsertarAbonoMotivoAsync(
				FbConnection con, FbTransaction tx,
				int idRelacion,
				string descripcion,
				string codigo,
				double valorUnitario,
				int cantidad,
				double porcentajeIvaEnValorIva)
		{
			const string sql = @"
					INSERT INTO T_ADICIONALES_ABONOS_MOTIVOS
					(
						IDRELACION,
						DESCRIPCION,
						CODIGO,
						VALOR,
						VALORIVA,
						CANTIDAD,
						PORCENTAJEIVA
					)
					VALUES
					(
						@IDRELACION,
						@DESCRIPCION,
						@CODIGO,
						@VALOR,
						@VALORIVA,
						@CANTIDAD,
						NULL
					)";

			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@IDRELACION", idRelacion);
			cmd.Parameters.AddWithValue("@DESCRIPCION", descripcion ?? "");
			cmd.Parameters.AddWithValue("@CODIGO", codigo ?? "");
			cmd.Parameters.AddWithValue("@VALOR", valorUnitario);
			cmd.Parameters.AddWithValue("@VALORIVA", porcentajeIvaEnValorIva); // ✅ porcentaje
			cmd.Parameters.AddWithValue("@CANTIDAD", cantidad);

			await cmd.ExecuteNonQueryAsync();
		}



		private async Task InsertarAbonoTipoPagoAsync(
			FbConnection con, FbTransaction tx,
			double valor, string descripcion, string tipoDePago,
			int idRelacion, string numero, string fechaTexto)
		{
			const string sql = @"
			INSERT INTO T_ABONOS_TIPO_PAGO
			(VALOR, DESCRIPCION, TIPODEPAGO, IDRELACION, ID, NUMERO, FECHA)
			VALUES
			(@VALOR, @DESCRIPCION, @TIPODEPAGO, @IDRELACION, GEN_ID(GEN_ABONOS_TIPO_PAGO,1), @NUMERO, @FECHA)
		";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@VALOR", valor);
			cmd.Parameters.AddWithValue("@DESCRIPCION", descripcion ?? "");
			cmd.Parameters.AddWithValue("@TIPODEPAGO", tipoDePago ?? "");
			cmd.Parameters.AddWithValue("@IDRELACION", idRelacion);
			cmd.Parameters.AddWithValue("@NUMERO", numero ?? "");
			cmd.Parameters.AddWithValue("@FECHA", fechaTexto ?? "");

			await cmd.ExecuteNonQueryAsync();
		}

		private async Task EjecutarIncrementarIdentificadorAsync(FbConnection con, FbTransaction tx, int idPaciente, int fase, int idDoctorTratante)
		{
			const string sql = @"EXECUTE PROCEDURE P_INCREMENTAR_IDENTIFICADOR(@ID, @FASE, @IDDOCTOR)";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);

			await cmd.ExecuteNonQueryAsync();
		}

		private async Task<string?> ObtenerFacturaSugeridaAsync(int doctorId)
		{
			using (var _db = new AppDbContext())
			{
				var lista = await _db.P_MAX_CON_LETRAS(doctorId);
				return lista
					.OrderByDescending(x => x.NUMERO)
					.Select(x => x.FACTURA)
					.FirstOrDefault();
			}
		}

		private static readonly Encoding _blobEncoding = Encoding.GetEncoding("ISO-8859-1");

		private static byte[]? TextoABlob(string? s)
		{
			if (string.IsNullOrWhiteSpace(s)) return null;
			return _blobEncoding.GetBytes(s);
		}

		private static string? BlobATexto(byte[]? b)
		{
			if (b == null || b.Length == 0) return null;
			return _blobEncoding.GetString(b);
		}



		private async Task<int> ObtenerTipoFacturacionDoctorAsync(int doctorId)
		{
			// ✅ Ya no es placeholder: devolvemos el tipo REAL
			using (var _db = new AppDbContext())
			{
				var con = (FbConnection)_db.Database.GetDbConnection();
				if (con.State != ConnectionState.Open)
					await con.OpenAsync();

				return await TipoFacturacionAsync(con, doctorId);
			}
		}

		private async Task<int?> ObtenerConfirmacionIntAsync(string nombre)
		{
			try
			{
				using (var _db = new AppDbContext())
				{
					await using var conn = _db.Database.GetDbConnection();
					if (conn.State != ConnectionState.Open)
						await conn.OpenAsync();

					await using var cmd = conn.CreateCommand();
					cmd.CommandText = @"
				SELECT FIRST 1 COALESCE(PERMISO, 1)
				FROM TCONFIGURACIONES_RYDENT
				WHERE UPPER(NOMBRE) = UPPER(@NOMBRE)
			";

					var p = cmd.CreateParameter();
					p.ParameterName = "@NOMBRE";
					p.Value = nombre;
					cmd.Parameters.Add(p);

					var result = await cmd.ExecuteScalarAsync();
					if (result == null || result == DBNull.Value) return 1;

					return Convert.ToInt32(result);
				}
			}
			catch
			{
				return null;
			}
		}


		private async Task<bool> ObtenerPersonalizacionBoolAsync(string NOMBRE)
		{
			try
			{
				using (var _db = new AppDbContext())
				{
					await using var conn = _db.Database.GetDbConnection();
					if (conn.State != ConnectionState.Open)
						await conn.OpenAsync();

					await using var cmd = conn.CreateCommand();
					cmd.CommandText = @"
						SELECT FIRST 1 PERMISO
						FROM T_PERSONALIZACIONES
						WHERE NOMBRE = @NOMBRE
					";

					var p = cmd.CreateParameter();
					p.ParameterName = "@NOMBRE";
					p.Value = NOMBRE;
					cmd.Parameters.Add(p);

					var result = await cmd.ExecuteScalarAsync();
					if (result == null || result == DBNull.Value) return false;

					var txt = result.ToString()?.Trim()?.ToUpperInvariant();
					return txt == "1" || txt == "S" || txt == "SI" || txt == "TRUE";
				}
			}
			catch { return false; }
		}

		private async Task<int?> ObtenerIdDoctorPorUsuarioAsync(FbConnection con, string usuario, FbTransaction? tx = null)
		{
			if (string.IsNullOrWhiteSpace(usuario)) return null;

			const string sql = @"
				SELECT FIRST 1 c.IDDOCTOR
				FROM TCLAVE c
				WHERE UPPER(TRIM(c.USUARIO)) = UPPER(TRIM(@USUARIO))
				  AND c.IDDOCTOR IS NOT NULL
			";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@USUARIO", usuario);

			var o = await cmd.ExecuteScalarAsync();
			if (o == null || o == DBNull.Value) return null;

			var v = Convert.ToInt32(o);
			return v > 0 ? v : (int?)null;
		}

		private async Task<List<int>> ObtenerIdRelacionesPorIdentificadorAsync(
			FbConnection con,
			int idPaciente,
			int fase,
			int idDoctorTratante,
			int identificador,
			FbTransaction? tx)
		{
			const string sql = @"
					SELECT DISTINCT a.IDRELACION
					FROM T_ADICIONALES_ABONOS a
					WHERE a.ID = @ID
					  AND a.FASE = @FASE
					  AND a.IDDOCTOR = @IDDOCTOR
					  AND a.IDENTIFICADOR = @IDENTIFICADOR
					  AND a.IDRELACION IS NOT NULL
				";

			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);
			cmd.Parameters.AddWithValue("@IDENTIFICADOR", identificador);

			var list = new List<int>();
			using var rd = await cmd.ExecuteReaderAsync();
			while (await rd.ReadAsync())
			{
				if (rd[0] == DBNull.Value) continue;
				list.Add(Convert.ToInt32(rd[0]));
			}

			return list;
		}

		private async Task<string> ConstruirResumenBorrarAbonoAsync(
			FbConnection con,
			int idPaciente,
			int fase,
			int idDoctorTratante,
			int identificador,
			FbTransaction? tx)
		{
			// Resumen simple: fecha, recibo, factura, total (suma VALOR)
			const string sql = @"
						SELECT
							MIN(a.FECHA) AS FECHA_MIN,
							MAX(a.RECIBO) AS RECIBO,
							MAX(a.FACTURA) AS FACTURA,
							COALESCE(SUM(a.VALOR), 0) AS TOTAL
						FROM T_ADICIONALES_ABONOS a
						WHERE a.ID = @ID
						  AND a.FASE = @FASE
						  AND a.IDDOCTOR = @IDDOCTOR
						  AND a.IDENTIFICADOR = @IDENTIFICADOR
					";

			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);
			cmd.Parameters.AddWithValue("@IDENTIFICADOR", identificador);

			using var rd = await cmd.ExecuteReaderAsync();
			if (!await rd.ReadAsync()) return "Abono encontrado, pero no se pudo armar el resumen.";

			var fecha = rd["FECHA_MIN"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(rd["FECHA_MIN"]);
			var recibo = rd["RECIBO"] == DBNull.Value ? "" : rd["RECIBO"]?.ToString();
			var factura = rd["FACTURA"] == DBNull.Value ? "" : rd["FACTURA"]?.ToString();
			var total = rd["TOTAL"] == DBNull.Value ? 0 : Convert.ToDouble(rd["TOTAL"]);

			return $"Identificador: {identificador} | Fecha: {(fecha.HasValue ? fecha.Value.ToString("yyyy-MM-dd") : "-")} | Recibo: {recibo} | Factura: {factura} | Total: {total:n0}";
		}

		private async Task<int> CopiarAbonoABorrarAsync(
			FbConnection con,
			FbTransaction tx,
			int idPaciente,
			int fase,
			int idDoctorTratante,
			int identificador,
			string motivo)
		{
			// Copia "tal cual" desde T_ADICIONALES_ABONOS hacia T_ADICIONALES_ABONOS_BORRAR
			// agregando FECHASUCESO (hoy) y MOTIVO.
			const string sql = @"
					INSERT INTO T_ADICIONALES_ABONOS_BORRAR
					(
						ID, FASE, TIPO, VALOR, FECHA, DESCRIPCION, RECIBO, IDENTIFICADOR, HORA,
						IDODONTOLOGO, IDDOCTOR, IDRELACION, IDRESOLIUCIONDIAN, DETALLE_TTO, FACTURA,
						RECIBIDO_POR, FIRMA, CONIVA, VALORIVA, PAGOTERCERO, NOMBRE_RECIBE,
						CODIGO_DESCRIPCION, CONSECUTIVO_NOTACREDITO, NC_ELABORADO_POR, NC_APROBADO_POR,
						RECIBO_RELACIONADO, TRANSACCIONID, COPAGO, CONCEPTOCOPAGO, COPAGOFACTURARELACIONADA,
						COBERTURA, NUMERO_POLIZA, TIPOOPERACION,
						FECHASUCESO, MOTIVO
					)
					SELECT
						a.ID, a.FASE, a.TIPO, a.VALOR, a.FECHA, a.DESCRIPCION, a.RECIBO, a.IDENTIFICADOR, a.HORA,
						a.IDODONTOLOGO, a.IDDOCTOR, a.IDRELACION, a.IDRESOLIUCIONDIAN, a.DETALLE_TTO, a.FACTURA,
						a.RECIBIDO_POR, a.FIRMA, a.CONIVA, a.VALORIVA, a.PAGOTERCERO, a.NOMBRE_RECIBE,
						a.CODIGO_DESCRIPCION, a.CONSECUTIVO_NOTACREDITO, a.NC_ELABORADO_POR, a.NC_APROBADO_POR,
						a.RECIBO_RELACIONADO, a.TRANSACCIONID, a.COPAGO, a.CONCEPTOCOPAGO, a.COPAGOFACTURARELACIONADA,
						a.COBERTURA, a.NUMERO_POLIZA, a.TIPOOPERACION,
						CAST('NOW' AS DATE), @MOTIVO
					FROM T_ADICIONALES_ABONOS a
					WHERE a.ID = @ID
					  AND a.FASE = @FASE
					  AND a.IDDOCTOR = @IDDOCTOR
					  AND a.IDENTIFICADOR = @IDENTIFICADOR
				";

			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);
			cmd.Parameters.AddWithValue("@IDENTIFICADOR", identificador);
			cmd.Parameters.AddWithValue("@MOTIVO", motivo);

			return await cmd.ExecuteNonQueryAsync();
		}

		private async Task<int> BorrarAdicionalesPorIdentificadorAsync(
			FbConnection con,
			FbTransaction tx,
			int idPaciente,
			int fase,
			int idDoctorTratante,
			int identificador)
		{
			const string sql = @"
					DELETE FROM T_ADICIONALES_ABONOS a
					WHERE a.ID = @ID
					  AND a.FASE = @FASE
					  AND a.IDDOCTOR = @IDDOCTOR
					  AND a.IDENTIFICADOR = @IDENTIFICADOR
				";

			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);
			cmd.Parameters.AddWithValue("@IDENTIFICADOR", identificador);

			return await cmd.ExecuteNonQueryAsync();
		}

		private async Task DeleteByIdRelacionAsync(
			FbConnection con,
			FbTransaction tx,
			string tableName,
			string columnName,
			int idRelacion)
		{
			var sql = $"DELETE FROM {tableName} WHERE {columnName} = @IDRELACION";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@IDRELACION", idRelacion);
			await cmd.ExecuteNonQueryAsync();
		}

		private async Task DeleteAbonosXdrPagarAsync(FbConnection con, FbTransaction tx, int idRelacion)
		{
			// Por seguridad borramos donde sea:
			// - IDABONO = idRelacion
			// - IDADICIONAL = idRelacion
			const string sql = @"
					DELETE FROM T_ABONOS_XDR_PAGAR x
					WHERE x.IDABONO = @IDRELACION
					   OR x.IDADICIONAL = @IDRELACION
				";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@IDRELACION", idRelacion);
			await cmd.ExecuteNonQueryAsync();
		}

		private async Task EjecutarActualizarEstadoCuentaAsync(
			FbConnection con,
			FbTransaction tx,
			int idPaciente,
			int fase,
			int idDoctorTratante)
		{
			const string sql = @"EXECUTE PROCEDURE ACTUALIZARESTACUENTA(@ID, @FASE, @IDDOCTOR)";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);
			await cmd.ExecuteNonQueryAsync();
		}



		private async Task<int> ObtenerIdPresupuestoMaestraAsync(
			FbConnection con,
			int idPaciente,
			int fase,
			int idDoctorTratante,
			FbTransaction? tx = null)
		{
			const string sql = @"
					SELECT COALESCE(d.IDPRESUPUESTOMAESTRA, 0)
					FROM T_DEFINICION_TRATAMIENTO d
					WHERE d.ID = @ID AND d.FASE = @FASE AND d.IDDOCTOR = @IDDOCTOR
				";

			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);

			var o = await cmd.ExecuteScalarAsync();
			if (o == null || o == DBNull.Value) return 0;

			return Convert.ToInt32(o);
		}

		private async Task<List<AnticipoDisponibleDto>> CargarAnticiposDisponiblesAsync(
			FbConnection con,
			int idPaciente,
			int fase,
			int idDoctorTratante,
			FbTransaction? tx = null)
		{
			const string sql = @"
					SELECT
						aa.VALOR - SUM(COALESCE(ap.VALOR, 0)) AS ACUMULADO,
						aa.IDRELACION,
						aa.IDENTIFICADOR
					FROM T_ADICIONALES_ABONOS aa
					LEFT JOIN T_DEFINICION_TRATAMIENTO dt
						ON (((aa.ID = dt.ID) AND aa.FASE = dt.FASE) AND aa.IDDOCTOR = dt.IDDOCTOR)
					LEFT JOIN T_ABONOS_XDR_PAGAR ap
						ON ap.IDABONO = aa.IDRELACION
					WHERE aa.TIPO = 1
					  AND aa.IDRELACION IS NOT NULL
					  AND aa.ID = @ID
					  AND aa.FASE = @FASE
					  AND aa.IDDOCTOR = @IDDOCTOR
					GROUP BY aa.VALOR, aa.IDRELACION, aa.IDENTIFICADOR
					HAVING aa.VALOR - SUM(COALESCE(ap.VALOR, 0)) > 0
					ORDER BY aa.IDENTIFICADOR
				";

			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);

			var list = new List<AnticipoDisponibleDto>();

			using var rd = await cmd.ExecuteReaderAsync();
			while (await rd.ReadAsync())
			{
				var acumulado = rd["ACUMULADO"] == DBNull.Value ? 0 : Convert.ToDouble(rd["ACUMULADO"]);
				var idRel = rd["IDRELACION"] == DBNull.Value ? 0 : Convert.ToInt32(rd["IDRELACION"]);
				var ident = rd["IDENTIFICADOR"] == DBNull.Value ? 0 : Convert.ToInt32(rd["IDENTIFICADOR"]);

				if (idRel <= 0) continue;
				if (acumulado <= 0) continue;

				list.Add(new AnticipoDisponibleDto
				{
					IdRelacionAbono = idRel,
					Acumulado = acumulado,
					Identificador = ident
				});
			}

			return list;
		}


		private async Task<List<(double Acumulado, int IdAbonoRelacion)>> ConsultarAnticiposDisponiblesAsync(
			FbConnection con,
			int idPaciente,
			int fase,
			int idDoctorTratante,
			FbTransaction? tx = null)
		{
			// Si en tu sistema el anticipo es TIPO=1, déjalo así.
			// Si también hay anticipos en otro tipo (ej: 4), cambia a: aa.TIPO IN (1,4)
			const string sql = @"
					SELECT
						(aa.VALOR - COALESCE(SUM(ap.VALOR), 0)) AS ACUMULADO,
						aa.IDRELACION
					FROM T_ADICIONALES_ABONOS aa
					LEFT JOIN T_ABONOS_XDR_PAGAR ap
						ON ap.IDABONO = aa.IDRELACION
					WHERE aa.TIPO = 1
					  AND aa.IDRELACION IS NOT NULL
					  AND aa.ID = @ID
					  AND aa.FASE = @FASE
					  AND aa.IDDOCTOR = @IDDOCTOR
					GROUP BY aa.VALOR, aa.IDRELACION, aa.IDENTIFICADOR
					HAVING (aa.VALOR - COALESCE(SUM(ap.VALOR), 0)) > 0
					ORDER BY aa.IDENTIFICADOR
				";

			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);

			var list = new List<(double, int)>();
			using var rd = await cmd.ExecuteReaderAsync();

			while (await rd.ReadAsync())
			{
				var acum = rd["ACUMULADO"] == DBNull.Value ? 0 : Convert.ToDouble(rd["ACUMULADO"]);
				var idRel = rd["IDRELACION"] == DBNull.Value ? 0 : Convert.ToInt32(rd["IDRELACION"]);

				if (idRel > 0 && acum > 0)
					list.Add((acum, idRel));
			}

			return list;
		}

		private async Task InsertarAbonoXdrPagarAsync(
			FbConnection con,
			FbTransaction tx,
			int idAdicional,
			double valor,
			int idAbono,
			int idRecibidoPor)
		{
			if (valor <= 0) return;

			const string sql = @"
					INSERT INTO T_ABONOS_XDR_PAGAR (ID, IDADICIONAL, VALOR, IDABONO, IDRECIBIDOPOR)
					SELECT
						GEN_ID(T_ABONOS_XDR_PAGAR_GEN, 1),
						@IDADICIONAL,
						@VALOR,
						@IDABONO,
						@IDRECIBIDOPOR
					FROM RDB$DATABASE
					WHERE NOT EXISTS (
						SELECT 1
						FROM T_ABONOS_XDR_PAGAR x
						WHERE x.IDADICIONAL = @IDADICIONAL
						  AND x.IDABONO = @IDABONO
					)
				";

			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@IDADICIONAL", idAdicional);
			cmd.Parameters.AddWithValue("@VALOR", valor);
			cmd.Parameters.AddWithValue("@IDABONO", idAbono);
			cmd.Parameters.AddWithValue("@IDRECIBIDOPOR", idRecibidoPor);

			await cmd.ExecuteNonQueryAsync();
		}


		private async Task<(bool Relaciono, double Restante)> RelacionarAnticiposConAdicionalAsync(
			FbConnection con,
			FbTransaction tx,
			double costo,
			int idPaciente,
			int fase,
			int idDoctorTratante,
			int idAdicionalRelacion)
		{
			double restante = costo;
			bool relaciono = false;

			var anticipos = await ConsultarAnticiposDisponiblesAsync(con, idPaciente, fase, idDoctorTratante, tx);

			foreach (var a in anticipos)
			{
				if (restante <= 0) break;

				var acumulado = a.Acumulado;
				var idAbono = a.IdAbonoRelacion;

				if (acumulado <= 0 || idAbono <= 0)
					continue;

				double valorAplicar;

				if (restante <= acumulado)
				{
					valorAplicar = restante;
					restante = 0;
				}
				else
				{
					valorAplicar = acumulado;
					restante = restante - acumulado;
				}

				// Delphi:
				// IDABONO := aa.IDRELACION
				// IDADICIONAL := IdRelacion (del adicional)
				// IDRECIBIDOPOR := idDoctor (doctor del tratamiento)
				await InsertarAbonoXdrPagarAsync(
					con, tx,
					idAdicional: idAdicionalRelacion,
					valor: valorAplicar,
					idAbono: idAbono,
					idRecibidoPor: idDoctorTratante // ✅ DELPHI EXACTO
				);

				relaciono = true;
			}

			return (relaciono, restante);
		}

		private async Task EjecutarPIncrementarIdentificadorAsync(
			FbConnection con, FbTransaction tx, int idPaciente, int fase, int idDoctorTratante)
		{
			const string sql = @"EXECUTE PROCEDURE P_INCREMENTAR_IDENTIFICADOR(@ID, @FASE, @IDDOCTOR)";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);
			await cmd.ExecuteNonQueryAsync();
		}

		private async Task EjecutarActualizarEstaCuentaAsync(
			FbConnection con, FbTransaction tx, int idPaciente, int fase, int idDoctorTratante)
		{
			const string sql = @"EXECUTE PROCEDURE ACTUALIZARESTACUENTA(@ID, @FASE, @IDDOCTOR)";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@ID", idPaciente);
			cmd.Parameters.AddWithValue("@FASE", fase);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);
			await cmd.ExecuteNonQueryAsync();
		}

		private async Task<int> ObtenerIdentificadorPorIdRelacionAsync(FbConnection con, FbTransaction tx, int idRelacion)
		{
			const string sql = @"SELECT FIRST 1 a.IDENTIFICADOR FROM T_ADICIONALES_ABONOS a WHERE a.IDRELACION = @IDREL";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@IDREL", idRelacion);
			var o = await cmd.ExecuteScalarAsync();
			return (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o);
		}

		private async Task<int> ConsectutivoAdicionalAsync(FbConnection con, FbTransaction tx, int idDoctorTratante)
		{
			const string sql = @"
					SELECT COALESCE(MAX(CAST(a.CONSECUTIVO_NOTACREDITO AS INTEGER)), 0) + 1
					FROM T_ADICIONALES_ABONOS a
					WHERE a.IDDOCTOR = @IDDOCTOR
					  AND a.CONSECUTIVO_NOTACREDITO IS NOT NULL
					  AND a.CONSECUTIVO_NOTACREDITO SIMILAR TO '[0-9]+'
				";
			using var cmd = new FbCommand(sql, con, tx);
			cmd.Parameters.AddWithValue("@IDDOCTOR", idDoctorTratante);
			var o = await cmd.ExecuteScalarAsync();
			return (o == null || o == DBNull.Value) ? 1 : Convert.ToInt32(o);
		}

		private async Task<List<MotivoItemDto>> CargarMotivosAdicionalesAsync(
			FbConnection con,
			FbTransaction? tx = null)
		{
			const string sql = @"
					SELECT FIRST 1000
						p.ID,
						p.NOMBRE,
						p.CODIGO,
						p.COSTO,
						p.IVA
					FROM TCODIGOS_PROCEDIMIENTOS p
					WHERE p.NOMBRE IS NOT NULL
					  AND TRIM(p.NOMBRE) <> ''
					ORDER BY p.NOMBRE
				";

			using var cmd = new FbCommand(sql, con, tx);
			using var rd = await cmd.ExecuteReaderAsync();

			var list = new List<MotivoItemDto>();
			while (await rd.ReadAsync())
			{
				list.Add(new MotivoItemDto
				{
					Id = rd["ID"] == DBNull.Value ? null : Convert.ToInt32(rd["ID"]),
					Nombre = rd["NOMBRE"]?.ToString() ?? "",
					Codigo = rd["CODIGO"] == DBNull.Value ? null : rd["CODIGO"]?.ToString(),
					Valor = rd["COSTO"] == DBNull.Value ? null : Convert.ToDouble(rd["COSTO"]),
					Iva = rd["IVA"] == DBNull.Value ? null : Convert.ToInt32(rd["IVA"])
				});
			}

			return list;
		}


		private async Task<(bool Relaciono, double Restante)> RelacionarAnticiposComoDelphiAsync(
			FbConnection con, FbTransaction tx,
			double costo, int idPaciente, int fase, int idDoctorTratante, int idAdicionalRelacion)
		{
			// 1) Respeta confirmación Delphi: RELACIONAR_ANTICIPOS = 0
			var conf = await ConsultarConfirmacionAsync(con, "RELACIONAR_ANTICIPOS", tx);
			if (conf != 0) return (false, costo);

			// 2) Delphi: solo si IDPRESUPUESTOMAESTRA > 0 (sale de la cuenta)
			var idPresupuestoMaestra = await ObtenerIdPresupuestoMaestraAsync(con, idPaciente, fase, idDoctorTratante, tx);
			if (idPresupuestoMaestra <= 0) return (false, costo);

			// 3) Tu implementación real (la que ya tienes)
			var relRes = await RelacionarAnticiposConAdicionalAsync(
				con, tx,
				costo: costo,
				idPaciente: idPaciente,
				fase: fase,
				idDoctorTratante: idDoctorTratante,
				idAdicionalRelacion: idAdicionalRelacion
			);

			return (relRes.Relaciono, relRes.Restante);
		}

	}
}
