using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.Fhir;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public static class RdaFhirConsultaValidator
	{
		private const string SystemNationalPersonIdentifier = "https://fhir.minsalud.gov.co/rda/NamingSystem/RNEC";
		private const string SystemPrestadorReps = "https://fhir.minsalud.gov.co/rda/NamingSystem/REPS";
		private const string SystemLoinc = "http://loinc.org";
		private const string ExpectedCompositionTypeCode = "51845-6";

		private static readonly string[] RequiredSectionCodes =
		{
			"11450-4",
			"48765-2",
			"10160-0",
			"61146-1",
			"55107-7"
		};

		public static RdaFhirValidationResult Validate(FhirBundleRdaConsulta bundle)
		{
			if (bundle == null)
				return Fail("El bundle FHIR es nulo.");

			if (bundle.resourceType != "Bundle")
				return Fail("resourceType debe ser 'Bundle'.");

			if (bundle.type != "document")
				return Fail("Bundle.type debe ser 'document'.");

			if (bundle.entry == null || !bundle.entry.Any())
				return Fail("El bundle no tiene entries.");

			if (bundle.entry.Any(x => x == null || x.resource == null))
				return Fail("Todos los entries deben tener resource.");

			if (bundle.entry.First().resource is not FhirComposition composition)
				return Fail("El primer entry debe ser Composition.");

			if (!HasConceptContent(composition.type))
				return Fail("Composition.type es obligatorio.");

			if (!IsExpectedCompositionType(composition.type))
				return Fail("Composition.type debe ser LOINC 51845-6.");

			if (composition.subject == null || string.IsNullOrWhiteSpace(composition.subject.reference))
				return Fail("Composition.subject.reference es obligatorio.");

			if (composition.encounter == null || string.IsNullOrWhiteSpace(composition.encounter.reference))
				return Fail("Composition.encounter.reference es obligatorio.");

			if (composition.author == null || !composition.author.Any())
				return Fail("Composition.author es obligatorio.");

			if (composition.custodian == null || string.IsNullOrWhiteSpace(composition.custodian.reference))
				return Fail("Composition.custodian.reference es obligatorio.");

			if (composition.attester == null || !composition.attester.Any())
				return Fail("Composition.attester es obligatorio.");

			if (composition.event_ == null || !composition.event_.Any())
				return Fail("Composition.event es obligatorio.");

			foreach (var ev in composition.event_)
			{
				if (ev?.period == null ||
					string.IsNullOrWhiteSpace(ev.period.start) ||
					string.IsNullOrWhiteSpace(ev.period.end))
				{
					return Fail("Composition.event.period.start/end es obligatorio.");
				}
			}

			if (composition.section == null || !composition.section.Any())
				return Fail("Composition.section es obligatorio.");

			foreach (var requiredSection in RequiredSectionCodes)
			{
				if (!HasSectionCode(composition.section, requiredSection))
					return Fail($"Falta la sección obligatoria LOINC {requiredSection}.");
			}

			var resourcesByRef = BuildResourceReferenceIndex(bundle);

			var patient = bundle.entry.Select(x => x.resource).OfType<FhirPatient>().FirstOrDefault();
			if (patient == null)
				return Fail("Falta Patient.");

			if (!HasValidPatientIdentifier(patient.identifier))
				return Fail("Patient.identifier debe incluir value limpio y preferiblemente RNEC.");

			var practitioner = bundle.entry.Select(x => x.resource).OfType<FhirPractitioner>().FirstOrDefault();
			if (practitioner == null)
				return Fail("Falta Practitioner.");

			if (!HasUsablePractitionerIdentifier(practitioner.identifier))
				return Fail("Practitioner.identifier debe incluir al menos un value limpio.");

			var organization = bundle.entry.Select(x => x.resource).OfType<FhirOrganization>().FirstOrDefault();
			if (organization == null)
				return Fail("Falta Organization.");

			if (!HasRepsIdentifier(organization.identifier))
				return Fail("Organization.identifier debe incluir REPS.");

			var encounter = bundle.entry.Select(x => x.resource).OfType<FhirEncounter>().FirstOrDefault();
			if (encounter == null)
				return Fail("Falta Encounter.");

			if (!MatchesInternalPattern(encounter.id, "Encounter"))
				return Fail("Encounter.id debe tener formato Encounter-0, Encounter-1, etc.");

			if (encounter.identifier == null || !encounter.identifier.Any())
				return Fail("Encounter.identifier es obligatorio.");

			if (encounter.type == null || encounter.type.Count < 3)
				return Fail("Encounter.type debe tener al menos modalidad, grupo y entorno.");

			if (encounter.period == null ||
				string.IsNullOrWhiteSpace(encounter.period.start) ||
				string.IsNullOrWhiteSpace(encounter.period.end))
			{
				return Fail("Encounter.period.start/end es obligatorio.");
			}

			if (encounter.participant == null || !encounter.participant.Any())
				return Fail("Encounter.participant es obligatorio.");

			if (encounter.location == null || !encounter.location.Any())
				return Fail("Encounter.location es obligatorio.");

			var condition = bundle.entry.Select(x => x.resource).OfType<FhirCondition>().FirstOrDefault();
			if (condition == null)
				return Fail("Falta Condition.");

			if (!MatchesInternalPattern(condition.id, "Condition"))
				return Fail("Condition.id debe tener formato Condition-0, Condition-1, etc.");

			var allergy = bundle.entry.Select(x => x.resource).OfType<FhirAllergyIntolerance>().FirstOrDefault();
			if (allergy != null && !MatchesInternalPattern(allergy.id, "AllergyIntolerance"))
				return Fail("AllergyIntolerance.id debe tener formato AllergyIntolerance-0, AllergyIntolerance-1, etc.");

			var location = bundle.entry.Select(x => x.resource).OfType<FhirLocation>().FirstOrDefault();
			if (location == null)
				return Fail("Falta Location.");

			var documentReference = bundle.entry.Select(x => x.resource).OfType<FhirDocumentReference>().FirstOrDefault();
			if (documentReference == null)
				return Fail("Falta DocumentReference.");

			if (!MatchesInternalPattern(documentReference.id, "DocumentReference"))
				return Fail("DocumentReference.id debe tener formato DocumentReference-0, DocumentReference-1, etc.");

			if (documentReference.content == null || !documentReference.content.Any())
				return Fail("DocumentReference.content es obligatorio.");

			var attachment = documentReference.content.First().attachment;
			if (attachment == null || string.IsNullOrWhiteSpace(attachment.data))
				return Fail("DocumentReference.content[0].attachment.data es obligatorio.");

			if (documentReference.subject == null || string.IsNullOrWhiteSpace(documentReference.subject.reference))
				return Fail("DocumentReference.subject.reference es obligatorio.");

			if (documentReference.context?.encounter == null || !documentReference.context.encounter.Any())
				return Fail("DocumentReference.context.encounter es obligatorio.");

			// =========================
			// Validaciones alineadas con errores reales del servidor
			// =========================

			if (patient.address != null)
			{
				foreach (var addr in patient.address)
				{
					if (addr != null && !string.IsNullOrWhiteSpace(addr.text))
						return Fail("Patient.address.text no debe enviarse para este perfil.");
				}
			}

			if (condition.note != null && condition.note.Any())
				return Fail("Condition.note no debe enviarse para este perfil.");

			if (encounter.note != null && encounter.note.Any())
				return Fail("Encounter.note no debe enviarse para este perfil.");

			if (documentReference.content != null)
			{
				foreach (var content in documentReference.content)
				{
					if (content?.attachment != null &&
						!string.IsNullOrWhiteSpace(content.attachment.contentType))
					{
						return Fail("DocumentReference.content.attachment.contentType no debe enviarse para este perfil.");
					}
				}
			}

			if (!ReferenceExists(resourcesByRef, composition.subject.reference))
				return Fail($"No existe referencia {composition.subject.reference}");

			if (!ReferenceExists(resourcesByRef, composition.encounter.reference))
				return Fail($"No existe referencia {composition.encounter.reference}");

			if (!ReferenceExists(resourcesByRef, composition.custodian.reference))
				return Fail($"No existe referencia {composition.custodian.reference}");

			foreach (var author in composition.author)
			{
				if (author == null || string.IsNullOrWhiteSpace(author.reference))
					return Fail("Composition.author.reference inválido.");

				if (!ReferenceExists(resourcesByRef, author.reference))
					return Fail($"No existe referencia {author.reference}");
			}

			foreach (var attester in composition.attester)
			{
				if (attester?.party == null || string.IsNullOrWhiteSpace(attester.party.reference))
					return Fail("Composition.attester.party.reference es obligatorio.");

				if (!ReferenceExists(resourcesByRef, attester.party.reference))
					return Fail($"No existe referencia {attester.party.reference}");
			}

			if (!ReferenceExists(resourcesByRef, documentReference.subject.reference))
				return Fail($"No existe referencia {documentReference.subject.reference}");

			foreach (var encRef in documentReference.context.encounter)
			{
				if (encRef == null || string.IsNullOrWhiteSpace(encRef.reference))
					return Fail("DocumentReference.context.encounter.reference inválido.");

				if (!ReferenceExists(resourcesByRef, encRef.reference))
					return Fail($"No existe referencia {encRef.reference}");
			}

			if (documentReference.custodian == null || string.IsNullOrWhiteSpace(documentReference.custodian.reference))
				return Fail("DocumentReference.custodian.reference es obligatorio.");

			{
				var custodianRef = documentReference.custodian.reference.Trim();

				var esReferenciaLocalValida = ReferenceExists(resourcesByRef, custodianRef);
				var esReferenciaEspecialPermitida =
					string.Equals(custodianRef, "Organization/MinSalud", System.StringComparison.OrdinalIgnoreCase);

				if (!esReferenciaLocalValida && !esReferenciaEspecialPermitida)
					return Fail($"No existe referencia {documentReference.custodian.reference}");
			}

			foreach (var section in composition.section)
			{
				if (section == null)
					return Fail("Hay secciones nulas.");

				if (string.IsNullOrWhiteSpace(section.title))
					return Fail("Cada sección debe tener title.");

				if (!HasConceptContent(section.code))
					return Fail($"La sección {section.title} no tiene code.");

				var tieneEntry = section.entry != null && section.entry.Any(x => x != null && !string.IsNullOrWhiteSpace(x.reference));
				var tieneNarrativa = section.text != null && !string.IsNullOrWhiteSpace(section.text.div);
				var tieneEmptyReason = HasConceptContent(section.emptyReason);

				if (!tieneEntry && !tieneNarrativa && !tieneEmptyReason)
					return Fail($"La sección {section.title} debe tener entry o text o emptyReason.");

				foreach (var entry in section.entry ?? new List<FhirReference>())
				{
					if (entry == null || string.IsNullOrWhiteSpace(entry.reference))
						continue;

					if (!ReferenceExists(resourcesByRef, entry.reference))
						return Fail($"No existe referencia {entry.reference}");
				}
			}

			return new RdaFhirValidationResult
			{
				Ok = true
			};
		}

		private static bool MatchesInternalPattern(string id, string resourceType)
		{
			if (string.IsNullOrWhiteSpace(id))
				return false;

			return Regex.IsMatch(id, $"^{Regex.Escape(resourceType)}-\\d+$");
		}

		private static bool HasConceptContent(FhirCodeableConcept concept)
		{
			if (concept == null)
				return false;

			if (!string.IsNullOrWhiteSpace(concept.text))
				return true;

			return concept.coding != null && concept.coding.Any(c =>
				c != null &&
				(!string.IsNullOrWhiteSpace(c.system) ||
				 !string.IsNullOrWhiteSpace(c.code) ||
				 !string.IsNullOrWhiteSpace(c.display)));
		}

		private static bool IsExpectedCompositionType(FhirCodeableConcept concept)
		{
			if (concept?.coding == null)
				return false;

			return concept.coding.Any(c =>
				c != null &&
				c.system == SystemLoinc &&
				c.code == ExpectedCompositionTypeCode);
		}

		private static bool HasValidPatientIdentifier(List<FhirIdentifier> identifiers)
		{
			if (identifiers == null || !identifiers.Any())
				return false;

			return identifiers.Any(x =>
				x != null &&
				!string.IsNullOrWhiteSpace(x.value) &&
				!x.value.Contains("|") &&
				(string.IsNullOrWhiteSpace(x.system) || x.system == SystemNationalPersonIdentifier || x.system == ""));
		}

		private static bool HasUsablePractitionerIdentifier(List<FhirIdentifier> identifiers)
		{
			if (identifiers == null || !identifiers.Any())
				return false;

			return identifiers.Any(x =>
				x != null &&
				!string.IsNullOrWhiteSpace(x.value) &&
				!x.value.Contains("|"));
		}

		private static bool HasRepsIdentifier(List<FhirIdentifier> identifiers)
		{
			if (identifiers == null || !identifiers.Any())
				return false;

			return identifiers.Any(x =>
				x != null &&
				x.system == SystemPrestadorReps &&
				!string.IsNullOrWhiteSpace(x.value));
		}

		private static bool HasSectionCode(IEnumerable<FhirCompositionSection> sections, string loincCode)
		{
			if (sections == null)
				return false;

			return sections.Any(s =>
				s != null &&
				s.code?.coding != null &&
				s.code.coding.Any(c =>
					c != null &&
					c.system == SystemLoinc &&
					c.code == loincCode));
		}

		private static RdaFhirValidationResult Fail(string error)
		{
			return new RdaFhirValidationResult
			{
				Ok = false,
				Error = error
			};
		}

		private static Dictionary<string, object> BuildResourceReferenceIndex(FhirBundleRdaConsulta bundle)
		{
			var dict = new Dictionary<string, object>();

			foreach (var entry in bundle.entry)
			{
				if (entry?.resource == null)
					continue;

				switch (entry.resource)
				{
					case FhirComposition x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"Composition/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;

					case FhirPatient x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"Patient/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;

					case FhirPractitioner x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"Practitioner/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;

					case FhirOrganization x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"Organization/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;

					case FhirEncounter x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"Encounter/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;

					case FhirCondition x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"Condition/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;

					case FhirAllergyIntolerance x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"AllergyIntolerance/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;

					case FhirLocation x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"Location/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;

					case FhirDocumentReference x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"DocumentReference/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;
				}
			}

			return dict;
		}

		private static void AddRef(Dictionary<string, object> dict, string key, object value)
		{
			if (!dict.ContainsKey(key))
				dict.Add(key, value);
		}

		private static bool ReferenceExists(Dictionary<string, object> resourcesByRef, string reference)
		{
			return resourcesByRef.ContainsKey(reference);
		}
	}
}