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
        public DatosPersonalesServicios(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public DatosPersonalesServicios()
        {
                var mapperConfig = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<TANAMNESIS, DatosPersonales>();
                    cfg.CreateMap<DatosPersonales, TANAMNESIS>();
                });
                _mapper = mapperConfig.CreateMapper();
        }

       
        public async Task<int> Agregar(DatosPersonales datosPersonales)
        {
            
            // Configura AutoMapper para ignorar el campo IDANAMNESIS solo en este mapeo
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DatosPersonales, TANAMNESIS>()
                    .ForMember(dest => dest.IDANAMNESIS, opt => opt.Ignore());
                cfg.CreateMap<TANAMNESIS, DatosPersonales>()
                    .ForMember(dest => dest.IDANAMNESIS, opt => opt.Ignore());
            });

            var mapper = mapperConfig.CreateMapper();

            var obj = mapper.Map<TANAMNESIS>(datosPersonales);
            _dbcontext.TANAMNESIS.Add(obj);
            await _dbcontext.SaveChangesAsync();
            return obj.IDANAMNESIS; // Retorna el ID generado por la base de datos
            
        }

        public async Task Borrar(int IDANAMNESIS)
        {
            
            var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
            _dbcontext.TANAMNESIS.Remove(obj);
            await _dbcontext.SaveChangesAsync();
            
        }

        public async Task<DatosPersonales> ConsultarPorId(int IDANAMNESIS)
        {
            
            try
            {
                var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
                var tdatospersonales = _mapper.Map<DatosPersonales>(obj);
                return tdatospersonales ??  new DatosPersonales();
            }
            catch (Exception error)
            {

                return new DatosPersonales();
            }
                
            
        }

        public async Task<int> ConsultarTotalPacientesPorDoctor(int IDDOCTOR)
        {
           
            var obj = await _dbcontext.TTRATAMIENTO.Where(x => x.ID_DOCTOR == IDDOCTOR).Select(x => x.IDTRATAMIENTO).Distinct().ToListAsync();
            return obj == null ? 0 : obj.Count();
            
        }

        public async Task<List<P_BUSCARPACIENTE>> BuscarPacientePorTipo(int TIPO, string BUSCAR)
        {
            
            BUSCAR = BUSCAR.Trim().Replace("  ", " ").Replace(" ", "%").ToUpper();
            var obj = await _dbcontext.P_BUSCARPACIENTE(TIPO, BUSCAR);
            return obj == null ? new List<P_BUSCARPACIENTE>() : obj;
            
        }

        public async Task<bool> Editar(int IDANAMNESIS, DatosPersonales datospersonales)
        {
            
            var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
            if (obj == null)
            {
                return false;
            }
            else
            {
                _dbcontext.Entry(obj).CurrentValues.SetValues(datospersonales);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            
        }
    }

    public interface IDatosPersonalesServicios
    {
        Task<int> Agregar(DatosPersonales datosPersonales);
        Task<bool> Editar(int IDANAMNESIS, DatosPersonales datosPersonales);
        Task<DatosPersonales> ConsultarPorId(int IDANAMNESIS);
        Task<List<P_BUSCARPACIENTE>> BuscarPacientePorTipo(int TIPO, string BUSCAR);
        Task Borrar(int IDANAMNESIS);
    }
}
