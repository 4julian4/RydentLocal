using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.Fhir;
using System.Collections.Generic;
using System.Linq;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public static class RdaFhirPacienteValidator
	{
		private const string SystemNationalPersonIdentifier = "https://fhir.minsalud.gov.co/rda/NamingSystem/RNEC";
		private const string SystemPrestadorReps = "https://fhir.minsalud.gov.co/rda/NamingSystem/REPS";
		private const string SystemLoinc = "http://loinc.org";
		private const string ExpectedCompositionTypeCode = "102089-0";
		private const string UrlFathersFamilyName = "https://fhir.minsalud.gov.co/rda/StructureDefinition/ExtensionFathersFamilyName";

		private static readonly string[] RequiredSectionCodes =
		{
			"11450-4",
			"48765-2",
			"10160-0",
			"10157-6"
		};

		public static RdaFhirValidationResult Validate(FhirBundleRdaConsulta bundle)
		{
			if (bundle == null)
				return Fail("Bundle nulo.");

			if (bundle.resourceType != "Bundle")
				return Fail("resourceType debe ser Bundle.");

			if (bundle.type != "document")
				return Fail("El Bundle de paciente debe ser type=document.");

			if (bundle.entry == null || bundle.entry.Count < 8)
				return Fail("El Bundle de paciente no tiene suficientes recursos.");

			if (bundle.entry.Any(x => x == null || x.resource == null))
				return Fail("Todos los entries deben tener resource.");

			if (bundle.entry.First().resource is not FhirComposition composition)
				return Fail("El primer resource debe ser Composition.");

			if (composition.type?.coding == null ||
				!composition.type.coding.Any(x => x != null && x.system == SystemLoinc && x.code == ExpectedCompositionTypeCode))
			{
				return Fail("Composition.type debe ser LOINC 102089-0.");
			}

			if (composition.subject == null || string.IsNullOrWhiteSpace(composition.subject.reference))
				return Fail("Composition.subject.reference es obligatorio.");

			if (composition.author == null || !composition.author.Any())
				return Fail("Composition.author es obligatorio.");

			if (composition.custodian == null || string.IsNullOrWhiteSpace(composition.custodian.reference))
				return Fail("Composition.custodian.reference es obligatorio.");

			if (composition.attester == null || !composition.attester.Any())
				return Fail("Composition.attester es obligatorio.");

			if (composition.event_ == null || !composition.event_.Any())
				return Fail("Composition.event es obligatorio.");

			var ev = composition.event_.First();
			if (ev.period == null || string.IsNullOrWhiteSpace(ev.period.start) || string.IsNullOrWhiteSpace(ev.period.end))
				return Fail("Composition.event.period es obligatorio.");

			if (ev.code == null || ev.code.Count < 2)
				return Fail("Composition.event.code debe incluir modalidad y grupo de servicio.");

			if (composition.section == null || !composition.section.Any())
				return Fail("Composition.section es obligatorio.");

			foreach (var sectionCode in RequiredSectionCodes)
			{
				var existe = composition.section.Any(s =>
					s != null &&
					s.code?.coding != null &&
					s.code.coding.Any(c => c != null && c.system == SystemLoinc && c.code == sectionCode));

				if (!existe)
					return Fail($"Falta la sección obligatoria {sectionCode}.");
			}

			var resourcesByRef = BuildResourceReferenceIndex(bundle);

			var patient = bundle.entry.Select(x => x.resource).OfType<FhirPatient>().FirstOrDefault();
			if (patient == null)
				return Fail("Falta Patient.");

			if (patient.identifier == null || !patient.identifier.Any(x =>
					x != null &&
					x.system == SystemNationalPersonIdentifier &&
					!string.IsNullOrWhiteSpace(x.value) &&
					!x.value.Contains("|")))
			{
				return Fail("Patient.identifier debe incluir RNEC.");
			}

			var officialName = patient.name?.FirstOrDefault(x => x.use == "official") ?? patient.name?.FirstOrDefault();
			if (officialName == null)
				return Fail("Patient.name es obligatorio.");

			if (string.IsNullOrWhiteSpace(officialName.family))
				return Fail("Patient.name.family es obligatorio.");

			var firstSurname = officialName.family
				.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
				.FirstOrDefault();

			var fatherExt = officialName._family?.extension?
				.FirstOrDefault(e => e.url == UrlFathersFamilyName)?.valueString;

			if (string.IsNullOrWhiteSpace(firstSurname) || string.IsNullOrWhiteSpace(fatherExt))
				return Fail("Patient.name._family.extension con primer apellido es obligatorio.");

			if (firstSurname.Trim().ToUpperInvariant() != fatherExt.Trim().ToUpperInvariant())
				return Fail("El primer apellido en ExtensionFathersFamilyName no coincide con Patient.name.family.");

			if (string.IsNullOrWhiteSpace(patient.gender))
				return Fail("Patient.gender es obligatorio.");

			if (patient.address != null && patient.address.Any())
			{
				var addr = patient.address.First();

				if (!string.IsNullOrWhiteSpace(addr.state))
					return Fail("Patient.address.state no debe enviarse para este perfil.");

				if (addr._state != null)
					return Fail("Patient.address._state no debe enviarse para este perfil.");
			}

			var practitioner = bundle.entry.Select(x => x.resource).OfType<FhirPractitioner>().FirstOrDefault();
			if (practitioner == null)
				return Fail("Falta Practitioner.");

			var practitionerHasValue = practitioner.identifier != null &&
				practitioner.identifier.Any(x =>
					x != null &&
					!string.IsNullOrWhiteSpace(x.value) &&
					!x.value.Contains("|"));

			if (!practitionerHasValue)
				return Fail("Practitioner.identifier debe incluir número de identificación válido.");

			var organization = bundle.entry.Select(x => x.resource).OfType<FhirOrganization>().FirstOrDefault();
			if (organization == null)
				return Fail("Falta Organization.");

			if (organization.identifier == null || !organization.identifier.Any(x =>
					x != null &&
					x.system == SystemPrestadorReps &&
					!string.IsNullOrWhiteSpace(x.value)))
			{
				return Fail("Organization.identifier debe incluir REPS.");
			}

			var condition = bundle.entry.Select(x => x.resource).OfType<FhirCondition>().FirstOrDefault();
			var allergy = bundle.entry.Select(x => x.resource).OfType<FhirAllergyIntolerance>().FirstOrDefault();
			var medication = bundle.entry.Select(x => x.resource).OfType<FhirMedicationStatement>().FirstOrDefault();
			var family = bundle.entry.Select(x => x.resource).OfType<FhirFamilyMemberHistory>().FirstOrDefault();

			if (condition == null || allergy == null || medication == null || family == null)
				return Fail("El bundle de paciente debe contener Condition, AllergyIntolerance, MedicationStatement y FamilyMemberHistory.");

			if (condition.subject == null || string.IsNullOrWhiteSpace(condition.subject.reference))
				return Fail("Condition.subject.reference es obligatorio.");

			if (allergy.patient == null || string.IsNullOrWhiteSpace(allergy.patient.reference))
				return Fail("AllergyIntolerance.patient.reference es obligatorio.");

			if (medication.subject == null || string.IsNullOrWhiteSpace(medication.subject.reference))
				return Fail("MedicationStatement.subject.reference es obligatorio.");

			if (family.patient == null || string.IsNullOrWhiteSpace(family.patient.reference))
				return Fail("FamilyMemberHistory.patient.reference es obligatorio.");

			if (!ReferenceExists(resourcesByRef, composition.subject.reference))
				return Fail($"No existe referencia {composition.subject.reference}");

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

			if (!ReferenceExists(resourcesByRef, condition.subject.reference))
				return Fail($"No existe referencia {condition.subject.reference}");

			if (!ReferenceExists(resourcesByRef, allergy.patient.reference))
				return Fail($"No existe referencia {allergy.patient.reference}");

			if (!ReferenceExists(resourcesByRef, medication.subject.reference))
				return Fail($"No existe referencia {medication.subject.reference}");

			if (!ReferenceExists(resourcesByRef, family.patient.reference))
				return Fail($"No existe referencia {family.patient.reference}");

			foreach (var section in composition.section)
			{
				if (section == null)
					return Fail("Hay secciones nulas.");

				if (string.IsNullOrWhiteSpace(section.title))
					return Fail("Cada sección debe tener title.");

				var tieneCode = section.code != null &&
								section.code.coding != null &&
								section.code.coding.Any();

				if (!tieneCode)
					return Fail($"La sección {section.title} debe tener code.");

				var tieneEntry = section.entry != null && section.entry.Any(x => x != null && !string.IsNullOrWhiteSpace(x.reference));
				var tieneText = section.text != null && !string.IsNullOrWhiteSpace(section.text.div);

				if (!tieneEntry && !tieneText)
					return Fail($"La sección {section.title} debe tener entry o text.");

				foreach (var entry in section.entry ?? new List<FhirReference>())
				{
					if (entry == null || string.IsNullOrWhiteSpace(entry.reference))
						continue;

					if (!ReferenceExists(resourcesByRef, entry.reference))
						return Fail($"No existe referencia {entry.reference}");
				}
			}

			return new RdaFhirValidationResult { Ok = true };
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

					case FhirCondition x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"Condition/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;

					case FhirAllergyIntolerance x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"AllergyIntolerance/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;

					case FhirMedicationStatement x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"MedicationStatement/{x.id}", x);
						AddRef(dict, $"#{x.id}", x);
						break;

					case FhirFamilyMemberHistory x when !string.IsNullOrWhiteSpace(x.id):
						AddRef(dict, $"FamilyMemberHistory/{x.id}", x);
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
			return !string.IsNullOrWhiteSpace(reference) && resourcesByRef.ContainsKey(reference);
		}

		private static RdaFhirValidationResult Fail(string error)
		{
			return new RdaFhirValidationResult
			{
				Ok = false,
				Error = error
			};
		}
	}
}