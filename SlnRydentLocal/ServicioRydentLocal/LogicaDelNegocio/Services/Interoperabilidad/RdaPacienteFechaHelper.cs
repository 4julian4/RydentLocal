using System;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public static class RdaPacienteFechaHelper
	{
		public static DateTime? ConstruirFechaNacimiento(string? dia, string? mes, string? anio)
		{
			if (string.IsNullOrWhiteSpace(dia) ||
				string.IsNullOrWhiteSpace(mes) ||
				string.IsNullOrWhiteSpace(anio))
			{
				return null;
			}

			if (!int.TryParse(dia.Trim(), out var d))
				return null;

			if (!int.TryParse(anio.Trim(), out var y))
				return null;

			var m = MesTextoANumero(mes);

			if (m <= 0)
				return null;

			try
			{
				return new DateTime(y, m, d);
			}
			catch
			{
				return null;
			}
		}

		private static int MesTextoANumero(string? mes)
		{
			if (string.IsNullOrWhiteSpace(mes))
				return 0;

			var valor = mes.Trim().ToUpperInvariant();

			switch (valor)
			{
				case "1":
				case "01":
				case "ENE":
				case "ENERO":
					return 1;

				case "2":
				case "02":
				case "FEB":
				case "FEBRERO":
					return 2;

				case "3":
				case "03":
				case "MAR":
				case "MARZO":
					return 3;

				case "4":
				case "04":
				case "ABR":
				case "ABRIL":
					return 4;

				case "5":
				case "05":
				case "MAY":
				case "MAYO":
					return 5;

				case "6":
				case "06":
				case "JUN":
				case "JUNIO":
					return 6;

				case "7":
				case "07":
				case "JUL":
				case "JULIO":
					return 7;

				case "8":
				case "08":
				case "AGO":
				case "AGOSTO":
					return 8;

				case "9":
				case "09":
				case "SEP":
				case "SEPT":
				case "SEPTIEMBRE":
					return 9;

				case "10":
				case "OCT":
				case "OCTUBRE":
					return 10;

				case "11":
				case "NOV":
				case "NOVIEMBRE":
					return 11;

				case "12":
				case "DIC":
				case "DICIEMBRE":
					return 12;

				default:
					return 0;
			}
		}
	}
}