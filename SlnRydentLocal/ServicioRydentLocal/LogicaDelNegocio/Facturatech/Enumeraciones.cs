using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Facturatech
{
    internal class Enumeraciones
    {
        public static class ResupestasFacturatech
        {
            public static class Estados
            {
                public static string Firmada { get { return "SIGNED_XML"; } }
            }
        }
        public static class TiposFacturatech
        {
            public static class TipoOperacion
            {
                public static string Factura { get { return "10"; } }
                public static string NC { get { return "20"; } }
            }
            public static class TipoEncabezadoXML
            {
                public static string Factura { get { return "FACTURA"; } }
                public static string NC { get { return "NOTA"; } }
            }
        }
    }
}
