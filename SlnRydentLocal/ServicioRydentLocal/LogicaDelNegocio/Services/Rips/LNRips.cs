using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Facturatech;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Rips
{
    public class LNRips
    {
        private string urlApiBase;
        private readonly IConfiguration _configuration;
        protected readonly AppDbContext _dbcontext;
        public LNRips(IConfiguration configuration)
        {
            _configuration = configuration;
            urlApiBase = _configuration.GetValue<string>("RipsDocker:urlApiBase");
        }
        public List<TDATOSDOCTORES> ListarDoctores()
        {
            using (var _dbcontext = new AppDbContext())
            {
                return _dbcontext.TDATOSDOCTORES.ToList();
            }
        }
        public List<TINFORMACIONREPORTES> ListarInformacionReportes()
        {
            using (var _dbcontext = new AppDbContext())
            {
                return _dbcontext.TINFORMACIONREPORTES.ToList();
            }
        }
        public TINFORMACIONREPORTES InformacionReportesXId(int id)
        {
            using (var _dbcontext = new AppDbContext())
            {
                return _dbcontext.TINFORMACIONREPORTES.Where(x => x.ID == id).FirstOrDefault();
            }
        }

        public async Task<TINFORMACIONREPORTES> InformacionReportesXIdAsync(int id)
        {
            using (var _dbcontext = new AppDbContext())
            {
                return await _dbcontext.TINFORMACIONREPORTES
                                       .Where(x => x.ID == id)
                                       .FirstOrDefaultAsync();
            }
        }

        public string obtenerTokenRips(TINFORMACIONREPORTES infoReportes)
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

        public async Task<List<RespuestaCargarRipsModel>> CargarRipsSinFacturaAsync(List<transaccionModel> lstRips, string token, bool conFactura)
        {
            var respuestaCargarRipsSinFactura = new List<RespuestaCargarRipsModel>();
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            HttpClient client = new HttpClient(handler);
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
                    objRIPS.xmlFevFile = fac.FacturaXMLBase64XId(objRIPS.rips.idRelacion ?? 0);
                }

                string json = System.Text.Json.JsonSerializer.Serialize(objRIPS);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content); // Usando await aquí

                var resBody = await response.Content.ReadAsStringAsync(); // Usando await aquí

                if (!string.IsNullOrEmpty(resBody))
                {
                    var opciones = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    RespuestaCargarRipsModel resultado = JsonSerializer.Deserialize<RespuestaCargarRipsModel>(resBody, opciones);
                    respuestaCargarRipsSinFactura.Add(resultado);
                }
            }

            return respuestaCargarRipsSinFactura;
        }


        public List<RespuestaCargarRipsModel> CargarRipsSinFactura(List<transaccionModel> lstRips, string token, bool conFactura)
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
                    objRIPS.xmlFevFile = fac.FacturaXMLBase64XId(objRIPS.rips.idRelacion ?? 0);

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


         public List<transaccionModel> MapearRipsSinFactura(List<transaccionModel> lstRips, bool conFactura)
         {
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
                     objRIPS.xmlFevFile = fac.FacturaXMLBase64XId(objRIPS.rips.idRelacion ?? 0);

                 }

             }
             return lstRips;
         }

        public List<transaccionModel> MapearRipsSinFacturaAsync(List<transaccionModel> lstRips, bool conFactura)
        {
            if (conFactura)
            {
                var fac = new LNDianGeneral(); // Instancia única para evitar múltiples creaciones
                foreach (var objRIPS in lstRips)
                {
                    objRIPS.xmlFevFile = fac.FacturaXMLBase64XId(objRIPS.rips.idRelacion ?? 0);
                }
            }
            else
            {
                foreach (var objRIPS in lstRips)
                {
                    objRIPS.rips.numNota = objRIPS.rips.numFactura;
                    objRIPS.rips.tipoNota = "RS";
                    objRIPS.rips.numFactura = null;
                }
            }
            return lstRips;
        }

        /*public async Task<List<transaccionModel>> MapearRipsSinFactura(List<transaccionModel> lstRips, bool conFactura)
        {
            if (conFactura)
            {
                var fac = new LNDianGeneral(); // Instancia única para evitar múltiples creaciones

                // Crear una lista de tareas para descargar todas las facturas en paralelo
                var tareas = lstRips.Select(async objRIPS =>
                {
                    objRIPS.xmlFevFile = await fac.FacturaXMLBase64XId(objRIPS.rips.idRelacion ?? 0);
                }).ToList();

                // Esperar que todas las descargas terminen
                await Task.WhenAll(tareas);
            }
            else
            {
                foreach (var objRIPS in lstRips)
                {
                    objRIPS.rips.numNota = objRIPS.rips.numFactura;
                    objRIPS.rips.tipoNota = "RS";
                    objRIPS.rips.numFactura = null;
                }
            }
            return lstRips;
        }*/







        public List<transaccionModel> ConsultarRips(DateTime FECHAINI, DateTime FECHAFIN, string EPS, string FACTURA, int IDREPORTE, int IDDOCTOR, string EXTRANJERO)
         {
             using (var _dbcontext = new AppDbContext())
             {
                 var lstTransaccion = new List<transaccionModel>();
                 var acModel = _dbcontext.SP_RIPS_JSON_AC(FECHAINI, FECHAFIN, EPS, FACTURA, IDREPORTE, IDDOCTOR, EXTRANJERO);
                 var usModel = _dbcontext.SP_RIPS_JSON_US(FECHAINI, FECHAFIN, EPS, FACTURA, IDREPORTE, IDDOCTOR, EXTRANJERO);

                 var apModel = _dbcontext.SP_RIPS_JSON_AP(FECHAINI, FECHAFIN, EPS, FACTURA, IDREPORTE, IDDOCTOR, EXTRANJERO);
                 var lstFacturas = usModel.Select(x => x.NUMERO_FACTURA).Distinct().ToList();
                 if (usModel != null && usModel.Any() && lstFacturas.Any())
                 {
                     foreach (var factura in lstFacturas.OrderBy(x => x))
                     {

                         var modelo = new transaccionModel();
                         modelo.rips = new transaccionRipsModel();

                         modelo.rips.numFactura = factura;
                         var usMOdelXFactura = usModel.Where(y => y.NUMERO_FACTURA == factura).ToList();
                         modelo.rips.numDocumentoIdObligado = usMOdelXFactura.FirstOrDefault().NUMDOCUMENTOIDOBLIGADO;
                         modelo.rips.idRelacion = usMOdelXFactura.FirstOrDefault().IDRELACION;
                         modelo.rips.usuarios = usMOdelXFactura.ConvertAll(x => new UsuariosModel()
                         {
                             codMunicipioResidencia = x.CODIGO_CIUDAD,
                             codPaisOrigen = x.CODPAISORIGEN,
                             codPaisResidencia = x.CODPAISRESIDENCIA,
                             codSexo = x.SEXO,
                             codZonaTerritorialResidencia = x.ZONA_RESIDENCIAL,
                             consecutivo = x.CONSECUTIVO ?? 0,
                             fechaNacimiento = x.FECHANACIMIENTO,
                             incapacidad = x.INCAPACIDAD,
                             numDocumentoIdentificacion = x.IDANAMNESIS_TEXTO,
                             tipoDocumentoIdentificacion = x.DOCUMENTO_IDENTIDAD,
                             tipoUsuario = x.TIPOUSUARIO,
                             servicios = new ServiciosModel()
                             {
                                 consultas = lstAC(acModel.Where(y => y.NUMERO_FACTURA == factura).OrderBy(x => x.CONSECUTIVO).ToList()),
                                 procedimientos = lstAP(apModel.Where(y => y.NUMERO_FACTURA == factura).OrderBy(x => x.CONSECUTIVO).ToList())
                             }
                         });
                         lstTransaccion.Add(modelo);
                     }

                 }
                 return lstTransaccion;
             }

         }

        
        private List<ConsultasModel> lstAC(List<SP_RIPS_JSON_ACResult> lstAC)
        {
            var modeloReturn = new List<ConsultasModel>();
            if (lstAC.Any())
            {
                modeloReturn = lstAC.ConvertAll(x => new ConsultasModel()
                {
                    causaMotivoAtencion = x.CODIGO_CAUSA,
                    codConsulta = x.CODIGO_CONSULTA,
                    codDiagnosticoPrincipal = x.DIAGNOSTICO_1 == "" ? null : x.DIAGNOSTICO_1,
                    codDiagnosticoRelacionado1 = x.DIAGNOSTICO_2 == "" ? null : x.DIAGNOSTICO_2,
                    codDiagnosticoRelacionado2 = x.DIAGNOSTICO_3 == "" ? null : x.DIAGNOSTICO_3,
                    codDiagnosticoRelacionado3 = x.DIAGNOSTICO_4 == "" ? null : x.DIAGNOSTICO_4,
                    codPrestador = x.CODIGO_PRESTADOR,
                    codServicio = x.CODSERVICIO != null ? Convert.ToInt32(x.CODSERVICIO ?? "0") : 0,
                    conceptoRecaudo = x.CONCEPTORECAUDO,
                    consecutivo = x.CONSECUTIVO ?? 0,
                    fechaInicioAtencion = (x.FECHA_TRAT ?? DateTime.MinValue).ToString("yyyy-MM-dd HH:mm"),
                    finalidadTecnologiaSalud = x.FINALIDADTECNOLOGIASALUD,
                    grupoServicios = x.GRUPOSERVICIOS,
                    modalidadGrupoServicioTecSal = x.MODALIDADGRUPOSERVICIOTECSAL,
                    numDocumentoIdentificacion = x.IDANAMNESIS_TEXTO,
                    numFEVPagoModerador = x.NUMFEVPAGOMODERADOR,
                    tipoDiagnosticoPrincipal = x.CODIGO_TIPO_DIAGNOSTICO,
                    tipoDocumentoIdentificacion = x.DOCUMENTO_IDENTIDAD,
                    valorPagoModerador = Convert.ToInt64(x.VALOR_CUOTA),
                    vrServicio = Convert.ToInt64(x.VALOR_CONSULTA)
                });

            }
            return modeloReturn;
        }



        private List<ProcedimientosModel> lstAP(List<SP_RIPS_JSON_APResult> lstAP)
        {
            var modeloReturn = new List<ProcedimientosModel>();
            if (lstAP.Any())
            {
                modeloReturn = lstAP.ConvertAll(x => new ProcedimientosModel()
                {
                    codComplicacion = x.DIAGNOSTICO3,
                    codDiagnosticoRelacionado = x.DIAGNOSTICO2,
                    codDiagnosticoPrincipal = x.DIAGNOSTICO1,
                    codProcedimiento = x.CODIGO_PROCEDIMIENTO,
                    idMIPRES = x.IDMIPRES,
                    numAutorizacion = x.NUMERO_AUTORIZACION,
                    codPrestador = x.CODIGO_PRESTADOR,
                    codServicio = x.CODSERVICIO != null ? Convert.ToInt32(x.CODSERVICIO ?? "0") : 0,
                    conceptoRecaudo = x.CONCEPTORECAUDO,
                    consecutivo = x.CONSECUTIVO ?? 0,
                    fechaInicioAtencion = (x.FECHAR_REALIZACION ?? DateTime.MinValue).ToString("yyyy-MM-dd HH:mm"),
                    finalidadTecnologiaSalud = x.FINALIDADTECNOLOGIASALUD,
                    grupoServicios = x.GRUPOSERVICIOS,
                    modalidadGrupoServicioTecSal = x.MODALIDADGRUPOSERVICIOTECSAL,
                    numDocumentoIdentificacion = x.IDANAMNESIS_TEXTO,
                    numFEVPagoModerador = x.NUMFEVPAGOMODERADOR,

                    tipoDocumentoIdentificacion = x.DOCUMENTO_IDENTIDAD,
                    valorPagoModerador = Convert.ToInt64(x.VALORPAGOMODERADOR),
                    viaIngresoServicioSalud = x.VIAINGRESOSERVICIOSALUD,
                    vrServicio = Convert.ToInt64(x.COSTO)
                });

            }
            return modeloReturn;
        }
    }
}
