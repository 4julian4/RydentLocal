using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServicioRydentLocal.LogicaDelNegocio.Services;
using ServicioRydentLocal.LogicaDelNegocio.Services.Rips;
using ServicioRydentLocal.LogicaDelNegocio.Services.TAnamnesis;
using ServicioRydentLocal.LogicaDelNegocio.Services.Dataico; // ApiIntermediaClient, FacturacionSaludRepository
using ServicioRydentLocal.LogicaDelNegocio.Repositorio;      // si inyectas repos de aquí

public class Program
{
	public static void Main(string[] args)
		=> CreateHostBuilder(args).Build().Run();

	public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureServices((hostContext, services) =>
			{
				var cfg = hostContext.Configuration;

				// 1) DbContext Firebird (Scoped)
				services.AddDbContext<AppDbContext>(options =>
					options.UseFirebird(
						cfg.GetConnectionString("FirebirdConnection"),
						fb => fb.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
					),
					contextLifetime: ServiceLifetime.Scoped,
					optionsLifetime: ServiceLifetime.Scoped
				);

				// 2) HttpClient para la API intermedia (Billing.Api)
				services.AddHttpClient<ApiIntermediaClient>(client =>
				{
					var baseUrl = cfg["ApiIntermedia:BaseUrl"];
					if (string.IsNullOrWhiteSpace(baseUrl))
						throw new InvalidOperationException("Falta ApiIntermedia:BaseUrl en appsettings.json");

					client.BaseAddress = new Uri(baseUrl);
					// Aquí podrías configurar timeouts o headers por defecto si quieres
				});

				// 3) Repositorios/Servicios usados por el Worker (Scoped)
				services.AddScoped<ITANAMNESISServicios, TANAMNESISServicios>();
				services.AddScoped<ITDATOSCLIENTESServicios, TDATOSCLIENTESServicios>();
				services.AddScoped<ITDATOSDOCTORESServicios, TDATOSDOCTORESServicios>();
				services.AddScoped<ITCODIGOS_EPSServicios, TCODIGOS_EPSServicios>();
				services.AddScoped<ITDETALLECITASServicios, TDETALLECITASServicios>();
				services.AddScoped<IT_ADICIONALES_ABONOSServicios, T_ADICIONALES_ABONOSServicios>();
				services.AddScoped<IT_RIPS_DXServicios, T_RIPS_DXServicios>();
				services.AddScoped<IGenerarRipsServicios, GenerarRipsServicios>();
				services.AddScoped<IEstadoCuentaService, EstadoCuentaService>();

				services.AddScoped<DatosPersonalesServicios>();
				services.AddScoped<AntecedentesServicios>();
				services.AddScoped<TEVOLUCIONServicios>();
				services.AddScoped<TFIRMAServicios>();
				services.AddScoped<TTRATAMIENTOServicios>();
				services.AddScoped<T_FRASE_XEVOLUCIONServicios>();
				services.AddScoped<TCITASServicios>();
				services.AddScoped<THORARIOSAGENDAServicios>();
				services.AddScoped<THORARIOSASUNTOSServicios>();
				services.AddScoped<TFESTIVOSServicios>();
				services.AddScoped<TCITASBORRADASServicios>();
				services.AddScoped<TCITASCANCELADASServicios>();
				services.AddScoped<TEGRESOServicios>();
				services.AddScoped<TINGRESOServicios>();
				services.AddScoped<TCODIGOS_PROCEDIMIENTOSServicios>();
				services.AddScoped<TCODIGOS_CONSLUTASServicios>();
				services.AddScoped<TCODIGOS_DEPARTAMENTOServicios>();
				services.AddScoped<TCODIGOS_CIUDADServicios>();
				services.AddScoped<TINFORMACIONREPORTESServicios>();
				services.AddScoped<TFOTOSFRONTALESServicios>();
				services.AddScoped<THISTORIALServicios>();
				services.AddScoped<T_RIPS_PROCEDIMIENTOSServicios>();
				services.AddScoped<T_DEFINICION_TRATAMIENTOServicios>();
				services.AddScoped<IRadoQueryService, RadoQueryService>();
				services.AddHttpClient<IRadoIntegrationService, RadoIntegrationService>();

				// 4) Repos para facturación salud (el que usas en PresentarFacturasEnDian)
				services.AddScoped<FacturacionSaludRepository>();


				// (Opcional) Si quieres inyectar también tu repo de Adicionales_Abonos_Dian:
				// services.AddScoped<ServicioRydentLocal.LogicaDelNegocio.Facturatech.Adicionales_Abonos_Dian>();

				// 5) Hosted Worker
				services.AddHostedService<Worker>();
			});
}
