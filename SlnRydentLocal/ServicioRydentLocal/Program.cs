using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Services;




public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ITANAMNESISServicios, TANAMNESISServicios>();
                services.AddSingleton<ITDATOSCLIENTESServicios, TDATOSCLIENTESServicios>();
                services.AddSingleton<ITDATOSDOCTORESServicios, TDATOSDOCTORESServicios>();
                services.AddSingleton<ITCODIGOS_EPSServicios, TCODIGOS_EPSServicios>();
                services.AddSingleton<ITDETALLECITASServicios, TDETALLECITASServicios>();
                services.AddSingleton<IT_ADICIONALES_ABONOSServicios, T_ADICIONALES_ABONOSServicios>();
                //services.AddDbContext<AppDbContext>(options =>
                //    options.UseFirebird("database=localhost:C:\\Program Files\\Acrom\\Bdr2\\R2.FDB;user=sysdba;password=masterkeyPort=3050;Dialect=3;Charset=ISO8859_1;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;", ef => ef.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)), ServiceLifetime.Scoped);
               // services.AddDbContext<AppDbContext>(options =>
               //     options.UseFirebird("database=localhost:C:\\Program Files\\Acrom\\Bdr2\\R2.FDB;user=sysdba;password=masterkeyPort=3050;Dialect=3;Charset=ISO8859_1;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;"), ServiceLifetime.Scoped);
                

                services.AddHostedService<Worker>();
            });

}


