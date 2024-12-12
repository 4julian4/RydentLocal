
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{  
    internal class WhatsAppService
    {
        //--------------------logica para manejar el envio de mensajes de whatsapp en nodejs---------------//

        private readonly HttpClient _httpClient;

        public WhatsAppService()
        {
            _httpClient = new HttpClient();
        }
        //envio con twilio
        //public async Task<bool> EnviarMensaje(string desdeNumero, string haciaNumero, string templateNombre, List<string> parametros)
        //------------------------envio con node.js----------------------------------//
        public async Task<bool> EnviarMensaje(string haciaNumero, string templateNombre, List<string> parametros)
        {
            try
            {
                
                var url = "http://localhost:3000/send-message"; // URL de tu servidor Node.js

                // Validar si el template y los parámetros son válidos
                if (string.IsNullOrEmpty(templateNombre))
                {
                    Console.WriteLine("La plantilla del mensaje está vacía.");
                    return false;
                }

                if (parametros == null || parametros.Count == 0)
                {
                    Console.WriteLine("No hay parámetros para reemplazar en la plantilla.");
                    return false;
                }

                // Reemplazar los marcadores de posición con los parámetros
                string mensajePersonalizado = templateNombre;
                for (int i = 0; i < parametros.Count; i++)
                {
                    mensajePersonalizado = mensajePersonalizado.Replace($"{{{i}}}", parametros[i]);
                }

                //Validar los saltos de linea
                var mensajeFormateado = mensajePersonalizado.Replace("\\n", "\n");

                var payload = new
                {
                    phoneNumber = haciaNumero,
                    message = mensajeFormateado
                };

                
                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Mensaje enviado con éxito.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error al enviar mensaje: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el mensaje: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarMensajeCitaAgenda(string haciaNumero, string templateNombre, List<string> parametros)
        {
            try
            {
                var url = "http://localhost:3000/send-message"; // URL de tu servidor Node.js

                // Validar si el template y los parámetros son válidos
                if (string.IsNullOrEmpty(templateNombre))
                {
                    Console.WriteLine("La plantilla del mensaje está vacía.");
                    return false;
                }

                if (parametros == null || parametros.Count == 0)
                {
                    Console.WriteLine("No hay parámetros para reemplazar en la plantilla.");
                    return false;
                }

                // Reemplazar los marcadores de posición con los parámetros
                string mensajePersonalizado = templateNombre;
                for (int i = 0; i < parametros.Count; i++)
                {
                    mensajePersonalizado = mensajePersonalizado.Replace($"{{{i}}}", parametros[i]);
                }

                //Validar los saltos de linea
                var mensajeFormateado = mensajePersonalizado.Replace("\\n", "\n");
                

                var payload = new
                {
                    phoneNumber = haciaNumero,
                    message = mensajeFormateado
                };

                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Mensaje enviado con éxito.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error al enviar mensaje: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el mensaje: {ex.Message}");
                return false;
            }
        }

    }
}
