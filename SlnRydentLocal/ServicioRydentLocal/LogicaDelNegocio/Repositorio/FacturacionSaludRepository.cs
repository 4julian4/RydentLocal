using Microsoft.EntityFrameworkCore;
// Entidades de salida del SP
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Repositorio
{
	/// <summary>
	/// Tipo de operación de salud que vamos a enviar a Dataico.
	/// Lo usamos como enum para no trabajar con strings sueltos en el código.
	/// </summary>
	public enum HealthOperationType
	{
		SinAporte,  // SS_SIN_APORTE
		Recaudo,    // SS_RECAUDO
		Cufe,       // SS_CUFE
		Reporte     // SS_REPORTE
	}

	public class FacturacionSaludRepository
	{
		private readonly AppDbContext _db;

		// Inyectamos el DbContext (no el connection string)
		public FacturacionSaludRepository(AppDbContext db)
		{
			_db = db;
		}

		// ============================================================
		// 1) FACHADA GENERAL PARA ARMAR EL DTO SEGÚN LA OPERACIÓN
		// ============================================================
		/// <summary>
		/// Método ÚNICO de entrada:
		/// Dado un idRelacion, un tipoFactura y un tipo de operación,
		/// construye el DTO correcto para Dataico.
		/// 
		/// - El worker solo llama a este método.
		/// - Internamente hacemos switch por operación y delegamos.
		/// </summary>
		public async Task<HealthInvoiceDto> BuildHealthInvoiceDtoAsync(
			int idRelacion,
			int tipoFactura,
			HealthOperationType operation,
			CancellationToken ct = default)
		{
			switch (operation)
			{
				case HealthOperationType.SinAporte:
					// Reutilizamos tu método actual para SS-SIN-APORTE
					return await BuildHealthInvoiceDtoSinAporteAsync(idRelacion, ct);

				case HealthOperationType.Recaudo:
					// Construye una factura SS-RECAUDO
					return await BuildHealthInvoiceDtoRecaudoAsync(idRelacion, tipoFactura, ct);

				case HealthOperationType.Cufe:
					// Lo implementaremos en el siguiente paso
					return await BuildHealthInvoiceDtoCufeAsync(idRelacion, tipoFactura, ct);

				case HealthOperationType.Reporte:
					// Lo implementaremos en el siguiente paso
					return await BuildHealthInvoiceDtoReporteAsync(idRelacion, tipoFactura, ct);

				default:
					throw new ArgumentOutOfRangeException(nameof(operation), operation, "Operación de salud no soportada.");
			}
		}

		/// <summary>
		/// Construye el DTO para Dataico usando tu SP P_ADICIONALES_ABONOS_DIAN.
		/// Caso específico SS-SIN-APORTE (sin copagos).
		/// </summary>
		public async Task<HealthInvoiceDto> BuildHealthInvoiceDtoSinAporteAsync(
			int idRelacion,
			CancellationToken ct = default)
		{
			// Ejecuta tu SP via AppDbContext (ya lo tienes listo)
			// Nota: tu método es síncrono; lo envolvemos para no bloquear el hilo.
			var rows = await Task.Run(() => _db.P_ADICIONALES_ABONOS_DIAN(idRelacion), ct);

			var r = rows.FirstOrDefault();
			if (r == null)
				throw new Exception($"No existe IDRELACION={idRelacion} en P_ADICIONALES_ABONOS_DIAN");

			var motivos = await Task.Run(() => _db.P_ADICIONALES_ABONOS_MOTIVOS_D(idRelacion), ct);
			if (motivos == null || motivos.Count == 0)
				throw new Exception($"No existen motivos asociados para IDRELACION={idRelacion} en P_ADICIONALES_ABONOS_MOTIVOS_D");

			// Cortamos FACTURA en prefijo + número si viene todo pegado
			var (factura, prefijo) = SplitFacturaAndPrefix(r.FACTURA, r.PREFIJO);

			// Lectura SEGURA con null-coalescing
			DateTime fechaFactura = r.FECHAFACTURA; // tu SP la retorna como DATE válido
			string resolucion = r.RESOLUCION ?? string.Empty;
			string nit = r.NIT ?? string.Empty;
			string departamento = NormalizeLocation(r.DEPARTAMENTO) ?? string.Empty;
			string ciudad = NormalizeLocation(r.CIUDAD) ?? string.Empty;
			string direccion = r.DIRECCION_PACIENTE ?? string.Empty;
			string pais = r.CODIGOPAIS ?? "CO";
			string telefono = r.TELF_P ?? "0000000";

			// Descripción general (no va directo a Dataico, pero te sirve como base)
			string descripcion = string.IsNullOrWhiteSpace(r.DESCRIPCION)
				? "Paquete sin aporte"
				: r.DESCRIPCION;

			decimal valor = r.VALOR;

			// Normalizamos régimen y nivel de impuesto
			string regimen = string.IsNullOrWhiteSpace(r.REGIMEN)
				? "ORDINARIO"
				: r.REGIMEN.Trim();

			string taxLevelCode = string.IsNullOrWhiteSpace(r.NIVEL_IMPUESTO)
				? "NO_RESPONSABLE_DE_IVA"
				: r.NIVEL_IMPUESTO.Trim();

			// Normalizamos tipo de persona (PERSONA_NATURAL / PERSONA_JURIDICA)
			// Aquí tiene sentido asumir que el paciente es persona natural por defecto
			string tipoPersona = string.IsNullOrWhiteSpace(r.TIPO_PERSONA)
				? "PERSONA_NATURAL"
				: r.TIPO_PERSONA.Trim().ToUpperInvariant();

			// Medio de pago: tomamos FORMAPAGO/TIPOFORMAPAGO del SP si vienen, si no, dejamos defaults
			string paymentMeans = string.IsNullOrWhiteSpace(r.FORMAPAGO)
				? "CASH"
				: r.FORMAPAGO.Trim();

			string paymentMeansType = string.IsNullOrWhiteSpace(r.TIPOFORMAPAGO)
				? "CONTADO"
				: r.TIPOFORMAPAGO.Trim();

			// Email: primero CORREO_PACIENTE, luego CORREO; si ninguno es válido => no-reply
			string email = IsValidEmail(r.CORREO_PACIENTE)
				? r.CORREO_PACIENTE
				: IsValidEmail(r.CORREO)
					? r.CORREO
					: "no-reply@example.com";

			// 1) ÍTEMS (uno por motivo)
			var items = motivos.Select(m =>
			{
				string desc = string.IsNullOrWhiteSpace(m.DESCRIPCION)
					? "Recaudo servicio de salud"
					: m.DESCRIPCION;

				// Aseguramos valores numéricos seguros
				decimal valorItem = Convert.ToDecimal(m.VALOR);
				decimal qty = Convert.ToDecimal(m.CANTIDAD);
				if (qty <= 0) qty = 1;

				// Si quieres incluir impuestos, descomenta la asignación de Taxes
				var taxes = BuildTaxesForMotivo(m, valorItem);

				return new ItemDto
				{
					Sku = string.IsNullOrWhiteSpace(m.CODIGO) ? "RECAUDO" : m.CODIGO,
					Quantity = qty,
					Description = desc,
					Price = valorItem,
					// Taxes    = taxes
				};
			}).ToList();

			// 2) Cliente (adquirente) alineado con Dataico:
			// - PERSONA_NATURAL  -> first_name + family_name
			// - PERSONA_JURIDICA -> company_name

			string fullName = (r.NOMBRE_PACIENTE ?? string.Empty).Trim();

			string? firstName = null;
			string? familyName = null;

			if (!string.IsNullOrEmpty(fullName))
			{
				var partes = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

				if (partes.Length == 1)
				{
					// Solo una palabra -> nombre
					firstName = partes[0];
				}
				else
				{
					firstName = partes[0];
					familyName = string.Join(' ', partes.Skip(1));
				}
			}

			var customer = new CustomerDto
			{
				PartyIdentificationType = r.TIPO_DOCUMENTO_PACIENTE, // CC, TI, etc.
				PartyIdentification = r.IDANAMNESIS_TEXTO,        // número de documento
				PartyType = tipoPersona,                // PERSONA_NATURAL / PERSONA_JURIDICA
				TaxLevelCode = taxLevelCode,
				Regimen = regimen,
				Email = email,
				Phone = telefono,
				Department = departamento,
				City = ciudad,
				AddressLine = direccion,
				CountryCode = pais
			};

			if (tipoPersona == "PERSONA_NATURAL")
			{
				customer.FirstName = firstName ?? fullName ?? "SIN_NOMBRE";
				customer.FamilyName = familyName ?? "SIN_APELLIDOS";
				// No llenamos CompanyName en este caso
			}
			else // PERSONA_JURIDICA
			{
				customer.CompanyName = string.IsNullOrEmpty(fullName)
					? "CLIENTE EPS/ENTIDAD"
					: fullName;
			}

			// 3) Construcción del DTO para Dataico
			//    OJO: AQUÍ YA NO ENVIAMOS HEALTH.Person NI RECAUDOS NI NADA EXTRA
			var dto = new HealthInvoiceDto
			{
				// En pruebas: no enviar a DIAN ni correo
				SendToDian = false,
				SendEmail = false,

				Number = factura,
				IssueDate = DdMMyyyy(fechaFactura),
				PaymentDate = DdMMyyyy(fechaFactura),
				InvoiceTypeCode = "FACTURA_VENTA",
				PaymentMeans = paymentMeans,
				PaymentMeansType = paymentMeansType,
				OrderReference = string.Empty,

				Numbering = new NumberingDto
				{
					ResolutionNumber = resolucion,
					Prefix = prefijo,
					Flexible = true
				},

				Customer = customer,
				Items = items,
				Operation = "SS_SIN_APORTE",

				Health = new HealthDto
				{
					Version = "API_SALUD_V2",
					Coverage = "PLAN_DE_BENEFICIOS",
					PaymentModality = "PAGO_POR_CAPITACION",
					ProviderCode = (r.CODIGO_PRESTADOR ?? string.Empty).Trim(), // si tu SP lo trae
					PeriodStartDate = DdMMyyyy(fechaFactura),
					PeriodEndDate = DdMMyyyy(fechaFactura)
					// Person, Recaudos, AssociatedUsers, etc. NO se envían en SS_SIN_APORTE
				},

				Currency = string.IsNullOrWhiteSpace(r.CODIGOMONEDA) ? "COP" : r.CODIGOMONEDA
			};

			return dto;
		}



		// ======================================================================
		// 3) NUEVO: IMPLEMENTACIÓN SS-RECAUDO
		// ======================================================================
		/// <summary>
		/// Construye una factura SS-RECAUDO.
		/// - Cabecera: P_ADICIONALES_ABONOS_DIAN(idRelacion)
		/// - Ítems:    P_ADICIONALES_ABONOS_MOTIVOS_D(idRelacion)
		/// - Recaudos: P_ADICIONALES_ABONOS_COPAGOS_D(factura)
		/// 
		/// tipoFactura:
		///   1   -> factura "normal"  (en motivos puedes tener varios)
		///   !=1 -> cuenta por cobrar (en motivos normalmente vendrá solo 1)
		/// 
		/// Pero el mismo SP de motivos sirve para los dos casos.
		/// </summary>
		private async Task<HealthInvoiceDto> BuildHealthInvoiceDtoRecaudoAsync(
			int idRelacion,
			int tipoFactura,
			CancellationToken ct)
		{
			// 1) Leemos la cabecera (mismo SP que usas para SS-SIN-APORTE)
			var cabRows = await Task.Run(() => _db.P_ADICIONALES_ABONOS_DIAN(idRelacion), ct);
			var r = cabRows.FirstOrDefault();
			if (r == null)
				throw new Exception($"No existe IDRELACION={idRelacion} en P_ADICIONALES_ABONOS_DIAN");

			// 2) Leemos los MOTIVOS asociados a esa relación (para los ítems)
			var motivos = await Task.Run(() => _db.P_ADICIONALES_ABONOS_MOTIVOS_D(idRelacion), ct);
			if (motivos == null || motivos.Count == 0)
				throw new Exception($"No existen motivos asociados para IDRELACION={idRelacion} en P_ADICIONALES_ABONOS_MOTIVOS_D");

			// 3) Campos base de la cabecera (fechas, numeración, etc.)
			// Cortamos FACTURA en prefijo + número si viene todo pegado
			var (factura, prefijo) = SplitFacturaAndPrefix(r.FACTURA, r.PREFIJO);
			DateTime fechaFactura = r.FECHAFACTURA;
			DateTime fechaIniPeriodo = r.FECHAFACTURAINI == default ? fechaFactura : r.FECHAFACTURAINI;

			string resolucion = r.RESOLUCION ?? string.Empty;
			string nit = r.NIT ?? string.Empty;
			string departamento = NormalizeLocation(r.DEPARTAMENTO) ?? string.Empty;
			string ciudad = NormalizeLocation(r.CIUDAD) ?? string.Empty;
			string direccion = r.DIRECCION ?? string.Empty;
			string pais = r.CODIGOPAIS ?? "CO";
			string telefono = r.TELF_P ?? "0000000";

			// Normalizamos régimen y nivel de impuesto según catálogos Dataico
			string regimen = string.IsNullOrWhiteSpace(r.REGIMEN)
				? "ORDINARIO"
				: r.REGIMEN.Trim();

			string taxLevelCode = string.IsNullOrWhiteSpace(r.NIVEL_IMPUESTO)
				? "NO_RESPONSABLE_DE_IVA"
				: r.NIVEL_IMPUESTO.Trim();

			// Normalizamos tipo de persona (PERSONA_NATURAL / PERSONA_JURIDICA)
			string tipoPersona = string.IsNullOrWhiteSpace(r.TIPO_PERSONA)
				? "PERSONA_NATURAL"
				: r.TIPO_PERSONA.Trim().ToUpperInvariant();

			// Medio de pago: tomamos FORMAPAGO/TIPOFORMAPAGO del SP si vienen, si no, dejamos defaults
			string paymentMeans = string.IsNullOrWhiteSpace(r.FORMAPAGO)
				? "CASH"
				: r.FORMAPAGO.Trim();

			string paymentMeansType = string.IsNullOrWhiteSpace(r.TIPOFORMAPAGO)
				? "CONTADO"
				: r.TIPOFORMAPAGO.Trim();

			// Cobertura y modalidad de pago (catálogos Minsalud/Dataico)
			string coverage = string.IsNullOrWhiteSpace(r.COBERTURA)
				? "PLAN_DE_BENEFICIOS"
				: r.COBERTURA.Trim();

			string paymentModality = string.IsNullOrWhiteSpace(r.MODALIDAD_PAGO)
				? "PAGO_POR_EVENTO"
				: r.MODALIDAD_PAGO.Trim();

			// Email: primero CORREO_PACIENTE, luego CORREO, si ninguno es válido -> no-reply
			string email = IsValidEmail(r.CORREO_PACIENTE)
				? r.CORREO_PACIENTE
				: IsValidEmail(r.CORREO)
					? r.CORREO
					: "no-reply@example.com";

			// 4) Armamos los ÍTEMS (uno por motivo)
			var items = motivos.Select(m =>
			{
				string desc = string.IsNullOrWhiteSpace(m.DESCRIPCION)
					? "Recaudo servicio de salud"
					: m.DESCRIPCION;

				// Aseguramos valores numéricos seguros
				decimal valorItem = Convert.ToDecimal(m.VALOR);
				decimal qty = Convert.ToDecimal(m.CANTIDAD);
				if (qty <= 0) qty = 1;

				var taxes = BuildTaxesForMotivo(m, valorItem);

				return new ItemDto
				{
					Sku = string.IsNullOrWhiteSpace(m.CODIGO) ? "RECAUDO" : m.CODIGO,
					Quantity = qty,
					Description = desc,
					Price = valorItem,
					Taxes = taxes
				};
			}).ToList();

			// 5) Armamos los RECAUDOS desde el SP de COPAGOS (por FACTURA base)
			var copagos = await Task.Run(
				() => _db.P_ADICIONALES_ABONOS_COPAGOS_D(factura),
				ct);

			var recaudos = copagos
				.Select(c => new RecaudoDto
				{
					Amount = Convert.ToDecimal(c.VALOR),
					IssueDate = DdMMyyyy(c.FECHA),
					MedicalFeeCode = MapMedicalFeeCode(c.CONCEPTOCOPAGO)
				})
				.ToList();

			// 6) Cliente (adquirente).
			// Alineado con documentación Dataico:
			// - PERSONA_NATURAL  -> first_name + family_name
			// - PERSONA_JURIDICA -> company_name

			string fullName = (r.NOMBRE_PACIENTE ?? string.Empty).Trim();

			string? firstName = null;
			string? familyName = null;

			if (!string.IsNullOrEmpty(fullName))
			{
				var partes = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

				if (partes.Length == 1)
				{
					// Solo una palabra -> la usamos como nombre
					firstName = partes[0];
				}
				else
				{
					// Primer token = nombre, resto = apellidos
					firstName = partes[0];
					familyName = string.Join(' ', partes.Skip(1));
				}
			}

			var customer = new CustomerDto
			{
				PartyIdentificationType = r.TIPO_DOCUMENTO_PACIENTE, // CC, NIT, etc.
				PartyIdentification = r.IDANAMNESIS_TEXTO,        // número de doc
				PartyType = tipoPersona,                // PERSONA_NATURAL / PERSONA_JURIDICA
				TaxLevelCode = taxLevelCode,               // NO_RESPONSABLE_DE_IVA, etc.
				Regimen = regimen,                    // ORDINARIO, SIMPLE, etc.
				Email = email,
				Phone = telefono,
				Department = departamento,
				City = ciudad,
				AddressLine = direccion,
				CountryCode = pais
			};

			if (tipoPersona == "PERSONA_NATURAL")
			{
				// Para persona natural Dataico pide first_name + family_name
				customer.FirstName = firstName ?? fullName ?? "SIN_NOMBRE";
				customer.FamilyName = familyName ?? "SIN_APELLIDOS";
				// OJO: aquí NO llenamos CompanyName
			}
			else // PERSONA_JURIDICA
			{
				// Para jurídica sí se usa company_name (razón social)
				customer.CompanyName = string.IsNullOrEmpty(fullName)
					? "CLIENTE EPS/ENTIDAD"
					: fullName;
			}

			// 7) Persona (paciente) en el nodo Health.Person (opcional)
			PersonDto? person = null;
			if (!string.IsNullOrWhiteSpace(fullName))
			{
				person = new PersonDto
				{
					// Podemos reutilizar el mismo fullName que armamos
					FirstName = fullName
				};
			}

			// 8) Construimos el DTO final para SS-RECAUDO
			var dto = new HealthInvoiceDto
			{
				// En esta fase podemos dejar SendToDian = false si quieres solo pruebas
				SendToDian = false,
				SendEmail = false,

				Number = factura,
				IssueDate = DdMMyyyy(fechaFactura),
				PaymentDate = DdMMyyyy(fechaFactura),
				InvoiceTypeCode = "FACTURA_VENTA",
				PaymentMeans = paymentMeans,
				PaymentMeansType = paymentMeansType,
				OrderReference = string.Empty,

				Numbering = new NumberingDto
				{
					ResolutionNumber = resolucion,
					Prefix = prefijo,
					Flexible = true
				},

				Customer = customer,
				Items = items,
				Operation = "SS_RECAUDO",

				Health = new HealthDto
				{
					Version = "API_SALUD_V2",
					Coverage = coverage,
					PaymentModality = paymentModality,
					ProviderCode = (r.CODIGO_PRESTADOR ?? string.Empty).Trim(),
					ContractNumber = r.NUMERO_CONTRATO,
					PolicyNumber = r.NUMERO_POLIZA,
					PeriodStartDate = DdMMyyyy(fechaIniPeriodo),
					PeriodEndDate = DdMMyyyy(fechaFactura),
					UserType = r.TIPOAFILIACION,
					Recaudos = recaudos,
					Person = person
				},

				Currency = string.IsNullOrWhiteSpace(r.CODIGOMONEDA) ? "COP" : r.CODIGOMONEDA
			};

			return dto;
		}


		// ======================================================================
		// 4) STUBS (POR AHORA) PARA CUFE / REPORTE
		// ======================================================================
		private Task<HealthInvoiceDto> BuildHealthInvoiceDtoCufeAsync(
			int idRelacion,
			int tipoFactura,
			CancellationToken ct)
		{
			// Aquí luego:
			// - Leer cabecera CxC
			// - Leer su motivo principal
			// - Leer y agrupar recaudos SS-RECAUDO relacionados
			// - Armar Operation = "SS_CUFE"
			throw new NotImplementedException("BuildHealthInvoiceDtoCufeAsync aún no está implementado.");
		}

		private Task<HealthInvoiceDto> BuildHealthInvoiceDtoReporteAsync(
			int idRelacion,
			int tipoFactura,
			CancellationToken ct)
		{
			// Similar a CUFE, pero los recaudos son informativos
			// y no restan el total. Operation = "SS_REPORTE".
			throw new NotImplementedException("BuildHealthInvoiceDtoReporteAsync aún no está implementado.");
		}

		// ======================================================================
		// 5) MARCAR TRANSACCIONID CUANDO LA RESPUESTA ES POSITIVA
		// ======================================================================
		/// <summary>
		/// Graba el UUID (externalId de Dataico) en las tablas
		/// T_ADICIONALES_ABONOS y/o T_CUENTASXCOBRAR, según el tipo de factura.
		/// 
		/// IMPORTANTE:
		/// - Aquí asumimos que ambas tablas tienen una columna IDRELACION.
		///   Si en tu base real se llama distinto, ajusta el WHERE.
		/// </summary>
		public async Task MarcarTransaccionIdAsync(
			int idRelacion,
			int tipoFactura,
			string externalId,
			CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(externalId))
				return;

			// Si tipoFactura == 1 consideramos que es factura "normal".
			if (tipoFactura == 1)
			{
				// Marca TRANSACCIONID en T_ADICIONALES_ABONOS
				await _db.Database.ExecuteSqlRawAsync(
					"UPDATE T_ADICIONALES_ABONOS SET TRANSACCIONID = {0} WHERE IDRELACION = {1}",
					new object[] { externalId, idRelacion },
					ct);
			}
			else
			{
				// Cuenta por cobrar (CxC): marcamos en T_CUENTASXCOBRAR
				await _db.Database.ExecuteSqlRawAsync(
					"UPDATE T_CUENTASXCOBRAR SET TRANSACCIONID = {0} WHERE IDRELACION = {1}",
					new object[] { externalId, idRelacion },
					ct);
			}

			// Si quieres dejar SIEMPRE las dos tablas marcadas (cuando exista registro),
			// podrías hacer un segundo UPDATE a la otra tabla también.
		}

		// ======================================================================
		// 6) HELPERS PRIVADOS
		// ======================================================================

		/// <summary>
		/// Convierte un DateTime a cadena "dd/MM/yyyy" como espera Dataico.
		/// </summary>
		private static string DdMMyyyy(DateTime dt)
			=> dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

		/// <summary>
		/// Construye la lista de impuestos (IVA) para un motivo.
		/// Si no tiene IVA o porcentaje 0, devuelve null.
		/// </summary>
		private static System.Collections.Generic.List<TaxDto>? BuildTaxesForMotivo(
			P_ADICIONALES_ABONOS_MOTIVOS_D_Result m,
			decimal baseAmount)
		{
			// Convertimos a decimal por si los campos vienen como double, float, etc.
			decimal rate = Convert.ToDecimal(m.PORCENTAJEIVA);
			if (rate <= 0)
				return null;

			decimal taxAmount = Convert.ToDecimal(m.VALORIVA);

			var list = new System.Collections.Generic.List<TaxDto>
			{
				new TaxDto
				{
					TaxCategory = "IVA",
					TaxRate = rate,
					TaxAmount = taxAmount,
					BaseAmount = baseAmount
				}
			};

			return list;
		}

		/// <summary>
		/// Mapea el código/descripcion de copago al dominio esperado por Dataico:
		/// COPAGO, CUOTA_MODERADORA, PAGOS_COMPARTIDOS, ANTICIPOS, OTROS.
		/// </summary>
		private static string MapMedicalFeeCode(string? codigoMotivo)
		{
			if (string.IsNullOrWhiteSpace(codigoMotivo))
				return "OTROS";

			var c = codigoMotivo.Trim().ToUpperInvariant();

			// Si ya viene con el código "oficial", lo usamos tal cual.
			if (c == "COPAGO" || c == "CUOTA_MODERADORA" || c == "PAGOS_COMPARTIDOS" || c == "ANTICIPOS" || c == "OTROS")
				return c;

			// Mapeos simplificados por texto:
			if (c.Contains("COP"))
				return "COPAGO";
			if (c.Contains("MODER"))
				return "CUOTA_MODERADORA";
			if (c.Contains("COMPART"))
				return "PAGOS_COMPARTIDOS";
			if (c.Contains("ANTICIP"))
				return "ANTICIPOS";

			return "OTROS";
		}

		/// <summary>
		/// "Tijerita" para separar prefijo y número de la factura.
		/// Reglas:
		/// - Si el SP ya manda PREFIJO y FACTURA separados, se respetan.
		/// - Si PREFIJO viene vacío pero FACTURA viene como "FE1061" o "1FEV1692",
		///   separa letras (y otros no-dígitos) del inicio como prefijo
		///   y el resto como número.
		/// - Si no se puede separar, deja todo en número y prefijo vacío.
		/// </summary>
		private static (string invoiceNumber, string prefix) SplitFacturaAndPrefix(
			string? facturaRaw,
			string? prefixRaw)
		{
			var factura = (facturaRaw ?? string.Empty).Trim();
			var prefijo = (prefixRaw ?? string.Empty).Trim();

			// Caso 1: si ya tenemos prefijo y FACTURA empieza con ese prefijo, lo usamos tal cual.
			if (!string.IsNullOrEmpty(prefijo) &&
				factura.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
			{
				var numberPart = factura.Substring(prefijo.Length).TrimStart('-', ' ').Trim();
				return (numberPart, prefijo.ToUpperInvariant());
			}

			// Caso 2: no hay prefijo definido, intentamos separar letras vs dígitos.
			if (string.IsNullOrEmpty(prefijo) && !string.IsNullOrEmpty(factura))
			{
				int idx = -1;

				for (int i = 0; i < factura.Length; i++)
				{
					if (char.IsDigit(factura[i]))
					{
						idx = i;
						break;
					}
				}

				if (idx > 0 && idx < factura.Length)
				{
					var posiblePrefijo = factura.Substring(0, idx).TrimEnd('-', ' ', '.');
					var posibleNumero = factura.Substring(idx).Trim();

					if (!string.IsNullOrEmpty(posiblePrefijo) &&
						!string.IsNullOrEmpty(posibleNumero))
					{
						return (posibleNumero, posiblePrefijo.ToUpperInvariant());
					}
				}
			}

			// Caso 3: no se pudo separar bien -> dejamos todo como número
			// y devolvemos el prefijo tal y como venía (mayusculado).
			return (factura, prefijo.ToUpperInvariant());
		}

		private static bool IsValidEmail(string? email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return false;

			try
			{
				var addr = new MailAddress(email.Trim());
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static string NormalizeLocation(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			// Quitamos espacios alrededor y ponemos mayúsculas
			var trimmed = value.Trim().ToUpperInvariant();

			// Normalizamos acentos: descomponemos caracteres (NFD)
			var normalized = trimmed.Normalize(NormalizationForm.FormD);

			var sb = new StringBuilder();

			foreach (var c in normalized)
			{
				var category = CharUnicodeInfo.GetUnicodeCategory(c);

				// Nos quedamos solo con los caracteres que NO sean "marcas" (tildes)
				if (category != UnicodeCategory.NonSpacingMark)
				{
					sb.Append(c);
				}
			}

			// Volvemos a componer
			return sb.ToString().Normalize(NormalizationForm.FormC);
		}

	}
}
