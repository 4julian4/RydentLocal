//using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;




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




        // Función para convertir una imagen PNG a JPEG y obtener el resultado como byte[]
        //public byte[] ConvertirPNGaJPEG(byte[] imagenPNG)
        //{
        //    using var imagen = Image.Load(imagenPNG);
        //    using var streamSalida = new MemoryStream();
        //    imagen.SaveAsJpeg(streamSalida);
        //    return streamSalida.ToArray();
        //}

        public byte[] ConvertirPNGaJPEG(byte[] imagenPNG)
        {
            using var imagen = Image.Load<Rgba32>(imagenPNG);
            imagen.Mutate(x => x.BackgroundColor(new Rgba32(255, 255, 255)));
            using var streamSalida = new MemoryStream();
            imagen.SaveAsJpeg(streamSalida);
            return streamSalida.ToArray();
        }


        public byte[] CrearImagenConBase64(string base64Image1, string base64Image2)
        {
            int newHeight = 215;
            // Crear una imagen en blanco
            int width = 1364, height = 482;
            using var blankImage = new Image<Rgba32>(width, height);

            if (!string.IsNullOrEmpty(base64Image1))
            {
                // Decodificar las imágenes base64
                using var image1 = Image.Load<Rgba32>(Convert.FromBase64String(base64Image1));

                // Redimensionar las imágenes a una altura de 215 manteniendo la proporción
                image1.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(image1.Width * newHeight / image1.Height, newHeight) }));


                // Calcular las posiciones para centrar las imágenes
                int x1 = (width - image1.Width) / 2;
                int y1 = (height / 2 - image1.Height) / 2;


                // Pegar las imágenes en la imagen en blanco
                blankImage.Mutate(ctx => ctx.DrawImage(image1, new Point(x1, y1), 1));
            }
            

            if (!string.IsNullOrEmpty(base64Image2))
            {

                // Decodificar las imágenes base64
                using var image2 = Image.Load<Rgba32>(Convert.FromBase64String(base64Image2));

                // Redimensionar las imágenes a una altura de 200 manteniendo la proporción
                image2.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(image2.Width * newHeight / image2.Height, newHeight) }));

                // Calcular las posiciones para centrar las imágenes
                int x2 = (width - image2.Width) / 2;
                int y2 = height / 2 + (height / 2 - image2.Height) / 2;


                // Pegar las imágenes en la imagen en blanco
                blankImage.Mutate(ctx => ctx.DrawImage(image2, new Point(x2, y2), 1));
            }

            // Convertir la imagen final a byte[]
            using var ms = new MemoryStream();
            blankImage.SaveAsPng(ms);
            return ms.ToArray();
        }

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

        public string ReducirTamañoImagen(string imagenOriginal, float porcentajeReduccion, int calidad)
        {
            if (imagenOriginal == null || imagenOriginal.Length == 0)
            {
                return "";
            }
            using var imagen = Image.Load(Convert.FromBase64String(imagenOriginal));
            int nuevoAncho = (int)(imagen.Width * porcentajeReduccion / 100);
            int nuevoAlto = (int)(imagen.Height * porcentajeReduccion / 100);

            imagen.Mutate(x => x.Resize(nuevoAncho, nuevoAlto));

            using var streamSalida = new MemoryStream();
            imagen.SaveAsJpeg(streamSalida, new JpegEncoder { Quality = calidad });
            return Convert.ToBase64String(streamSalida.ToArray());
        }
    }
}
