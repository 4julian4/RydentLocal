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

        public async Task<int> Editar(int IDANAMNESIS, DatosPersonales datospersonales)
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
