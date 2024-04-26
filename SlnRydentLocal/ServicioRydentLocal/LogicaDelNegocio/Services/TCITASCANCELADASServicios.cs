﻿using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TCITASCANCELADASServicios : ITCITASCANCELADASServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TCITASCANCELADASServicios()
        {
        }

        public async Task<TCITASCANCELADAS> Agregar(TCITASCANCELADAS tcitascanceladas)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TCITASCANCELADAS.Add(tcitascanceladas);
                await _dbcontext.SaveChangesAsync();
                return tcitascanceladas;
            }
        }


    }

    public interface ITCITASCANCELADASServicios
    {
        Task<TCITASCANCELADAS> Agregar(TCITASCANCELADAS tcitascanceladas);
    }

}
