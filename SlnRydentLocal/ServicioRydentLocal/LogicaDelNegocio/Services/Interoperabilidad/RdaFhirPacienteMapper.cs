using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.Fhir;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public static class RdaFhirPacienteMapper
	{
		private const string SystemNationalPersonIdentifier = "https://fhir.minsalud.gov.co/rda/NamingSystem/RNEC";
		private const string SystemPrestadorReps = "https://fhir.minsalud.gov.co/rda/NamingSystem/REPS";
		private const string SystemDian = "https://fhir.minsalud.gov.co/rda/NamingSystem/DIAN";

		private const string SystemV20203 = "http://terminology.hl7.org/CodeSystem/v2-0203";
		private const string SystemColombianPersonIdentifier = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianPersonIdentifier";
		private const string SystemColombianOrganizationIdentifiers = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianOrganizationIdentifiers";
		private const string SystemLoinc = "http://loinc.org";
		private const string SystemConditionClinical = "http://terminology.hl7.org/CodeSystem/condition-clinical";
		private const string SystemConditionCategory = "http://terminology.hl7.org/CodeSystem/condition-category";
		private const string SystemTipoAlergia = "https://fhir.minsalud.gov.co/rda/CodeSystem/TipoAlergia";
		private const string SystemColombianTechModality = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianTechModality";
		private const string SystemGrupoServicios = "https://fhir.minsalud.gov.co/rda/CodeSystem/GrupoServicios";
		private const string SystemProviderClass = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianProviderClass";
		private const string SystemLegalNature = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianLegalNatureType";
		private const string SystemDivipola = "https://fhir.minsalud.gov.co/rda/CodeSystem/DIVIPOLA";
		private const string SystemIso31661 = "https://fhir.minsalud.gov.co/rda/CodeSystem/ISO31661";
		private const string SystemResidenceZone = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianResidenceZone";
		private const string SystemColombianEthnicGroup = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianEthnicGroup";
		private const string SystemColombianDisabilityClassification = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianDisabilityClassification";
		private const string SystemColombianGenderIdentity = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianGenderIdentity";
		private const string SystemColombianGenderGroup = "https://fhir.minsalud.gov.co/rda/CodeSystem/ColombianGenderGroup";
		private const string SystemParentescoAntecedente = "https://fhir.minsalud.gov.co/rda/CodeSystem/ParentescoAntecedente";

		private const string ProfileComposition = "https://fhir.minsalud.gov.co/rda/StructureDefinition/CompositionPatientStatementRDA";
		private const string ProfilePatient = "https://fhir.minsalud.gov.co/rda/StructureDefinition/PatientRDA";
		private const string ProfileOrganization = "https://fhir.minsalud.gov.co/rda/StructureDefinition/CareDeliveryOrganizationRDA";
		private const string ProfilePractitioner = "https://fhir.minsalud.gov.co/rda/StructureDefinition/PractitionerRDA";
		private const string ProfileCondition = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ConditionStatementRDA";
		private const string ProfileAllergy = "https://fhir.minsalud.gov.co/rda/StructureDefinition/AllergyIntoleranceStatementRDA";
		private const string ProfileMedication = "https://fhir.minsalud.gov.co/rda/StructureDefinition/MedicationStatementRDA";
		private const string ProfileFamily = "https://fhir.minsalud.gov.co/rda/StructureDefinition/FamilyMemberHistoryRDA";

		private const string UrlFathersFamilyName = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionFathersFamilyName";
		private const string UrlMothersFamilyName = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionMothersFamilyName";
		private const string UrlPatientNationality = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionPatientNationality";
		private const string UrlPatientEthnicity = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionPatientEthnicity";
		private const string UrlPatientDisability = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionPatientDisability";
		private const string UrlPatientGenderIdentity = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionPatientGenderIdentity";
		private const string UrlBiologicalGender = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionBiologicalGender";
		private const string UrlBirthTime = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionBirthTime";
		private const string UrlResidenceZone = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionResidenceZone";
		private const string UrlDivipolaMunicipality = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionDivipolaMunicipality";
		private const string UrlCountryCode = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionCountryCode";

		public static FhirBundleRdaConsulta Map(RdaDocumentoInterno source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			var consulta = source.Documento?.Consulta ?? new RdaConsultaSource();
			var encounter = consulta.Encounter ?? new RdaEncounterSource();
			var antecedentes = consulta.Antecedentes ?? new RdaAntecedentesSource();
			var diagnostico = consulta.Diagnostico ?? new RdaDiagnosticoSource();
			var prestador = source.Documento?.Prestador ?? new RdaPrestadorSource();

			var fecha = encounter.FechaConsulta ?? DateTime.Now.Date;
			var fechaDoc = new DateTimeOffset(
				fecha.Year,
				fecha.Month,
				fecha.Day,
				10, 0, 0,
				TimeSpan.FromHours(-5));

			var patientId = BuildPersonId(encounter.TipoDocumento, encounter.NumeroDocumento, encounter.IdAnamnesis, "patient");
			var practitionerId = BuildPersonId(prestador.TipoDocumentoDoctor, prestador.NumeroDocumentoDoctor, prestador.IdDoctor ?? 0, "practitioner");
			var organizationId = SafeToken(FirstNonEmpty(prestador.CodigoPrestador, prestador.NitPrestador, "ips"));

			var patientRef = $"#{patientId}";
			var practitionerRef = $"#{practitionerId}";
			var organizationRef = $"#{organizationId}";

			const string conditionId = "Condition-0";
			const string allergyId = "AllergyIntolerance-0";
			const string medicationId = "MedicationStatement-0";
			const string familyId = "FamilyMemberHistory-0";

			var conditionRef = $"#{conditionId}";
			var allergyRef = $"#{allergyId}";
			var medicationRef = $"#{medicationId}";
			var familyRef = $"#{familyId}";

			var bundle = new FhirBundleRdaConsulta
			{
				resourceType = "Bundle",
				language = "es-CO",
				type = "document",
				timestamp = fechaDoc
			};

			var composition = new FhirComposition
			{
				resourceType = "Composition",
				meta = BuildMeta(ProfileComposition),
				status = "final",
				type = BuildCodeableConcept(SystemLoinc, "102089-0", "FHIR resource patient medical record"),
				subject = new FhirReference { reference = patientRef },
				date = fechaDoc,
				title = "Resumen Digital de Atención en Salud - RDA de antecedentes manifestados por el paciente",
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
				code = new List<FhirCodeableConcept>
				{
					BuildCodeableConcept(SystemColombianTechModality, "01", "Intramural"),
					BuildCodeableConcept(SystemGrupoServicios, "01", "Consulta externa")
				},
				period = new FhirPeriod
				{
					start = fechaDoc.ToString("yyyy-MM-ddTHH:mm:sszzz"),
					end = fechaDoc.AddMinutes(15).ToString("yyyy-MM-ddTHH:mm:sszzz")
				},
				detail = new List<FhirReference>()
			});

			composition.section.Add(new FhirCompositionSection
			{
				title = "Historial de diagnósticos de problemas de salud",
				code = BuildCodeableConcept(SystemLoinc, "11450-4", "Problem list - Reported"),
				text = BuildNarrative(BuildConditionNarrative(antecedentes, diagnostico)),
				entry = new List<FhirReference>
				{
					new FhirReference { reference = conditionRef }
				}
			});

			composition.section.Add(new FhirCompositionSection
			{
				title = "Historial de alergias, intolerancias y reacciones adversas",
				code = BuildCodeableConcept(SystemLoinc, "48765-2", "Allergies and adverse reactions Document"),
				text = BuildNarrative(BuildAllergyNarrative(antecedentes)),
				entry = new List<FhirReference>
				{
					new FhirReference { reference = allergyRef }
				}
			});

			composition.section.Add(new FhirCompositionSection
			{
				title = "Historial de medicamentos",
				code = BuildCodeableConcept(SystemLoinc, "10160-0", "History of Medication use Narrative"),
				text = BuildNarrative(BuildMedicationNarrative(antecedentes)),
				entry = new List<FhirReference>
				{
					new FhirReference { reference = medicationRef }
				}
			});

			composition.section.Add(new FhirCompositionSection
			{
				title = "Historial de antecedentes familiares",
				code = BuildCodeableConcept(SystemLoinc, "10157-6", "History of family member diseases Narrative"),
				text = BuildNarrative(BuildFamilyNarrative(antecedentes)),
				entry = new List<FhirReference>
				{
					new FhirReference { reference = familyRef }
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
			AddNationalPersonIdentifier(patient.identifier, encounter.TipoDocumento, encounter.NumeroDocumento);

			var patientName = BuildOfficialHumanName(
				encounter.Nombres,
				encounter.Apellidos,
				addFamilyExtensions: true);

			if (patientName != null)
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

			AddOrganizationAddress(organization, prestador);
			AddOrganizationTelecom(organization, prestador);

			var practitioner = new FhirPractitioner
			{
				resourceType = "Practitioner",
				id = practitionerId,
				meta = BuildMeta(ProfilePractitioner),
				active = true
			};

			AddPractitionerIdentifierLikeOkJson(practitioner.identifier, prestador.TipoDocumentoDoctor, prestador.NumeroDocumentoDoctor);

			var practitionerFullName = FirstNonEmpty(prestador.NombreDoctor, encounter.Doctor, "Profesional no identificado");
			var practitionerName = BuildHumanNameFromFullName(practitionerFullName);

			if (practitionerName != null)
				practitioner.name.Add(practitionerName);

			var condition = new FhirCondition
			{
				resourceType = "Condition",
				id = conditionId,
				meta = BuildMeta(ProfileCondition),
				clinicalStatus = BuildCodeableConcept(SystemConditionClinical, "active", "Active"),
				verificationStatus = BuildCodeableConcept(null, "unconfirmed", "Unconfirmed"),
				subject = new FhirReference { reference = patientRef },
				code = new FhirCodeableConcept
				{
					text = BuildConditionNarrative(antecedentes, diagnostico)
				}
			};
			condition.category.Add(BuildCodeableConcept(SystemConditionCategory, "encounter-diagnosis", "Encounter Diagnosis"));

			var allergy = new FhirAllergyIntolerance
			{
				resourceType = "AllergyIntolerance",
				id = allergyId,
				meta = BuildMeta(ProfileAllergy),
				clinicalStatus = BuildCodeableConcept(null, "active", "Active"),
				verificationStatus = BuildCodeableConcept(null, "unconfirmed", "Unconfirmed"),
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
					text = BuildAllergyNarrative(antecedentes)
				},
				patient = new FhirReference { reference = patientRef }
			};

			var medication = new FhirMedicationStatement
			{
				resourceType = "MedicationStatement",
				id = medicationId,
				meta = BuildMeta(ProfileMedication),
				status = "completed",
				subject = new FhirReference { reference = patientRef },
				medicationCodeableConcept = new FhirCodeableConcept
				{
					text = BuildMedicationNarrative(antecedentes)
				}
			};

			var family = new FhirFamilyMemberHistory
			{
				resourceType = "FamilyMemberHistory",
				id = familyId,
				meta = BuildMeta(ProfileFamily),
				status = "partial",
				patient = new FhirReference { reference = patientRef },
				relationship = new FhirCodeableConcept
				{
					coding = new List<FhirCoding>
					{
						new FhirCoding
						{
							system = SystemParentescoAntecedente,
							code = "01",
							display = "Padres"
						}
					}
				}
			};

			family.condition.Add(new FhirFamilyMemberHistoryCondition
			{
				code = new FhirCodeableConcept
				{
					text = BuildFamilyNarrative(antecedentes)
				}
			});

			bundle.entry.Add(new FhirBundleEntry { resource = composition });
			bundle.entry.Add(new FhirBundleEntry { resource = patient });
			bundle.entry.Add(new FhirBundleEntry { resource = organization });
			bundle.entry.Add(new FhirBundleEntry { resource = practitioner });
			bundle.entry.Add(new FhirBundleEntry { resource = condition });
			bundle.entry.Add(new FhirBundleEntry { resource = allergy });
			bundle.entry.Add(new FhirBundleEntry { resource = family });
			bundle.entry.Add(new FhirBundleEntry { resource = medication });

			return bundle;
		}

		private static void AddPatientAddress(FhirPatient patient, RdaEncounterSource encounter)
		{
			if (patient == null)
				return;

			var city = NullIfWhite(encounter.NombreCiudad);
			var municipalityCode = NormalizeMunicipalityCode(encounter.CodigoCiudad, encounter.CodigoDepartamento);
			var shouldAddAddress =
				!string.IsNullOrWhiteSpace(city) ||
				!string.IsNullOrWhiteSpace(municipalityCode);

			if (!shouldAddAddress)
				return;

			var address = new FhirAddress
			{
				id = "HomeAddress-0",
				use = "home",
				type = "physical",
				city = city,
				country = "Colombia"
			};

			if (!string.IsNullOrWhiteSpace(municipalityCode))
			{
				address._city = new FhirPrimitiveElement();
				address._city.extension.Add(new FhirExtension
				{
					url = UrlDivipolaMunicipality,
					valueCoding = new FhirCoding
					{
						system = SystemDivipola,
						code = municipalityCode
					}
				});
			}

			address._country = new FhirPrimitiveElement();
			address._country.extension.Add(new FhirExtension
			{
				url = UrlCountryCode,
				valueCoding = new FhirCoding
				{
					system = SystemIso31661,
					code = "170"
				}
			});

			address.extension.Add(new FhirExtension
			{
				url = UrlResidenceZone,
				valueCoding = new FhirCoding
				{
					system = SystemResidenceZone,
					code = MapResidenceZoneCode(encounter.ZonaRecidencial),
					display = MapResidenceZoneDisplay(encounter.ZonaRecidencial)
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

		private static void AddOrganizationAddress(FhirOrganization organization, RdaPrestadorSource prestador)
		{
			if (organization == null)
				return;

			var city = NullIfWhite(prestador.CiudadPrestador);
			var addressText = NullIfWhite(prestador.DireccionPrestador);

			if (string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(addressText))
				return;

			organization.address.Add(new FhirAddress
			{
				use = "work",
				type = "physical",
				text = FirstNonEmpty(addressText, "Sin información"),
				city = city,
				country = "Colombia"
			});
		}

		private static void AddOrganizationTelecom(FhirOrganization organization, RdaPrestadorSource prestador)
		{
			if (organization == null)
				return;

			var telefono = SanitizePhone(prestador.TelefonoPrestador);
			if (string.IsNullOrWhiteSpace(telefono))
				return;

			organization.telecom.Add(new FhirContactPoint
			{
				system = "phone",
				value = telefono
			});
		}

		private static void AddDefaultPatientExtensions(FhirPatient patient, RdaEncounterSource encounter)
		{
			patient.extension.Add(new FhirExtension
			{
				url = UrlPatientNationality,
				valueCoding = new FhirCoding
				{
					system = SystemIso31661,
					code = "170",
					display = "Colombia"
				}
			});

			patient.extension.Add(new FhirExtension
			{
				url = UrlPatientEthnicity,
				valueCoding = new FhirCoding
				{
					system = SystemColombianEthnicGroup,
					code = "6",
					display = "Otras etnias"
				}
			});

			patient.extension.Add(new FhirExtension
			{
				url = UrlPatientDisability,
				valueCoding = new FhirCoding
				{
					system = SystemColombianDisabilityClassification,
					code = "08",
					display = "Sin discapacidad"
				}
			});

			patient.extension.Add(new FhirExtension
			{
				url = UrlPatientGenderIdentity,
				valueCoding = new FhirCoding
				{
					system = SystemColombianGenderIdentity,
					code = MapGenderIdentityCode(encounter.Sexo),
					display = MapGenderIdentityDisplay(encounter.Sexo)
				}
			});

			patient._gender = new FhirPrimitiveElement();
			patient._gender.extension.Add(new FhirExtension
			{
				url = UrlBiologicalGender,
				valueCoding = new FhirCoding
				{
					system = SystemColombianGenderGroup,
					code = MapBiologicalGenderCode(encounter.Sexo),
					display = MapBiologicalGenderDisplay(encounter.Sexo)
				}
			});

			if (encounter.FechaNacimiento.HasValue)
			{
				patient._birthDate = new FhirPrimitiveElement();
				patient._birthDate.extension.Add(new FhirExtension
				{
					url = UrlBirthTime,
					valueTime = "08:00:00"
				});
			}
		}

		private static FhirHumanName BuildOfficialHumanName(string nombres, string apellidos, bool addFamilyExtensions)
		{
			var family = NormalizeSpaces(apellidos);
			var given = SplitWords(nombres);

			if (string.IsNullOrWhiteSpace(family) && (given == null || given.Count == 0))
				return null;

			var name = new FhirHumanName
			{
				use = "official",
				family = family,
				given = given,
				text = BuildDisplayName(nombres, apellidos)
			};

			if (addFamilyExtensions && !string.IsNullOrWhiteSpace(family))
			{
				var ap = SplitWords(family);

				if (ap.Count > 0)
				{
					name._family ??= new FhirPrimitiveElement();
					name._family.extension.Add(new FhirExtension
					{
						url = UrlFathersFamilyName,
						valueString = ap[0]
					});
				}

				if (ap.Count > 1)
				{
					name._family ??= new FhirPrimitiveElement();
					name._family.extension.Add(new FhirExtension
					{
						url = UrlMothersFamilyName,
						valueString = string.Join(" ", ap.Skip(1))
					});
				}
			}

			return name;
		}

		private static FhirHumanName BuildHumanNameFromFullName(string fullName)
		{
			if (string.IsNullOrWhiteSpace(fullName))
				return null;

			var family = BuildFamilyFromFullName(fullName);
			var givenText = BuildGivenTextFromFullName(fullName);

			return BuildOfficialHumanName(givenText, family, true);
		}

		private static string BuildConditionNarrative(RdaAntecedentesSource antecedentes, RdaDiagnosticoSource diagnostico)
		{
			var partes = new List<string>();

			var enfPrevias = CleanClinicalText(antecedentes.EnfermedadesPreviasTexto);
			if (!string.IsNullOrWhiteSpace(enfPrevias))
				partes.Add($"Antecedentes patológicos: {enfPrevias}");

			var enfActual = CleanClinicalText(antecedentes.EnfermedadActual);
			if (!string.IsNullOrWhiteSpace(enfActual))
				partes.Add($"Enfermedad actual: {enfActual}");

			var dx = CleanClinicalText(diagnostico?.NombreDiagnostico1);
			if (string.IsNullOrWhiteSpace(dx))
				dx = CleanClinicalText(diagnostico?.Diagnostico1);

			if (!string.IsNullOrWhiteSpace(dx))
				partes.Add($"Diagnóstico referido: {dx}");

			return partes.Any()
				? string.Join(". ", partes) + "."
				: "Sin antecedentes patológicos relevantes.";
		}

		private static string BuildAllergyNarrative(RdaAntecedentesSource antecedentes)
		{
			var alergias = CleanClinicalText(antecedentes.AlergiasTexto);
			if (!string.IsNullOrWhiteSpace(alergias))
				return alergias;

			return "Sin alergias conocidas";
		}

		private static string BuildMedicationNarrative(RdaAntecedentesSource antecedentes)
		{
			var meds = CleanClinicalText(antecedentes.MedicamentosActualesTexto);
			if (!string.IsNullOrWhiteSpace(meds))
				return meds;

			return "No refiere medicamentos actuales";
		}

		private static string BuildFamilyNarrative(RdaAntecedentesSource antecedentes)
		{
			var fam = CleanClinicalText(antecedentes.ObservacionesAntecedentes);
			if (!string.IsNullOrWhiteSpace(fam))
				return fam;

			return "Sin antecedentes familiares relevantes";
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

		private static string CleanClinicalText(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;

			var txt = NormalizeSpaces(value);

			if (string.IsNullOrWhiteSpace(txt))
				return null;

			var up = txt.ToUpperInvariant();

			if (up == "NO" ||
				up == "N/A" ||
				up == "NA" ||
				up == "-" ||
				up == "." ||
				up == "NINGUNO" ||
				up == "NINGUNA" ||
				up == "SIN DATO" ||
				up == "NULL")
			{
				return null;
			}

			return txt;
		}

		private static string SanitizePhone(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;

			var txt = value.Trim();

			if (txt.Equals("NO", StringComparison.OrdinalIgnoreCase) ||
				txt.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
				txt.Equals("-", StringComparison.OrdinalIgnoreCase) ||
				txt.Equals(".", StringComparison.OrdinalIgnoreCase) ||
				txt.Equals("NULL", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			return txt;
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

		private static FhirMeta BuildMeta(string profile)
		{
			return new FhirMeta
			{
				profile = new List<string> { profile }
			};
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

		private static void AddNationalPersonIdentifier(List<FhirIdentifier> list, string tipoDocumento, string numeroDocumento)
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
				system = SystemNationalPersonIdentifier,
				value = numeroDocumento.Trim()
			});
		}

		private static void AddPractitionerIdentifierLikeOkJson(List<FhirIdentifier> list, string tipoDocumento, string numeroDocumento)
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
				case "RC": return "Registro civil";
				case "CN": return "Certificado de nacido vivo";
				case "PE": return "Permiso especial de permanencia";
				case "SC": return "Salvoconducto";
				case "CD": return "Carné diplomático";
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

		private static string BuildDisplayName(string nombres, string apellidos)
		{
			return NormalizeSpaces(FirstNonEmpty(
				$"{(nombres ?? string.Empty).Trim()} {(apellidos ?? string.Empty).Trim()}".Trim(),
				nombres,
				apellidos,
				"SIN NOMBRE"));
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

		private static string BuildGivenTextFromFullName(string fullName)
		{
			if (string.IsNullOrWhiteSpace(fullName))
				return null;

			var parts = SplitWords(fullName);

			if (parts.Count == 0) return null;
			if (parts.Count == 1) return parts[0];
			if (parts.Count == 2) return parts[0];
			if (parts.Count == 3) return $"{parts[0]} {parts[1]}";

			return string.Join(" ", parts.Take(parts.Count - 2));
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

		private static string NormalizeSpaces(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;

			return string.Join(" ",
				value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
					 .Select(x => x.Trim()));
		}

		private static string NormalizeDepartmentCode(string code)
		{
			if (string.IsNullOrWhiteSpace(code))
				return null;

			var digits = new string(code.Trim().Where(char.IsDigit).ToArray());
			if (string.IsNullOrWhiteSpace(digits))
				return null;

			if (digits.Length == 1)
				digits = "0" + digits;

			if (digits.Length != 2)
				return null;

			return digits;
		}

		private static string NormalizeMunicipalityCode(string cityCode, string departmentCode)
		{
			if (string.IsNullOrWhiteSpace(cityCode))
				return null;

			var cityDigits = new string(cityCode.Trim().Where(char.IsDigit).ToArray());
			var deptDigits = NormalizeDepartmentCode(departmentCode);

			if (string.IsNullOrWhiteSpace(cityDigits))
				return null;

			if (cityDigits.Length == 5)
				return cityDigits;

			if (cityDigits.Length == 3 && !string.IsNullOrWhiteSpace(deptDigits))
				return deptDigits + cityDigits;

			return null;
		}

		private static string MapResidenceZoneCode(string zonaRecidencial)
		{
			var value = (zonaRecidencial ?? string.Empty).Trim().ToUpperInvariant();

			if (value == "RURAL")
				return "02";

			if (value == "URBANA")
				return "01";

			return "01";
		}

		private static string MapResidenceZoneDisplay(string zonaRecidencial)
		{
			var value = (zonaRecidencial ?? string.Empty).Trim().ToUpperInvariant();

			if (value == "RURAL")
				return "Rural";

			return "Urbana";
		}
	}
}