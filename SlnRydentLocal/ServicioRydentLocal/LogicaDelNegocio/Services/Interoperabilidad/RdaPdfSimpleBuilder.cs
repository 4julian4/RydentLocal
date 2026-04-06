using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public static class RdaPdfSimpleBuilder
	{
		private const int PdfPageWidth = 595;
		private const int PdfPageHeight = 842;
		private const int PdfLeft = 50;
		private const int PdfTop = 790;
		private const int PdfBottomMargin = 55;
		private const int PdfLineHeight = 15;

		private const int WrapWidth = 92;
		private const int LabelWidth = 24;

		public static string BuildConsultaPdfBase64(RdaConsultaPdfData data)
		{
			data ??= new RdaConsultaPdfData();

			var lines = new List<string>();

			// =========================
			// ENCABEZADO
			// =========================
			AddMainTitle(lines, "DOCUMENTO DE SOPORTE RDA - CONSULTA");
			AddText(lines, $"Fecha de generación: {DateTime.Now:yyyy-MM-dd HH:mm}");
			AddDivider(lines, '=');
			AddBlank(lines);

			// =========================
			// IDENTIFICACION DEL ENCUENTRO
			// =========================
			AddSection(lines, "IDENTIFICACION DEL ENCUENTRO");
			AddField(lines, "Paciente", data.Paciente);
			AddField(lines, "Tipo documento", data.TipoDocumento);
			AddField(lines, "Documento", data.Documento);
			AddField(lines, "Doctor", data.Doctor);
			AddField(lines, "Prestador", data.Prestador);
			AddField(lines, "Fecha atención", data.FechaAtencion);
			AddField(lines, "Factura", data.Factura);
			AddField(lines, "Nro autorización", data.NumeroAutorizacion);
			AddField(lines, "Código consulta", data.CodigoConsulta);
			AddField(lines, "Consulta", data.NombreConsulta);
			AddField(lines, "Causa atención", data.CausaAtencion);
			AddField(lines, "Tipo diagnóstico", data.TipoDiagnostico);
			AddBlank(lines);

			// =========================
			// ASEGURAMIENTO
			// =========================
			AddSection(lines, "ASEGURAMIENTO");
			AddField(lines, "Entidad responsable", data.EntidadResponsable);
			AddField(lines, "Código entidad", data.CodigoEntidad);
			AddField(lines, "Tipo afiliación", data.TipoAfiliacion);
			AddField(lines, "Nro afiliación", data.NumeroAfiliacion);
			AddBlank(lines);

			// =========================
			// UBICACION Y CONTACTO
			// =========================
			AddSection(lines, "UBICACION Y CONTACTO");
			AddField(lines, "Ciudad", data.Ciudad);
			AddField(lines, "Departamento", data.Departamento);
			AddField(lines, "Dirección", data.Direccion);
			AddField(lines, "Teléfono", data.Telefono);
			AddField(lines, "Celular", data.Celular);
			AddBlank(lines);

			// =========================
			// DATOS CLINICOS
			// =========================
			AddSection(lines, "DATOS CLINICOS");
			AddField(lines, "Dx principal", data.DiagnosticoPrincipal);
			AddField(lines, "Dx secundario", data.Diagnostico2);
			AddField(lines, "Dx tercero", data.Diagnostico3);
			AddField(lines, "Dx cuarto", data.Diagnostico4);
			AddField(lines, "Motivo consulta", data.MotivoConsulta);
			AddField(lines, "Enfermedad actual", data.EnfermedadActual);
			AddField(lines, "Alergias", data.Alergias);
			AddField(lines, "Medicamentos", data.Medicamentos);
			AddField(lines, "Factores riesgo", data.FactoresRiesgo);
			AddField(lines, "Enf. previas", data.EnfermedadesPrevias);
			AddField(lines, "Cirugías", data.Cirugias);
			AddField(lines, "Revisión sistemas", data.RevisionSistemas);
			AddField(lines, "Observaciones", data.Observaciones);
			AddBlank(lines);

			// =========================
			// PROCEDIMIENTO
			// =========================
			AddSection(lines, "PROCEDIMIENTO");
			AddField(lines, "Código", data.ProcedimientoCodigo);
			AddField(lines, "Nombre", data.ProcedimientoNombre);
			AddField(lines, "Dx principal proc.", data.ProcedimientoDxPrincipal);
			AddField(lines, "Dx relacionado", data.ProcedimientoDxRelacionado);
			AddField(lines, "Ámbito", data.Ambito);
			AddField(lines, "Finalidad", data.FinalidadProcedimiento);
			AddField(lines, "Personal atiende", data.PersonalAtiende);
			AddField(lines, "Complicación", data.Complicacion);
			AddField(lines, "Forma acto quir.", data.FormaActoQuir);
			AddField(lines, "Valor", data.ValorProcedimiento);
			AddField(lines, "Entidad proc.", data.EntidadProcedimiento);
			AddField(lines, "Extranjero", data.Extranjero);
			AddField(lines, "País", data.Pais);
			AddBlank(lines);

			AddDivider(lines, '=');
			AddText(lines, "Fin del documento soporte.");
			AddBlank(lines);

			var pdfBytes = BuildSimplePdf(lines);
			return Convert.ToBase64String(pdfBytes);
		}

		private static byte[] BuildSimplePdf(List<string> lines)
		{
			var pages = Paginate(lines, PdfTop, PdfBottomMargin, PdfLineHeight);

			var pdf = new StringBuilder();
			var offsets = new List<long> { 0 };

			pdf.AppendLine("%PDF-1.4");

			int totalObjects = 2 + (pages.Count * 2) + 1; // catalog + pages + (page/content) + font
			int fontObj = 3 + (pages.Count * 2);

			// 1 Catalog
			offsets.Add(pdf.Length);
			pdf.AppendLine("1 0 obj");
			pdf.AppendLine("<< /Type /Catalog /Pages 2 0 R >>");
			pdf.AppendLine("endobj");

			// 2 Pages
			offsets.Add(pdf.Length);
			pdf.AppendLine("2 0 obj");
			pdf.Append("<< /Type /Pages /Kids [");
			for (int i = 0; i < pages.Count; i++)
			{
				int pageObj = 3 + (i * 2);
				pdf.Append($"{pageObj} 0 R ");
			}
			pdf.AppendLine($"] /Count {pages.Count} >>");
			pdf.AppendLine("endobj");

			// Pages + contents
			for (int i = 0; i < pages.Count; i++)
			{
				int pageObj = 3 + (i * 2);
				int contentObj = 4 + (i * 2);

				offsets.Add(pdf.Length);
				pdf.AppendLine($"{pageObj} 0 obj");
				pdf.AppendLine($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PdfPageWidth} {PdfPageHeight}] /Resources << /Font << /F1 {fontObj} 0 R >> >> /Contents {contentObj} 0 R >>");
				pdf.AppendLine("endobj");

				var stream = BuildPageContent(pages[i], i + 1, pages.Count);
				var streamBytes = Encoding.Latin1.GetBytes(stream);

				offsets.Add(pdf.Length);
				pdf.AppendLine($"{contentObj} 0 obj");
				pdf.AppendLine($"<< /Length {streamBytes.Length} >>");
				pdf.AppendLine("stream");
				pdf.Append(stream);
				pdf.AppendLine("endstream");
				pdf.AppendLine("endobj");
			}

			// Font
			offsets.Add(pdf.Length);
			pdf.AppendLine($"{fontObj} 0 obj");
			pdf.AppendLine("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
			pdf.AppendLine("endobj");

			long xref = pdf.Length;
			pdf.AppendLine("xref");
			pdf.AppendLine($"0 {totalObjects + 1}");
			pdf.AppendLine("0000000000 65535 f ");

			for (int i = 1; i < offsets.Count; i++)
			{
				pdf.AppendLine($"{offsets[i]:D10} 00000 n ");
			}

			pdf.AppendLine("trailer");
			pdf.AppendLine($"<< /Root 1 0 R /Size {totalObjects + 1} >>");
			pdf.AppendLine("startxref");
			pdf.AppendLine(xref.ToString());
			pdf.AppendLine("%%EOF");

			return Encoding.Latin1.GetBytes(pdf.ToString());
		}

		private static string BuildPageContent(List<string> lines, int pageNumber, int totalPages)
		{
			var sb = new StringBuilder();

			sb.AppendLine("BT");
			sb.AppendLine("/F1 10 Tf");
			sb.AppendLine($"{PdfLeft} {PdfTop} Td");

			bool first = true;
			foreach (var raw in lines)
			{
				var line = EscapePdfText(NormalizeLatin1(raw ?? string.Empty));

				if (!first)
					sb.AppendLine($"0 -{PdfLineHeight} Td");

				sb.AppendLine($"({line}) Tj");
				first = false;
			}

			sb.AppendLine("ET");

			// Pie simple de página
			sb.AppendLine("BT");
			sb.AppendLine("/F1 9 Tf");
			sb.AppendLine($"{PdfLeft} 25 Td");
			sb.AppendLine($"(Página {pageNumber} de {totalPages}) Tj");
			sb.AppendLine("ET");

			return sb.ToString();
		}

		private static List<List<string>> Paginate(List<string> lines, int top, int bottomMargin, int lineHeight)
		{
			var result = new List<List<string>>();
			var currentPage = new List<string>();
			int maxLinesPerPage = (top - bottomMargin) / lineHeight;

			foreach (var rawLine in lines)
			{
				var wrappedLines = WrapLine(rawLine, WrapWidth).ToList();

				foreach (var wrapped in wrappedLines)
				{
					if (currentPage.Count >= maxLinesPerPage)
					{
						result.Add(currentPage);
						currentPage = new List<string>();
					}

					currentPage.Add(wrapped);
				}
			}

			if (currentPage.Count > 0)
				result.Add(currentPage);

			if (result.Count == 0)
				result.Add(new List<string> { "-" });

			return result;
		}

		private static IEnumerable<string> WrapLine(string text, int maxLen)
		{
			var clean = NormalizeSpaces(text ?? string.Empty);

			if (clean.Length <= maxLen)
			{
				yield return clean;
				yield break;
			}

			var words = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			var current = new StringBuilder();

			foreach (var word in words)
			{
				if (current.Length == 0)
				{
					current.Append(word);
					continue;
				}

				if (current.Length + 1 + word.Length <= maxLen)
				{
					current.Append(' ').Append(word);
				}
				else
				{
					yield return current.ToString();
					current.Clear();
					current.Append("  ").Append(word); // sangría visual en continuación
				}
			}

			if (current.Length > 0)
				yield return current.ToString();
		}

		private static void AddMainTitle(List<string> lines, string title)
		{
			lines.Add(title);
			lines.Add(new string('=', Math.Min(Math.Max(title.Length, 30), 70)));
		}

		private static void AddSection(List<string> lines, string title)
		{
			lines.Add(title);
			lines.Add(new string('-', Math.Min(Math.Max(title.Length, 20), 60)));
		}

		private static void AddDivider(List<string> lines, char c)
		{
			lines.Add(new string(c, 70));
		}

		private static void AddField(List<string> lines, string label, string? value)
		{
			lines.Add($"{PadRight(label, LabelWidth)}: {Safe(value)}");
		}

		private static void AddText(List<string> lines, string text)
		{
			lines.Add(Safe(text));
		}

		private static void AddBlank(List<string> lines)
		{
			lines.Add(" ");
		}

		private static string PadRight(string value, int width)
		{
			value ??= string.Empty;
			return value.Length >= width ? value : value.PadRight(width);
		}

		private static string NormalizeSpaces(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			return string.Join(" ",
				value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
					 .Select(x => x.Trim()));
		}

		private static string EscapePdfText(string value)
		{
			return value
				.Replace("\\", "\\\\")
				.Replace("(", "\\(")
				.Replace(")", "\\)");
		}

		private static string NormalizeLatin1(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return value ?? string.Empty;

			return value
				.Replace("–", "-")
				.Replace("—", "-")
				.Replace("“", "\"")
				.Replace("”", "\"")
				.Replace("‘", "'")
				.Replace("’", "'")
				.Replace("…", "...");
		}

		private static string Safe(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return "-";

			var txt = NormalizeSpaces(value.Trim());

			if (txt.Equals("NO", StringComparison.OrdinalIgnoreCase) ||
				txt.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
				txt.Equals("-", StringComparison.OrdinalIgnoreCase) ||
				txt.Equals(".", StringComparison.OrdinalIgnoreCase) ||
				txt.Equals("NULL", StringComparison.OrdinalIgnoreCase))
			{
				return "-";
			}

			return txt;
		}
	}
}