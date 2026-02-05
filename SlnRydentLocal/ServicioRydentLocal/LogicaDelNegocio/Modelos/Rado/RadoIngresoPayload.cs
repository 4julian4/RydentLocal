using System.Text.Json.Serialization;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rado
{
	public sealed class RadoIngresoPayload
	{
		// Paciente
		[JsonPropertyName("Numero_documento")]
		public string? Numero_documento { get; set; }

		[JsonPropertyName("Tipo_documento")]
		public string? Tipo_documento { get; set; }

		[JsonPropertyName("Primer_nombre")]
		public string? Primer_nombre { get; set; }

		[JsonPropertyName("Segundo_nombre")]
		public string? Segundo_nombre { get; set; }

		[JsonPropertyName("Primer_apellido")]
		public string? Primer_apellido { get; set; }

		[JsonPropertyName("Segundo_apellido")]
		public string? Segundo_apellido { get; set; }

		[JsonPropertyName("Fecha_nacimiento")]
		public string? Fecha_nacimiento { get; set; }

		[JsonPropertyName("Genero")]
		public short? Genero { get; set; }

		[JsonPropertyName("Indicativo_pais")]
		public string? Indicativo_pais { get; set; }

		[JsonPropertyName("Codigo_pais")]
		public int? Codigo_pais { get; set; }

		[JsonPropertyName("Codigo_departamento")]
		public int? Codigo_departamento { get; set; }

		[JsonPropertyName("Codigo_ciudad")]
		public int? Codigo_ciudad { get; set; }

		[JsonPropertyName("Direccion")]
		public string? Direccion { get; set; }

		[JsonPropertyName("Telefono")]
		public string? Telefono { get; set; }

		[JsonPropertyName("Correo_electronico")]
		public string? Correo_electronico { get; set; }

		// Orden / Estudio
		[JsonPropertyName("Personal_codigo")]
		public string? Personal_codigo { get; set; }

		[JsonPropertyName("Personal")]
		public string? Personal { get; set; }

		[JsonPropertyName("Especialidad")]
		public string? Especialidad { get; set; }

		[JsonPropertyName("Entidad")]
		public int? Entidad { get; set; }

		[JsonPropertyName("Regimen")]
		public int? Regimen { get; set; }

		[JsonPropertyName("Centro_Atencion")]
		public int? Centro_Atencion { get; set; }

		[JsonPropertyName("Tipo_Estudio")]
		public int? Tipo_Estudio { get; set; }

		[JsonPropertyName("Codigo_Servicio")]
		public int? Codigo_Servicio { get; set; }

		[JsonPropertyName("Servicio_Ips")]
		public string? Servicio_Ips { get; set; }

		[JsonPropertyName("Cantidad")]
		public int? Cantidad { get; set; }

		[JsonPropertyName("Fecha_Solicitud")]
		public string? Fecha_Solicitud { get; set; }

		[JsonPropertyName("Id_orden")]
		public int? Id_orden { get; set; }

		[JsonPropertyName("Id_paciente")]
		public int? Id_paciente { get; set; }

		[JsonPropertyName("Id_ingreso")]
		public long? Id_ingreso { get; set; }

		[JsonPropertyName("Ingreso")]
		public decimal? Ingreso { get; set; }

		// No disponibles
		[JsonPropertyName("Hora_estudio")]
		public string? Hora_estudio { get; set; }

		[JsonPropertyName("Minuto_estudio")]
		public string? Minuto_estudio { get; set; }

		[JsonPropertyName("Duracion_estudio")]
		public string? Duracion_estudio { get; set; }

		[JsonPropertyName("Equipo_modalidad")]
		public int? Equipo_modalidad { get; set; }

		[JsonPropertyName("_token")]
		public string? Token { get; set; }

		[JsonPropertyName("tipo_transaccion")]
		public string? TipoTransaccion { get; set; }

		[JsonPropertyName("id_transaccion")]
		public long? IdTransaccion { get; set; }

	}
}
