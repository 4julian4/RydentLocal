using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Helpers;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Facturatech
{
    public class Adicionales_Abonos_Dian
    {
        public List<P_Adicionales_Abonos_Dian> listarXid(int PIDRELACION)
        {
            List<P_Adicionales_Abonos_Dian> resultado = new List<P_Adicionales_Abonos_Dian>();
            using (var db = new AppDbContext())
            {
                var _P_ADICIONALES_ABONOS_DIAN_Result = db.P_ADICIONALES_ABONOS_DIAN(PIDRELACION);
                foreach (var _ADICIONALES_ABONOS_DIAN in _P_ADICIONALES_ABONOS_DIAN_Result)
                {
                    var s = Automap.AutoMapearDesdeObjeto<P_Adicionales_Abonos_Dian>(_ADICIONALES_ABONOS_DIAN);
                    resultado.Add(s);
                }
            }
            return resultado;
        }

        public List<T_ADICIONALES_ABONOS_MOTIVOS> ListAdicionalesAbonosMotivosXIdRelacion(int PIDRELACION)
        {
            using (var db = new AppDbContext())
            {
                return db.T_ADICIONALES_ABONOS_MOTIVOS.Where(x => x.IDRELACION == PIDRELACION).ToList();
            }
            
        }
        public List<P_Adicionales_Abonos_Dian> listarCXCXid(int PIDRELACION)
        {
            List<P_Adicionales_Abonos_Dian> resultado = new List<P_Adicionales_Abonos_Dian>();
            using (var db = new AppDbContext())
            {
                var _P_CXC_DIAN = db.P_CXC_DIAN(PIDRELACION);
                foreach (var _CXC_DIAN in _P_CXC_DIAN)
                {
                    var s = Automap.AutoMapearDesdeObjeto<P_Adicionales_Abonos_Dian>(_CXC_DIAN);
                    resultado.Add(s);
                }
            }
            return resultado;
        }


        public List<EntSPDianFacturasPendientes> listarFacturasPendientes()
        {
            List<EntSPDianFacturasPendientes> resultado = new List<EntSPDianFacturasPendientes>();
            using (var db = new AppDbContext())
            {
                var _P_DIAN_FACTURASPENDIENTES_Result = db.P_DIAN_FACTURASPENDIENTES();
                foreach (var _P_DIAN_FACTURASPENDIENTE in _P_DIAN_FACTURASPENDIENTES_Result)
                {
                    var s = Automap.AutoMapearDesdeObjeto<EntSPDianFacturasPendientes>(_P_DIAN_FACTURASPENDIENTE);
                    resultado.Add(s);
                }
            }
            return resultado;
        }

        public List<EntSPDianFacturasCreadas> ListarFacturasCreadas(string Factura)
        {

            List<EntSPDianFacturasCreadas> resultado = new List<EntSPDianFacturasCreadas>();
            using (var db = new AppDbContext())
            {
                var _P_DIAN_FACTURASCREADAS = db.P_DIAN_FACTURASCREADAS(Factura);
                foreach (var _P_DIAN_FACTURACREADA in _P_DIAN_FACTURASCREADAS)
                {
                    var s = Automap.AutoMapearDesdeObjeto<EntSPDianFacturasCreadas>(_P_DIAN_FACTURACREADA);
                    resultado.Add(s);
                }
            }
            return resultado;
        }
    }
}
