﻿using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
    public class RespuestaPinModel
    {
        public TCLAVE clave { set; get; }=new TCLAVE();
        public List<ListadoItemModel> lstDoctores { set; get; } = new List<ListadoItemModel>();
        public List<TCODIGOS_EPS> lstEps { set; get; } = new List<TCODIGOS_EPS>();
        public List<TCODIGOS_CIUDAD> lstCiudades { set; get; } = new List<TCODIGOS_CIUDAD>();
        public List<TCODIGOS_DEPARTAMENTO> lstDepartamentos { set; get; } = new List<TCODIGOS_DEPARTAMENTO>();
        public List<T_FRASE_XEVOLUCION> lstFrasesXEvolucion { set; get; } = new List<T_FRASE_XEVOLUCION>();
        public List<THORARIOSAGENDA> lstHorariosAgenda { set; get; } = new List<THORARIOSAGENDA>();
        public List<TFESTIVOS> lstFestivos { set; get; } = new List<TFESTIVOS>();
        public List<TCONFIGURACIONES_RYDENT> lstConfiguracionesRydent { set; get; } = new List<TCONFIGURACIONES_RYDENT>();
        
    }
}
