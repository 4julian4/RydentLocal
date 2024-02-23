
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using ServicioRydentLocal.LogicaDelNegocio.Helpers;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;
using ServicioRydentLocal.LogicaDelNegocio.Services;
using ServicioRydentLocal.LogicaDelNegocio.Services.TAnamnesis;
using SixLabors.ImageSharp;
using static System.Net.Mime.MediaTypeNames;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private HubConnection _hubConnection;
    private readonly IConfiguration _configuration;

    // private readonly AppDbContext _dbContext;

    




    // Constructor: Recibe una instancia de ILogger para realizar el registro de eventos.
    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        string url = _configuration.GetValue<string>("signalRServer:url");
        _hubConnection = new HubConnectionBuilder().WithUrl(url).Build();
    }
    //----------------------Pasos:
    //1. Conectar con el servicio de SignalR usando la funcion ConnectToServer 
    //2. Recibir el pin de acceso de Rydent se usa el evento _hubConnection.On<string, string>("ObtenerPin"
    //3. Autenticar el pin de acceso de Rydent se usando await RecibirPinRydent(pin, clientId);
    //4. Enviar el pin de acceso de Rydent al servidor de Rydent
    public async Task ConnectToServer() //conectamos al servidor de SR y registramos el equipo
    {
        //_hubConnection.On<string,string>("ReceiveMessage",(user, message) =>
        //{
        //    Console.WriteLine($"Usuario Recibido: {user}");
        //    Console.WriteLine($"Received message: {System.Environment.MachineName}");
        //    Console.WriteLine($"Received message: {message}");
        //});

        //Cuando el servidor de SR nos envie un mensaje ObtenerPin a nivel local validamos por medio de RecibirPinRydent
        //el pin de acceso de Rydent si exiiste invocamos la funcion RespuestaObtenerPin que esta en el servidor de SR
        _hubConnection.On<string, string>("ObtenerPin", async (clientId, pin) =>
        {
            await RecibirPinRydent(pin, clientId);
        });

        _hubConnection.On<string, string>("ObtenerDoctor", async (clientId, idDoctor) =>
        {
            await ConsultarPorIdDoctor(clientId, Convert.ToInt32(idDoctor));
        });

        _hubConnection.On<string, string, string>("BuscarPaciente", async (clientId, tipoBuqueda, valorDeBusqueda) =>
        {
            await BuscarPaciente(valorDeBusqueda, Convert.ToInt32(tipoBuqueda), clientId);
        });

        _hubConnection.On<string, string>("ObtenerDatosPersonalesCompletosPaciente", async (clientId, idAnanesis) =>
        {
            await BuscarDatosPersonalesCompletosPacientes(clientId, Convert.ToInt32(idAnanesis));
        });

        _hubConnection.On<string, string>("ObtenerAntecedentesPaciente", async (clientId, idAnanesis) =>
        {
            await BuscarAntecedentesPacientes(clientId, Convert.ToInt32(idAnanesis));
        });

        _hubConnection.On<string, string>("ObtenerDatosEvolucion", async (clientId, idAnanesis) =>
        {
            await ObtenerDatosEvolucion(clientId, Convert.ToInt32(idAnanesis));
        });

        //clientId es el id de la conexion del cliente angular que pidio  ConsultarPorDiaYPorUnidad
        //el punto On indica que se esta suscribiendo a un evento llamado ConsultarPorDiaYPorUnidad los parametros <string, int, DateTime>
        //determinan los tipos de datos que se van a recibir en el evento y que vemos reflejados async (clientId, silla, fecha)
        _hubConnection.On<string, string, DateTime>("ConsultarPorDiaYPorUnidad", async (clientId, silla, fecha) =>
        {
            await ConsultarPorDiaYPorUnidad(clientId, Convert.ToInt32(silla), fecha);
        });
        await _hubConnection.StartAsync();
        // aca estamos invocando una funcion que tenemos en el servidor signalR llamada RegistrarEquipo
        // y le pasamos el id de la conexion al siganlR el cual fue asignado por el servidor signalR
        // y el id encriptado del disco duro del equipo que esta corriendo el worker
        var datosClientes = await new TDATOSCLIENTESServicios().ConsultarPorId(System.Environment.MachineName);
        await _hubConnection.InvokeAsync("RegistrarEquipo", _hubConnection.ConnectionId, datosClientes.ENTRADA);
    }
    

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Iniciando la consulta a la tabla ");

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    //var objFirma = new TFIRMAServicios();
                    //var resultadoFirma = await objFirma.ConsultarPorId(29);
                    //string firma = Convert.ToBase64String(resultadoFirma.FIRMA);
                    var objAname = new TANAMNESISServicios();
                    var resultadoAnamenseis = await objAname.BuscarPacientePorTipo(1,"ANDREA G");
                    if (_hubConnection.State != HubConnectionState.Connected)
                    {
                        await ConnectToServer();
                        // await BuscarDatosPersonalesCompletosPacientes(clientId: "1", idAnanesis: 1);
                        await ObtenerDatosEvolucion(clientId: "1", idAnanesis: 25);
                        var objFirma = new TFIRMAServicios();
                        var archivosHelper = new ArchivosHelper();
                        var resultadoFirma = await objFirma.ConsultarPorId(785);
                        string firma = archivosHelper.obtenerBase64ConPrefijo(Convert.ToBase64String(resultadoFirma.FIRMA));
                        var s = archivosHelper.obtenerDimensionFromBytes(resultadoFirma.FIRMA);
                    }
                    _logger.LogInformation("Consulta completada correctamente.");
                }
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error durante la ejecución: {ex.Message}");
            }
        }
       // await ConnectToServer();
    }



    public async Task RecibirPinRydent(string pinacceso, string clientId)
    {
        var respuestaPin = new RespuestaPinModel();
       
        var objPINACCESO = new TCLAVEServicios();
        var objDOCTORES = new TDATOSDOCTORESServicios();
        respuestaPin.clave = await objPINACCESO.ConsultarPorId(pinacceso);
        if (respuestaPin.clave != null)
        {
            respuestaPin.clave.CLAVE = "";
        }   
        var listDoctores =  await objDOCTORES.ConsultarTodos();
        if (listDoctores != null && listDoctores.Count() > 0) 
        {
            respuestaPin.lstDoctores = listDoctores.ConvertAll(item => new ListadoItemModel() { id = item.ID.ToString(), nombre = (item.NOMBRE ?? "") });
            //foreach (var item in listDoctores)
            //{
            //    respuestaPin.lstDoctores.Add(new ListadoItemModel() { id = item.ID.ToString(), nombre = (item.NOMBRE ?? "")});
            //}
        }
        try
        {
            await _hubConnection.InvokeAsync("RespuestaObtenerPin", clientId, JsonConvert.SerializeObject(respuestaPin));
        }
        catch (Exception ex)
        {

            throw;
        }
        
    }

    public async Task ConsultarPorIdDoctor(string clientId, int idDoctor)
    {
        var objDOCTORES = new TDATOSDOCTORESServicios();
        var objAname = new TANAMNESISServicios();
        var respuestaObtenerDoctor = new RespuestaObtenerDoctorModel();
        respuestaObtenerDoctor.doctor = await objDOCTORES.ConsultarPorId(idDoctor);
        respuestaObtenerDoctor.totalPacientes = await objAname.ConsultarTotalPacientesPorDoctor(idDoctor);
        await _hubConnection.InvokeAsync("RespuestaObtenerDoctor", clientId, JsonConvert.SerializeObject(respuestaObtenerDoctor));
    }
    
    
    public async Task ConsultarPorDiaYPorUnidad(string clientId, int silla, DateTime fecha)
    {
        var objDetalleCitasServicios= new TDETALLECITASServicios();
        var objCitasServicios = new TCITASServicios();
        var objRespuestaConsultarPorDiaYPorUnidad = new RespuestaConsultarPorDiaYPorUnidadModel();
        objRespuestaConsultarPorDiaYPorUnidad.lstDetallaCitas = await objDetalleCitasServicios.ConsultarPorFechaySilla(fecha, silla);
        objRespuestaConsultarPorDiaYPorUnidad.cita = await objCitasServicios.ConsultarPorId(silla, fecha);
        // Las invocaciones siempre llaman a una funcion que estan en el Hub en el servidor de SR
        await _hubConnection.InvokeAsync("RespuestaConsultarPorDiaYPorUnidad", clientId, objRespuestaConsultarPorDiaYPorUnidad);
    }
    public async Task BuscarPaciente(string valorDeBusqueda, int tipoBusqueda,  string clientId)
    {
        var objAname = new TANAMNESISServicios();
        
        var listAnamnesis = await objAname.BuscarPacientePorTipo(tipoBusqueda, valorDeBusqueda);
        var respuestaBuscarPaciente = listAnamnesis.ConvertAll(item => new RespuestaBusquedaPacienteModel()
        {
           IDANAMNESIS=item.IDANAMNESIS,
           NOMBRE_PACIENTE=item.NOMBRE_PACIENTE,
           IDANAMNESISTEXTO=item.IDANAMNESISTEXTO,
           NUMDOCUMENTO=item.NUMDOCUMENTO,
           DOCTOR=item.DOCTOR,
           PERFIL=item.PERFIL,
           NUMAFILIACION=item.NUMAFILIACION,
           TELEFONO=item.TELEFONO
        });
        await _hubConnection.InvokeAsync("RespuestaBuscarPaciente", clientId, JsonConvert.SerializeObject(respuestaBuscarPaciente));
    }
    
    public async Task BuscarDatosPersonalesCompletosPacientes(string clientId, int idAnanesis)
    {
        var objDatosPersonales = new DatosPersonalesServicios();
        var datosPersonales = await objDatosPersonales.ConsultarPorId(idAnanesis);
        var respuestaBuscarDatosPersonalesCompletosPacientes = datosPersonales;
        await _hubConnection.InvokeAsync("RespuestaObtenerDatosPersonalesCompletosPaciente", clientId, JsonConvert.SerializeObject(respuestaBuscarDatosPersonalesCompletosPacientes));
    }

    public async Task BuscarAntecedentesPacientes(string clientId, int idAnanesis)
    {
        var objAntecedentes = new AntecedentesServicios();
        var antecedentes = await objAntecedentes.ConsultarPorId(idAnanesis);
        var respuestaBuscarAntecedentesPacientes = antecedentes;
        await _hubConnection.InvokeAsync("RespuestaObtenerAntecedentesPaciente", clientId, JsonConvert.SerializeObject(respuestaBuscarAntecedentesPacientes));
    }

    public async Task ObtenerDatosEvolucion(string clientId, int idAnanesis)
    {
        var objEvolucion = new TEVOLUCIONServicios();
        var listEvolucion = await objEvolucion.ConsultarPorAnamnesis(idAnanesis);
        var respuestaBuscarEvolucion = new List<RespuestaEvolucionPacienteModel>();
        var archivosHelper = new ArchivosHelper();
        foreach (var item in listEvolucion)
        {
            var objEvolucion1 = new RespuestaEvolucionPacienteModel();
            objEvolucion1.evolucion = item;
            objEvolucion1.imgFirmaPaciente = "";
            objEvolucion1.imgFirmaDoctor = "";
            if (item.FIRMA != null)
            {
                if (item.FIRMA > 0)
                {
                    try
                    {
                        objEvolucion1.imgFirmaPaciente = item.FIRMA == null ? "" : (item.FIRMA <= 0 ? "" : await RetornarFotoEnBase64ConPrefijo(item.FIRMA ?? -1, 1));
                        objEvolucion1.imgFirmaDoctor = item.FIRMA == null ? "" : (item.FIRMA <= 0 ? "" : await RetornarFotoEnBase64ConPrefijo(item.FIRMA ?? -1, 2));
                    }
                    catch (Exception e)
                    {

                        objEvolucion1.imgFirmaPaciente = "";
                        objEvolucion1.imgFirmaDoctor = "";
                    }
                   
                }
            }   
           
            respuestaBuscarEvolucion.Add(objEvolucion1);
            //respuestaBuscarEvolucion.Add(new RespuestaEvolucionPacienteModel()
            //{
            //    evolucion = item,
            //    //---item es un registro de la tabla TEVOLUCION en item.FIRMA esta almacenado el id de la firma que debemos buscar en la tabla T_FIRMAS------//
            //    imgFirmaPaciente = item.FIRMA == null ? "" :(item.FIRMA <= 0 ? "":await RetornarFotoEnBase64ConPrefijo(item.FIRMA ?? -1,1)),
            //    imgFirmaDoctor = item.FIRMA == null ? "" : (item.FIRMA <= 0 ? "" : await RetornarFotoEnBase64ConPrefijo(item.FIRMA ?? -1, 2))
            //}) ;
        }
        var objRespuestaObtenerDatosEvolucion = JsonConvert.SerializeObject(respuestaBuscarEvolucion);
        try
        {
            await _hubConnection.InvokeAsync("RespuestaObtenerDatosEvolucion", clientId, objRespuestaObtenerDatosEvolucion);
        }
        catch (Exception e)
        {

            throw;
        }
        
    }
    private async Task<string> RetornarFotoEnBase64ConPrefijo(int idFirma, int tipo)
    {
        var archivosHelper = new ArchivosHelper();
        var objFirma = new TFIRMAServicios();
        var resultadoFirma = await objFirma.ConsultarPorId(idFirma);
        if (tipo == 1)
        {
            var recorteFirmaPaciente = archivosHelper.recortarImganFromBytes(resultadoFirma.FIRMA, new Rectangle(0, 0, 1364, 225));
            return archivosHelper.obtenerBase64ConPrefijo(recorteFirmaPaciente);
        }
        else
        {
            var recorteFirmaDoctor = archivosHelper.recortarImganFromBytes(resultadoFirma.FIRMA, new Rectangle(0, (482 - 215), 1364, 215));
            return archivosHelper.obtenerBase64ConPrefijo(recorteFirmaDoctor);
        }   
       
    }


    private void AutenticarPinDeRydent(string pin)
    {
        //var resultadoAnamenseis = await objAname.ConsultarPorId(8703);
        //resultadoAnamenseis.NOMBRES = "JUAN PEREZ1";
        //await objAname.Editar(8703, resultadoAnamenseis);
    }

    private void ProcessResults<T>(List<T>  results)
    {
        foreach (var obj in results)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        }
    }

    private void ProcessResults<T>(T results)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(results));
    }
}




//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//public class Worker : BackgroundService
//{
//    private readonly ILogger<Worker> _logger;

//    // Constructor: Recibe una instancia de ILogger para realizar el registro de eventos.
//    public Worker(ILogger<Worker> logger)
//    {
//        _logger = logger;
//    }

//    // Método ExecuteAsync: Implementa la lógica principal del servicio de trabajador.
//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        // Bucle que se ejecuta mientras no se solicite la cancelación del servicio.
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            // Registro de un mensaje "Hola Mundo desde el Servicio de Trabajador".
//            _logger.LogInformation("Hola Mundo desde el Servicio de Trabajador.");

//            // Simulación de una tarea de trabajo con una espera de 5 segundos.
//            await Task.Delay(5000, stoppingToken);
//        }
//    }
//}
