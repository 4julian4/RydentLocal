
using FirebirdSql.Data.FirebirdClient;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Helpers;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;
using ServicioRydentLocal.LogicaDelNegocio.Services;
using ServicioRydentLocal.LogicaDelNegocio.Services.TAnamnesis;
using SixLabors.ImageSharp;
using System.Data;
using System.Security.Principal;
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

    private async Task registrarSuscripciones()
    {
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

        _hubConnection.On<string, string>("GuardarDatosEvolucion", async (clientId, datosEvolucion) =>
        {
            Console.WriteLine("***************************");
            await GuardarDatosEvolucion(clientId, datosEvolucion);
        });

        _hubConnection.On<string>("ObtenerCodigosEps", async (clientId) =>
        {
            await ObtenerCodigosEps(clientId);
        });

        _hubConnection.On<string, string, DateTime>("ObtenerConsultaPorDiaYPorUnidad", async (clientId, silla, fecha) =>
        {
            await ConsultarPorDiaYPorUnidad(clientId, Convert.ToInt32(silla), fecha);
        });



        //clientId es el id de la conexion del cliente angular que pidio  ConsultarPorDiaYPorUnidad
        //el punto On indica que se esta suscribiendo a un evento llamado ConsultarPorDiaYPorUnidad los parametros <string, int, DateTime>
        //determinan los tipos de datos que se van a recibir en el evento y que vemos reflejados async (clientId, silla, fecha)
        _hubConnection.On<string, string>("AgendarCita", async (clientId, modelocrearcita) =>
        {
            await ObtenerValidacionesAgenda(clientId, modelocrearcita);
        });
    }
    //----------------------Pasos:
    //1. Conectar con el servicio de SignalR usando la funcion ConnectToServer 
    //2. Recibir el pin de acceso de Rydent se usa el evento _hubConnection.On<string, string>("ObtenerPin"
    //3. Autenticar el pin de acceso de Rydent se usando await RecibirPinRydent(pin, clientId);
    //4. Enviar el pin de acceso de Rydent al servidor de Rydent
    public async Task ConnectToServer(bool primerIntento) //conectamos al servidor de SR y registramos el equipo
    {
        //_hubConnection.On<string,string>("ReceiveMessage",(user, message) =>
        //{
        //    Console.WriteLine($"Usuario Recibido: {user}");
        //    Console.WriteLine($"Received message: {System.Environment.MachineName}");
        //    Console.WriteLine($"Received message: {message}");
        //});

        //Cuando el servidor de SR nos envie un mensaje ObtenerPin a nivel local validamos por medio de RecibirPinRydent
        //el pin de acceso de Rydent si exiiste invocamos la funcion RespuestaObtenerPin que esta en el servidor de SR
        
        
        await _hubConnection.StartAsync();
        // aca estamos invocando una funcion que tenemos en el servidor signalR llamada RegistrarEquipo
        // y le pasamos el id de la conexion al siganlR el cual fue asignado por el servidor signalR
        // y el id encriptado del disco duro del equipo que esta corriendo el worker
        var datosClientes = await new TDATOSCLIENTESServicios().ConsultarPorId(System.Environment.MachineName);
        await _hubConnection.InvokeAsync("RegistrarEquipo", _hubConnection.ConnectionId, datosClientes.ENTRADA);
    }
    

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool primerIntento = true;
        await registrarSuscripciones();
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Iniciando la consulta a la tabla ");

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                   
                    if (_hubConnection.State != HubConnectionState.Connected)
                    {
                        await ConnectToServer(primerIntento);
                        primerIntento = false;
                        
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
        var objEPS = new TCODIGOS_EPSServicios();
        var objDepartamentos = new TCODIGOS_DEPARTAMENTOServicios();
        var objCiudades = new TCODIGOS_CIUDADServicios();
        var objFrasesXEvolucion = new T_FRASE_XEVOLUCIONServicios();
        var objHorariosAgenda = new THORARIOSAGENDAServicios();
        var objFestivos = new TFESTIVOSServicios();
        var objConfiguracionesRydent = new TCONFIGURACIONES_RYDENTServicios();
        respuestaPin.clave = await objPINACCESO.ConsultarPorId(pinacceso);
        if (respuestaPin.clave != null)
        {
            respuestaPin.clave.CLAVE = "";
        }   
        var listDoctores =  await objDOCTORES.ConsultarTodos();
        var listEps = await objEPS.ConsultarTodos();
        var listDepartamentos = await objDepartamentos.ConsultarTodos();
        var listCiudades = await objCiudades.ConsultarTodos();
        var lstFrasesXEvolucion = await objFrasesXEvolucion.ConsultarTodos();
        var listHorariosAgenda = await objHorariosAgenda.ConsultarTodos();
        var listFestivos = await objFestivos.ConsultarTodos();
        var listConfiguracionesRydent = await objConfiguracionesRydent.ConsultarTodos();
        if (listDoctores != null && listDoctores.Count() > 0) 
        {
            respuestaPin.lstEps = listEps;
            respuestaPin.lstDepartamentos = listDepartamentos;
            respuestaPin.lstCiudades = listCiudades;
            respuestaPin.lstDoctores = listDoctores.ConvertAll(item => new ListadoItemModel() { id = item.ID.ToString(), nombre = (item.NOMBRE ?? "") });
            respuestaPin.lstFrasesXEvolucion = lstFrasesXEvolucion;
            respuestaPin.lstHorariosAgenda = listHorariosAgenda;
            respuestaPin.lstFestivos = listFestivos;
            respuestaPin.lstConfiguracionesRydent=listConfiguracionesRydent;
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
        var respuestaBuscarDatosPersonalesCompletosPacientes=new RespuestaDatosPersonales();
        respuestaBuscarDatosPersonalesCompletosPacientes.datosPersonales = datosPersonales;
        var fotoBase64 = await new TFOTOSFRONTALESServicios().ConsultarBase64PorId(idAnanesis);
        if (!string.IsNullOrEmpty(fotoBase64))
        {
            respuestaBuscarDatosPersonalesCompletosPacientes.strFotoFrontal = new ArchivosHelper().obtenerBase64ConPrefijo(fotoBase64);

        }
        await _hubConnection.InvokeAsync("RespuestaObtenerDatosPersonalesCompletosPaciente", clientId, JsonConvert.SerializeObject(respuestaBuscarDatosPersonalesCompletosPacientes));
    }

    public async Task BuscarAntecedentesPacientes(string clientId, int idAnanesis)
    {
        var objAntecedentes = new AntecedentesServicios();
        var antecedentes = await objAntecedentes.ConsultarPorId(idAnanesis);
        var respuestaBuscarAntecedentesPacientes = antecedentes;
        await _hubConnection.InvokeAsync("RespuestaObtenerAntecedentesPaciente", clientId, JsonConvert.SerializeObject(respuestaBuscarAntecedentesPacientes));
    }

    public async Task ObtenerCodigosEps (string clientId)
    {
        var objEps = new TCODIGOS_EPSServicios();
        var listEps = await objEps.ConsultarTodos();
        var respuestaBuscarEps = listEps;
        await _hubConnection.InvokeAsync("RespuestaObtenerCodigosEps", clientId, JsonConvert.SerializeObject(respuestaBuscarEps));
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

    public async Task GuardarDatosEvolucion(string clientId, string datosEvolucion)
    {
        try
        {
            var objEvolucion = JsonConvert.DeserializeObject<RespuestaEvolucionPacienteModel>(datosEvolucion);
            var objEvolucionServicios = new TEVOLUCIONServicios();
            var objFirma = new TFIRMAServicios();
            var archivosHelper = new ArchivosHelper();
            int idFirma = 0;
            if ((!string.IsNullOrEmpty(objEvolucion.imgFirmaPaciente) || !string.IsNullOrEmpty(objEvolucion.imgFirmaDoctor)))
            {
                var firma = new TFIRMA();
                firma.FECHA = DateTime.Now.Date.ToString("dd/MM/yyyy");
                firma.HORA = DateTime.Now.Date.ToString("hh:mm tt");
                byte[] firmaPaciente = archivosHelper.CrearImagenConBase64(objEvolucion.imgFirmaPaciente, objEvolucion.imgFirmaDoctor);
                if (firmaPaciente != null)
                {
                    firma.FIRMA = archivosHelper.ConvertirPNGaJPEG(firmaPaciente);
                }
                
                idFirma = await objFirma.Agregar(firma);
                if (idFirma > 0)
                {
                    objEvolucion.evolucion.FIRMA = idFirma;
                }
            }
            var resultado = await objEvolucionServicios.Agregar(objEvolucion.evolucion);
            await _hubConnection.InvokeAsync("RespuestaGuardarDatosEvolucion", clientId, resultado.ToString());
        }
        catch (Exception e)
        {
            throw;
        }
        
    }

   
    public async Task<RespuestaConsultarPorDiaYPorUnidadModel> ConsultarPorDiaYPorUnidad(string clientId, int silla, DateTime fecha)
    {
        var objHorariosAgenda = new THORARIOSAGENDAServicios();
        var objRespuestaConsultarPorDiaYPorUnidad = new P_AGENDA1();
        var objRespuestaConsultarHorariosSilla = await objHorariosAgenda.ConsultarPorId(silla); 
        var sillaStr=silla.ToString();
       
        var intervalo = objRespuestaConsultarHorariosSilla.INTERVALO;
        using (var _dbcontext = new AppDbContext())
        {
            if (intervalo == null || intervalo < 0)
            {
                intervalo = 15;

            }
            var obj = await _dbcontext.P_AGENDA1(sillaStr, fecha.Date, "1", objRespuestaConsultarHorariosSilla.HORAINICIAL, objRespuestaConsultarHorariosSilla.HORAFINAL, intervalo ?? 0, "", "");
            var modelo = new RespuestaConsultarPorDiaYPorUnidadModel();
           //---------------------------validar si el dia seleccoionado es festivo----------------------//
            var Festivo = new TFESTIVOSServicios();
            var objRespuestaBuscarFestivo = await Festivo.ConsultarPorFecha(fecha.Date);
            modelo.lstP_AGENDA1 = obj;
            if (objRespuestaBuscarFestivo !=  null && objRespuestaBuscarFestivo.FECHA > DateTime.MinValue)
            {
                modelo.esFestivo = true;
            }
            else
            {
                modelo.esFestivo = false;
            }
            //-----------------------------------------------------------------------------------------------//
            await _hubConnection.InvokeAsync("RespuestaObtenerConsultaPorDiaYPorUnidad", clientId, JsonConvert.SerializeObject(modelo));
            return modelo == null ? new RespuestaConsultarPorDiaYPorUnidadModel() : modelo;
            
            
        }
    }

    private async Task<string> validacionesExclullentes(string clientId, string datosAgenda)
    {
        var mensaje = "ACA VA MENSAJE SI VALIDACION NO PASO";
        //validar doctor no tenga cita a la misma hora en otra silla
        //validar que la agenda tenga espacio
        return mensaje ;
    }

    private void respuestaCrearCitaVacia(string clientId, List<ConfirmacionesPedidasModel> lstRespuestaConfirmacionesPedidas)
    {
        var objResp = new RespuestaConsultarPorDiaYPorUnidadModel();
        objResp.lstConfirmacionesPedidas = lstRespuestaConfirmacionesPedidas;
        _hubConnection.InvokeAsync("RespuestaAgendarCita", clientId, JsonConvert.SerializeObject(objResp));
    }

    private string BuscarHoraFinal(string horaCita, string duracion)
    {
        var horaFinal = AgregarMinutosAHora(horaCita, duracion);
        return horaFinal;
    }

    private string AgregarMinutosAHora(string hora, string minutos)
    {
        // Convierte la hora y los minutos a un TimeSpan
        TimeSpan horaCita = TimeSpan.Parse(hora);
        int duracion = int.Parse(minutos);

        // Agrega la duración a la hora de la cita
        TimeSpan horaFinal = horaCita.Add(TimeSpan.FromMinutes(duracion));

        // Convierte la hora final de vuelta a una cadena de hora
        string horaFinalString = horaFinal.ToString(@"hh\:mm");

        return horaFinalString;
    }

    public async Task ObtenerValidacionesAgenda(string clientId, string datosAgenda)
    {
        
        try
        {
            var exigeRevisarCronograma = false;
            var objDetallesCitasServicios = new TDETALLECITASServicios();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TimeSpanConverter());
            var objAgenda = JsonConvert.DeserializeObject<RespuestaConsultarPorDiaYPorUnidadModel>(datosAgenda, settings);

            //var objAgenda = JsonConvert.DeserializeObject<RespuestaConsultarPorDiaYPorUnidadModel>(datosAgenda);
            var fecha = objAgenda.lstDetallaCitas[0].FECHA;
            var nombre = objAgenda.lstDetallaCitas[0].NOMBRE;
            var historia = objAgenda.lstDetallaCitas[0].ID;
            //var editarCita = await objDetallesCitasServicios.ConsultarPorFechaSillaHora(fecha.Value, objAgenda.lstDetallaCitas[0].SILLA ?? 0, objAgenda.lstDetallaCitas[0].HORA);
            
            
            if (objAgenda.lstConfirmacionesPedidas != null && objAgenda.lstConfirmacionesPedidas.Count() > 0)
            {
                var lstConfirmacionesPedidas = objAgenda.lstConfirmacionesPedidas;
                List<ConfirmacionesPedidasModel> lstRespuestaConfirmacionesPedidas = new List<ConfirmacionesPedidasModel>();

                if (lstConfirmacionesPedidas.Where(x => x.nombreConfirmacion == "QDoctoresConCitaOtraUnidad").Any())
                {
                    bool estaRepetido = false;
                    var duracion = Convert.ToInt32(objAgenda.lstDetallaCitas[0].DURACION);
                    var horaCita = objAgenda.lstDetallaCitas[0].HORA;
                    var horaFinal = BuscarHoraFinal(horaCita.ToString(), duracion.ToString());
                    var horaFinalTs = TimeSpan.Parse(horaFinal);
                    var doctor = objAgenda.lstDetallaCitas[0].DOCTOR;
                    var silla = objAgenda.lstDetallaCitas[0].SILLA;
                    
                    //hacer consulta de hora incial de la agenda y hora final de la agenda
                    var objHorariosAgenda = new THORARIOSAGENDAServicios();
                    var objRespuestaConsultarHorariosSilla = await objHorariosAgenda.ConsultarPorId(objAgenda.lstDetallaCitas[0].SILLA??0);
                    var hayEspacio = await BuscarEspacioAgenda(objAgenda.lstDetallaCitas[0].SILLA.ToString(), objAgenda.lstDetallaCitas[0].FECHA.Value, "1", objRespuestaConsultarHorariosSilla.HORAINICIAL.ToString(), objRespuestaConsultarHorariosSilla.HORAFINAL.ToString(), 15, "", "", objAgenda.lstDetallaCitas[0].HORA.ToString(), BuscarHoraFinal(objAgenda.lstDetallaCitas[0].HORA.ToString(), objAgenda.lstDetallaCitas[0].DURACION.ToString()));
                    if (!hayEspacio)
                    {
                        lstRespuestaConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel()
                        {
                            mensaje = "Este espacio ya esta ocupado por otra cita",
                            nombreConfirmacion = "NO_HAY_ESPACIO",
                            pedirConfirmar = false,
                            esMensajeRestrictivo = true
                        });
                        respuestaCrearCitaVacia(clientId, lstRespuestaConfirmacionesPedidas);
                        return;
                    }

                    //aca hacemos el query para ver si el doctor tiene cita en otra unidad
                    if (fecha.HasValue && horaCita.HasValue)
                    {
                        estaRepetido = await objDetallesCitasServicios.ConsultarDoctoresConCitaOtraUnidad(doctor, fecha.Value, horaCita.Value, horaFinalTs);
                    }

                    if (estaRepetido)
                    {
                        lstRespuestaConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel()
                        {
                            mensaje = "El doctor tiene una cita en otra unidad a la misma hora, No se puede dar la cita",
                            nombreConfirmacion = "QDoctoresConCitaOtraUnidad",
                            pedirConfirmar = false,
                            esMensajeRestrictivo = true
                        });
                        respuestaCrearCitaVacia(clientId, lstRespuestaConfirmacionesPedidas);
                        return; // preguntar si este return nos saca de la funcion
                    }
                }
                if (lstConfirmacionesPedidas.Where(x => x.nombreConfirmacion == "CITA_REPETIDA").Any())
                {
                    bool estaRepetido = false;
                    var resultado = await objDetallesCitasServicios.ConsultarPacienteConCitaRepetida(nombre, fecha.Value, historia);
                    if (resultado != null)
                    {
                        lstRespuestaConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel()
                        {
                            mensaje = "El paciente ya tiene cita asignada el dia a la hora desea continuar asignando ésta cita?",
                            nombreConfirmacion = "CITA_REPETIDA",
                            pedirConfirmar = true,
                            esMensajeRestrictivo = false
                        });
                    }
                }

                if (lstConfirmacionesPedidas.Where(x => x.nombreConfirmacion == "PROXIMA_CITA_ASUNTO").Any())
                {
                    var idHistoria = objAgenda.lstDetallaCitas[0].ID;
                    if (idHistoria != null)
                    {
                        var objAnamnesis = new TANAMNESISServicios();
                        var objAnamnesisModel = await objAnamnesis.ConsultarPorIdTexto(idHistoria);
                        if (objAnamnesisModel != null && objAnamnesisModel.IDANAMNESIS > 0)
                        {
                            var objEvolucion = new TEVOLUCIONServicios();
                            var objEvolucionModel = await objEvolucion.ConsultarUltimaEvolucion(objAnamnesisModel.IDANAMNESIS);
                            if (objEvolucionModel != null)
                            {
                                objAgenda.lstDetallaCitas[0].ASUNTO = objEvolucionModel.PROXIMA_CITAstr;
                            }
                        }
                    }
                }

                
                // A partir de aca ya no hay validaciones ya entramos a guardar la cita
                if (lstRespuestaConfirmacionesPedidas.Any())
                {
                    objAgenda.lstConfirmacionesPedidas = lstRespuestaConfirmacionesPedidas;
                    _hubConnection.InvokeAsync("RespuestaAgendarCita", clientId, JsonConvert.SerializeObject(objAgenda));
                }
                else
                {
                    objAgenda.lstConfirmacionesPedidas = new List<ConfirmacionesPedidasModel>();
                    await GuardarDatosAgenda(clientId, objAgenda);

                }
            }
            else
            {
                await GuardarDatosAgenda(clientId, objAgenda);
            }
        }
        catch (Exception e)
        {

            throw;
        }

        
    }


    public async Task GuardarDatosAgenda(string clientId, RespuestaConsultarPorDiaYPorUnidadModel objAgenda)
    {
        try
        {
            
            var objTCitasServicios = new TCITASServicios();
            var objDetalleCitasServicios = new TDETALLECITASServicios();
            var objDetalleCitas = objAgenda.lstDetallaCitas[0];
            //-----------------Aca deben ir validaciones----------------------//
            if (objDetalleCitas.SILLA != null && objDetalleCitas.FECHA != null)
            {
                var existeAgenda = await objTCitasServicios.ConsultarPorId(objDetalleCitas.SILLA??0, objDetalleCitas.FECHA??DateTime.MinValue);
                if (existeAgenda == null || existeAgenda.SILLA <= 0)
                {
                    var objCita = new TCITAS();
                    objCita.SILLA = objDetalleCitas.SILLA??0;
                    objCita.FECHA = objDetalleCitas.FECHA??DateTime.MinValue;
                    objCita.FECHA_TEXTO = objAgenda.citas.FECHA_TEXTO; //Toca consultar el intervalo segun la silla y ponerlo;
                    await objTCitasServicios.Agregar(objCita);
                }
                await objDetalleCitasServicios.Agregar(objDetalleCitas);
                var objResp = new RespuestaConsultarPorDiaYPorUnidadModel();
                objResp.lstConfirmacionesPedidas = new List<ConfirmacionesPedidasModel>();
                objResp.lstConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel() { mensaje = "Cita guardada correctamente", nombreConfirmacion = "CITA_GUARDADA", pedirConfirmar = false, esMensajeRestrictivo = false }); 
                await _hubConnection.InvokeAsync("RespuestaAgendarCita", clientId, JsonConvert.SerializeObject(objResp));

            }
            
        }
        catch (Exception e)
        {
            var objResp = new RespuestaConsultarPorDiaYPorUnidadModel();
            objResp.lstConfirmacionesPedidas = new List<ConfirmacionesPedidasModel>();
            objResp.lstConfirmacionesPedidas.Add(new ConfirmacionesPedidasModel() { mensaje = "No se puedo guardar la cita: "+e.Message, nombreConfirmacion = "CITA_GUARDADA", pedirConfirmar = false, esMensajeRestrictivo = true });
            await _hubConnection.InvokeAsync("RespuestaAgendarCita", clientId, JsonConvert.SerializeObject(objResp));
        }
    }

    public async Task<bool> BuscarEspacioAgenda(string IN_SILLA, DateTime IN_FECHA, string IN_TIPO, string HORAINI, string HORAFIN, int INTERVALO, string PARARINI, string PARARFIN, string h1, string h2)
    {
        //var time1 = TimeSpan.Parse(h1);
        //var time2 = TimeSpan.Parse(h2);

        using (var _dbcontext = new AppDbContext())
        {
            try
            {
                var lstAgendaDelDiaPorFecha = await _dbcontext.P_AGENDA1(IN_SILLA, IN_FECHA.Date, "1", HORAINI, HORAFIN, INTERVALO, "", "");
                var lstAgendaDelDiaPorFechaFiltrado = lstAgendaDelDiaPorFecha.Where(x => x.OUT_HORA >= TimeSpan.Parse(h1) && x.OUT_HORA <= TimeSpan.Parse(h2).Subtract(TimeSpan.FromMinutes(1)) && !string.IsNullOrEmpty(x.OUT_NOMBRE));
                //if (editar)
                //{
                //    var lstAgendaDelDiaPorFechaFiltradoMenosElQueSeEdita = lstAgendaDelDiaPorFechaFiltrado.Where(x => x.OUT_HORA_CITA != HORACITAEDITAR && x.OUT_NOMBRE != NOMBREAEDITAR);
                //    return lstAgendaDelDiaPorFechaFiltradoMenosElQueSeEdita.Count() == 0;
                //}
                //else
                {
                    return lstAgendaDelDiaPorFechaFiltrado.Count() == 0;
                }
                
            }
            catch (Exception e)
            {

                return false;
            }
            
        }
    }





    public async Task BorrarDatosAgenda(string clientId, string datosAgenda)
    {

    }



    private async Task<string> RetornarFotoEnBase64ConPrefijo(int idFirma, int tipo)
    {
        var archivosHelper = new ArchivosHelper();
        var objFirma = new TFIRMAServicios();
        var resultadoFirma = await objFirma.ConsultarPorId(idFirma);
        if (tipo == 1)
        {
            var recorteFirmaPaciente = archivosHelper.recortarImganFromBytes(resultadoFirma.FIRMA, new Rectangle(0, 0, 1364, 225));
            var imagenReducida = archivosHelper.ReducirTamañoImagen(recorteFirmaPaciente, 30, 10);
            return archivosHelper.obtenerBase64ConPrefijo(imagenReducida);
        }
        else
        {
            var recorteFirmaDoctor = archivosHelper.recortarImganFromBytes(resultadoFirma.FIRMA, new Rectangle(0, (482 - 215), 1364, 215));
            var imagenReducida = archivosHelper.ReducirTamañoImagen(recorteFirmaDoctor, 30, 10);
            return archivosHelper.obtenerBase64ConPrefijo(imagenReducida);
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

    public class TimeSpanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dateTime = (DateTime)reader.Value;
            //var dateTime = DateTime.Parse(timeString);
            return dateTime.TimeOfDay;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var timeSpan = (TimeSpan)value;
            writer.WriteValue(timeSpan.ToString());
        }
    }

}

