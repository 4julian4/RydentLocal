using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ServicioRydentLocal.LogicaDelNegocio.Facturatech
{
	public class EstructurasAnotacion
	{
		public static void EncSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////

			// <ENC>..</ENC>
			writer.WriteStartElement("ENC");


			// <ENC_1>..</ENC_1>
			writer.WriteStartElement("ENC_1");
			writer.WriteString("INVOIC");
			writer.WriteEndElement();


			// <ENC_2>..</ENC_2>
			writer.WriteStartElement("ENC_2");
			writer.WriteString(adicionalAbono.nit);
			writer.WriteEndElement();

			// <ENC_3>..</ENC_3>
			writer.WriteStartElement("ENC_3");
			writer.WriteString(adicionalAbono.idAnamnesis_Texto);
			writer.WriteEndElement();

			// <ENC_4>..</ENC_4>
			writer.WriteStartElement("ENC_4");
			writer.WriteString("UBL 2.1");
			writer.WriteEndElement();

			// <ENC_5>..</ENC_5>
			writer.WriteStartElement("ENC_5");
			writer.WriteString("DIAN 2.1");
			writer.WriteEndElement();

			// <ENC_6>..</ENC_6>
			writer.WriteStartElement("ENC_6");
			writer.WriteString(adicionalAbono.factura);
			writer.WriteEndElement();

			// <ENC_7>..</ENC_7>
			writer.WriteStartElement("ENC_7");
			writer.WriteString(adicionalAbono.fechaFactura.ToString("yyyy-MM-dd"));
			writer.WriteEndElement();

			// <ENC_8>..</ENC_8>
			writer.WriteStartElement("ENC_8");
			writer.WriteString(adicionalAbono.HoraFactura.ToString("HH:mm:ss") + "-05:00");
			writer.WriteEndElement();

			// <ENC_9>..</ENC_9>NO ES NUEVO PERO REVISAR
			//Tipo de Factura (01,03) Nota: Se define con valor 03 exclusivamente en el caso de ENC_21= SS02.
			//En el caso de los valore 01 aplica para los restantes ENC_21 = (SS01, SS07, SS06, SS05)

			//// ojo somos SS01 Consulta médica y odontológica
			writer.WriteStartElement("ENC_9");
			writer.WriteString("01");
			writer.WriteEndElement();

			// <ENC_10>..</ENC_10>
			writer.WriteStartElement("ENC_10");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();

			// <ENC_11>..</ENC_11> NUEVO REVISAR
			//Fecha y hora inicio del período facturado.
			//Nota: El formato solicitado será el siguiente YYYY - MM - DDTHH:MM: SS
			writer.WriteStartElement("ENC_11");
			writer.WriteString(adicionalAbono.fechaFacturaIni.ToString("yyyy-MM-dd") + "T" + adicionalAbono.HoraFactura.ToString("HH:mm:ss"));
			writer.WriteEndElement();

			// <ENC_12>..</ENC_12> NUEVO REVISAR
			//Fecha y hora fin del período facturado.
			//Nota: El formato solicitado será el siguiente YYYY - MM - DDTHH:MM: SS
			writer.WriteStartElement("ENC_12");
			writer.WriteString(adicionalAbono.fechaFactura.ToString("yyyy-MM-dd") + "T" + adicionalAbono.HoraFactura.ToString("HH:mm:ss"));
			writer.WriteEndElement();

			// <ENC_15>..</ENC_15> numero de lineas en el documento
			writer.WriteStartElement("ENC_15");
			writer.WriteString("1");
			writer.WriteEndElement();

			// <ENC_16>..</ENC_16>
			writer.WriteStartElement("ENC_16");
			writer.WriteString(adicionalAbono.fechaFactura.ToString("yyyy-MM-dd"));
			writer.WriteEndElement();

			// <ENC_20>..</ENC_20>
			writer.WriteStartElement("ENC_20");
			writer.WriteString(adicionalAbono.FTAmbienteProduccion);
			writer.WriteEndElement();

			// <ENC_21>..</ENC_21>NO ES NUEVO PERO REVISAR
			//Tipo de documento salud a usar.
			//Nota: Debe de contener un valor de la tabla 45 del simplificado.
			writer.WriteStartElement("ENC_21");
			writer.WriteString("SS06");
			writer.WriteEndElement();

			//</ENC>
			writer.WriteEndElement();
		}

		public static void EmiSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			// <EMI>..</EMI>
			writer.WriteStartElement("EMI");


			// <EMI_1>..</EMI_1>
			writer.WriteStartElement("EMI_1");
			writer.WriteString(adicionalAbono.tipoPersona);
			writer.WriteEndElement();

			// <EMI_2>..</EMI_2>
			writer.WriteStartElement("EMI_2");
			writer.WriteString(adicionalAbono.nit);
			writer.WriteEndElement();

			// <EMI_3>..</EMI_3>
			writer.WriteStartElement("EMI_3");
			writer.WriteString(adicionalAbono.identificadorFiscal);
			writer.WriteEndElement();

			// <EMI_4>..</EMI_4>
			writer.WriteStartElement("EMI_4");
			writer.WriteString(adicionalAbono.regimen);
			writer.WriteEndElement();

			// <EMI_6>..</EMI_6>
			writer.WriteStartElement("EMI_6");
			writer.WriteString(adicionalAbono.nombre);
			writer.WriteEndElement();

			// <EMI_7>..</EMI_7>
			writer.WriteStartElement("EMI_7");
			writer.WriteString(adicionalAbono.nombre);
			writer.WriteEndElement();


			// <EMI_10>..</EMI_10>
			writer.WriteStartElement("EMI_10");
			writer.WriteString(adicionalAbono.direccion);
			writer.WriteEndElement();

			// <EMI_11>..</EMI_11>
			writer.WriteStartElement("EMI_11");
			writer.WriteString(adicionalAbono.codigoDepartamento);
			writer.WriteEndElement();

			// <EMI_13>..</EMI_13>
			writer.WriteStartElement("EMI_13");
			writer.WriteString(adicionalAbono.ciudad);
			writer.WriteEndElement();

			// <EMI_14>..</EMI_14>
			writer.WriteStartElement("EMI_14");
			writer.WriteString(adicionalAbono.codigoCiudad);
			writer.WriteEndElement();

			// <EMI_15>..</EMI_15>
			writer.WriteStartElement("EMI_15");
			writer.WriteString(adicionalAbono.codigoPais);
			writer.WriteEndElement();

			// <EMI_19>..</EMI_19>
			writer.WriteStartElement("EMI_19");
			writer.WriteString(adicionalAbono.departamento);
			writer.WriteEndElement();

			// <EMI_21>..</EMI_21>
			writer.WriteStartElement("EMI_21");
			writer.WriteString(adicionalAbono.pais);
			writer.WriteEndElement();

			// <EMI_22>..</EMI_22>
			writer.WriteStartElement("EMI_22");
			writer.WriteString(Encriptacion.CalcularDigitoVerificacion(adicionalAbono.nit, "31"));
			writer.WriteEndElement();

			// <EMI_23>..</EMI_23>
			writer.WriteStartElement("EMI_23");
			writer.WriteString(adicionalAbono.codigoCiudad);
			writer.WriteEndElement();

			// <EMI_24>..</EMI_24>
			writer.WriteStartElement("EMI_24");
			writer.WriteString(adicionalAbono.nombre);
			writer.WriteEndElement();


			if (TipoOperacion == Enumeraciones.TiposFacturatech.TipoOperacion.NC)
			{
				// <EMI_25>..</EMI_25>
				writer.WriteStartElement("EMI_25");
				writer.WriteString(adicionalAbono.obligacionContribuyente);
				writer.WriteEndElement();
			}


			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////


			TacSeccion(writer, TipoOperacion, adicionalAbono);
			DFESeccion(writer, TipoOperacion, adicionalAbono);
			ICCSeccion(writer, TipoOperacion, adicionalAbono);
			CdeSeccion(writer, TipoOperacion, adicionalAbono);
			GteSeccion(writer, TipoOperacion, adicionalAbono);



			//</EMI>
			writer.WriteEndElement();
		}

		////////////////////////////    //////////////////////////////////////////////////// ///////////////////////////////////// //////////////////////////////////////////////////// 
		/// <summary>
		/// Secciones dentro de la sección Emi
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="TipoOperacion"></param>
		/// <param name="adicionalAbono"></param>
		/// <param name="adicionalAbono"></param>
		public static void TacSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			// <TAC>..</TAC>
			writer.WriteStartElement("TAC");


			// <TAC_1>..</TAC_1>
			writer.WriteStartElement("TAC_1");
			writer.WriteString(adicionalAbono.obligacionContribuyente);
			writer.WriteEndElement();


			//</TAC>
			writer.WriteEndElement();


			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////

		}
		public static void DFESeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			// <DFE>..</DFE>
			writer.WriteStartElement("DFE");


			// <DFE_1>..</DFE_1>
			writer.WriteStartElement("DFE_1");
			writer.WriteString(adicionalAbono.codigoCiudad);
			writer.WriteEndElement();

			// <DFE_2>..</DFE_2>
			writer.WriteStartElement("DFE_2");
			writer.WriteString(adicionalAbono.codigoDepartamento);
			writer.WriteEndElement();

			// <DFE_3>..</DFE_3>
			writer.WriteStartElement("DFE_3");
			writer.WriteString(adicionalAbono.codigoPais);
			writer.WriteEndElement();

			// <DFE_4>..</DFE_4>
			writer.WriteStartElement("DFE_4");
			writer.WriteString(adicionalAbono.codigoCiudad);
			writer.WriteEndElement();

			// <DFE_5>..</DFE_5>
			writer.WriteStartElement("DFE_5");
			writer.WriteString(adicionalAbono.pais);
			writer.WriteEndElement();

			// <DFE_6>..</DFE_6>
			writer.WriteStartElement("DFE_6");
			writer.WriteString(adicionalAbono.departamento);
			writer.WriteEndElement();

			// <DFE_7>..</DFE_7>
			writer.WriteStartElement("DFE_7");
			writer.WriteString(adicionalAbono.ciudad);
			writer.WriteEndElement();

			// <DFE_8>..</DFE_8>
			writer.WriteStartElement("DFE_8");
			writer.WriteString(adicionalAbono.direccion);
			writer.WriteEndElement();


			//</DFE>
			writer.WriteEndElement();


			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////

		}
		public static void ICCSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{

			// <ICC>..</ICC>
			writer.WriteStartElement("ICC");


			// <ICC_1>..</ICC_1>
			writer.WriteStartElement("ICC_1");
			writer.WriteString(adicionalAbono.matriculaMercantil);
			writer.WriteEndElement();

			// <ICC_9>..</ICC_9>
			writer.WriteStartElement("ICC_9");
			writer.WriteString(adicionalAbono.prefijo);
			writer.WriteEndElement();


			//</ICC>
			writer.WriteEndElement();

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////

		}
		public static void CdeSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{

			// <CDE>..</CDE>
			writer.WriteStartElement("CDE");


			// <CDE_1>..</CDE_1>
			writer.WriteStartElement("CDE_1");
			writer.WriteString("1");
			writer.WriteEndElement();

			// <CDE_2>..</CDE_2>
			writer.WriteStartElement("CDE_2");
			writer.WriteString(adicionalAbono.nombre);
			writer.WriteEndElement();

			// <CDE_3>..</CDE_3>
			writer.WriteStartElement("CDE_3");
			writer.WriteString(adicionalAbono.telefono);
			writer.WriteEndElement();

			// <CDE_4>..</CDE_4>
			writer.WriteStartElement("CDE_4");
			writer.WriteString(adicionalAbono.correo);
			writer.WriteEndElement();


			//</CDE>
			writer.WriteEndElement();

			//if (TipoOperacion == Enumeraciones.TiposFacturatech.TipoOperacion.NC)
			//{
			//	// <CDE>..</CDE>
			//	writer.WriteStartElement("CDE");


			//	// <CDE_1>..</CDE_1>
			//	writer.WriteStartElement("CDE_1");
			//	writer.WriteString("4");
			//	writer.WriteEndElement();

			//	// <CDE_2>..</CDE_2>
			//	writer.WriteStartElement("CDE_2");
			//	writer.WriteString("Ventas.");
			//	writer.WriteEndElement();
			//	//</CDE>
			//	writer.WriteEndElement();

			//}

		}
		public static void GteSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			// <GTE>..</GTE>
			writer.WriteStartElement("GTE");


			// <GTE_1>..</GTE_1>
			writer.WriteStartElement("GTE_1");
			writer.WriteString(adicionalAbono.CodigoImpuesto);
			writer.WriteEndElement();

			// <GTE_2>..</GTE_2>
			writer.WriteStartElement("GTE_2");
			writer.WriteString(adicionalAbono.NombreImpuesto);
			writer.WriteEndElement();


			//</GTE>
			writer.WriteEndElement();

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		}

		////////////////////////////    //////////////////////////////////////////////////// ///////////////////////////////////// //////////////////////////////////////////////////// 
		public static void ADQSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			// <ADQ>..</ADQ>
			writer.WriteStartElement("ADQ");


			// <ADQ_1>..</ADQ_1>
			writer.WriteStartElement("ADQ_1");
			writer.WriteString("2");
			writer.WriteEndElement();

			// <ADQ_2>..</ADQ_2>
			writer.WriteStartElement("ADQ_2");
			writer.WriteString(adicionalAbono.idAnamnesis_Texto);
			writer.WriteEndElement();

			// <ADQ_3>..</ADQ_3>
			writer.WriteStartElement("ADQ_3");
			writer.WriteString(adicionalAbono.Tipo_Documento_Paciente);  //13 CC -- 31 = NIT      --   41 PA
			writer.WriteEndElement();

			// <ADQ_4>..</ADQ_4>
			writer.WriteStartElement("ADQ_4");
			writer.WriteString(adicionalAbono.regimen); // No responsable
			writer.WriteEndElement();


			// <ADQ_6>..</ADQ_6>
			writer.WriteStartElement("ADQ_6");
			writer.WriteString(adicionalAbono.nombre_Paciente);
			writer.WriteEndElement();

			// <ADQ_7>..</ADQ_7>
			writer.WriteStartElement("ADQ_7");
			writer.WriteString(adicionalAbono.nombre_Paciente);
			writer.WriteEndElement();

			// <ADQ_8>..</ADQ_8>
			writer.WriteStartElement("ADQ_8");
			writer.WriteString(adicionalAbono.nombre_Paciente);
			writer.WriteEndElement();

			// <ADQ_9>..</ADQ_9>
			writer.WriteStartElement("ADQ_9");
			writer.WriteString(adicionalAbono.nombre_Paciente);
			writer.WriteEndElement();

			// <ADQ_10>..</ADQ_10>
			writer.WriteStartElement("ADQ_10");
			writer.WriteString(adicionalAbono.direccion_Paciente);
			writer.WriteEndElement();


			// <ADQ_11>..</ADQ_11>
			writer.WriteStartElement("ADQ_11");
			writer.WriteString(adicionalAbono.codigoDepartamento);
			writer.WriteEndElement();

			// <ADQ_13>..</ADQ_13>
			writer.WriteStartElement("ADQ_13");
			writer.WriteString(adicionalAbono.Ciudad_Paciente);
			writer.WriteEndElement();

			// <ADQ_14>..</ADQ_14>
			writer.WriteStartElement("ADQ_14");
			writer.WriteString(adicionalAbono.codigoCiudad_Paciente);
			writer.WriteEndElement();

			// <ADQ_15>..</ADQ_15>
			writer.WriteStartElement("ADQ_15");
			writer.WriteString(adicionalAbono.codigoPais_Paciente);
			writer.WriteEndElement();

			// <ADQ_19>..</ADQ_19>
			writer.WriteStartElement("ADQ_19");
			writer.WriteString(adicionalAbono.departamento_Paciente);
			writer.WriteEndElement();

			// <ADQ_21>..</ADQ_21>
			writer.WriteStartElement("ADQ_21");
			writer.WriteString(adicionalAbono.pais_Paciente);
			writer.WriteEndElement();

			// <ADQ_22>..</ADQ_22>
			writer.WriteStartElement("ADQ_22");
			writer.WriteString(Encriptacion.CalcularDigitoVerificacion(adicionalAbono.idAnamnesis_Texto, adicionalAbono.Tipo_Documento_Paciente));
			writer.WriteEndElement();

			// <ADQ_23>..</ADQ_23>
			writer.WriteStartElement("ADQ_23");
			writer.WriteString(adicionalAbono.codigoCiudad_Paciente);
			writer.WriteEndElement();


			TCRSeccion(writer, TipoOperacion, adicionalAbono);
			ILASeccion(writer, TipoOperacion, adicionalAbono);
			DFASeccion(writer, TipoOperacion, adicionalAbono);
			ICRSeccion(writer, TipoOperacion, adicionalAbono);
			//if (Enumeraciones.TiposFacturatech.TipoOperacion.Factura == TipoOperacion)
			//{
			//	ICRSeccion(writer, TipoOperacion, adicionalAbono);
			//}
			CDASeccion(writer, TipoOperacion, adicionalAbono);
			GTASeccion(writer, TipoOperacion, adicionalAbono);

			//</ADQ>
			writer.WriteEndElement();
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static void TCRSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			// <TCR>..</TCR>
			writer.WriteStartElement("TCR");


			// <TCR_1>..</TCR_1>
			writer.WriteStartElement("TCR_1");
			writer.WriteString(adicionalAbono.obligacionContribuyente);
			writer.WriteEndElement();


			//</TCR>
			writer.WriteEndElement();

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////

		}
		public static void ILASeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			// <ILA>..</ILA>
			writer.WriteStartElement("ILA");


			// <ILA_1>..</ILA_1>
			writer.WriteStartElement("ILA_1");
			writer.WriteString(adicionalAbono.nombre);
			writer.WriteEndElement();

			// <ILA_2>..</ILA_2>
			writer.WriteStartElement("ILA_2");
			writer.WriteString(adicionalAbono.idAnamnesis_Texto);
			writer.WriteEndElement();

			// <ILA_3>..</ILA_3>
			writer.WriteStartElement("ILA_3");
			writer.WriteString(adicionalAbono.Tipo_Documento_Paciente);  //CC = 13 NIT = 31  PA = 41
			writer.WriteEndElement();

			// <ILA_4>..</ILA_4>
			writer.WriteStartElement("ILA_4");
			writer.WriteString(Encriptacion.CalcularDigitoVerificacion(adicionalAbono.idAnamnesis_Texto, adicionalAbono.Tipo_Documento_Paciente));
			writer.WriteEndElement();


			//</ILA>
			writer.WriteEndElement();

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		}

		public static void DFASeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{

			// <DFA>..</DFA>
			writer.WriteStartElement("DFA");


			// <DFA_1>..</DFA_1>
			writer.WriteStartElement("DFA_1");
			writer.WriteString(adicionalAbono.codigoPais_Paciente);
			writer.WriteEndElement();

			// <DFA_2>..</DFA_2>
			writer.WriteStartElement("DFA_2");
			writer.WriteString(adicionalAbono.codigoDepartamento_Paciente);
			writer.WriteEndElement();

			// <DFA_3>..</DFA_3>
			writer.WriteStartElement("DFA_3");
			writer.WriteString(adicionalAbono.codigoCiudad_Paciente);
			writer.WriteEndElement();

			// <DFA_4>..</DFA_4>
			writer.WriteStartElement("DFA_4");
			writer.WriteString(adicionalAbono.codigoCiudad_Paciente);
			writer.WriteEndElement();

			// <DFA_5>..</DFA_5>
			writer.WriteStartElement("DFA_5");
			writer.WriteString(adicionalAbono.pais_Paciente);
			writer.WriteEndElement();

			// <DFA_6>..</DFA_6>
			writer.WriteStartElement("DFA_6");
			writer.WriteString(adicionalAbono.departamento_Paciente);
			writer.WriteEndElement();

			// <DFA_7>..</DFA_7>
			writer.WriteStartElement("DFA_7");
			writer.WriteString(adicionalAbono.Ciudad_Paciente);
			writer.WriteEndElement();

			// <DFA_8>..</DFA_8>
			writer.WriteStartElement("DFA_8");
			writer.WriteString(adicionalAbono.direccion_Paciente);
			writer.WriteEndElement();


			//</DFA>
			writer.WriteEndElement();


			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////}
		}

		public static void ICRSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			if (!string.IsNullOrEmpty(adicionalAbono.matriculaMercantil))
			{
				// <ICR>..</ICR>
				writer.WriteStartElement("ICR");


				// <ICR_1>..</ICR_1>
				writer.WriteStartElement("ICR_1");
				writer.WriteString(adicionalAbono.matriculaMercantil);
				writer.WriteEndElement();


				//</ICR>
				writer.WriteEndElement();

				/////////////////////////////////////////////////////////////////////////////////////////////////////////////
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			}


		}

		public static void CDASeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			// <CDA>..</CDA>
			writer.WriteStartElement("CDA");


			// <CDA_1>..</CDA_1>
			writer.WriteStartElement("CDA_1");
			writer.WriteString("1");
			writer.WriteEndElement();

			// <CDA_2>..</CDA_2>
			writer.WriteStartElement("CDA_2");
			writer.WriteString(adicionalAbono.nombre_Paciente);
			writer.WriteEndElement();

			// <CDA_3>..</CDA_3>
			writer.WriteStartElement("CDA_3");
			writer.WriteString(adicionalAbono.telf_P);
			writer.WriteEndElement();

			// <CDA_4>..</CDA_4>
			writer.WriteStartElement("CDA_4");
			if (Encriptacion.ValidEmail(adicionalAbono.correo_Paciente))
			{
				writer.WriteString(adicionalAbono.correo_Paciente);
			}
			else
			{
				writer.WriteString(adicionalAbono.correo);
			}
			writer.WriteEndElement();


			//</CDA>
			writer.WriteEndElement();

		}
		public static void GTASeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{

			// <GTA>..</GTA>
			writer.WriteStartElement("GTA");


			// <GTA_1>..</GTA_1>
			writer.WriteStartElement("GTA_1");
			writer.WriteString(adicionalAbono.CodigoImpuesto);
			writer.WriteEndElement();

			// <GTA_2>..</GTA_2>
			writer.WriteStartElement("GTA_2");
			writer.WriteString(adicionalAbono.NombreImpuesto);
			writer.WriteEndElement();

			//</GTA>
			writer.WriteEndElement();

		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void TOTSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			string __valor = String.Format("{0:0.00}", adicionalAbono.valor).Replace(",", ".");
			string __valorIva = String.Format("{0:0.00}", adicionalAbono.ValorIva).Replace(",", ".");
			string __ValorTotal = String.Format("{0:0.00}", (adicionalAbono.valor + adicionalAbono.ValorIva)).Replace(",", ".");
			// <TOT>..</TOT>
			writer.WriteStartElement("TOT");


			// <TOT_1>..</TOT_1>
			writer.WriteStartElement("TOT_1");
			writer.WriteString(__valor);
			writer.WriteEndElement();

			// <TOT_2>..</TOT_2>
			writer.WriteStartElement("TOT_2");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();

			// <TOT_3>..</TOT_3>
			writer.WriteStartElement("TOT_3");
			writer.WriteString(__valor);
			//if (adicionalAbono.ValorIva > 0)
			//{
			//	writer.WriteString(__valor);
			//}
			//else
			//{
			//	writer.WriteString("0.00");
			//}
			writer.WriteEndElement();

			// <TOT_4>..</TOT_4>
			writer.WriteStartElement("TOT_4");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();

			// <TOT_5>..</TOT_5>  NO ES NUEVO PERO REVISAR
			//Total de la factura
			// Nota: En el caso de ENC_21=SS01, SS02 el valor del total de la factura se calculará de la sig forma.
			//TOT_5 = TOT_7 - Descuentos a nivel total(DSC) + Cargos a nivel total(DSC) - Anticipos(ANT)
			//En el caso de ENC_21 = SS05, SS06, SS07 el valor del total de la factura se calculará de la sig forma.
			//TOT_5 = TOT_7 - Descuentos a nivel total(DSC) + Cargos a nivel total(DSC)

			// ojo somos SS01 Consulta médica y odontológica
			writer.WriteStartElement("TOT_5");
			writer.WriteString(__ValorTotal);
			writer.WriteEndElement();

			// <TOT_6>..</TOT_6>
			writer.WriteStartElement("TOT_6");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();

			// <TOT_7>..</TOT_7>
			writer.WriteStartElement("TOT_7");
			writer.WriteString(__ValorTotal);
			writer.WriteEndElement();

			// <TOT_8>..</TOT_8>
			writer.WriteStartElement("TOT_8");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();


			// <TOT_13>..</TOT_13> ES NUEVO REVISAR
			//Total del Anticipo
			//Nota: Aplica para tipos de operación(SS - CUDE, SS - CUFE, SS - Reporte) es decir cuando
			//ENC_21 = SS01 ó SS02 ó SS05
			/*writer.WriteStartElement("TOT_13");
            writer.WriteString("0");
            writer.WriteEndElement();*/

			//<TOT_14>..</TOT_14> ES NUEVO REVISAR
			//Tipo de Moneda(Tabla 13).
			//Nota: Aplica para tipos de operación(SS - CUDE, SS - CUFE, SS - Reporte) es decir cuando ENC_21 = SS01 ó SS02 ó SS05
			/*writer.WriteStartElement("TOT_14");
            writer.WriteString(adicionalAbono.codigoMoneda);// este esta bien revisado
            writer.WriteEndElement();*/

			//</TOT>
			writer.WriteEndElement();

		}



		public static void TIMSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{// <TIM>..</TIM>
			string __valor = String.Format("{0:0.00}", adicionalAbono.valor).Replace(",", ".");
			string __valorIva = String.Format("{0:0.00}", adicionalAbono.ValorIva).Replace(",", ".");
			string __ValorTotal = String.Format("{0:0.00}", (adicionalAbono.valor + adicionalAbono.ValorIva)).Replace(",", ".");
			writer.WriteStartElement("TIM");


			// <TIM_1>..</TIM_1>
			writer.WriteStartElement("TIM_1");
			writer.WriteString("false");
			writer.WriteEndElement();

			// <TIM_2>..</TIM_2>
			writer.WriteStartElement("TIM_2");
			writer.WriteString(__valorIva);
			writer.WriteEndElement();

			// <TIM_3>..</TIM_3>
			writer.WriteStartElement("TIM_3");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();

			IMPSeccion(writer, TipoOperacion, adicionalAbono);
			if (adicionalAbono.ValorIva > 0)
			{


				/////////////////////////////////////////////////////////////////////////////////////////////////////////////
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			}

			//</TIM>
			writer.WriteEndElement();

		}

		//OJO ESTA PARTE ANT SOLO APLICA PARA COPAGOS Y CUOTAS MODERADORAS POR ESO NO LO PUSIMOS
		//Nota: Mandatorio para tipos de operación (SS-CUDE, SS-CUFE, SS-Reporte) es decir cuando
		//ENC_21=SS01 ó SS02 ó SS05
		//ANT es todo nuevo no estaba en la estructura anterior REVISAR TODO ANT
		/*  public static void ANTSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		  {// <ANT>..</ANT>
			  string __valor = String.Format("{0:0.00}", adicionalAbono.valor).Replace(",", ".");
			  string __valorIva = String.Format("{0:0.00}", adicionalAbono.ValorIva).Replace(",", ".");
			  string __ValorTotal = String.Format("{0:0.00}", (adicionalAbono.valor + adicionalAbono.ValorIva)).Replace(",", ".");
			  writer.WriteStartElement("ANT");


			  // <ANT_1>..</ANT_1>
			  //Cantidad total acreditar a esta factura que se libra a la ERP/EPS.
			  writer.WriteStartElement("ANT_1");
			  writer.WriteString("0");
			  writer.WriteEndElement();

			  // <ANT_2>..</ANT_2>
			  //Código de moneda. Debe reportarse el Literal "COP".
			  writer.WriteStartElement("ANT_2");
			  writer.WriteString(adicionalAbono.codigoMoneda);
			  writer.WriteEndElement();

			  // <ANT_3>..</ANT_3>
			  // Fecha en la cual el pago fue realizado.
			  writer.WriteStartElement("ANT_3");
			  writer.WriteString(adicionalAbono.fechaFactura.ToString("yyyy-MM-dd"));
			  writer.WriteEndElement();

			  // <ANT_4>..</ANT_4>
			  // Hora en que se realizó el pagó Nota: En formato HH:MM:SS. Nota: En formato HH:MM:SS.
			  //HH: hora UTC(número de horas contadas desde la media noche, o sea, de 00 hasta 23) MM: minutos  SS: segundos
			  writer.WriteStartElement("ANT_4");
			  writer.WriteString(adicionalAbono.HoraFactura.ToString("HH:mm:ss") + "-05:00");
			  writer.WriteEndElement();

			  // <ANT_5>..</ANT_5>
			  //Identificación del Pago.
			  writer.WriteStartElement("ANT_5");
			  writer.WriteString(adicionalAbono.codigoMoneda);
			  writer.WriteEndElement();

			  // <ANT_6>..</ANT_6>
			  //Fecha en la que el pago fue recibido Nota: En formato AA - MM - DD
			  writer.WriteStartElement("ANT_6");
			  writer.WriteString("0.00");
			  writer.WriteEndElement();

			  // <ANT_7>..</ANT_7>
			  //Instrucciones relativas al pago
			  writer.WriteStartElement("ANT_7");
			  writer.WriteString(adicionalAbono.codigoMoneda);
			  writer.WriteEndElement();

			  // <ANT_8>..</ANT_8>
			  //Código de Identificador de concepto de recaudo. (Tabla 50)
			  writer.WriteStartElement("ANT_8");
			  writer.WriteString("0.00");
			  writer.WriteEndElement();

			  IMPSeccion(writer, TipoOperacion, adicionalAbono);
			  if (adicionalAbono.ValorIva > 0)
			  {


				  /////////////////////////////////////////////////////////////////////////////////////////////////////////////
				  /////////////////////////////////////////////////////////////////////////////////////////////////////////////
			  }

			  //</ANT>
			  writer.WriteEndElement();

		  }*/
		public static void IMPSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			string __valor = String.Format("{0:0.00}", adicionalAbono.valor).Replace(",", ".");
			string __valorIva = String.Format("{0:0.00}", adicionalAbono.ValorIva).Replace(",", ".");
			string __ValorTotal = String.Format("{0:0.00}", (adicionalAbono.valor + adicionalAbono.ValorIva)).Replace(",", ".");
			string _PorcentajeIva = String.Format("{0:0.00}", adicionalAbono.PorcentajeIva).Replace(",", ".");

			// <IMP>..</IMP>
			writer.WriteStartElement("IMP");

			// <IMP_1>..</IMP_1>
			writer.WriteStartElement("IMP_1");
			writer.WriteString(adicionalAbono.CodigoImpuesto);
			writer.WriteEndElement();

			// <IMP_2>..</IMP_2>
			writer.WriteStartElement("IMP_2");
			writer.WriteString(__valor);
			writer.WriteEndElement();

			// <IMP_3>..</IMP_3>
			writer.WriteStartElement("IMP_3");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();

			// <IMP_4>..</IMP_4>
			writer.WriteStartElement("IMP_4");
			writer.WriteString(__valorIva);
			writer.WriteEndElement();

			// <IMP_5>..</IMP_5>
			writer.WriteStartElement("IMP_5");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();

			// <IMP_6>..</IMP_6>
			writer.WriteStartElement("IMP_6");
			writer.WriteString(_PorcentajeIva);
			writer.WriteEndElement();

			if (Enumeraciones.TiposFacturatech.TipoOperacion.Factura == TipoOperacion && adicionalAbono.ValorIva > 0)
			{
				// <IMP_10>..</IMP_10>
				writer.WriteStartElement("IMP_10");
				writer.WriteString(adicionalAbono.codigoMoneda);
				writer.WriteEndElement();
			}

			//</IMP>
			writer.WriteEndElement();

		}

		public static void DRFSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			// <DRF>..</DRF>
			writer.WriteStartElement("DRF");


			// <DRF_1>..</DRF_1>
			writer.WriteStartElement("DRF_1");
			writer.WriteString(adicionalAbono.numeroAutorizacionFacturacion);
			writer.WriteEndElement();

			// <DRF_2>..</DRF_2>
			writer.WriteStartElement("DRF_2");
			writer.WriteString(adicionalAbono.fechaResolucion.ToString("yyyy-MM-dd"));
			writer.WriteEndElement();

			// <DRF_3>..</DRF_3>
			writer.WriteStartElement("DRF_3");
			writer.WriteString(adicionalAbono.fechaResolucionFin.ToString("yyyy-MM-dd"));
			writer.WriteEndElement();

			// <DRF_4>..</DRF_4>
			writer.WriteStartElement("DRF_4");
			writer.WriteString(adicionalAbono.prefijo);
			writer.WriteEndElement();

			// <DRF_5>..</DRF_5>
			writer.WriteStartElement("DRF_5");
			writer.WriteString(adicionalAbono.rangoIni.ToString());
			writer.WriteEndElement();

			// <DRF_6>..</DRF_6>
			writer.WriteStartElement("DRF_6");
			writer.WriteString(adicionalAbono.rangoFin.ToString());
			writer.WriteEndElement();



			//</DRF>
			writer.WriteEndElement();

		}


		public static void MEPSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			// <MEP>..</MEP>
			writer.WriteStartElement("MEP");


			// <MEP_1>..</MEP_1>
			writer.WriteStartElement("MEP_1");
			writer.WriteString(adicionalAbono.FormaPago);   //Forma de pago efectivo
			writer.WriteEndElement();


			// <MEP_2>..</MEP_2>
			writer.WriteStartElement("MEP_2");
			writer.WriteString("1");
			writer.WriteEndElement();


			//</MEP>
			writer.WriteEndElement();

		}


		public static void ITESeccionSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono,
			decimal valorUnitarioSinIva, decimal valorIva, decimal ValorTotalMasIva, decimal PorcentajeIva, string numero = "1", int cantidad = 1, string descripcion = "", string strCodigo = "")
		{
			string sValorUnitarioSinIva = String.Format("{0:0.00}", valorUnitarioSinIva).Replace(",", ".");
			string sValorIva = String.Format("{0:0.00}", valorIva).Replace(",", ".");
			string sValorTotalUnitarioSinIva = String.Format("{0:0.00}", valorUnitarioSinIva * cantidad).Replace(",", ".");
			string sValorTotalConIvaXCantidad = String.Format("{0:0.00}", ValorTotalMasIva * cantidad).Replace(",", ".");
			string sPorcentajeIva = String.Format("{0:0.00}", PorcentajeIva).Replace(",", ".");

			// <ITE>..</ITE>
			writer.WriteStartElement("ITE");


			// <ITE_1>..</ITE_1>
			writer.WriteStartElement("ITE_1");
			writer.WriteString(numero);
			writer.WriteEndElement();



			// <ITE_3>..</ITE_3>
			writer.WriteStartElement("ITE_3");
			writer.WriteString(cantidad.ToString());
			writer.WriteEndElement();

			// <ITE_4>..</ITE_4>
			writer.WriteStartElement("ITE_4");
			writer.WriteString("94");
			writer.WriteEndElement();

			// <ITE_5>..</ITE_5>
			writer.WriteStartElement("ITE_5");
			writer.WriteString(sValorTotalUnitarioSinIva);
			writer.WriteEndElement();


			// <ITE_6>..</ITE_6>
			writer.WriteStartElement("ITE_6");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();

			// <ITE_7>..</ITE_7>
			writer.WriteStartElement("ITE_7");
			writer.WriteString(sValorUnitarioSinIva);
			writer.WriteEndElement();


			// <ITE_8>..</ITE_8>
			writer.WriteStartElement("ITE_8");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();



			//En el caso de los nodos ITE_11,12,13 en comprobantes de salud tendrán el siguiente
			//rol.Descripción del artículo o servicio a que se refiere esta línea de la
			//factura / nota(Mandatorio).  REVISAR ITE_11,12,13	

			// <ITE_11>..</ITE_11>
			writer.WriteStartElement("ITE_11");
			writer.WriteString(adicionalAbono.descripcion);
			writer.WriteEndElement();

			/*// <ITE_12>..</ITE_12>  estos son opcionales
			writer.WriteStartElement("ITE_12");
			writer.WriteString(!string.IsNullOrEmpty(descripcion) ? descripcion : adicionalAbono.descripcion);
			writer.WriteEndElement();

            // <ITE_13>..</ITE_13>
            writer.WriteStartElement("ITE_13");
            writer.WriteString(!string.IsNullOrEmpty(descripcion) ? descripcion : adicionalAbono.descripcion);
            writer.WriteEndElement();*/
			//---------------------------------------------------------------------------------------------------------//

			// <ITE_17>..</ITE_17>
			if (strCodigo != "")
			{
				writer.WriteStartElement("ITE_17");
				writer.WriteString(strCodigo);
				writer.WriteEndElement();
			}

			// <ITE_19>..</ITE_19>
			//(ITE_19 = (ITE 27*ITE_7)-Descuentos a nivel Ítem (IDE) +Cargos a nivel Ítem (IDE)) --Total del ítem (incluyendo Descuentos y cargos) *
			writer.WriteStartElement("ITE_19");
			writer.WriteString(sValorTotalUnitarioSinIva);
			writer.WriteEndElement();


			// <ITE_20>..</ITE_20>
			writer.WriteStartElement("ITE_20");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();


			// <ITE_21>..</ITE_21>
			//(ITE_21= ITE_19+Suma de todos los Impuestos (IIM_2) a nivel Ítem)   ---  Valor a Pagar del Item*
			writer.WriteStartElement("ITE_21");
			writer.WriteString(sValorTotalConIvaXCantidad);
			writer.WriteEndElement();


			// <ITE_22>..</ITE_22>
			writer.WriteStartElement("ITE_22");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();


			// <ITE_23>..</ITE_23>
			//writer.WriteStartElement("ITE_23");
			//writer.WriteString(__valor);
			//writer.WriteEndElement();

			// <ITE_27>..</ITE_27>
			writer.WriteStartElement("ITE_27");
			writer.WriteString(cantidad.ToString());
			writer.WriteEndElement();

			// <ITE_28>..</ITE_28>
			writer.WriteStartElement("ITE_28");
			writer.WriteString("94");
			writer.WriteEndElement();


			/////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////


			//// <IAE>..</IAE>
			//writer.WriteStartElement("IAE");
			//writer.WriteEndElement();


			//// <IAE_1>..</IAE_1>
			//writer.WriteStartElement("IAE_1");
			//writer.WriteString("0");
			//writer.WriteEndElement();

			//// <IAE_2>..</IAE_2>
			//writer.WriteStartElement("IAE_2");
			//writer.WriteString("999");
			//writer.WriteEndElement();

			////</IAE>
			//writer.WriteEndElement();

			///////////////////////////////////////////////////////////////////////////////////////////////////////////////
			////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			TIISeccion(writer, TipoOperacion, adicionalAbono, sValorUnitarioSinIva, sValorIva, sValorTotalUnitarioSinIva, sPorcentajeIva);
			//if (adicionalAbono.ValorIva > 0)
			//{
			//	TIISeccion(writer, TipoOperacion, adicionalAbono);
			//}
			//</ITE>
			writer.WriteEndElement();

		}

		public static void ITESeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			ITESeccionSeccion(writer, TipoOperacion, adicionalAbono, adicionalAbono.valor, adicionalAbono.ValorIva,
				adicionalAbono.valor + adicionalAbono.ValorIva, adicionalAbono.PorcentajeIva);

		}
		public static void ITESeccionDetalle(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono, List<T_ADICIONALES_ABONOS_MOTIVOS> lstDetalles)
		{
			// <ITE>..</ITE>
			if (lstDetalles.Any())
			{
				int contador = 1;
				foreach (var detalle in lstDetalles)
				{
					decimal valor = detalle.VALOR ?? 0;
					decimal valorIva = 0;
					decimal porcentaje = detalle.VALORIVA ?? 0;
					if ((detalle.CANTIDAD ?? 1) > 1)
					{
						valor = valor / detalle.CANTIDAD ?? 1;
					}
					if (porcentaje > 0)
					{
						valorIva = decimal.Round(valor - (valor * (100 / (porcentaje + 100))));
						valor = decimal.Round(valor * (100 / (porcentaje + 100)));
					}
					//round(a.VALOR -( a.VALOR * 100/ (coalesce(a.VALORIVA,0) + 100)),0) IVA, round((a.VALOR * 100/ (coalesce(a.VALORIVA,0) + 100)),0) VALOR_SINIVA
					//round((a.VALOR * 100/ (coalesce(a.VALORIVA,0) + 100)),0) VALOR_SINIVA
					ITESeccionSeccion(writer, TipoOperacion, adicionalAbono, valor, valorIva,
						valor + valorIva, porcentaje, contador.ToString(), detalle.CANTIDAD ?? 1, detalle.DESCRIPCION ?? "", detalle.CODIGO ?? "");

					//ITESeccionSeccion(writer, TipoOperacion, adicionalAbono, detalle.VALOR ?? 0, detalle.VALORIVA ?? 0,
					//	(detalle.VALOR ?? 0) + (detalle.VALORIVA ?? 0), detalle.PORCENTAJEIVA ?? 0, contador.ToString(), detalle.CANTIDAD ?? 1, detalle.DESCRIPCION ?? "");
					contador++;
				}
			}
			else
			{
				ITESeccionSeccion(writer, TipoOperacion, adicionalAbono, adicionalAbono.valor, adicionalAbono.ValorIva,
					adicionalAbono.valor + adicionalAbono.ValorIva, adicionalAbono.PorcentajeIva);

			}

		}

		/*public static void RFDSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono,
			string __valor, string __valorIva, string __ValorTotal, string _PorcentajeIva)
		{
			//<RFD>..</RFD>  REVISAR todo el bloque RFD dice que se puede repetir 3 veces por eso lo hice tres veces
			writer.WriteStartElement("RFD");
			writer.WriteEndElement();


			//<RFD_1>..</RFD_1>
			//esto es nuevo todo RFD_1: descripción del recaudo facturado.
			//Nota: Contendrá algún código de la tabla 46.Este nodo se podrá repetir máximo tres veces.
			writer.WriteStartElement("RFD_1");
			writer.WriteString(adicionalAbono.FormaPago);
			writer.WriteEndElement();

			//</RFD>
			writer.WriteEndElement();*/

		/*//<RFD>..</RFD>
		writer.WriteStartElement("RFD");
		writer.WriteEndElement();

		//<RFD_1>..</RFD_1>
		//esto es nuevo todo RFD_1: descripción del recaudo facturado.
		//Nota: Contendrá algún código de la tabla 46.Este nodo se podrá repetir máximo tres veces.
		writer.WriteStartElement("RFD_1");
		writer.WriteString(adicionalAbono.nombre);
		writer.WriteEndElement();

		//</RFD>
		writer.WriteEndElement();

		//<RFD>..</RFD>
		writer.WriteStartElement("RFD");
		writer.WriteEndElement();

		//<RFD_1>..</RFD_1>
		//esto es nuevo todo RFD_1: descripción del recaudo facturado.
		//Nota: Contendrá algún código de la tabla 46.Este nodo se podrá repetir máximo tres veces.
		writer.WriteStartElement("RFD_1");
		writer.WriteString(adicionalAbono.nombre);
		writer.WriteEndElement();

		//</RFD>
		writer.WriteEndElement();

		// se puede repetir hasta 3 veces si es necesario
	}*/



		public static void TIISeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono,
			string __valor, string __valorIva, string __ValorTotal, string _PorcentajeIva)
		{
			// <TII>..</TII>
			writer.WriteStartElement("TII");


			// <TII_1>..</TII_1>
			writer.WriteStartElement("TII_1");

			writer.WriteString(__valorIva);
			writer.WriteEndElement();

			// <TII_2>..</TII_2>
			writer.WriteStartElement("TII_2");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();

			// <TII_3>..</TII_3>
			writer.WriteStartElement("TII_3");
			writer.WriteString("false");
			writer.WriteEndElement();

			{
				IIMSeccion(writer, TipoOperacion, adicionalAbono, __valor, __valorIva, __ValorTotal, _PorcentajeIva);
			}

			//</TII>
			writer.WriteEndElement();

		}
		public static void IIMSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono,
			string __valor, string __valorIva, string __ValorTotal, string _PorcentajeIva)
		{
			//= String.Format("{0:0.00}", adicionalAbono.valor).Replace(",", ".");

			//= String.Format("{0:0.00}", (adicionalAbono.valor + adicionalAbono.ValorIva)).Replace(",", ".");
			//string _PorcentajeIva = String.Format("{0:0.00}", (adicionalAbono.PorcentajeIva)).Replace(",", ".");
			// <IIM>..</IIM>
			writer.WriteStartElement("IIM");


			// <IIM_1>..</IIM_1>
			writer.WriteStartElement("IIM_1");
			writer.WriteString(adicionalAbono.CodigoImpuesto);
			writer.WriteEndElement();

			// <IIM_2>..</IIM_2>
			writer.WriteStartElement("IIM_2");

			writer.WriteString(__valorIva);
			//writer.WriteString("0");
			writer.WriteEndElement();

			// <IIM_3>..</IIM_3>
			writer.WriteStartElement("IIM_3");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();

			// <IIM_4>..</IIM_4>
			writer.WriteStartElement("IIM_4");
			writer.WriteString(__valor);
			writer.WriteEndElement();

			// <IIM_5>..</IIM_5>
			writer.WriteStartElement("IIM_5");
			writer.WriteString(adicionalAbono.codigoMoneda);
			writer.WriteEndElement();

			// <IIM_6>..</IIM_6>
			writer.WriteStartElement("IIM_6");
			writer.WriteString(_PorcentajeIva);
			writer.WriteEndElement();

			//</IIM>
			writer.WriteEndElement();

		}


		public static void CSLSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			// <CSL>..</CSL> REVISAR TODO CSL ES NUEVO
			//Este es un nuevo nodo, exclusivo de complemento salud que define el uso del mismo.Se define el nodo USS
			//cuando ENC_21 = SS01, SS02, SS03, SS04, SS05, SS06 y Se define el nodo SLD cuando ENC_21 = SS07
			writer.WriteStartElement("CSL");

			//Composición: 6 Sub nodos. Mandatorios: SLD_2,3,6. Opcionales: SLD_1. Dependientes: SLD_4,5. Nodos complementarios: Ninguno
			// <SLD>..</SLD>
			writer.WriteStartElement("SLD");
			var datos = adicionalAbono;
			// <SLD_1>..</SLD_1> Código prestador de servicios de salud
			writer.WriteStartElement("SLD_1");
			writer.WriteString(adicionalAbono.codigo_Prestador);
			writer.WriteEndElement();

			// <SLD_2>..</SLD_2> Modalidades de Pago (Ver tabla 48)
			writer.WriteStartElement("SLD_2");
			writer.WriteString("01");
			writer.WriteEndElement();

			// <SLD_3>..</SLD_3> Cobertura o Plan de Beneficios (Ver tabla 49)
			writer.WriteStartElement("SLD_3");
			writer.WriteString("15");
			writer.WriteEndElement();

			// <SLD_4>..</SLD_4> Número de contrato.
			//Nota: La definición de SLD_4 Y 5 es mandataria, pero su dependencia va con respecto a que si
			//SLD_4 tiene un valor, SLD_5 tendrá que estar vacío.
			writer.WriteStartElement("SLD_4");
			writer.WriteString("0");
			writer.WriteEndElement();

			// <SLD_5>..</SLD_5> Número de póliza.
			//Nota: La definición de SLD_4 Y 5 es mandataria, pero su dependencia va con respecto a que si
			//SLD_5 tiene un valor, SLD_4 tendrá que estar vacío.
			writer.WriteStartElement("SLD_5");
			//writer.WriteString(null);
			writer.WriteEndElement();

			// <SLD_6>..</SLD_6> Nombre del miembro o unidad del sector
			writer.WriteStartElement("SLD_6");
			writer.WriteString("Usuario1");
			writer.WriteEndElement();

			//</SLD>
			writer.WriteEndElement();

			/*  // <USS>..</USS>
			  //Composición: 6 Sub nodos. Mandatorios: USS_1, 2. Opcionales: USS_3, 4, IDR, RAD. Dependientes: Ninguno
			  //Nodos complementarios: IDR, RAD
				  writer.WriteStartElement("USS");

				  // <USS_1>..</USS_1> Identificador del Usuario beneficiario del servicio de salud.
				  writer.WriteStartElement("USS_1");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  // <USS_2>..</USS_2> Códigos para identificación fiscal. (Ver tabla 3)
				  writer.WriteStartElement("USS_2");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  // <USS_3>..</USS_3> Nombres del Usuario beneficiario del servicio de salud.
				  writer.WriteStartElement("USS_3");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  // <USS_4>..</USS_4> Apellidos del Usuario beneficiario del servicio de salud.
				  writer.WriteStartElement("USS_4");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  //</USS>

				  // <IDR>..</IDR>
				  writer.WriteStartElement("IDR");

				  // <IDR_1>..</IDR_1> Identificador del Usuario beneficiario del servicio de salud.
				  writer.WriteStartElement("IDR_1");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  // <IDR_2>..</IDR_2> Códigos para identificación fiscal. (Ver tabla 3)
				  writer.WriteStartElement("IDR_2");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  // <IDR_3>..</IDR_3> Parte expedidora del documento del Usuario beneficiario del servicio de salud.
				  writer.WriteStartElement("IDR_3");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  // <IDR_4>..</IDR_4> Código del país de la entidad expedidora del documento. (Ver tabla 1)
				  //Nota: Debe contener un valor correspondiente a la columna código alfa-2.
				  writer.WriteStartElement("IDR_4");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  //</IDR>
				  writer.WriteEndElement();

				  // <RAD>..</RAD>
				  writer.WriteStartElement("RAD");

				  // <RAD_1>..</RAD_1> Código de la ciudad del Usuario beneficiario del servicio de salud. (Ver tabla 35)
				  writer.WriteStartElement("RAD_1");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  // <RAD_2>..</RAD_2> Nombre de la ciudad del Usuario beneficiario del servicio de salud.
				  writer.WriteStartElement("RAD_2");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  // <RAD_3>..</RAD_3> Código del país del Usuario beneficiario del servicio de salud. (Ver tabla 1)
				  writer.WriteStartElement("RAD_3");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  //</RAD>
				  writer.WriteEndElement();

				  // <ADL>..</ADL>
				  writer.WriteStartElement("ADL");

				  // <ADL_1>..</ADL_1> Dirección Línea-1
				  writer.WriteStartElement("ADL_1");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  //</ADL>
				  writer.WriteEndElement();


				  // <ADL>..</ADL>
				  writer.WriteStartElement("ADL");

				  // <ADL_1>..</ADL_1> Dirección Línea-2
				  writer.WriteStartElement("ADL_1");
				  writer.WriteString("false");
				  writer.WriteEndElement();

				  //</ADL>
				  writer.WriteEndElement();

				  //</USS>
				  writer.WriteEndElement();*/

			//</CSL>
			writer.WriteEndElement();

		}

		public static void NOTSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			string __valor = String.Format("{0:0.00}", adicionalAbono.valor).Replace(",", ".");
			// <NOT>..</NOT>
			writer.WriteStartElement("NOT");

			writer.WriteStartElement("NOT_1");
			writer.WriteString(adicionalAbono.nota_Factura);   //Forma de pago efectivo
			writer.WriteEndElement();

			// <NOT_1>..</NOT_1>
			writer.WriteStartElement("NOT_1");
			writer.WriteString("CR(" + __valor + ")");
			writer.WriteEndElement();


			// <NOT_2>..</NOT_2>
			writer.WriteStartElement("NOT_2");
			writer.WriteString("Crédito 1 días");
			writer.WriteEndElement();


			//</NOT>
			writer.WriteEndElement();

		}
		public static void REFSeccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{
			string __valor = String.Format("{0:0.00}", adicionalAbono.valor).Replace(",", ".");
			// <REF>..</REF>
			writer.WriteStartElement("REF");


			// <REF_1>..</REF_1>
			writer.WriteStartElement("REF_1");
			writer.WriteString("IV");
			writer.WriteEndElement();


			// <REF_2>..</REF_2>
			writer.WriteStartElement("REF_2");
			writer.WriteString(adicionalAbono.factura);
			writer.WriteEndElement();

			// <REF_3>..</REF_3>
			writer.WriteStartElement("REF_3");
			writer.WriteString(adicionalAbono.fechaFactura.ToString("yyyy-MM-dd"));
			writer.WriteEndElement();


			// <REF_4>..</REF_4>
			writer.WriteStartElement("REF_4");
			writer.WriteString(Encriptacion.ComputeSha384Hash(adicionalAbono.factura));
			writer.WriteEndElement();

			// <REF_5>..</REF_5>
			writer.WriteStartElement("REF_5");
			writer.WriteString("CUFE-SHA384)");
			writer.WriteEndElement();


			//</REF>
			writer.WriteEndElement();

		}
		public static void Seccion(XmlTextWriter writer, string TipoOperacion, P_Adicionales_Abonos_Dian adicionalAbono)
		{

		}


	}


}
