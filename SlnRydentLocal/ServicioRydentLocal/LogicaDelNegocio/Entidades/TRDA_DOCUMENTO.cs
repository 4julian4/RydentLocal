using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
	[Table("TRDA_DOCUMENTO")]
	public class TRDA_DOCUMENTO
	{
		[Key]
		public int ID { get; set; }

		public int IDANAMNESIS { get; set; }
		public int? IDEVOLUCION { get; set; }
		public int? IDINFORMACIONREPORTE { get; set; }
		public DateTime? FECHA_ATENCION { get; set; }
		public TimeSpan? HORA_ATENCION { get; set; }
		public string? FACTURA { get; set; }
		public string? TIPO_DOCUMENTO { get; set; }
		public string? ESTADO { get; set; }

		public byte[]? JSON_RDA { get; set; }
		public byte[]? JSON_SNAPSHOT { get; set; }

		public string? MENSAJE_ERROR { get; set; }
		public DateTime? FECHA_GENERACION { get; set; }
		public DateTime? FECHA_ENVIO { get; set; }
		public int? INTENTOS { get; set; }

		public int? CODIGO_HTTP { get; set; }
		public byte[]? REQUEST_API { get; set; }
		public byte[]? RESPUESTA_API { get; set; }

		[NotMapped]
		public string? JSON_RDAstr
		{
			get
			{
				if (JSON_RDA == null || JSON_RDA.Length == 0)
					return null;

				return Encoding.UTF8.GetString(JSON_RDA);
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					JSON_RDA = null;
				else
					JSON_RDA = Encoding.UTF8.GetBytes(value);
			}
		}

		[NotMapped]
		public string? JSON_SNAPSHOTstr
		{
			get
			{
				if (JSON_SNAPSHOT == null || JSON_SNAPSHOT.Length == 0)
					return null;

				return Encoding.UTF8.GetString(JSON_SNAPSHOT);
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					JSON_SNAPSHOT = null;
				else
					JSON_SNAPSHOT = Encoding.UTF8.GetBytes(value);
			}
		}

		[NotMapped]
		public string? REQUEST_APIstr
		{
			get
			{
				if (REQUEST_API == null || REQUEST_API.Length == 0)
					return null;

				return Encoding.UTF8.GetString(REQUEST_API);
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					REQUEST_API = null;
				else
					REQUEST_API = Encoding.UTF8.GetBytes(value);
			}
		}

		[NotMapped]
		public string? RESPUESTA_APIstr
		{
			get
			{
				if (RESPUESTA_API == null || RESPUESTA_API.Length == 0)
					return null;

				return Encoding.UTF8.GetString(RESPUESTA_API);
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					RESPUESTA_API = null;
				else
					RESPUESTA_API = Encoding.UTF8.GetBytes(value);
			}
		}
	}
}