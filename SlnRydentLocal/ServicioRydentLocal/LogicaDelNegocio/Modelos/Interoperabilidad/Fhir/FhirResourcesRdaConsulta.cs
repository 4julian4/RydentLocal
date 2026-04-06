using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.Fhir
{
	public sealed class FhirMeta
	{
		public List<string> profile { get; set; } = new List<string>();
	}

	public sealed class FhirNarrative
	{
		public string status { get; set; } = "generated";
		public string div { get; set; } = "";
	}

	public sealed class FhirPrimitiveElement
	{
		public List<FhirExtension> extension { get; set; } = new List<FhirExtension>();
	}

	public sealed class FhirReference
	{
		public string reference { get; set; } = "";
		public string? display { get; set; }
	}

	public sealed class FhirCoding
	{
		public string? system { get; set; }
		public string? code { get; set; }
		public string? display { get; set; }
	}

	public sealed class FhirCodeableConcept
	{
		public List<FhirCoding> coding { get; set; } = new List<FhirCoding>();
		public string? text { get; set; }
	}

	public sealed class FhirExtension
	{
		public string url { get; set; } = "";

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirCoding? valueCoding { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? valueString { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? valueTime { get; set; }
	}

	public sealed class FhirAnnotation
	{
		public string? text { get; set; }
	}

	public sealed class FhirPeriod
	{
		public string? start { get; set; }
		public string? end { get; set; }
	}

	public sealed class FhirIdentifierAssigner
	{
		public string? type { get; set; }
		public string? display { get; set; }
	}

	public sealed class FhirIdentifier
	{
		public string? id { get; set; }
		public string? use { get; set; }
		public FhirCodeableConcept? type { get; set; }
		public string system { get; set; } = "";
		public string value { get; set; } = "";

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirPeriod? period { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirIdentifierAssigner? assigner { get; set; }
	}


	public sealed class FhirHumanName
	{
		public string? use { get; set; }
		public string? text { get; set; }
		public string? family { get; set; }
		public List<string> given { get; set; } = new List<string>();

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirPrimitiveElement? _family { get; set; }
	}

	public sealed class FhirAddress
	{
		public string? id { get; set; }
		public string? use { get; set; }
		public string? type { get; set; }
		public string? text { get; set; }
		public string? city { get; set; }
		public string? district { get; set; }
		public string? state { get; set; }
		public string? country { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<FhirExtension> extension { get; set; } = new List<FhirExtension>();

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirPrimitiveElement? _city { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirPrimitiveElement? _state { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirPrimitiveElement? _country { get; set; }
	}

	public sealed class FhirContactPoint
	{
		public string? system { get; set; }
		public string? value { get; set; }
	}

	public sealed class FhirOrganization
	{
		public string resourceType { get; set; } = "Organization";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		public List<FhirIdentifier> identifier { get; set; } = new List<FhirIdentifier>();
		public string? name { get; set; }
		public bool? active { get; set; }
		public List<FhirCodeableConcept> type { get; set; } = new List<FhirCodeableConcept>();
		public List<FhirAddress> address { get; set; } = new List<FhirAddress>();
		public List<FhirContactPoint> telecom { get; set; } = new List<FhirContactPoint>();
	}

	public sealed class FhirComposition
	{
		public string resourceType { get; set; } = "Composition";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		public string status { get; set; } = "final";
		public FhirCodeableConcept? type { get; set; }
		public FhirReference? subject { get; set; }
		public FhirReference? encounter { get; set; }
		public DateTimeOffset date { get; set; }
		public List<FhirReference> author { get; set; } = new List<FhirReference>();
		public string title { get; set; } = "Resumen Digital de Atención";
		public string? confidentiality { get; set; }
		public List<FhirCompositionAttester> attester { get; set; } = new List<FhirCompositionAttester>();
		public FhirReference? custodian { get; set; }

		[JsonProperty("event")]
		public List<FhirCompositionEvent> event_ { get; set; } = new List<FhirCompositionEvent>();

		public List<FhirCompositionSection> section { get; set; } = new List<FhirCompositionSection>();
	}

	public sealed class FhirCompositionAttester
	{
		public string? mode { get; set; }
		public FhirReference? party { get; set; }
	}

	public sealed class FhirCompositionEvent
	{
		public List<FhirCodeableConcept> code { get; set; } = new List<FhirCodeableConcept>();
		public FhirPeriod? period { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<FhirReference> detail { get; set; } = new List<FhirReference>();
	}

	public sealed class FhirCompositionSection
	{
		public string? title { get; set; }
		public FhirCodeableConcept? code { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirNarrative? text { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<FhirReference> entry { get; set; } = new List<FhirReference>();

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirCodeableConcept? emptyReason { get; set; }
	}

	public sealed class FhirPatient
	{
		public string resourceType { get; set; } = "Patient";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<FhirExtension> extension { get; set; } = new List<FhirExtension>();

		public List<FhirIdentifier> identifier { get; set; } = new List<FhirIdentifier>();
		public List<FhirHumanName> name { get; set; } = new List<FhirHumanName>();
		public bool? active { get; set; }
		public string? gender { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirPrimitiveElement? _gender { get; set; }

		public string? birthDate { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirPrimitiveElement? _birthDate { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool? deceasedBoolean { get; set; }

		public List<FhirAddress> address { get; set; } = new List<FhirAddress>();
		public List<FhirContactPoint> telecom { get; set; } = new List<FhirContactPoint>();
	}

	public sealed class FhirPractitionerQualification
	{
		public List<FhirIdentifier> identifier { get; set; } = new List<FhirIdentifier>();
		public FhirCodeableConcept? code { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirPeriod? period { get; set; }
	}

	public sealed class FhirPractitioner
	{
		public string resourceType { get; set; } = "Practitioner";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		public List<FhirIdentifier> identifier { get; set; } = new List<FhirIdentifier>();
		public List<FhirHumanName> name { get; set; } = new List<FhirHumanName>();
		public bool? active { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<FhirPractitionerQualification> qualification { get; set; } = new List<FhirPractitionerQualification>();
	}

	public sealed class FhirEncounter
	{
		public string resourceType { get; set; } = "Encounter";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		public List<FhirIdentifier> identifier { get; set; } = new List<FhirIdentifier>();
		public string status { get; set; } = "finished";
		public FhirCoding? @class { get; set; }
		public List<FhirCodeableConcept> type { get; set; } = new List<FhirCodeableConcept>();
		public FhirCodeableConcept? serviceType { get; set; }
		public FhirReference? subject { get; set; }
		public List<FhirEncounterParticipant> participant { get; set; } = new List<FhirEncounterParticipant>();
		public FhirReference? serviceProvider { get; set; }
		public FhirPeriod? period { get; set; }
		public List<FhirCodeableConcept> reasonCode { get; set; } = new List<FhirCodeableConcept>();
		public List<FhirEncounterDiagnosis> diagnosis { get; set; } = new List<FhirEncounterDiagnosis>();
		public List<FhirEncounterLocation> location { get; set; } = new List<FhirEncounterLocation>();
		public List<FhirAnnotation> note { get; set; } = new List<FhirAnnotation>();
	}

	
	public sealed class FhirEncounterParticipant
	{
		public string? id { get; set; }
		public List<FhirCodeableConcept> type { get; set; } = new List<FhirCodeableConcept>();
		public FhirReference? individual { get; set; }
	}

	public sealed class FhirEncounterDiagnosis
	{
		public string? id { get; set; }
		public List<FhirExtension> extension { get; set; } = new List<FhirExtension>();
		public FhirReference? condition { get; set; }
		public FhirCodeableConcept? use { get; set; }
		public int? rank { get; set; }
	}

	public sealed class FhirEncounterLocation
	{
		public FhirReference? location { get; set; }
	}

	public sealed class FhirCondition
	{
		public string resourceType { get; set; } = "Condition";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		public FhirCodeableConcept? clinicalStatus { get; set; }
		public FhirCodeableConcept? verificationStatus { get; set; }
		public List<FhirCodeableConcept> category { get; set; } = new List<FhirCodeableConcept>();
		public FhirCodeableConcept? code { get; set; }
		public FhirReference? subject { get; set; }
		public FhirReference? encounter { get; set; }
		public string? recordedDate { get; set; }
		public List<FhirAnnotation> note { get; set; } = new List<FhirAnnotation>();
	}

	public sealed class FhirAllergyIntolerance
	{
		public string resourceType { get; set; } = "AllergyIntolerance";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		public FhirCodeableConcept? clinicalStatus { get; set; }
		public FhirCodeableConcept? verificationStatus { get; set; }
		public FhirCodeableConcept? code { get; set; }
		public FhirReference? patient { get; set; }
		public FhirReference? encounter { get; set; }
	}

	

	public sealed class FhirLocation
	{
		public string resourceType { get; set; } = "Location";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirNarrative? text { get; set; }

		public List<FhirIdentifier> identifier { get; set; } = new List<FhirIdentifier>();
		public string? status { get; set; }
		public string? name { get; set; }
		public FhirAddress? address { get; set; }
		public FhirReference? managingOrganization { get; set; }
	}

	public sealed class FhirAttachment
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? data { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? url { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? contentType { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? title { get; set; }
	}

	public sealed class FhirDocumentReferenceContent
	{
		public FhirAttachment? attachment { get; set; }
		public FhirCoding? format { get; set; }
	}

	public sealed class FhirDocumentReferenceContext
	{
		public List<FhirReference> encounter { get; set; } = new List<FhirReference>();
	}

	public sealed class FhirDocumentReference
	{
		public string resourceType { get; set; } = "DocumentReference";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public FhirNarrative? text { get; set; }

		public string? status { get; set; }
		public FhirCodeableConcept? type { get; set; }
		public List<FhirCodeableConcept> category { get; set; } = new List<FhirCodeableConcept>();
		public FhirReference? subject { get; set; }
		public string? date { get; set; }
		public List<FhirReference> author { get; set; } = new List<FhirReference>();
		public FhirReference? custodian { get; set; }
		public string? description { get; set; }
		public List<FhirCodeableConcept> securityLabel { get; set; } = new List<FhirCodeableConcept>();
		public List<FhirDocumentReferenceContent> content { get; set; } = new List<FhirDocumentReferenceContent>();
		public FhirDocumentReferenceContext? context { get; set; }
	}

	public sealed class FhirMedicationStatement
	{
		public string resourceType { get; set; } = "MedicationStatement";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		public string? status { get; set; }
		public FhirCodeableConcept? medicationCodeableConcept { get; set; }
		public FhirReference? subject { get; set; }
	}

	public sealed class FhirFamilyMemberHistoryCondition
	{
		public FhirCodeableConcept? code { get; set; }
	}

	public sealed class FhirFamilyMemberHistory
	{
		public string resourceType { get; set; } = "FamilyMemberHistory";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		public string? status { get; set; }
		public FhirReference? patient { get; set; }
		public FhirCodeableConcept? relationship { get; set; }
		public List<FhirFamilyMemberHistoryCondition> condition { get; set; } = new List<FhirFamilyMemberHistoryCondition>();
	}
}