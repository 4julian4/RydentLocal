﻿using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.TablasFraccionadas.TAnamnesis;
using AutoMapper;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.TAnamnesis
{
    public class AntecedentesServicios : IAntecedentesServicios
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbcontext;
        
        public AntecedentesServicios()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TANAMNESIS, Antecedentes>();
                cfg.CreateMap<Antecedentes, TANAMNESIS>();
            });
            _mapper = mapperConfig.CreateMapper();
        }


        public async Task<Antecedentes> ConsultarPorId(int IDANAMNESIS)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
                var antecedentes = _mapper.Map<Antecedentes>(obj);
                return antecedentes ?? new Antecedentes();
            }
        }

        public async Task<int> Editar(int IDANAMNESIS, Antecedentes antecedentes)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
                if (obj == null)
                {
                    return 0; // Retorna 0 si no se encuentra el registro
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(antecedentes);
                    await _dbcontext.SaveChangesAsync();
                    return obj.IDANAMNESIS; // Retorna el ID después de la actualización
                }
            }
        }
    }

    public interface IAntecedentesServicios
    {
        Task<int> Editar(int IDANAMNESIS, Antecedentes antecedentes);
        Task<Antecedentes> ConsultarPorId(int IDANAMNESIS);
    }
}


