using ServiceReference1;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ServicioRydentLocal.LogicaDelNegocio.Facturatech
{
    internal class FacturaTec
    {
        public MemoryStream ConsultarFactura(string prefijo, string numero, P_Adicionales_Abonos_Dian _P_Adicionales_Abonos_Dian)
        {
            if (_P_Adicionales_Abonos_Dian.FTAmbienteProduccion == "1")
            {
                WSV2PROSoapClient clientw = new WSV2PROSoapClient(WSV2PROSoapClient.EndpointConfiguration.WSV2PROSoap12, _P_Adicionales_Abonos_Dian.FTRutaWebService);
                //var request = new downloadPDFFileRequest();
                //request.Body.username = _P_Adicionales_Abonos_Dian.FTUsuario;
                //request.Body.password = _P_Adicionales_Abonos_Dian.FTPswWebService;
                //request.Body.prefijo = prefijo;
                //request.Body.folio = numero;
                var _response_Xml = clientw.downloadPDFFile(_P_Adicionales_Abonos_Dian.FTUsuario, _P_Adicionales_Abonos_Dian.FTPswWebService, prefijo, numero);

                if (!string.IsNullOrEmpty(_response_Xml.resourceData) && string.IsNullOrEmpty(_response_Xml.Msgerror))
                {
                    var s = TransferenciaArchivos.Base64ToStream(_response_Xml.resourceData);

                    MemoryStream archivoStream = new MemoryStream();
                    s.CopyTo(archivoStream);

                    archivoStream.Flush(); //Always catches me out
                    archivoStream.Position = 0; //Not sure if this is required
                    return archivoStream;
                }
                else return null;

            }
            else
            {
                WSV2PROSoapClient clientw = new WSV2PROSoapClient(WSV2PROSoapClient.EndpointConfiguration.WSV2PROSoap12, _P_Adicionales_Abonos_Dian.FTRutaWebService);

                //var request = new downloadPDFFileRequest();
                //request.Body.username = _P_Adicionales_Abonos_Dian.FTUsuario;
                //request.Body.password = _P_Adicionales_Abonos_Dian.FTPswWebService;
                //request.Body.prefijo = prefijo;
                //request.Body.folio = numero;
                var _response_Xml = clientw.downloadPDFFile(_P_Adicionales_Abonos_Dian.FTUsuario, _P_Adicionales_Abonos_Dian.FTPswWebService, prefijo, numero);
                if (!string.IsNullOrEmpty(_response_Xml.resourceData) && string.IsNullOrEmpty(_response_Xml.Msgerror))
                {
                    var s = TransferenciaArchivos.Base64ToStream(_response_Xml.resourceData);

                    MemoryStream archivoStream = new MemoryStream();
                    s.CopyTo(archivoStream);

                    archivoStream.Flush(); //Always catches me out
                    archivoStream.Position = 0; //Not sure if this is required
                    return archivoStream;
                }
                else return null;

            }
        }
        public string DescargarXMLFactura(string prefijo, string numero, P_Adicionales_Abonos_Dian _P_Adicionales_Abonos_Dian)
        {
            if (_P_Adicionales_Abonos_Dian.FTAmbienteProduccion == "1")
            {
                WSV2PROSoapClient clientw = new WSV2PROSoapClient(WSV2PROSoapClient.EndpointConfiguration.WSV2PROSoap12, _P_Adicionales_Abonos_Dian.FTRutaWebService);
                var _response_Xml = clientw.downloadXMLFile(_P_Adicionales_Abonos_Dian.FTUsuario, _P_Adicionales_Abonos_Dian.FTPswWebService, prefijo, numero);

                if (!string.IsNullOrEmpty(_response_Xml.resourceData) && string.IsNullOrEmpty(_response_Xml.Msgerror))
                {
                    return _response_Xml.resourceData;
                }
                else return null;

            }
            else
            {
                WSV2PROSoapClient clientw = new WSV2PROSoapClient(WSV2PROSoapClient.EndpointConfiguration.WSV2PROSoap12, _P_Adicionales_Abonos_Dian.FTRutaWebService);
                var _response_Xml = clientw.downloadXMLFile(_P_Adicionales_Abonos_Dian.FTUsuario, _P_Adicionales_Abonos_Dian.FTPswWebService, prefijo, numero);
                if (!string.IsNullOrEmpty(_response_Xml.resourceData) && string.IsNullOrEmpty(_response_Xml.Msgerror))
                {
                    return _response_Xml.resourceData;
                }
                else return null;

            }
        }

		public string DescargarBase64Factura(string prefijo, string numero, P_Adicionales_Abonos_Dian _P_Adicionales_Abonos_Dian)
		{
			var clientw = new WSV2PROSoapClient(
				WSV2PROSoapClient.EndpointConfiguration.WSV2PROSoap12,
				_P_Adicionales_Abonos_Dian.FTRutaWebService);

			using (new OperationContextScope(clientw.InnerChannel))
			{
				var httpRequest = new HttpRequestMessageProperty();
				httpRequest.Headers["User-Agent"] = "Rydent 2.0";
				OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequest;

				var _response_Xml = clientw.downloadXMLFile(
					_P_Adicionales_Abonos_Dian.FTUsuario,
					_P_Adicionales_Abonos_Dian.FTPswWebService,
					prefijo,
					numero);

				if (!string.IsNullOrEmpty(_response_Xml.resourceData) && string.IsNullOrEmpty(_response_Xml.Msgerror))
				{
					return _response_Xml.resourceData;
				}

				return null;
			}
		}

		/*public string DescargarXMLFactura(string prefijo, string numero, P_Adicionales_Abonos_Dian _P_Adicionales_Abonos_Dian)
        {
            // Crear el cliente SOAP solo una vez
            using (var clientw = new WSV2PROSoapClient(WSV2PROSoapClient.EndpointConfiguration.WSV2PROSoap12, _P_Adicionales_Abonos_Dian.FTRutaWebService))
            {
                var _response_Xml = clientw.downloadXMLFile(
                    _P_Adicionales_Abonos_Dian.FTUsuario,
                    _P_Adicionales_Abonos_Dian.FTPswWebService,
                    prefijo,
                    numero
                );

                return (!string.IsNullOrEmpty(_response_Xml.resourceData) && string.IsNullOrEmpty(_response_Xml.Msgerror))
                    ? _response_Xml.resourceData
                    : null;
            }
        }*/

		/*public async Task<string> DescargarXMLFactura(string prefijo, string numero, P_Adicionales_Abonos_Dian _P_Adicionales_Abonos_Dian)
        {
            using (var clientw = new WSV2PROSoapClient(WSV2PROSoapClient.EndpointConfiguration.WSV2PROSoap12, _P_Adicionales_Abonos_Dian.FTRutaWebService))
            {
                /*var response = await clientw.downloadXMLFileAsync(
                    _P_Adicionales_Abonos_Dian.FTUsuario,
                    _P_Adicionales_Abonos_Dian.FTPswWebService,
                    prefijo,
                    numero
                );*/
		/*var response = (await clientw.downloadXMLFileAsync(
			_P_Adicionales_Abonos_Dian.FTUsuario,
			_P_Adicionales_Abonos_Dian.FTPswWebService,
			prefijo,
			numero
		)).Body; // Accede al Body


		return (!string.IsNullOrEmpty(response.downloadXMLFileResult.resourceData) &&
				string.IsNullOrEmpty(response.downloadXMLFileResult.Msgerror))
				? response.downloadXMLFileResult.resourceData
				: null;
		//return (!string.IsNullOrEmpty(response.resourceData) && string.IsNullOrEmpty(response.Msgerror))
			//? response.resourceData
			//: null;
	}
}*/



		public NegMensaje GenerarFactura(byte[] bytes, P_Adicionales_Abonos_Dian _P_Adicionales_Abonos_Dian)
        {
            if (_P_Adicionales_Abonos_Dian.FTAmbienteProduccion == "1")
            {
                WSV2PROSoapClient clientw = new WSV2PROSoapClient(WSV2PROSoapClient.EndpointConfiguration.WSV2PROSoap12, _P_Adicionales_Abonos_Dian.FTRutaWebService);
                String file = Convert.ToBase64String(bytes);
                //var requestUpload = new uploadInvoiceFileRequest();
                //requestUpload.Body.username = _P_Adicionales_Abonos_Dian.FTUsuario;
                //requestUpload.Body.password = _P_Adicionales_Abonos_Dian.FTPswWebService;
                //requestUpload.Body.xmlBase64 = file;
                var uploadF = clientw.uploadInvoiceFile(_P_Adicionales_Abonos_Dian.FTUsuario, _P_Adicionales_Abonos_Dian.FTPswWebService, file);
                if (!string.IsNullOrEmpty(uploadF.Msgerror) && string.IsNullOrEmpty(uploadF.transaccionID))
                {
                    return new NegMensaje()
                    {
                        MensajeCorrecto = uploadF.success,
                        Status = "",
                        error = uploadF.Msgerror,
                        transaccionID = uploadF.transaccionID
                    };
                }
                //var requestDoc = new documentStatusFileRequest();
                //requestDoc.Body.username = _P_Adicionales_Abonos_Dian.FTUsuario;
                //requestDoc.Body.password = _P_Adicionales_Abonos_Dian.FTPswWebService;
                //requestDoc.Body.transaccionID = uploadF.transaccionID;
                var status = clientw.documentStatusFile(_P_Adicionales_Abonos_Dian.FTUsuario, _P_Adicionales_Abonos_Dian.FTPswWebService, uploadF.transaccionID);
                return new NegMensaje()
                {
                    MensajeCorrecto = status.success,
                    Status = status.status,
                    error = status.Msgerror,
                    transaccionID = uploadF.transaccionID
                };
            }
            else
            {
                WSV2PROSoapClient clientw = new WSV2PROSoapClient(WSV2PROSoapClient.EndpointConfiguration.WSV2PROSoap12, _P_Adicionales_Abonos_Dian.FTRutaWebService);
                String file = Convert.ToBase64String(bytes);
                //var requestUpload = new uploadInvoiceFileRequest();
                //requestUpload.Body.username = _P_Adicionales_Abonos_Dian.FTUsuario;
                //requestUpload.Body.password = _P_Adicionales_Abonos_Dian.FTPswWebService;
                //requestUpload.Body.xmlBase64 = file;
                var uploadF = clientw.uploadInvoiceFile(_P_Adicionales_Abonos_Dian.FTUsuario, _P_Adicionales_Abonos_Dian.FTPswWebService, file);
                if (string.IsNullOrEmpty(uploadF.transaccionID))
                {
                    return new NegMensaje()
                    {
                        MensajeCorrecto = uploadF.success,
                        Status = uploadF.success,
                        error = uploadF.Msgerror,
                        transaccionID = uploadF.transaccionID
                    };
                }
                //var requestDoc = new documentStatusFileRequest();
                //requestDoc.Body.username = _P_Adicionales_Abonos_Dian.FTUsuario;
                //requestDoc.Body.password = _P_Adicionales_Abonos_Dian.FTPswWebService;
                //requestDoc.Body.transaccionID = uploadF.transaccionID;
                var status = clientw.documentStatusFile(_P_Adicionales_Abonos_Dian.FTUsuario, _P_Adicionales_Abonos_Dian.FTPswWebService, uploadF.transaccionID);
                return new NegMensaje()
                {
                    MensajeCorrecto = status.success,
                    Status = status.status,
                    error = status.Msgerror,
                    transaccionID = uploadF.transaccionID
                };
            }
        }


        public NegMensaje GenerarFacturaCXC(byte[] bytes, P_Adicionales_Abonos_Dian _P_CXC_Dian)
        {
            if (_P_CXC_Dian.FTAmbienteProduccion == "1")
            {
                WSV2PROSoapClient clientw = new WSV2PROSoapClient(WSV2PROSoapClient.EndpointConfiguration.WSV2PROSoap12, _P_CXC_Dian.FTRutaWebService);
                String file = Convert.ToBase64String(bytes);


                //var requestUpload = new uploadInvoiceFileRequest();
                //requestUpload.Body.username = _P_CXC_Dian.FTUsuario;
                //requestUpload.Body.password = _P_CXC_Dian.FTPswWebService;
                //requestUpload.Body.xmlBase64 = file;
                var uploadF = clientw.uploadInvoiceFile(_P_CXC_Dian.FTUsuario, _P_CXC_Dian.FTPswWebService, file);
                if (!string.IsNullOrEmpty(uploadF.Msgerror) && string.IsNullOrEmpty(uploadF.transaccionID))
                {
                    return new NegMensaje()
                    {
                        MensajeCorrecto = uploadF.success,
                        Status = "",
                        error = uploadF.Msgerror,
                        transaccionID = uploadF.transaccionID
                    };
                }
                //var requestDoc = new documentStatusFileRequest();
                //requestDoc.Body.username = _P_CXC_Dian.FTUsuario;
                //requestDoc.Body.password = _P_CXC_Dian.FTPswWebService;
                //requestDoc.Body.transaccionID = uploadF.transaccionID;
                var status = clientw.documentStatusFile(_P_CXC_Dian.FTUsuario, _P_CXC_Dian.FTPswWebService, uploadF.transaccionID);
                return new NegMensaje()
                {
                    MensajeCorrecto = status.success,
                    Status = status.status,
                    error = status.Msgerror,
                    transaccionID = uploadF.transaccionID
                };
            }
            else
            {
                WSV2PROSoapClient clientw = new WSV2PROSoapClient(WSV2PROSoapClient.EndpointConfiguration.WSV2PROSoap12, _P_CXC_Dian.FTRutaWebService);
                String file = Convert.ToBase64String(bytes);

                //var requestUpload = new uploadInvoiceFileRequest();
                //requestUpload.Body.username = _P_CXC_Dian.FTUsuario;
                //requestUpload.Body.password = _P_CXC_Dian.FTPswWebService;
                //requestUpload.Body.xmlBase64 = file;
                var uploadF = clientw.uploadInvoiceFile(_P_CXC_Dian.FTUsuario, _P_CXC_Dian.FTPswWebService, file);
                if (string.IsNullOrEmpty(uploadF.transaccionID))
                {
                    return new NegMensaje()
                    {
                        MensajeCorrecto = uploadF.success,
                        Status = uploadF.success,
                        error = uploadF.Msgerror,
                        transaccionID = uploadF.transaccionID
                    };
                }
                var requestDoc = new documentStatusFileRequest();
                requestDoc.Body.username = _P_CXC_Dian.FTUsuario;
                requestDoc.Body.password = _P_CXC_Dian.FTPswWebService;
                requestDoc.Body.transaccionID = uploadF.transaccionID;
                var status = clientw.documentStatusFile(_P_CXC_Dian.FTUsuario, _P_CXC_Dian.FTPswWebService, uploadF.transaccionID);
                return new NegMensaje()
                {
                    MensajeCorrecto = status.success,
                    Status = status.status,
                    error = status.Msgerror,
                    transaccionID = uploadF.transaccionID
                };
            }
        }

        public byte[] formatoFactura(P_Adicionales_Abonos_Dian adicionalAbono, List<T_ADICIONALES_ABONOS_MOTIVOS> lstDetalles)
        {
            //string __valor = String.Format("{0:0.00}", adicionalAbono.valor).Replace(",",".");
            //string __valorIva= String.Format("{0:0.00}", adicionalAbono.ValorIva).Replace(",", ".");
            //string __ValorTotal = String.Format("{0:0.00}", (adicionalAbono.valor + adicionalAbono.ValorIva)).Replace(",", ".");
            //string retornar = "";
            //using (StringWriter sw = new StringWriter())
            //using (XmlTextWriter writer = new XmlTextWriter(sw))
            //{
            //	//
            //	writer.Formatting = Formatting.Indented; // if you want it indented

            //	writer.WriteStartDocument(); // <?xml version="1.0" encoding="utf-16"?>
            //	writer.WriteStartElement(Enumeraciones.TiposFacturatech.TipoEncabezadoXML.Factura); //<FACTURA>
            //	writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            //	writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");


            //	EstructurasAnotacion.EncSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono, _EntResolucion_Dian_Otros);
            //	EstructurasAnotacion.EmiSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono, _EntResolucion_Dian_Otros);
            //	EstructurasAnotacion.ADQSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono, _EntResolucion_Dian_Otros);
            //	EstructurasAnotacion.TOTSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono, _EntResolucion_Dian_Otros);
            //	EstructurasAnotacion.TIMSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono, _EntResolucion_Dian_Otros);
            //	EstructurasAnotacion.DRFSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono, _EntResolucion_Dian_Otros);
            //	EstructurasAnotacion.MEPSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono, _EntResolucion_Dian_Otros);
            //	EstructurasAnotacion.ITESeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono, _EntResolucion_Dian_Otros);

            //	writer.WriteEndElement(); //</FACTURA>
            //	writer.WriteEndDocument();
            //	retornar = sw.ToString();
            //}
            return System.Text.Encoding.Unicode.GetBytes(formatoStringFactura(adicionalAbono, lstDetalles));
        }

        public sealed class StringWriterWithEncoding : StringWriter
        {
            private readonly Encoding encoding;

            public StringWriterWithEncoding(Encoding encoding)
            {
                this.encoding = encoding;
            }

            public override Encoding Encoding
            {
                get { return encoding; }
            }
        }

        public string formatoStringFactura(P_Adicionales_Abonos_Dian adicionalAbono, List<T_ADICIONALES_ABONOS_MOTIVOS> lstDetalles)
        {
            string __valor = String.Format("{0:0.00}", adicionalAbono.valor).Replace(",", ".");
            string __valorIva = String.Format("{0:0.00}", adicionalAbono.ValorIva).Replace(",", ".");
            string __ValorTotal = String.Format("{0:0.00}", (adicionalAbono.valor + adicionalAbono.ValorIva)).Replace(",", ".");
            string retornar = "";
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8
            };
            using (StringWriter sw = new StringWriterWithEncoding(Encoding.UTF8))
            {
                using (XmlTextWriter writer = new XmlTextWriter(sw))
                {
                    //
                    writer.Formatting = Formatting.Indented; // if you want it indented

                    writer.WriteStartDocument(); // <?xml version="1.0" encoding="utf-16"?>
                                                 //writer.set. = settings;
                    writer.WriteStartElement(Enumeraciones.TiposFacturatech.TipoEncabezadoXML.Factura); //<FACTURA>
                    writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");


                    EstructurasAnotacion.EncSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono);
                    EstructurasAnotacion.EmiSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono);
                    EstructurasAnotacion.ADQSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono);
                    EstructurasAnotacion.TOTSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono);
                    EstructurasAnotacion.TIMSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono);
                    EstructurasAnotacion.DRFSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono);
                    EstructurasAnotacion.MEPSeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono);
                    if (lstDetalles.Any())
                    {
                        EstructurasAnotacion.ITESeccionDetalle(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono, lstDetalles);

                    }
                    else
                    {
                        EstructurasAnotacion.ITESeccion(writer, Enumeraciones.TiposFacturatech.TipoOperacion.Factura, adicionalAbono);

                    }

                    writer.WriteEndElement(); //</FACTURA>
                    writer.WriteEndDocument();
                    retornar = sw.ToString();
                }
            }

            return retornar;
        }
    }
}
