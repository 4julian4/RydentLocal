using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.Fhir;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public static class RdaFhirConsultaMapper
	{
		private const string SystemNationalPersonIdentifier = "https://fhir.minsalud.gov.co/rda/NamingSystem/RNEC";
		private const string SystemPrestadorReps = "https://fhir.minsalud.gov.co/rda/NamingSystem/REPS";
		private const string SystemDian = "https://fhir.minsalud.gov.co/rda/NamingSystem/DIAN";

		private const string SystemV20203 = "http://terminology.hl7.org/CodeSystem/v2-0203";
		private const string SystemColombianPersonIdentifier = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianPersonIdentifier";
		private const string SystemColombianOrganizationIdentifiers = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianOrganizationIdentifiers";
		private const string SystemLoinc = "http://loinc.org";
		private const string SystemIcd10 = "http://hl7.org/fhir/sid/icd-10";
		private const string SystemCups = "https://fhir.minsalud.gov.co/rda/CodeSystem/CUPS";
		private const string SystemEncounterClass = "http://terminology.hl7.org/CodeSystem/v3-ActCode";
		private const string SystemCause = "https://fhir.minsalud.gov.co/rda/CodeSystem/RIPSCausaExternaVersion2";
		private const string SystemGrupoServicios = "https://fhir.minsalud.gov.co/rda/CodeSystem/GrupoServicios";
		private const string SystemColombianTechModality = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianTechModality";
		private const string SystemEntornoAtencion = "https://fhir.minsalud.gov.co/rda/CodeSystem/EntornoAtencion";
		private const string SystemProviderClass = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianProviderClass";
		private const string SystemLegalNature = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianLegalNatureType";
		private const string SystemTipoAlergia = "https://fhir.minsalud.gov.co/rda/CodeSystem/TipoAlergia";
		private const string SystemParticipationType = "http://terminology.hl7.org/CodeSystem/v3-ParticipationType";
		private const string SystemConditionClinical = "http://terminology.hl7.org/CodeSystem/condition-clinical";
		private const string SystemConditionCategory = "http://terminology.hl7.org/CodeSystem/condition-category";
		private const string SystemListEmptyReason = "http://terminology.hl7.org/CodeSystem/list-empty-reason";
		private const string SystemDiagnosisRole = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianDiagnosisRole";
		private const string SystemDiagnosisType = "https://fhir.minsalud.gov.co/rda/CodeSystem/RIPSTipoDiagnosticoPrincipalVersion2";
		private const string SystemConfidentiality = "http://terminology.hl7.org/CodeSystem/v3-Confidentiality";
		private const string SystemDivipola = "https://fhir.minsalud.gov.co/rda/CodeSystem/DIVIPOLA";
		private const string SystemIso31661 = "https://fhir.minsalud.gov.co/rda/CodeSystem/ISO31661";
		private const string SystemResidenceZone = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianResidenceZone";
		private const string SystemGenderIdentity = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianGenderIdentity";
		private const string SystemBiologicalGender = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianGenderGroup";
		private const string SystemEthnicity = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianEthnicGroup";
		private const string SystemDisability = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianDisabilityClassification";

		private const string UrlExtensionDivipolaMunicipality = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionDivipolaMunicipality";
		private const string UrlExtensionCountryCode = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionCountryCode";
		private const string UrlExtensionResidenceZone = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionResidenceZone";
		private const string UrlExtensionBirthTime = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionBirthTime";
		private const string UrlExtensionPatientNationality = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionPatientNationality";
		private const string UrlExtensionPatientEthnicity = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionPatientEthnicity";
		private const string UrlExtensionPatientDisability = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionPatientDisability";
		private const string UrlExtensionPatientGenderIdentity = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionPatientGenderIdentity";
		private const string UrlExtensionBiologicalGender = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionBiologicalGender";
		private const string UrlExtensionFathersFamilyName = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionFathersFamilyName";
		private const string UrlExtensionMothersFamilyName = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionMothersFamilyName";
		private const string UrlExtensionDiagnosisType = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionDiagnosisType";

		private const string ProfileComposition = "https://fhir.minsalud.gov.co/rda/StructureDefinition/CompositionAmbulatoryRDA";
		private const string ProfilePatient = "https://fhir.minsalud.gov.co/rda/StructureDefinition/PatientRDA";
		private const string ProfileOrganization = "https://fhir.minsalud.gov.co/rda/StructureDefinition/CareDeliveryOrganizationRDA";
		private const string ProfilePractitioner = "https://fhir.minsalud.gov.co/rda/StructureDefinition/PractitionerRDA";
		private const string ProfileEncounter = "https://fhir.minsalud.gov.co/rda/StructureDefinition/EncounterAmbulatoryRDA";
		private const string ProfileCondition = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ConditionRDA";
		private const string ProfileAllergy = "https://fhir.minsalud.gov.co/rda/StructureDefinition/AllergyIntoleranceRDA";
		private const string ProfileLocation = "https://fhir.minsalud.gov.co/rda/StructureDefinition/CareDeliveryLocationRDA";
		private const string ProfileDocumentReference = "https://fhir.minsalud.gov.co/rda/StructureDefinition/DocumentReferenceEPIRDA";

		public static FhirBundleRdaConsulta Map(RdaDocumentoInterno source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			var consulta = source.Documento?.Consulta ?? new RdaConsultaSource();
			var encounter = consulta.Encounter ?? new RdaEncounterSource();
			var antecedentes = consulta.Antecedentes ?? new RdaAntecedentesSource();
			var diagnostico = consulta.Diagnostico ?? new RdaDiagnosticoSource();
			var procedimiento = consulta.Procedimiento ?? new RdaProcedimientoSource();
			var prestador = source.Documento?.Prestador ?? new RdaPrestadorSource();

			var fecha = encounter.FechaConsulta ?? DateTime.Now.Date;
			var inicio = new DateTimeOffset(fecha.Year, fecha.Month, fecha.Day, 10, 0, 0, TimeSpan.FromHours(-5));
			var fin = inicio.AddMinutes(30);

			var patientId = BuildPersonId(encounter.TipoDocumento, encounter.NumeroDocumento, encounter.IdAnamnesis, "patient");
			var practitionerId = BuildPersonId(prestador.TipoDocumentoDoctor, prestador.NumeroDocumentoDoctor, prestador.IdDoctor ?? 0, "practitioner");
			var organizationId = SafeToken(FirstNonEmpty(prestador.CodigoPrestador, prestador.NitPrestador, "ips"));

			const string encounterId = "Encounter-0";
			const string conditionId = "Condition-0";
			const string allergyId = "AllergyIntolerance-0";
			const string documentReferenceId = "DocumentReference-0";
			var locationId = $"{organizationId}-01";

			var patientRef = $"#{patientId}";
			var practitionerRef = $"#{practitionerId}";
			var organizationRef = $"#{organizationId}";
			var encounterRef = $"#{encounterId}";
			var conditionRef = $"#{conditionId}";
			var allergyRef = $"#{allergyId}";
			var locationRef = $"#{locationId}";
			var documentReferenceRef = $"#{documentReferenceId}";

			var bundle = new FhirBundleRdaConsulta
			{
				language = "es-CO",
				type = "document"
			};

			var composition = new FhirComposition
			{
				resourceType = "Composition",
				meta = BuildMeta(ProfileComposition),
				status = "final",
				type = BuildCodeableConcept(SystemLoinc, "51845-6", "Outpatient Consult note"),
				subject = new FhirReference { reference = patientRef },
				encounter = new FhirReference { reference = encounterRef },
				date = fin,
				title = "RDA Consulta",
				confidentiality = "N",
				custodian = new FhirReference { reference = organizationRef }
			};

			composition.author.Add(new FhirReference { reference = practitionerRef });
			composition.attester.Add(new FhirCompositionAttester
			{
				mode = "legal",
				party = new FhirReference { reference = organizationRef }
			});

			composition.event_.Add(new FhirCompositionEvent
			{
				code = new List<FhirCodeableConcept>(),
				period = new FhirPeriod
				{
					start = inicio.ToString("yyyy-MM-ddTHH:mm:sszzz"),
					end = fin.ToString("yyyy-MM-ddTHH:mm:sszzz")
				},
				detail = new List<FhirReference>()
			});

			composition.section.Add(BuildNarrativeSection(
				"Entidad(es) responsable(s) por el plan de beneficios en salud (consulta)",
				"48768-6",
				"Payment sources Document",
				BuildPaymentSectionText(encounter)));

			composition.section.Add(BuildNarrativeSection(
				"Otros datos demográficos",
				"74208-0",
				"Demographic information + History of occupation Document",
				BuildDemographicSectionText(encounter)));

			composition.section.Add(BuildNarrativeSection(
				"Datos incapacidad (SIPE – Sistema de Incapacidades y Prestaciones Economicas)",
				"105583-9",
				"Worker Sick leave form",
				"No se generó incapacidad asociada a este encuentro ambulatorio."));

			composition.section.Add(new FhirCompositionSection
			{
				title = "Historial de diagnósticos de problemas de salud",
				code = BuildCodeableConcept(SystemLoinc, "11450-4", "Problem list - Reported"),
				entry = new List<FhirReference>
				{
					new FhirReference { reference = conditionRef }
				}
			});

			if (!string.IsNullOrWhiteSpace(antecedentes.AlergiasTexto))
			{
				composition.section.Add(new FhirCompositionSection
				{
					title = "Historial de alergias, intolerancias y reacciones adversas",
					code = BuildCodeableConcept(SystemLoinc, "48765-2", "Allergies and adverse reactions Document"),
					entry = new List<FhirReference>
					{
						new FhirReference { reference = allergyRef }
					}
				});
			}
			else
			{
				composition.section.Add(new FhirCompositionSection
				{
					title = "Historial de alergias, intolerancias y reacciones adversas",
					code = BuildCodeableConcept(SystemLoinc, "48765-2", "Allergies and adverse reactions Document"),
					text = BuildNarrative("No se reportan alergias medicamentosas, alimentarias ni otras reacciones adversas relevantes al momento de esta atención."),
					entry = new List<FhirReference>(),
					emptyReason = BuildCodeableConcept(SystemListEmptyReason, "nilknown", "Nil Known")
				});
			}

			composition.section.Add(BuildNarrativeSection(
				"Factores de riesgo",
				"75492-9",
				"Risk assessment and screening note",
				BuildRiskFactorsSectionText(antecedentes)));

			composition.section.Add(BuildNarrativeSection(
				"Historial de medicamentos",
				"10160-0",
				"History of Medication use Narrative",
				BuildMedicationSectionText(antecedentes)));

			composition.section.Add(BuildNarrativeSection(
				"Órdenes, prescripciones o solicitudes de servicio",
				"61146-1",
				"Orders for services Document",
				BuildOrdersSectionText(procedimiento)));

			var pdfBase64 = RdaPdfSimpleBuilder.BuildConsultaPdfBase64(new RdaConsultaPdfData
			{
				Paciente = BuildDisplayName(encounter.Nombres, encounter.Apellidos),
				TipoDocumento = encounter.TipoDocumento,
				Documento = encounter.NumeroDocumento,
				Doctor = FirstNonEmpty(prestador.NombreDoctor, encounter.Doctor),
				Prestador = prestador.NombrePrestador,

				FechaAtencion = encounter.FechaConsulta?.ToString("yyyy-MM-dd"),
				Factura = encounter.Factura,
				NumeroAutorizacion = FirstNonEmpty(procedimiento.NumeroAutorizacion, diagnostico.NumeroAutorizacion),

				CodigoConsulta = encounter.CodigoConsulta,
				NombreConsulta = encounter.NombreConsulta,
				CausaAtencion = MapCauseDisplay(FirstNonEmpty(diagnostico.CodigoCausa, "38")),
				TipoDiagnostico = MapDiagnosisTypeDisplay(FirstNonEmpty(diagnostico.CodigoTipoDiagnostico, "02")),

				EntidadResponsable = encounter.NombreEps,
				CodigoEntidad = encounter.CodigoEps,
				TipoAfiliacion = encounter.TipoUsuario,
				NumeroAfiliacion = encounter.NroAfiliacion,

				Ciudad = encounter.NombreCiudad,
				Departamento = encounter.NombreDepartamento,
				Direccion = encounter.Direccion,
				Telefono = SanitizePhone(encounter.Telefono),
				Celular = SanitizePhone(encounter.Celular),

				DiagnosticoPrincipal = FirstNonEmpty(diagnostico.NombreDiagnostico1, diagnostico.Diagnostico1),
				Diagnostico2 = FirstNonEmpty(diagnostico.NombreDiagnostico2, diagnostico.Diagnostico2),
				Diagnostico3 = FirstNonEmpty(diagnostico.NombreDiagnostico3, diagnostico.Diagnostico3),
				Diagnostico4 = FirstNonEmpty(diagnostico.NombreDiagnostico4, diagnostico.Diagnostico4),

				MotivoConsulta = antecedentes.MotivoConsulta,
				EnfermedadActual = antecedentes.EnfermedadActual,
				Alergias = antecedentes.AlergiasTexto,
				Medicamentos = antecedentes.MedicamentosActualesTexto,
				FactoresRiesgo = BuildRiskFactorsSectionText(antecedentes),
				EnfermedadesPrevias = antecedentes.EnfermedadesPreviasTexto,
				Cirugias = antecedentes.CirugiasTexto,
				RevisionSistemas = antecedentes.RevisionSistemas,
				Observaciones = FirstNonEmpty(
					diagnostico.Observaciones,
					diagnostico.Pronostico,
					antecedentes.ObservacionesAntecedentes),

				ProcedimientoCodigo = procedimiento.CodigoProcedimiento,
				ProcedimientoNombre = procedimiento.NombreProcedimiento,
				ProcedimientoDxPrincipal = FirstNonEmpty(procedimiento.NombreDxPrincipal, procedimiento.DxPrincipal),
				ProcedimientoDxRelacionado = FirstNonEmpty(procedimiento.NombreDxRelacionado, procedimiento.DxRelacionado),
				Ambito = procedimiento.AmbitoRealizacion,
				FinalidadProcedimiento = procedimiento.FinalidadProcedimiento,
				PersonalAtiende = procedimiento.PersonalQueAtiende,
				Complicacion = procedimiento.Complicacion,
				FormaActoQuir = procedimiento.FormaRealizacionActoQuir,
				ValorProcedimiento = procedimiento.ValorProcedimiento?.ToString(),
				EntidadProcedimiento = procedimiento.NombreEntidad,
				Extranjero = procedimiento.Extranjero,
				Pais = procedimiento.Pais
			});

			composition.section.Add(new FhirCompositionSection
			{
				title = "Documentos de soporte",
				code = BuildCodeableConcept(SystemLoinc, "55107-7", "Addendum Document"),
				entry = new List<FhirReference>
				{
					new FhirReference { reference = documentReferenceRef }
				}
			});

			var patient = new FhirPatient
			{
				resourceType = "Patient",
				id = patientId,
				meta = BuildMeta(ProfilePatient),
				active = true,
				gender = MapGender(encounter.Sexo),
				birthDate = encounter.FechaNacimiento?.ToString("yyyy-MM-dd"),
				deceasedBoolean = false
			};

			AddDefaultPatientExtensions(patient, encounter);
			AddNationalPersonIdentifier(patient.identifier, encounter.TipoDocumento, encounter.NumeroDocumento, includeSystem: true);

			var patientName = new FhirHumanName
			{
				use = "official",
				text = BuildDisplayName(encounter.Nombres, encounter.Apellidos),
				family = NullIfWhite(encounter.Apellidos),
				given = SplitWords(encounter.Nombres)
			};
			AddFamilyExtensions(patientName, encounter.Apellidos);
			patient.name.Add(patientName);

			AddPatientAddress(patient, encounter);
			AddPatientTelecom(patient, encounter);

			var organization = new FhirOrganization
			{
				resourceType = "Organization",
				id = organizationId,
				meta = BuildMeta(ProfileOrganization),
				active = true,
				name = FirstNonEmpty(prestador.NombrePrestador, "IPS no identificada")
			};

			if (!string.IsNullOrWhiteSpace(prestador.NitPrestador))
			{
				organization.identifier.Add(new FhirIdentifier
				{
					id = "TaxIdentifier-0",
					use = "official",
					type = BuildCodeableConceptWithTwoCodings(
						SystemV20203, "TAX", "Tax ID number",
						SystemColombianOrganizationIdentifiers, "NIT", "Número de Identificación Tributaria"),
					system = SystemDian,
					value = prestador.NitPrestador.Trim()
				});
			}

			if (!string.IsNullOrWhiteSpace(prestador.CodigoPrestador))
			{
				organization.identifier.Add(new FhirIdentifier
				{
					id = "HealthcareProviderIdentifier-0",
					use = "official",
					type = BuildCodeableConceptWithTwoCodings(
						SystemV20203, "PRN", "Provider number",
						SystemColombianOrganizationIdentifiers, "CodigoPrestador", "Código de habilitación de prestador de servicios de salud"),
					system = SystemPrestadorReps,
					value = prestador.CodigoPrestador.Trim()
				});
			}

			organization.type.Add(BuildCodeableConcept(SystemProviderClass, "IPS", "Institución Prestadora de Servicios de Salud"));
			organization.type.Add(BuildCodeableConcept(SystemLegalNature, "PRIV", "Privada"));

			organization.address.Add(new FhirAddress
			{
				use = "work",
				type = "physical",
				text = FirstNonEmpty(prestador.DireccionPrestador, "Sin información"),
				city = FirstNonEmpty(prestador.CiudadPrestador, "SIN CIUDAD"),
				country = "CO",
				extension = new List<FhirExtension>()
			});

			if (!string.IsNullOrWhiteSpace(prestador.TelefonoPrestador))
			{
				organization.telecom.Add(new FhirContactPoint
				{
					system = "phone",
					value = prestador.TelefonoPrestador.Trim()
				});
			}

			var practitioner = new FhirPractitioner
			{
				resourceType = "Practitioner",
				id = practitionerId,
				meta = BuildMeta(ProfilePractitioner),
				active = true
			};

			AddNationalPersonIdentifier(practitioner.identifier, prestador.TipoDocumentoDoctor, prestador.NumeroDocumentoDoctor, includeSystem: true);

			var doctorFullName = FirstNonEmpty(prestador.NombreDoctor, encounter.Doctor, "Profesional no identificado");
			var practitionerName = new FhirHumanName
			{
				use = "official",
				text = doctorFullName,
				family = BuildFamilyFromFullName(doctorFullName),
				given = BuildGivenFromFullName(doctorFullName)
			};
			AddFamilyExtensions(practitionerName, practitionerName.family);
			practitioner.name.Add(practitionerName);

			var condition = new FhirCondition
			{
				resourceType = "Condition",
				id = conditionId,
				meta = BuildMeta(ProfileCondition),
				clinicalStatus = BuildCodeableConcept(SystemConditionClinical, "active", "Active"),
				verificationStatus = BuildCodeableConcept(null, "confirmed", "Confirmed"),
				subject = new FhirReference { reference = patientRef },
				code = BuildCodeableConcept(
					SystemIcd10,
					FirstNonEmpty(diagnostico.Diagnostico1, encounter.CodigoDiagnosticoPrincipal, "R69X"),
					FirstNonEmpty(diagnostico.NombreDiagnostico1, encounter.NombreDiagnosticoPrincipal, "DIAGNÓSTICO NO ESPECIFICADO"))
			};
			condition.category.Add(BuildCodeableConcept(SystemConditionCategory, "encounter-diagnosis", "Encounter Diagnosis"));

			

			FhirAllergyIntolerance allergy = null;
			if (!string.IsNullOrWhiteSpace(antecedentes.AlergiasTexto))
			{
				allergy = new FhirAllergyIntolerance
				{
					resourceType = "AllergyIntolerance",
					id = allergyId,
					meta = BuildMeta(ProfileAllergy),
					clinicalStatus = BuildCodeableConcept(null, "active", "Active"),
					verificationStatus = BuildCodeableConcept(null, "confirmed", "Confirmed"),
					code = new FhirCodeableConcept
					{
						coding = new List<FhirCoding>
						{
							new FhirCoding
							{
								system = SystemTipoAlergia,
								code = "01",
								display = "Medicamento"
							}
						},
						text = antecedentes.AlergiasTexto.Trim()
					},
					patient = new FhirReference { reference = patientRef }
				};
			}

			var encounterFhir = new FhirEncounter
			{
				resourceType = "Encounter",
				id = encounterId,
				meta = BuildMeta(ProfileEncounter),
				status = "finished",
				@class = new FhirCoding
				{
					system = SystemEncounterClass,
					code = "AMB",
					display = "ambulatory"
				},
				serviceType = BuildCodeableConcept(
					SystemCups,
					FirstNonEmpty(encounter.CodigoConsulta, "890201"),
					MapCupsDisplay(FirstNonEmpty(encounter.CodigoConsulta, "890201"), encounter.NombreConsulta)),
				subject = new FhirReference { reference = patientRef },
				serviceProvider = new FhirReference { reference = organizationRef },
				period = new FhirPeriod
				{
					start = inicio.ToString("yyyy-MM-ddTHH:mm:sszzz"),
					end = fin.ToString("yyyy-MM-ddTHH:mm:sszzz")
				}
			};

			encounterFhir.identifier.Add(new FhirIdentifier
			{
				id = "EncounterIdentifier",
				use = "usual",
				system = "https://fhir.minsalud.gov.co/rda/NamingSystem/Encounters",
				value = $"RDA-CE-{SafeToken(encounter.NumeroDocumento)}-{fecha:yyyyMMdd}-1000"
			});

			encounterFhir.type.Add(BuildCodeableConcept(SystemColombianTechModality, "01", "Intramural"));
			encounterFhir.type.Add(BuildCodeableConcept(SystemGrupoServicios, "01", "Consulta externa"));
			encounterFhir.type.Add(BuildCodeableConcept(SystemEntornoAtencion, "05", "Institucional"));

			encounterFhir.participant.Add(new FhirEncounterParticipant
			{
				id = "AttenderPhysician",
				type = new List<FhirCodeableConcept>
				{
					BuildCodeableConcept(SystemParticipationType, "ATND", "attender")
				},
				individual = new FhirReference { reference = practitionerRef }
			});

			encounterFhir.reasonCode.Add(BuildCodeableConcept(
				SystemCause,
				FirstNonEmpty(diagnostico.CodigoCausa, "38"),
				MapCauseDisplay(FirstNonEmpty(diagnostico.CodigoCausa, "38"))));

			encounterFhir.diagnosis.Add(new FhirEncounterDiagnosis
			{
				id = "MainDiagnosis",
				condition = new FhirReference { reference = conditionRef },
				use = BuildCodeableConcept(SystemDiagnosisRole, "8319008", "diagnóstico primario"),
				rank = 1,
				extension = new List<FhirExtension>
				{
					new FhirExtension
					{
						url = UrlExtensionDiagnosisType,
						valueCoding = new FhirCoding
						{
							system = SystemDiagnosisType,
							code = FirstNonEmpty(diagnostico.CodigoTipoDiagnostico, "02"),
							display = MapDiagnosisTypeDisplay(FirstNonEmpty(diagnostico.CodigoTipoDiagnostico, "02"))
						}
					}
				}
			});

			

			var location = new FhirLocation
			{
				resourceType = "Location",
				id = locationId,
				meta = BuildMeta(ProfileLocation),
				status = "active",
				name = FirstNonEmpty(prestador.NombrePrestador, "Sede principal"),
				managingOrganization = new FhirReference { reference = organizationRef },
				address = new FhirAddress
				{
					use = "work",
					type = "physical",
					text = FirstNonEmpty(prestador.DireccionPrestador, "Sin información"),
					city = FirstNonEmpty(prestador.CiudadPrestador, "SIN CIUDAD"),
					country = "CO",
					extension = new List<FhirExtension>()
				}
			};

			location.identifier.Add(new FhirIdentifier
			{
				use = "official",
				system = SystemPrestadorReps,
				value = locationId
			});

			encounterFhir.location.Add(new FhirEncounterLocation
			{
				location = new FhirReference { reference = locationRef }
			});

			var documentReference = new FhirDocumentReference
			{
				resourceType = "DocumentReference",
				id = documentReferenceId,
				meta = BuildMeta(ProfileDocumentReference),
				status = "current",
				text = BuildNarrative("Document Reference"),
				type = new FhirCodeableConcept
				{
					coding = new List<FhirCoding>
					{
						new FhirCoding
						{
							system = SystemLoinc,
							code = "18842-5",
							display = "Discharge summary"
						},
						new FhirCoding
						{
							system = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianDocumentTypes",
							code = "EPI",
							display = "Epicrisis"
						}
					}
				},
				subject = new FhirReference { reference = patientRef },
				date = fin.ToString("yyyy-MM-ddTHH:mm:sszzz"),
				custodian = new FhirReference { reference = "Organization/MinSalud" },
				description = "Epicrisis del encuentro de atención en salud - RDA",
				context = new FhirDocumentReferenceContext
				{
					encounter = new List<FhirReference>
					{
						new FhirReference { reference = encounterRef }
					}
				}
			};

			documentReference.author.Add(new FhirReference { reference = organizationRef });
			documentReference.category.Add(BuildCodeableConcept(SystemLoinc, "55108-5", "Clinical presentation Document"));
			documentReference.securityLabel.Add(BuildCodeableConcept(SystemConfidentiality, "R", "restricted"));

			documentReference.content.Add(new FhirDocumentReferenceContent
			{
				attachment = new FhirAttachment
				{
					data = pdfBase64,
					title = "epicrisis-rda.pdf"
				},
				format = new FhirCoding
				{
					system = "urn:ietf:bcp:13",
					code = "application/pdf",
					display = "PDF"
				}
			});

			bundle.entry.Add(new FhirBundleEntry { resource = composition });
			bundle.entry.Add(new FhirBundleEntry { resource = patient });
			bundle.entry.Add(new FhirBundleEntry { resource = organization });
			bundle.entry.Add(new FhirBundleEntry { resource = practitioner });
			bundle.entry.Add(new FhirBundleEntry { resource = condition });
			if (allergy != null)
				bundle.entry.Add(new FhirBundleEntry { resource = allergy });
			bundle.entry.Add(new FhirBundleEntry { resource = encounterFhir });
			bundle.entry.Add(new FhirBundleEntry { resource = location });
			bundle.entry.Add(new FhirBundleEntry { resource = documentReference });

			return bundle;
		}

		private static void AddPatientAddress(FhirPatient patient, RdaEncounterSource encounter)
		{
			if (patient == null)
				return;

			var city = NullIfWhite(encounter.NombreCiudad);
			var municipioCode = NormalizeMunicipalityCode(encounter.CodigoDepartamento, encounter.CodigoCiudad);
			var direccion = NullIfWhite(encounter.Direccion);

			if (string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(municipioCode) && string.IsNullOrWhiteSpace(direccion))
				return;

			var address = new FhirAddress
			{
				id = "HomeAddress-0",
				use = "home",
				type = "physical",
				city = city,
				country = "Colombia"
			};

			if (!string.IsNullOrWhiteSpace(municipioCode))
			{
				address._city = new FhirPrimitiveElement();
				address._city.extension.Add(new FhirExtension
				{
					url = UrlExtensionDivipolaMunicipality,
					valueCoding = new FhirCoding
					{
						system = SystemDivipola,
						code = municipioCode
					}
				});
			}

			address._country = new FhirPrimitiveElement();
			address._country.extension.Add(new FhirExtension
			{
				url = UrlExtensionCountryCode,
				valueCoding = new FhirCoding
				{
					system = SystemIso31661,
					code = "170"
				}
			});

			address.extension.Add(new FhirExtension
			{
				url = UrlExtensionResidenceZone,
				valueCoding = new FhirCoding
				{
					system = SystemResidenceZone,
					code = "01",
					display = "Urbana"
				}
			});

			patient.address.Add(address);
		}

		private static void AddPatientTelecom(FhirPatient patient, RdaEncounterSource encounter)
		{
			if (patient == null)
				return;

			var telefono = SanitizePhone(encounter.Telefono);
			var celular = SanitizePhone(encounter.Celular);

			if (!string.IsNullOrWhiteSpace(telefono))
			{
				patient.telecom.Add(new FhirContactPoint
				{
					system = "phone",
					value = telefono
				});
			}

			if (!string.IsNullOrWhiteSpace(celular) &&
				!string.Equals(celular, telefono, StringComparison.OrdinalIgnoreCase))
			{
				patient.telecom.Add(new FhirContactPoint
				{
					system = "phone",
					value = celular
				});
			}
		}

		private static string? SanitizePhone(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;

			var txt = value.Trim();

			if (txt.Equals("NO", StringComparison.OrdinalIgnoreCase) ||
				txt.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
				txt.Equals("-", StringComparison.OrdinalIgnoreCase))
				return null;

			return txt;
		}

		private static void AddDefaultPatientExtensions(FhirPatient patient, RdaEncounterSource encounter)
		{
			patient.extension.Add(new FhirExtension
			{
				url = UrlExtensionPatientNationality,
				valueCoding = new FhirCoding
				{
					system = SystemIso31661,
					code = "170",
					display = "Colombia"
				}
			});

			patient.extension.Add(new FhirExtension
			{
				url = UrlExtensionPatientEthnicity,
				valueCoding = new FhirCoding
				{
					system = SystemEthnicity,
					code = "6",
					display = "Otras etnias"
				}
			});

			patient.extension.Add(new FhirExtension
			{
				url = UrlExtensionPatientDisability,
				valueCoding = new FhirCoding
				{
					system = SystemDisability,
					code = "08",
					display = "Sin discapacidad"
				}
			});

			patient.extension.Add(new FhirExtension
			{
				url = UrlExtensionPatientGenderIdentity,
				valueCoding = new FhirCoding
				{
					system = SystemGenderIdentity,
					code = MapGenderIdentityCode(encounter.Sexo),
					display = MapGenderIdentityDisplay(encounter.Sexo)
				}
			});

			patient._gender = new FhirPrimitiveElement();
			patient._gender.extension.Add(new FhirExtension
			{
				url = UrlExtensionBiologicalGender,
				valueCoding = new FhirCoding
				{
					system = SystemBiologicalGender,
					code = MapBiologicalGenderCode(encounter.Sexo),
					display = MapBiologicalGenderDisplay(encounter.Sexo)
				}
			});

			if (encounter.FechaNacimiento.HasValue)
			{
				patient._birthDate = new FhirPrimitiveElement();
				patient._birthDate.extension.Add(new FhirExtension
				{
					url = UrlExtensionBirthTime,
					valueTime = "08:00:00"
				});
			}
		}

		private static void AddFamilyExtensions(FhirHumanName name, string family)
		{
			if (name == null || string.IsNullOrWhiteSpace(family))
				return;

			var parts = SplitWords(family);
			if (parts.Count < 2)
				return;

			name._family = new FhirPrimitiveElement();
			name._family.extension.Add(new FhirExtension
			{
				url = UrlExtensionFathersFamilyName,
				valueString = parts[0]
			});
			name._family.extension.Add(new FhirExtension
			{
				url = UrlExtensionMothersFamilyName,
				valueString = string.Join(" ", parts.Skip(1))
			});
		}

		private static List<FhirAnnotation> BuildConditionNotes(RdaDiagnosticoSource diagnostico, RdaAntecedentesSource antecedentes)
		{
			var notes = new List<FhirAnnotation>();

			var observaciones = FirstNonEmpty(
				diagnostico?.Observaciones,
				diagnostico?.Pronostico,
				antecedentes?.EnfermedadActual);

			if (!string.IsNullOrWhiteSpace(observaciones))
			{
				notes.Add(new FhirAnnotation
				{
					text = observaciones.Trim()
				});
			}

			return notes;
		}

		private static string BuildPaymentSectionText(RdaEncounterSource encounter)
		{
			var partes = new List<string>();

			if (!string.IsNullOrWhiteSpace(encounter.NombreEps))
				partes.Add($"Entidad responsable reportada para el encuentro: {encounter.NombreEps.Trim()}.");

			if (!string.IsNullOrWhiteSpace(encounter.CodigoEps))
				partes.Add($"Código de entidad: {encounter.CodigoEps.Trim()}.");

			if (!string.IsNullOrWhiteSpace(encounter.NroAfiliacion))
				partes.Add($"Número de afiliación: {encounter.NroAfiliacion.Trim()}.");

			if (!string.IsNullOrWhiteSpace(encounter.TipoUsuario))
				partes.Add($"Tipo de afiliación: {encounter.TipoUsuario.Trim()}.");

			if (!partes.Any())
				return "No se reporta pagador en este encuentro.";

			return string.Join(" ", partes);
		}

		private static string BuildDemographicSectionText(RdaEncounterSource encounter)
		{
			var partes = new List<string>();

			var sexo = MapSexoNarrative(encounter.Sexo);
			if (!string.IsNullOrWhiteSpace(sexo))
				partes.Add($"Paciente {sexo}");

			var ciudadDepto = BuildCityDepartmentCountry(encounter.NombreCiudad, encounter.NombreDepartamento);
			if (!string.IsNullOrWhiteSpace(ciudadDepto))
				partes.Add($"residente en {ciudadDepto}.");

			var telefono = SanitizePhone(encounter.Telefono);
			var celular = SanitizePhone(encounter.Celular);

			if (!string.IsNullOrWhiteSpace(telefono))
				partes.Add($"Teléfono de contacto: {telefono}.");

			if (!string.IsNullOrWhiteSpace(celular))
				partes.Add($"Celular: {celular}.");

			if (!string.IsNullOrWhiteSpace(encounter.Direccion))
				partes.Add($"Dirección registrada: {encounter.Direccion.Trim()}.");

			if (!partes.Any())
				return "No se reportan datos demográficos ampliados para este encuentro.";

			return string.Join(" ", partes);
		}

		private static string BuildRiskFactorsSectionText(RdaAntecedentesSource antecedentes)
		{
			var partes = new List<string>();

			if (!string.IsNullOrWhiteSpace(antecedentes.Fuma))
				partes.Add(ToYesNoNarrative(antecedentes.Fuma, "El paciente refiere tabaquismo actual.", "El paciente niega tabaquismo actual."));

			if (!string.IsNullOrWhiteSpace(antecedentes.Diabetes))
				partes.Add(ToYesNoNarrative(antecedentes.Diabetes, "Refiere diabetes.", "No refiere diabetes."));

			if (!string.IsNullOrWhiteSpace(antecedentes.Gastricos))
				partes.Add(ToYesNoNarrative(antecedentes.Gastricos, "Refiere alteraciones gástricas, renales o respiratorias relevantes.", "No refiere alteraciones gástricas, renales o respiratorias relevantes."));

			if (!string.IsNullOrWhiteSpace(antecedentes.Hepatitis))
				partes.Add(ToYesNoNarrative(antecedentes.Hepatitis, "Refiere hepatitis o antecedente hepático relevante.", "No refiere hepatitis."));

			if (!string.IsNullOrWhiteSpace(antecedentes.Hemorragias))
				partes.Add(ToYesNoNarrative(antecedentes.Hemorragias, "Refiere antecedente hemorrágico o hematológico.", "No refiere antecedentes hemorrágicos."));

			if (!string.IsNullOrWhiteSpace(antecedentes.Presion))
				partes.Add($"Presión arterial / cardiopatías: {antecedentes.Presion.Trim()}.");

			if (!string.IsNullOrWhiteSpace(antecedentes.Asma))
				partes.Add($"Asma: {antecedentes.Asma.Trim()}.");

			if (!string.IsNullOrWhiteSpace(antecedentes.Embarazo))
				partes.Add($"Embarazo: {antecedentes.Embarazo.Trim()}.");

			if (!partes.Any())
				return "No se identifican factores de riesgo clínico adicionales en esta consulta.";

			return string.Join(" ", partes);
		}

		private static string BuildMedicationSectionText(RdaAntecedentesSource antecedentes)
		{
			if (!string.IsNullOrWhiteSpace(antecedentes.MedicamentosActualesTexto))
				return $"Medicamentos o sustancias referidas por el paciente: {antecedentes.MedicamentosActualesTexto.Trim()}.";

			return "El paciente no refiere medicamentos actuales ni uso activo de sustancias naturales al momento del encuentro.";
		}

		private static string BuildOrdersSectionText(RdaProcedimientoSource procedimiento)
		{
			var partes = new List<string>();

			if (!string.IsNullOrWhiteSpace(procedimiento.CodigoProcedimiento))
				partes.Add($"Se registra procedimiento relacionado con código {procedimiento.CodigoProcedimiento.Trim()}.");

			if (!string.IsNullOrWhiteSpace(procedimiento.NombreProcedimiento))
				partes.Add($"Procedimiento reportado: {procedimiento.NombreProcedimiento.Trim()}.");

			if (!string.IsNullOrWhiteSpace(procedimiento.NumeroAutorizacion))
				partes.Add($"Número de autorización asociado: {procedimiento.NumeroAutorizacion.Trim()}.");

			if (!partes.Any())
				return "No se generaron órdenes externas, remisiones, interconsultas ni solicitudes de servicios complementarios al egreso de la consulta.";

			return string.Join(" ", partes);
		}

		private static string BuildCityDepartmentCountry(string ciudad, string departamento)
		{
			var partes = new List<string>();

			if (!string.IsNullOrWhiteSpace(ciudad))
				partes.Add(ciudad.Trim());

			if (!string.IsNullOrWhiteSpace(departamento))
				partes.Add(departamento.Trim());

			if (!partes.Any())
				return null;

			partes.Add("Colombia");
			return string.Join(", ", partes);
		}

		private static string ToYesNoNarrative(string rawValue, string siText, string noText)
		{
			var value = (rawValue ?? string.Empty).Trim().ToUpperInvariant();

			if (value == "SI" || value == "SÍ" || value == "S")
				return siText;

			if (value == "NO" || value == "N")
				return noText;

			return rawValue.Trim() + ".";
		}

		private static string MapSexoNarrative(string sexo)
		{
			var s = (sexo ?? string.Empty).Trim().ToUpperInvariant();

			if (s == "M" || s == "MASCULINO")
				return "masculino";

			if (s == "F" || s == "FEMENINO")
				return "femenina";

			return null;
		}

		private static string MapCauseDisplay(string code)
		{
			switch ((code ?? string.Empty).Trim())
			{
				case "38": return "ENFERMEDAD GENERAL";
				default: return "CAUSA DE ATENCIÓN";
			}
		}

		private static string MapDiagnosisTypeDisplay(string code)
		{
			switch ((code ?? string.Empty).Trim())
			{
				case "01": return "Impresión diagnóstica";
				case "02": return "Confirmado Nuevo";
				case "03": return "Confirmado Repetido";
				default: return "Tipo diagnóstico";
			}
		}

		private static FhirMeta BuildMeta(string profile)
		{
			return new FhirMeta
			{
				profile = new List<string> { profile }
			};
		}

		private static FhirCompositionSection BuildNarrativeSection(string title, string loincCode, string loincDisplay, string texto)
		{
			return new FhirCompositionSection
			{
				title = title,
				code = BuildCodeableConcept(SystemLoinc, loincCode, loincDisplay),
				text = BuildNarrative(texto),
				entry = new List<FhirReference>()
			};
		}

		private static FhirNarrative BuildNarrative(string texto)
		{
			return new FhirNarrative
			{
				status = "generated",
				div = $"<div xmlns=\"http://www.w3.org/1999/xhtml\">{EscapeHtml(texto)}</div>"
			};
		}

		private static string EscapeHtml(string texto)
		{
			if (string.IsNullOrWhiteSpace(texto))
				return string.Empty;

			return texto
				.Replace("&", "&amp;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;")
				.Replace("\"", "&quot;");
		}

		private static FhirCodeableConcept BuildCodeableConcept(string system, string code, string display, string text = null)
		{
			var concept = new FhirCodeableConcept
			{
				text = text
			};

			if (!string.IsNullOrWhiteSpace(system) ||
				!string.IsNullOrWhiteSpace(code) ||
				!string.IsNullOrWhiteSpace(display))
			{
				concept.coding.Add(new FhirCoding
				{
					system = system,
					code = code,
					display = display
				});
			}

			return concept;
		}

		private static FhirCodeableConcept BuildCodeableConceptWithTwoCodings(
			string system1, string code1, string display1,
			string system2, string code2, string display2)
		{
			return new FhirCodeableConcept
			{
				coding = new List<FhirCoding>
				{
					new FhirCoding { system = system1, code = code1, display = display1 },
					new FhirCoding { system = system2, code = code2, display = display2 }
				}
			};
		}

		private static void AddNationalPersonIdentifier(List<FhirIdentifier> list, string tipoDocumento, string numeroDocumento, bool includeSystem)
		{
			if (string.IsNullOrWhiteSpace(numeroDocumento))
				return;

			list.Add(new FhirIdentifier
			{
				id = "NationalPersonIdentifier-0",
				use = "official",
				type = BuildCodeableConceptWithTwoCodings(
					SystemV20203, "PN", "Person number",
					SystemColombianPersonIdentifier, NormalizeTipoDocumento(tipoDocumento), MapTipoDocumentoDisplay(tipoDocumento)),
				system = includeSystem ? SystemNationalPersonIdentifier : null,
				value = numeroDocumento.Trim()
			});
		}

		private static string NormalizeTipoDocumento(string tipoDocumento)
		{
			var tipo = (tipoDocumento ?? string.Empty).Trim().ToUpperInvariant();
			return string.IsNullOrWhiteSpace(tipo) ? "CC" : tipo;
		}

		private static string MapTipoDocumentoDisplay(string tipoDocumento)
		{
			switch (NormalizeTipoDocumento(tipoDocumento))
			{
				case "CC": return "Cédula ciudadanía";
				case "TI": return "Tarjeta identidad";
				case "CE": return "Cédula extranjería";
				case "PA": return "Pasaporte";
				default: return "Documento";
			}
		}

		private static string MapGender(string sexo)
		{
			var s = (sexo ?? string.Empty).Trim().ToUpperInvariant();

			if (s == "M" || s == "MASCULINO")
				return "male";

			if (s == "F" || s == "FEMENINO")
				return "female";

			return "unknown";
		}

		private static string MapGenderIdentityCode(string sexo)
		{
			var s = (sexo ?? string.Empty).Trim().ToUpperInvariant();
			return s == "F" || s == "FEMENINO" ? "02" : "01";
		}

		private static string MapGenderIdentityDisplay(string sexo)
		{
			var s = (sexo ?? string.Empty).Trim().ToUpperInvariant();
			return s == "F" || s == "FEMENINO" ? "Femenino" : "Masculino";
		}

		private static string MapBiologicalGenderCode(string sexo)
		{
			var s = (sexo ?? string.Empty).Trim().ToUpperInvariant();
			return s == "F" || s == "FEMENINO" ? "02" : "01";
		}

		private static string MapBiologicalGenderDisplay(string sexo)
		{
			var s = (sexo ?? string.Empty).Trim().ToUpperInvariant();
			return s == "F" || s == "FEMENINO" ? "Mujer" : "Hombre";
		}

		private static string BuildDisplayName(string nombres, string apellidos)
		{
			return FirstNonEmpty($"{nombres} {apellidos}".Trim(), nombres, apellidos, "SIN NOMBRE");
		}

		private static List<string> SplitWords(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return new List<string>();

			return text
				.Split(' ', StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim())
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.ToList();
		}

		private static string BuildFamilyFromFullName(string fullName)
		{
			if (string.IsNullOrWhiteSpace(fullName))
				return null;

			var parts = SplitWords(fullName);

			if (parts.Count == 1) return parts[0];
			if (parts.Count == 2) return parts[1];
			if (parts.Count == 3) return parts[2];

			return string.Join(" ", parts.Skip(parts.Count - 2));
		}

		private static List<string> BuildGivenFromFullName(string fullName)
		{
			var parts = SplitWords(fullName);

			if (parts.Count == 0) return new List<string>();
			if (parts.Count == 1) return new List<string> { parts[0] };
			if (parts.Count == 2) return new List<string> { parts[0] };
			if (parts.Count == 3) return new List<string> { parts[0], parts[1] };

			return parts.Take(parts.Count - 2).ToList();
		}

		private static string BuildPersonId(string tipoDocumento, string numeroDocumento, int fallback, string prefix)
		{
			if (!string.IsNullOrWhiteSpace(tipoDocumento) && !string.IsNullOrWhiteSpace(numeroDocumento))
				return $"{NormalizeTipoDocumento(tipoDocumento)}-{SafeToken(numeroDocumento)}";

			return $"{prefix}-{fallback}";
		}

		private static string SafeToken(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return "NA";

			return value.Trim()
				.Replace(" ", "-")
				.Replace("/", "-")
				.Replace("\\", "-");
		}

		private static string FirstNonEmpty(params string[] values)
		{
			foreach (var value in values)
			{
				if (!string.IsNullOrWhiteSpace(value))
					return value.Trim();
			}

			return null;
		}

		private static string NullIfWhite(string value)
		{
			return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
		}

		private static string NormalizeMunicipalityCode(string depto, string municipio)
		{
			var d = (depto ?? string.Empty).Trim();
			var m = (municipio ?? string.Empty).Trim();

			if (string.IsNullOrWhiteSpace(m))
				return null;

			if (m.Length == 5)
				return m;

			if (m.Length == 3 && d.Length == 2)
				return d + m;

			return m;
		}

		private static string MapCupsDisplay(string codigo, string nombreConsulta)
		{
			var code = (codigo ?? string.Empty).Trim();

			if (code == "890201")
				return "CONSULTA DE PRIMERA VEZ POR MEDICINA GENERAL";

			return FirstNonEmpty(nombreConsulta, "CONSULTA GENERAL");
		}
	}
}