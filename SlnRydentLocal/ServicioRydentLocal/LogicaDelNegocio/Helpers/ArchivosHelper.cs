//using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.IO;


using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace ServicioRydentLocal.LogicaDelNegocio.Helpers
{
    public class ArchivosHelper
    {
        public string obtenerBase64ConPrefijo(string datosArchivo)
        {
            string prefijo = "";
            if (datosArchivo != "")
            {
                ;
                var extFile = obtenerExtensionBase64(datosArchivo).Replace(".", "");
                if (datosArchivo != "" && extFile == "pdf")
                {
                    prefijo = "data:application/pdf;base64,";
                }
                else if (datosArchivo != "" && validarJPG(extFile))
                {
                    prefijo = "data:image/png;base64,";
                }
                else if (datosArchivo != "" && extFile == "zip")
                {
                    prefijo = "data:application/zip;base64,";
                }
                if (!string.IsNullOrEmpty(prefijo))
                {
                    datosArchivo = prefijo + datosArchivo;
                }

            }
            return datosArchivo;
        }

        public string obtenerExtensionBase64(string base64String)
        {
            var data = base64String.Substring(0, 5);

            switch (data.ToUpper())
            {
                case "IVBOR":
                    return "png";
                case "/9J/4":
                    return "jpg";
                case "AAAAF":
                    return "mp4";
                case "JVBER":
                    return "pdf";
                case "AAABA":
                    return "ico";
                case "UMFYI":
                    return "rar";
                case "E1XYD":
                    return "rtf";
                case "U1PKC":
                    return "txt";
                case "MQOWM":
                case "77U/M":
                    return "srt";
                default:
                    return string.Empty;
            }
        }
        private bool validarJPG(string valor)
        {
            return valor == "png" || valor == "jpg" || valor == "jpeg";
        }

        public (int Width, int Height) obtenerDimensionFromBytes(byte[]? imageBytes)
        {
            //byte[] imageBytes = Convert.FromBase64String(base64String);
            if (imageBytes == null)
            {
                return (0, 0);
            }

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
               using (var image = Image.Load(ms))
                {
                    return (image.Width, image.Height);
                }
            }
        }

        public string recortarImganFromBytes(byte[]? imageBytes, Rectangle point1)
        {
            //byte[] imageBytes = Convert.FromBase64String(base64String);
            
            if (imageBytes == null)
            {
                return "";
            }
            //point1.

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                using (var image = Image.Load(ms))
                {
                    var s = image.Clone(x => x.Crop(point1));
                    using (var ms2 = new MemoryStream())
                    {
                        s.SaveAsPng(ms2);
                        var base64String = Convert.ToBase64String(ms2.ToArray());
                        return base64String;
                    }
                }
            }
        }
    }
}
