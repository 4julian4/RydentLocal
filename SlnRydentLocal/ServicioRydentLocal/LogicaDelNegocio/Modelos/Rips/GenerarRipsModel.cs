using System;
using System.Collections.Generic;


namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
    internal class GenerarRipsModel
    {
        public DateTime FECHAINI { get; set; }
        public DateTime FECHAFIN { get; set; }
        public string EPS { get; set; }
        public string FACTURA { get; set; }
        public int IDDOCTOR { get; set; } = 0;
        public int IDREPORTE { get; set; } = 0;
        public string EXTRANJERO { get; set; }
        public List<ItemModel> lstDoctores = new List<ItemModel>();
        public List<ItemModel> lstInformacionReporte = new List<ItemModel>();
    }
}
