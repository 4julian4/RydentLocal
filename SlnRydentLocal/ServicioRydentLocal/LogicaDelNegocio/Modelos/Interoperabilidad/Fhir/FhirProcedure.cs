using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.Fhir;
using System.Collections.Generic;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.Fhir
{
	public sealed class FhirProcedure
	{
		public string resourceType { get; set; } = "Procedure";
		public string id { get; set; } = "";
		public FhirMeta? meta { get; set; }

		public string status { get; set; } = "completed";
		public FhirCodeableConcept? category { get; set; }
		public FhirCodeableConcept? code { get; set; }
		public FhirReference? subject { get; set; }
		public FhirReference? encounter { get; set; }

		// ✅ requerido por el perfil ProcedureRDA
		public List<FhirCodeableConcept> reasonCode { get; set; } = new List<FhirCodeableConcept>();
		public List<FhirReference> reasonReference { get; set; } = new List<FhirReference>();

		public string? performedDateTime { get; set; }
		public List<FhirProcedurePerformer> performer { get; set; } = new List<FhirProcedurePerformer>();
		public List<FhirAnnotation> note { get; set; } = new List<FhirAnnotation>();
	}

	public sealed class FhirProcedurePerformer
	{
		public FhirReference? actor { get; set; }
	}

}


