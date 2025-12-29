using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Repositorio
{

    /*public class Adicionales_Abonos_Dian
    {
        public List<P_Adicionales_Abonos_Dian> listarXid(int PIDRELACION)
        {
            List<P_Adicionales_Abonos_Dian> resultado = new List<P_Adicionales_Abonos_Dian>();
            using (var _dbcontext = new AppDbContext())
            {
                var _P_ADICIONALES_ABONOS_DIAN_Result = _dbcontext.P_ADICIONALES_ABONOS_DIAN(PIDRELACION);
                foreach (var _ADICIONALES_ABONOS_DIAN in _P_ADICIONALES_ABONOS_DIAN_Result)
                {
                    var s = Automap.AutoMapearDesdeObjeto<P_Adicionales_Abonos_Dian>(_ADICIONALES_ABONOS_DIAN);
                    resultado.Add(s);
                }
            }
            return resultado;
        }
    }*/

    public class Adicionales_Abonos_Dian
    {
        private readonly AppDbContext _dbcontext;

        public Adicionales_Abonos_Dian(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public List<P_Adicionales_Abonos_Dian> listarXid(int PIDRELACION)
        {
            return _dbcontext.P_ADICIONALES_ABONOS_DIAN(PIDRELACION)
                .Select(x => Automap.AutoMapearDesdeObjeto<P_Adicionales_Abonos_Dian>(x))
                .ToList();
        }
    }
}
