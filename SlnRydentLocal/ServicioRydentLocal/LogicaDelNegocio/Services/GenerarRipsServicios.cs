using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Facturatech;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class GenerarRipsServicios : IGenerarRipsServicios
    {

        private string urlApiBase;
        private readonly IConfiguration _configuration;
        protected readonly AppDbContext _dbcontext;
        public GenerarRipsServicios(IConfiguration configuration)
        {
            _configuration = configuration;
            urlApiBase = _configuration.GetValue<string>("RipsDocker:urlApiBase");
        }
        public async Task<List<TDATOSDOCTORES>> ListarDoctores()
        {
            using (var _dbcontext = new AppDbContext())
            {
                return _dbcontext.TDATOSDOCTORES.ToList();
            }
        }
        public async Task<List<TINFORMACIONREPORTES>> ListarInformacionReportes()
        {
            using (var _dbcontext = new AppDbContext())
            {
                return _dbcontext.TINFORMACIONREPORTES.ToList();
            }
        }
        public async Task<TINFORMACIONREPORTES> InformacionReportesXId(int id)
        {
            using (var _dbcontext = new AppDbContext())
            {
                return _dbcontext.TINFORMACIONREPORTES.Where(x => x.ID == id).FirstOrDefault();
            }
        }

        public async Task<string> obtenerTokenRips(TINFORMACIONREPORTES infoReportes)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            HttpClient client = new HttpClient(handler);
            string res = string.Empty;
            string url = $"{urlApiBase}/auth/LoginSISPRO";
            var objConsultar = new SolicitarTokenModel();
            objConsultar.persona.identificacion.tipo = infoReportes.TIPO_ID;
            objConsultar.persona.identificacion.numero = infoReportes.IDSISPRO;
            objConsultar.nit = infoReportes.NUMDOCUMENTOIDOBLIGADO;
            objConsultar.clave = infoReportes.PASSISPRO;
            string json = System.Text.Json.JsonSerializer.Serialize(objConsultar);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = client.PostAsync(url, content);
            response.Wait();
            var respuesta = response.Result;
            if (respuesta.IsSuccessStatusCode)
            {
                var resBody = respuesta.Content.ReadAsStringAsync();
                resBody.Wait();
                if (!string.IsNullOrEmpty(resBody.Result))
                {
                    RespuestaSolicitarTokenModel resultado = JsonSerializer.Deserialize<RespuestaSolicitarTokenModel>(resBody.Result);
                    return resultado.token;
                }
            }
            return res;
        }

        public async Task<List<RespuestaCargarRipsModel>> CargarRipsSinFactura(List<transaccionModel> lstRips, string token, bool conFactura)
        {
            var respuestaCargarRipsSinFactura = new List<RespuestaCargarRipsModel>();
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            HttpClient client = new HttpClient(handler);
            string res = string.Empty;
            string url = conFactura ? $"{urlApiBase}/PaquetesFevRips/CargarFevRips" : $"{urlApiBase}/PaquetesFevRips/CargarRipsSinFactura";
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            foreach (var objRIPS in lstRips)
            {
                if (!conFactura)
                {
                    objRIPS.rips.numNota = objRIPS.rips.numFactura;
                    objRIPS.rips.tipoNota = "RS";
                    objRIPS.rips.numFactura = null;

                }
                else
                {
                    var fac = new LNDianGeneral();
                    objRIPS.xmlFevFile =  fac.FacturaXMLBase64XId(objRIPS.rips.idRelacion ?? 0);

                }
                string json = System.Text.Json.JsonSerializer.Serialize(objRIPS);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync(url, content);

                response.Wait();
                var respuesta = response.Result;

                var resBody = respuesta.Content.ReadAsStringAsync();
                resBody.Wait();
                if (!string.IsNullOrEmpty(resBody.Result))
                {
                    var opciones = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    RespuestaCargarRipsModel resultado = JsonSerializer.Deserialize<RespuestaCargarRipsModel>(resBody.Result, opciones);

                    respuestaCargarRipsSinFactura.Add(resultado);
                }

            }
            return respuestaCargarRipsSinFactura;
        }
    }
    
    public interface IGenerarRipsServicios
    {
        Task<List<TDATOSDOCTORES>> ListarDoctores();
        Task<List<TINFORMACIONREPORTES>> ListarInformacionReportes();
        Task<TINFORMACIONREPORTES> InformacionReportesXId(int id);
        Task<string> obtenerTokenRips(TINFORMACIONREPORTES infoReportes);
        Task<List<RespuestaCargarRipsModel>> CargarRipsSinFactura(List<transaccionModel> lstRips, string token, bool conFactura);
    }
}
 