using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Rado;
using SixLabors.ImageSharp;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	public interface IRadoQueryService
	{
		Task<AnamnesisIngresoRow?> ConsultarIngresoPorIdRelacion(long idRelacion, CancellationToken ct = default);
	}

	public sealed class RadoQueryService : IRadoQueryService
	{
		public async Task<AnamnesisIngresoRow?> ConsultarIngresoPorIdRelacion(
	        long idRelacion,
	        CancellationToken ct = default)
		{
			//if (idRelacion <= 0) return null;
            //
			//try
			//{
			//	using var db = new AppDbContext();
            //
			//	var p = new FbParameter("idRelacion", FbDbType.BigInt) { Value = idRelacion };
            //
			//	return await db.AnamnesisIngresoRows
			//		.FromSqlRaw(SqlIngresoByRelacion, p)
			//		.AsNoTracking()
			//		.FirstOrDefaultAsync(ct);
			//}
			//catch (Exception ex)
			//{
			//	// aquí capturas TODO lo que necesitas ver
			//	var detalle = $"ERROR: {ex}\n\nSQL:\n{SqlIngresoByRelacion}\n\nPARAM idRelacion={idRelacion}";
			//	throw new Exception(detalle);
			//}
		

			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();
			var strConn = configuration.GetValue<string>("ConnectionStrings:FirebirdConnection") ?? "";
			await using var con = new FbConnection(strConn);
			await con.OpenAsync(ct);

			await using var cmd = new FbCommand(SqlIngresoByRelacion, con);

			// IMPORTANTE: que coincida con el placeholder :idRelacion
			cmd.Parameters.Add(new FbParameter("idRelacion", FbDbType.BigInt) { Value = idRelacion });

			await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);

			if (!await rd.ReadAsync(ct))
				return null;

			// Mapeo manual (ejemplo). Ajusta según tu clase y aliases.
			return new AnamnesisIngresoRow
			{
				Numero_documento = rd["NUMERO_DOCUMENTO"]?.ToString(),
				Tipo_documento = rd["TIPO_DOCUMENTO"]?.ToString(),
				PRIMER_NOMBRE = rd["PRIMER_NOMBRE"]?.ToString(),
				SEGUNDO_NOMBRE = rd["SEGUNDO_NOMBRE"]?.ToString(),
				PRIMER_APELLIDO = rd["PRIMER_APELLIDO"]?.ToString(),
				SEGUNDO_APELLIDO = rd["SEGUNDO_APELLIDO"]?.ToString(),
				Fecha_nacimiento = rd["FECHA_NACIMIENTO"]?.ToString(),
				Genero = rd["GENERO"]?.ToString(),
				Indicativo_pais = rd["INDICATIVO_PAIS"]?.ToString(),
				Codigo_pais = rd["CODIGO_PAIS"]?.ToString(),
				Codigo_departamento = rd["CODIGO_DEPARTAMENTO"]?.ToString(),
				Codigo_ciudad = rd["CODIGO_CIUDAD"]?.ToString(),
				direccion = rd["DIRECCION"]?.ToString(),
				telefono = rd["TELEFONO"]?.ToString(),
				Correo_electronico = rd["CORREO_ELECTRONICO"]?.ToString(),
				Personal_codigo = rd["PERSONAL_CODIGO"]?.ToString(),
				Personal = rd["PERSONAL"]?.ToString(),
				ESPECIALIDAD = rd["ESPECIALIDAD"]?.ToString(),
				Entidad = rd["ENTIDAD"]?.ToString(),
				Regimen = rd["REGIMEN"]?.ToString(),
				CentroAtencion = rd["CENTROATENCION"]?.ToString(),
				Tipo_Estudio = rd["TIPO_ESTUDIO"]?.ToString(),
				Codigo_Servicio = rd["CODIGO_SERVICIO"]?.ToString(),
				Servicio_Ips = rd["SERVICIO_IPS"]?.ToString(),
				cantidad = rd["CANTIDAD"] is DBNull ? 0 : Convert.ToInt32(rd["CANTIDAD"]),
				Fecha_Solicitud = rd["FECHA_SOLICITUD"]?.ToString(),
				Id_Orden = rd["ID_ORDEN"]?.ToString(),
				Id_paciente = rd["ID_PACIENTE"] is DBNull ? null : Convert.ToInt32(rd["ID_PACIENTE"]),
				Id_ingreso = rd["ID_INGRESO"] is DBNull ? null : Convert.ToInt64(rd["ID_INGRESO"]),
				ingreso = rd["INGRESO"] is DBNull ? null : Convert.ToDecimal(rd["INGRESO"])
			};
		}


		private const string SqlIngresoByRelacion = @"
            select 
                a.CEDULA_NUMERO as Numero_documento,
                a.DOCUMENTO_IDENTIDAD as Tipo_documento,
                TRIM(SUBSTRING(a.NOMBRES FROM 1 FOR POSITION(' ' IN a.NOMBRES || ' ') - 1)) as PRIMER_NOMBRE,
                TRIM(SUBSTRING(a.NOMBRES FROM POSITION(' ' IN a.NOMBRES || ' ') + 1)) as SEGUNDO_NOMBRE,
                TRIM(SUBSTRING(a.APELLIDOS FROM 1 FOR POSITION(' ' IN a.APELLIDOS || ' ') - 1)) as PRIMER_APELLIDO,
                TRIM(SUBSTRING(a.APELLIDOS FROM POSITION(' ' IN a.APELLIDOS || ' ') + 1)) as SEGUNDO_APELLIDO,
                a.APELLIDOS as APELLIDOS,
                a.FECHAN_ANO || 
                case upper(a.FECHAN_MES)
                   when 'ENE' then '01'
                   when 'FEB' then '02'
                   when 'MAR' then '03'
                   when 'ABR' then '04'
                   when 'MAY' then '05'
                   when 'JUN' then '06'
                   when 'JUL' then '07'
                   when 'AGO' then '08'
                   when 'SEP' then '09'
                   when 'OCT' then '10'
                   when 'NOV' then '11'
                   when 'DIC' then '12'
                end
                || LPAD(a.FECHAN_DIA, 2, '0') as Fecha_nacimiento,    
                SUBSTRING(a.SEXO FROM 1 FOR 1) as Genero,
                case upper(a.PAISDEORIGEN)
                    when 'CO' then '+57'
                    when 'VE' then '+58'
                    when 'EU' then '+1'
                    when 'ES' then '+34'
                    when 'MX' then '+52'
                    when 'AR' then '+54'
                    when 'BR' then '+55'
                    when 'CH' then '+56'
                    when 'PR' then '+51'
                    when 'EC' then '+593'
                    when 'UR' then '+598'
                    when 'PY' then '+595'
                    when 'CR' then '+506'
                    when 'PA' then '+507'
                    else '+57'
                end as Indicativo_pais,
                case upper(a.PAISDEORIGEN)
                    when 'CO' then '57'
                    when 'VE' then '58'
                    when 'EU' then '01'
                    when 'ES' then '34'
                    when 'MX' then '52'
                    when 'AR' then '54'
                    when 'BR' then '55'
                    when 'CH' then '56'
                    when 'PR' then '51'
                    when 'EC' then '593'
                    when 'UR' then '598'
                    when 'PY' then '595'
                    when 'CR' then '506'
                    when 'PA' then '507'
                    else '57'
                end as Codigo_pais,
                a.CODIGO_DEPARTAMENTO as Codigo_departamento,
                a.CODIGO_CIUDAD as Codigo_ciudad,
                a.DIRECCION_PACIENTE as direccion,
                a.TELF_P as telefono,
                a.E_MAIL_RESP as Correo_electronico,
                dd.NUMERO_DOCUMENTO as Personal_codigo,
                dd.NOMBRE as Personal,
                dd.ESPECIALIDAD as ESPECIALIDAD,
                a.CODIGO_EPS as Entidad,
                a.PARENTESCO as Regimen,
                ir.NOMBRE as CentroAtencion,
                aa.DESCRIPCION as Tipo_Estudio,
                aa.CODIGO_DESCRIPCION as Codigo_Servicio,
                '' as Servicio_Ips,
                1 as cantidad,
                EXTRACT(YEAR FROM aa.FECHA) ||
                    LPAD(EXTRACT(MONTH FROM aa.FECHA), 2, '0') ||
                    LPAD(EXTRACT(DAY FROM aa.FECHA), 2, '0') as Fecha_Solicitud, 
                TRIM(aa.RECIBO_RELACIONADO) as Id_Orden, 
                a.IDANAMNESIS_TEXTO as Id_paciente,
                aa.IDRELACION as Id_ingreso,
                aa.VALOR as ingreso 
            from TANAMNESIS a
            inner join T_ADICIONALES_ABONOS aa on aa.id = a.idanamnesis
            inner join TDATOSDOCTORES dd1 on dd1.ID = aa.IDDOCTOR 
            inner join TINFORMACIONREPORTES ir on ir.ID = dd1.IDREPORTE
            inner join TDATOSDOCTORES dd on dd.ID = aa.recibido_por 
            left join TCODIGOS_EPS ep on ep.CODIGO = a.CODIGO_EPS
            where aa.IDRELACION = @idRelacion
            ";
	}
}
