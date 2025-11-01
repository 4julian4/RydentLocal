using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Helpers
{
    internal class TransferenciaArchivos
    {
        public static Stream Base64ToStream(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            Stream s = new MemoryStream(imageBytes);
            return s;
        }
    }
}
