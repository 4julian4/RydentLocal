using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rado
{
	public sealed class RadoOptions
	{
		public bool Enabled { get; set; }
		public string? Endpoint { get; set; }
		public string? ApiKey { get; set; }
	}
}
