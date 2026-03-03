using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.TablasFraccionadas.TAnamnesis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.TAnamnesis
{
    public class DatosPersonalesServicios : IDatosPersonalesServicios
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbcontext;
        
        public DatosPersonalesServicios()
        {
            using (var _dbcontext = new AppDbContext())
            {

                var mapperConfig = new MapperConfiguration(cfg =>
                {
                cfg.CreateMap<TANAMNESIS, DatosPersonales>();
                cfg.CreateMap<DatosPersonales, TANAMNESIS>();
                });
                _mapper = mapperConfig.CreateMapper();
            }
        }

       
        public async Task<int> Agregar(DatosPersonales datosPersonales)
        {
            using (var _dbcontext = new AppDbContext())
            {
                // Obtener el IDANAMNESIS más alto actual y sumarle 1
                int nuevoId = (_dbcontext.TANAMNESIS.Max(x => (int?)x.IDANAMNESIS) ?? 0) + 1;


                // Configura AutoMapper para ignorar el campo IDANAMNESIS solo en este mapeo
                var mapperConfig = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<DatosPersonales, TANAMNESIS>();
                    /*.ForMember(dest => dest.IDANAMNESIS, opt => opt.Ignore());
                    cfg.CreateMap<TANAMNESIS, DatosPersonales>()
                    .ForMember(dest => dest.IDANAMNESIS, opt => opt.Ignore());*/
                });

                var mapper = mapperConfig.CreateMapper();

                var obj = mapper.Map<TANAMNESIS>(datosPersonales);
                obj.IDANAMNESIS = nuevoId; // Asignar el nuevo ID manualmente
                _dbcontext.TANAMNESIS.Add(obj);
                await _dbcontext.SaveChangesAsync();
                return obj.IDANAMNESIS; // Retorna el ID generado por la base de datos
            }
        }

        public async Task Borrar(int IDANAMNESIS)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
                _dbcontext.TANAMNESIS.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<DatosPersonales> ConsultarPorId(int IDANAMNESIS)
        {
            using (var _dbcontext = new AppDbContext())
            {
                try
                {
                    var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
                    var tdatospersonales = _mapper.Map<DatosPersonales>(obj);
                    return tdatospersonales ?? new DatosPersonales();
                }
                catch (Exception error)
                {

                    return new DatosPersonales();
                }
            }    
            
        }

        public async Task<int> ConsultarTotalPacientesPorDoctor(int IDDOCTOR)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TTRATAMIENTO.Where(x => x.ID_DOCTOR == IDDOCTOR).Select(x => x.IDTRATAMIENTO).Distinct().ToListAsync();
                return obj == null ? 0 : obj.Count();
            }
        }

        public async Task<List<P_BUSCARPACIENTE>> BuscarPacientePorTipo(int TIPO, string BUSCAR)
        {
            using (var _dbcontext = new AppDbContext())
            {
                BUSCAR = BUSCAR.Trim().Replace("  ", " ").Replace(" ", "%").ToUpper();
                var obj = await _dbcontext.P_BUSCARPACIENTE(TIPO, BUSCAR);
                return obj == null ? new List<P_BUSCARPACIENTE>() : obj;
            }
        }

		/*public async Task<int> Editar(int IDANAMNESIS, DatosPersonales datospersonales)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
                if (obj == null)
                {
                    return 0; // Retorna 0 si no se encuentra el registro
                }

                _dbcontext.Entry(obj).CurrentValues.SetValues(datospersonales);
                try
                {
                    await _dbcontext.SaveChangesAsync();
                    return obj.IDANAMNESIS; // Retorna el ID después de la actualización

                }

                catch (Exception e)
                {

                    return obj.IDANAMNESIS=0;
                }
                
            }
                
        }*/

		public async Task<int> Editar(int id, DatosPersonales dto)
		{
			using var db = new AppDbContext();

			var obj = await db.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == id);
			if (obj == null)
				return 0;

			// ✅ NO TOCAR: obj.IMPORTANTE   (evita pisar el IMPORTANTE de Antecedentes)

			// ✅ WHITELIST: SOLO campos de DatosPersonales
			obj.IDANAMNESIS_TEXTO = dto.IDANAMNESIS_TEXTO;
			obj.NOTA_IMPORTANTE = dto.NOTA_IMPORTANTE;
			obj.COMPARACION = dto.COMPARACION;

			obj.FECHA_INGRESO = dto.FECHA_INGRESO;
			obj.FECHA_INGRESO_DATE = dto.FECHA_INGRESO_DATE;
			obj.HORA_INGRESO = dto.HORA_INGRESO;

			obj.NOMBRES = dto.NOMBRES;
			obj.APELLIDOS = dto.APELLIDOS;
			obj.NOMBRE_PACIENTE = dto.NOMBRE_PACIENTE;

			obj.FECHAN_DIA = dto.FECHAN_DIA;
			obj.FECHAN_MES = dto.FECHAN_MES;
			obj.FECHAN_ANO = dto.FECHAN_ANO;

			obj.DOCUMENTO_IDENTIDAD = dto.DOCUMENTO_IDENTIDAD;
			obj.SEXO = dto.SEXO;
			obj.EDAD = dto.EDAD;
			obj.EDADMES = dto.EDADMES;

			obj.DIRECCION_PACIENTE = dto.DIRECCION_PACIENTE;
			obj.TELF_P = dto.TELF_P;
			obj.TELF_P_OTRO = dto.TELF_P_OTRO;
			obj.CELULAR_P = dto.CELULAR_P;

			obj.NOMBRE_RESPONS = dto.NOMBRE_RESPONS;
			obj.DIRECCION_RESPONSABLE = dto.DIRECCION_RESPONSABLE;
			obj.TELF_RESP = dto.TELF_RESP;
			obj.TELF_OF_RESP = dto.TELF_OF_RESP;
			obj.CELULAR_RESPONSABLE = dto.CELULAR_RESPONSABLE;

			obj.BEEPER_RESPONSABLE = dto.BEEPER_RESPONSABLE;
			obj.COD_BEEPR_RESP = dto.COD_BEEPR_RESP;
			obj.E_MAIL_RESP = dto.E_MAIL_RESP;

			obj.REFERIDO_POR = dto.REFERIDO_POR;
			obj.NRO_AFILIACION = dto.NRO_AFILIACION;
			obj.CONVENIO = dto.CONVENIO;
			obj.ESTADO_TRATAMIENTO = dto.ESTADO_TRATAMIENTO;
			obj.TIPO_PACIENTE = dto.TIPO_PACIENTE;

			obj.CEDULA_NUMERO = dto.CEDULA_NUMERO;
			obj.ESTADOCIVIL = dto.ESTADOCIVIL;
			obj.PARENTESCO = dto.PARENTESCO;
			obj.NIVELESCOLAR = dto.NIVELESCOLAR;
			obj.ZONA_RECIDENCIAL = dto.ZONA_RECIDENCIAL;
			obj.PARENTESCO_RESPONSABLE = dto.PARENTESCO_RESPONSABLE;

			obj.DOMICILIO = dto.DOMICILIO;
			obj.EMERGENCIA = dto.EMERGENCIA;
			obj.ACOMPANATE_TEL = dto.ACOMPANATE_TEL;
			obj.ACOMPANATE = dto.ACOMPANATE;

			obj.BARRIO = dto.BARRIO;
			obj.LUGAR = dto.LUGAR;
			obj.DOCUMENTO_RESPONS = dto.DOCUMENTO_RESPONS;

			obj.ACTIVIDAD_ECONOMICA = dto.ACTIVIDAD_ECONOMICA;
			obj.ESTRATO = dto.ESTRATO;
			obj.LUGAR_NACIMIENTO = dto.LUGAR_NACIMIENTO;

			obj.CODIGO_CIUDAD = dto.CODIGO_CIUDAD;
			obj.CODIGO_DEPARTAMENTO = dto.CODIGO_DEPARTAMENTO;
			obj.CODIGO_EPS = dto.CODIGO_EPS;
			obj.CODIGO_EPS_LISTADO = dto.CODIGO_EPS_LISTADO;

			obj.NUMERO_TTITULAR = dto.NUMERO_TTITULAR;
			obj.NOMBREPADRE = dto.NOMBREPADRE;
			obj.TELEFONOPADRE = dto.TELEFONOPADRE;
			obj.NOMBRE_MADRE = dto.NOMBRE_MADRE;
			obj.TELEFONOMADRE = dto.TELEFONOMADRE;
			obj.CEL_PADRE = dto.CEL_PADRE;
			obj.CEL_MADRE = dto.CEL_MADRE;
			obj.OCUPACION_PADRE = dto.OCUPACION_PADRE;
			obj.OCUPACION_MADRE = dto.OCUPACION_MADRE;
			obj.NUMEROHERMANOS = dto.NUMEROHERMANOS;
			obj.RELACIONPADRES = dto.RELACIONPADRES;

			obj.ACTIVO = dto.ACTIVO;
			obj.IDREFERIDOPOR = dto.IDREFERIDOPOR;

			obj.COD_DOCTOR = dto.COD_DOCTOR;
			obj.DOCTOR = dto.DOCTOR;

			try
			{
				await db.SaveChangesAsync();
				return obj.IDANAMNESIS;
			}
			catch (Exception)
			{
				return 0;
			}
		}

	}

    public interface IDatosPersonalesServicios
    {
        Task<int> Agregar(DatosPersonales datosPersonales);
        Task<int> Editar(int IDANAMNESIS, DatosPersonales datosPersonales);
        Task<DatosPersonales> ConsultarPorId(int IDANAMNESIS);
        Task<List<P_BUSCARPACIENTE>> BuscarPacientePorTipo(int TIPO, string BUSCAR);
        Task Borrar(int IDANAMNESIS);
    }
}
