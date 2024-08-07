﻿using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{

    public class TPLANTRATAMIENTOServicios : ITPLANTRATAMIENTOServicios
    {
        private readonly AppDbContext _dbcontext;
        public TPLANTRATAMIENTOServicios(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public TPLANTRATAMIENTOServicios()
        {
        }

        public async Task<int> Agregar(TPLANTRATAMIENTO tplantratamiento)
        {


            _dbcontext.TPLANTRATAMIENTO.Add(tplantratamiento);
            await _dbcontext.SaveChangesAsync();
            return tplantratamiento.ID;

        }

        public async Task Borrar(int ID)
        {

            var obj = await _dbcontext.TPLANTRATAMIENTO.FirstOrDefaultAsync(x => x.ID == ID);
            _dbcontext.TPLANTRATAMIENTO.Remove(obj);
            await _dbcontext.SaveChangesAsync();

        }

        public async Task<TPLANTRATAMIENTO> ConsultarPorId(int ID)
        {

            var obj = await _dbcontext.TPLANTRATAMIENTO.FirstOrDefaultAsync(x => x.ID == ID);
            return obj == null ? new TPLANTRATAMIENTO() : obj;

        }




        public async Task<bool> Editar(int ID, TPLANTRATAMIENTO tplantratamiento)
        {

            var obj = await _dbcontext.TPLANTRATAMIENTO.FirstOrDefaultAsync(x => x.ID == ID);
            if (obj == null)
            {
                return false;
            }
            else
            {
                _dbcontext.Entry(obj).CurrentValues.SetValues(tplantratamiento);
                await _dbcontext.SaveChangesAsync();
                return true;
            }

        }
    }

    public interface ITPLANTRATAMIENTOServicios
    {
        Task<int> Agregar(TPLANTRATAMIENTO tplantratamiento);
        Task<bool> Editar(int ID, TPLANTRATAMIENTO tplantratamiento);
        Task<TPLANTRATAMIENTO> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}
