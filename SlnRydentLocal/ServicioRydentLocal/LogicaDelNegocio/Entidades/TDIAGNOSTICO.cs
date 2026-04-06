using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades
{
	[Table("TDIAGNOSTICO")]
	public class TDIAGNOSTICO
	{
		[Key]
		public int ID { get; set; }

		public int IDDIAGNOS { get; set; }
		public DateTime? FECHA { get; set; }

		public string? MEDICO_GENERAL { get; set; }

		public byte[]? PERIODONTAL { get; set; }
		public byte[]? PULPAR { get; set; }
		public byte[]? DENTAL { get; set; }
		public byte[]? SAGITAL { get; set; }
		public byte[]? TRASVERSAL { get; set; }
		public byte[]? ARTICULAR { get; set; }
		public byte[]? VERTICAL { get; set; }
		public byte[]? APINAMIENTO { get; set; }
		public byte[]? INSICIVOS { get; set; }
		public byte[]? ESQUELETICO { get; set; }
		public byte[]? FUNCIONAL { get; set; }
		public byte[]? FACIAL { get; set; }
		public byte[]? SISTEMICO { get; set; }
		public byte[]? OTROS { get; set; }
		public byte[]? DENTALES { get; set; }
		public byte[]? ESQUELETICOS { get; set; }
		public byte[]? FUNCIONALES { get; set; }
		public byte[]? FACIALES { get; set; }
		public byte[]? DESCRIPCION { get; set; }
		public byte[]? INTERCONSULTAS { get; set; }

		public string? CON_EXTRACCIONES { get; set; }
		public string? DIENTES_CON_EXTRACCIONES { get; set; }

		public byte[]? TIPO_ANCLAJE { get; set; }
		public byte[]? CONTENCION { get; set; }

		public string? SUPERNUMERARIOS { get; set; }
		public string? ABRASION { get; set; }
		public string? MANCHAS { get; set; }
		public string? CARIES { get; set; }
		public string? SENOSMAXILARES { get; set; }
		public string? HABITOS { get; set; }
		public string? MORDIDAABIERTA { get; set; }
		public string? MORDIDAPROFUNDA { get; set; }
		public string? MORDIDACRUZADA { get; set; }
		public string? RETROGNATISMO { get; set; }
		public string? PROGNATISMO { get; set; }
		public string? CLASEMOLAR { get; set; }
		public string? CONTACTOSPREMATUROS { get; set; }
		public string? PATOLOGIAPULPAR { get; set; }
		public string? PRUEBASVITALIDAD { get; set; }
		public string? DOLOR { get; set; }
		public string? ENDODONCIA { get; set; }
		public string? ATM { get; set; }
		public string? TIPOLESION { get; set; }
		public string? LADOAFECTADO { get; set; }
		public string? MOVIMIENTO { get; set; }
		public string? SEALIMENTABIEN { get; set; }
		public string? DIETARICAEND { get; set; }

		public byte[]? HABITOSALIMENTI { get; set; }
		public byte[]? DIETAESPECIAL { get; set; }
		public byte[]? OBS_HST_DIETETICA { get; set; }
		public byte[]? OBSERVACIONES { get; set; }
		public byte[]? ENDODONTICO { get; set; }
		public byte[]? OCLUSAL { get; set; }
		public byte[]? ESTOMATOLOGICO { get; set; }

		public string? NUMERO_AUTORIZACION { get; set; }
		public string? CODIGO_CONSULTA { get; set; }
		public string? NOMBRE_COD_CONSULTA { get; set; }
		public string? CODIGO_FINALIDAD { get; set; }
		public string? CODIGO_CAUSA { get; set; }
		public string? DIAGNOSTICO1 { get; set; }
		public string? DIAGNOSTICO2 { get; set; }
		public string? DIAGNOSTICO3 { get; set; }
		public string? DIAGNOSTICO4 { get; set; }
		public string? CODIGO_TIPO_DIAGNOSTICO { get; set; }
		public string? VALOR_CONSULTA { get; set; }
		public string? VALOR_CUOTA { get; set; }
		public string? VALOR_NETO { get; set; }
		public string? NUMERO_FACTURA { get; set; }

		public DateTime? FECHA_REALIZACION { get; set; }
		public string? EMBARAZO { get; set; }
		public string? DOCTOR { get; set; }
		public string? CONTROLADO { get; set; }
		public string? PRONOSTICO { get; set; }
		public string? EPS { get; set; }

		[NotMapped]
		public string? DESCRIPCIONstr
		{
			get => BlobToString(DESCRIPCION);
			set => DESCRIPCION = StringToBlob(value);
		}

		[NotMapped]
		public string? INTERCONSULTASstr
		{
			get => BlobToString(INTERCONSULTAS);
			set => INTERCONSULTAS = StringToBlob(value);
		}

		[NotMapped]
		public string? OBSERVACIONESstr
		{
			get => BlobToString(OBSERVACIONES);
			set => OBSERVACIONES = StringToBlob(value);
		}

		private static string? BlobToString(byte[]? value)
		{
			if (value == null || value.Length == 0)
				return null;

			return Encoding.ASCII.GetString(value);
		}

		private static byte[]? StringToBlob(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;

			return Encoding.ASCII.GetBytes(value);
		}
	}
}