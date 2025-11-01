using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Facturatech
{
    public class Encriptacion
    {
        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string ComputeSha384Hash(string source)
        {
            //string source = "Hello World!";
            using (SHA384 sha384Hash = SHA384.Create())
            {
                //From String to byte array
                byte[] sourceBytes = Encoding.UTF8.GetBytes(source);
                byte[] hashBytes = sha384Hash.ComputeHash(sourceBytes);
                return BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            }
        }

        public static string CalcularDigitoVerificacion(string numeroIdentificacion, string tipo)
        {
            if (tipo != "31")
            {
                return "";
            }
            var ConNit = numeroIdentificacion.Split('-');
            if (ConNit.Length > 1)
            {
                var resultado = ConNit[1];
                return resultado;
            }
            string Temp;
            int Contador;
            int Residuo;
            int Acumulador;
            int[] Vector = new int[15];

            Vector[0] = 3;
            Vector[1] = 7;
            Vector[2] = 13;
            Vector[3] = 17;
            Vector[4] = 19;
            Vector[5] = 23;
            Vector[6] = 29;
            Vector[7] = 37;
            Vector[8] = 41;
            Vector[9] = 43;
            Vector[10] = 47;
            Vector[11] = 53;
            Vector[12] = 59;
            Vector[13] = 67;
            Vector[14] = 71;

            Acumulador = 0;

            Residuo = 0;

            for (Contador = 0; Contador < numeroIdentificacion.Length; Contador++)
            {
                Temp = numeroIdentificacion[(numeroIdentificacion.Length - 1) - Contador].ToString();
                Acumulador = Acumulador + (Convert.ToInt32(Temp) * Vector[Contador]);
            }

            Residuo = Acumulador % 11;

            if (Residuo > 1)
                return Convert.ToString(11 - Residuo);

            return Residuo.ToString();
        }

        public static bool ValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            else
                return new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z").IsMatch(email.ToLower().Trim());
        }
    }
}
