
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

                var payload = new
                {
                    phoneNumber = haciaNumero,
                    message = $"Hola {parametros[0]}, te recordamos que tu cita está agendada para el dia {parametros[1]} a las {parametros[2]}."
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

                var payload = new
                {
                    phoneNumber = haciaNumero,
                    message = $"Hola {parametros[0]}, tu cita a sido agendada para el dia {parametros[1]} a las {parametros[2]}."
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
