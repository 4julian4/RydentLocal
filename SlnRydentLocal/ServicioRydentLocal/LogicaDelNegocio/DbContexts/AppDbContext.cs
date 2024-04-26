using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;
//using RydentDatos.RydentDB;

using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.TablasFraccionadas;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.TablasFraccionadas.TAnamnesis;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;

public class AppDbContext : DbContext
{
    public DbSet<TANAMNESIS> TANAMNESIS { get; set; }
    public DbSet<TCITAS> TCITAS { get; set; }
    public DbSet<TDETALLECITAS> TDETALLECITAS { get; set; }
    public DbSet<TCITASBORRADAS> TCITASBORRADAS { get; set; }
    public DbSet<TDATOSDOCTORES> TDATOSDOCTORES { get; set; }
    public DbSet<TEVOLUCION> TEVOLUCION { get; set; }
    public DbSet<THORARIOSAGENDA> THORARIOSAGENDA { get; set; }
    public DbSet<TINFORMACIONREPORTES> TINFORMACIONREPORTES { get; set; }
    public DbSet<TTRATAMIENTO> TTRATAMIENTO { get; set; }
    public DbSet<TPLANTRATAMIENTO> TPLANTRATAMIENTO { get; set; }
    //public DbSet<HC_ARCHIVO> HC_ARCHIVO { get; set; }
    //public DbSet<HC_ARCHIVO_SA> HC_ARCHIVO_SA { get; set; }
    //public DbSet<HC_FOTOS_MINI> HC_FOTOS_MINI { get; set; }
    //public DbSet<HC_FOTOS_SA> HC_FOTOS_SA { get; set; }
    //public DbSet<HC_FOTOS> HC_FOTOS { get; set; }
    public DbSet<T_ADICIONALES_ABONOS> T_ADICIONALES_ABONOS { get; set; }
    public DbSet<T_CONFIGURACION_VALOR> T_CONFIGURACION_VALOR { get; set; }
    public DbSet<T_CUENTASXCOBRAR> T_CUENTASXCOBRAR { get; set; }
    public DbSet<TCITASPENDIENTES> TCITASPENDIENTES { get; set; }
    public DbSet<TCONFIGURACIONCALENDARIO> TCONFIGURACIONCALENDARIO { get; set; }
    public DbSet<TCONFIGURACIONES_RYDENT> TCONFIGURACIONES_RYDENT { get; set; }
    public DbSet<TMENUS> TMENUS { get; set; }
    public DbSet<TMENUSITEMS> TMENUSITEMS { get; set; }
    public DbSet<TNOTIFICARCALENDARIO> TNOTIFICARCALENDARIO { get; set; }
    public DbSet<TRESOLUCION_DIAN> TRESOLUCION_DIAN { get; set; }
    public DbSet<TRESOLUCION_DIAN_OTROS> TRESOLUCION_DIAN_OTROS { get; set; }
    public DbSet<UNIDADREVISADA> UNIDADREVISADA { get; set; }
    public DbSet<T_PRESUPUESTO> T_PRESUPUESTO { get; set; }
    public DbSet<TCLAVE> TCLAVE { get; set; }
    public DbSet<TDATOSCLIENTES> TDATOSCLIENTES { get; set; }
    public DbSet<TFIRMA> TFIRMA { get; set; }
    public DbSet<TCODIGOS_EPS> TCODIGOS_EPS { get; set; }
    public DbSet<TCODIGOS_PROCEDIMIENTOS> TCODIGOS_PROCEDIMIENTOS { get; set; }
    public DbSet<TCODIGOS_CONSLUTAS> TCODIGOS_CONSLUTAS { get; set; }
    public DbSet<TFESTIVOS> TFESTIVOS { get; set; }
    public DbSet<TCODIGOS_DEPARTAMENTO> TCODIGOS_DEPARTAMENTO { get; set; }
    public DbSet<TCODIGOS_CIUDAD> TCODIGOS_CIUDAD { get; set; } 
    public DbSet<TFOTOSFRONTALES> TFOTOSFRONTALES { get; set; }
    public DbSet<T_FRASE_XEVOLUCION> T_FRASE_XEVOLUCION { get; set; }
    public DbSet<THISTORIAL> THISTORIAL { get; set; }
    public DbSet<TCITASCANCELADAS> TCITASCANCELADAS { get; set; }
    public DbSet<T_RIPS_DX> T_RIPS_DX { get; set; }
    public DbSet<T_RIPS_PROCEDIMIENTOS> T_RIPS_PROCEDIMIENTOS { get; set; }

    
    //public DbSet<Antecedentes> Antecedentes { get; set; }
    //public DbSet<DatosPersonales> DatosPersonales { get; set; }

    //[Keyless]
    DbSet<P_BUSCARPACIENTE> P_BUSCARPACIENTE_Result { get; set; }
    DbSet<P_AGENDA1> P_AGENDA1_Result { get; set; }
    public async Task<List<P_BUSCARPACIENTE>> P_BUSCARPACIENTE(int TIPO, string P_VALOR)
    {
        var TIPOParameter = new FbParameter("TIPO", TIPO);
        var P_VALORParameter = new FbParameter("P_VALOR", P_VALOR);
        var s = await this.P_BUSCARPACIENTE_Result.FromSqlRaw("select * from P_BUSCARPACIENTE(@TIPO, @P_VALOR)", TIPOParameter, P_VALORParameter).ToListAsync();
        return s;
    }

   
    public async Task<List<P_AGENDA1>> P_AGENDA1(string IN_SILLA, DateTime IN_FECHA, string IN_TIPO, string HORAINI, string HORAFIN, int INTERVALO, string PARARINI, string PARARFIN)
    {
        var IN_SILLAParameter = new FbParameter("IN_SILLA", IN_SILLA);
        var IN_FECHAParameter = new FbParameter("IN_FECHA", IN_FECHA);
        var IN_TIPOParameter = new FbParameter("IN_TIPO", IN_TIPO);
        var HORAINIParameter = new FbParameter("HORAINI", HORAINI);
        var HORAFINParameter = new FbParameter("HORAFIN", HORAFIN);
        var INTERVALOParameter = new FbParameter("INTERVALO", INTERVALO);
        var PARARINIParameter = new FbParameter("PARARINI", PARARINI);
        var PARARFINParameter = new FbParameter("PARARFIN", PARARFIN);
        if (PARARINI == "")
        {
            PARARINIParameter.Value = null;
        }
        if (PARARFIN == "")
        {
            PARARFINParameter.Value = null;
        }
        try
        {
            var s = await this.P_AGENDA1_Result.FromSqlRaw("select * from P_AGENDA1(@IN_SILLA, @IN_FECHA, @IN_TIPO, @HORAINI, @HORAFIN, @INTERVALO, @PARARINI, @PARARFIN)", IN_SILLAParameter, IN_FECHAParameter, IN_TIPOParameter, HORAINIParameter, HORAFINParameter, INTERVALOParameter, PARARINIParameter, PARARFINParameter).ToListAsync();
            return s;
        }
        catch (Exception e)
        {

            throw;
        }
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<THISTORIAL>()
            .HasKey(c => new { c.FECHA, c.HORA });
        modelBuilder.Entity<TCITASCANCELADAS>()
            .HasKey(c => new { c.SILLA, c.FECHA, c.HORA });
        modelBuilder.Entity<T_ADICIONALES_ABONOS>()
            .HasKey(c => new { c.ID, c.IDENTIFICADOR, c.IDDOCTOR, c.FASE });    
        modelBuilder.Entity<TCITAS>()
            .HasKey(c => new { c.SILLA, c.FECHA });
        modelBuilder.Entity<TDETALLECITAS>()
            .HasKey(c => new { c.SILLA, c.FECHA, c.HORA });
        modelBuilder.Entity<TCITASBORRADAS>()
            .HasKey(c => new { c.SILLA, c.FECHA, c.HORA });
        modelBuilder.Entity<TTRATAMIENTO>()
           .HasKey(c => new { c.IDTRATAMIENTO, c.FECHA });
        modelBuilder.Entity<TCODIGOS_EPS>()
            .HasKey(c => new { c.CODIGO });
        modelBuilder.Entity<TCODIGOS_PROCEDIMIENTOS>()
            .HasKey(c => new { c.CODIGO });
        modelBuilder.Entity<TCODIGOS_CONSLUTAS>()
            .HasKey(c => new { c.CODIGO });
        modelBuilder.Entity<TFESTIVOS>()
            .HasKey(c => new { c.FECHA });
        modelBuilder.Entity<TCODIGOS_CIUDAD>()
            .HasKey(c => new { c.CODIGO_CIUDAD });
        //modelBuilder.Entity<TANAMNESIS>()
        //   .HasOne(t => t.DatosPersonales)
        //   .WithOne()
        //   .HasForeignKey<DatosPersonales>(dp => dp.IDANAMNESIS);

        //modelBuilder.Entity<TANAMNESIS>()
        //    .HasOne(t => t.Antecedentes)        //    .WithOne()
        //    .HasForeignKey<Antecedentes>(da => da.IDANAMNESIS);
        modelBuilder.Entity<TANAMNESIS>().ToTable("TANAMNESIS");
    }


  
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Ajusta la cadena de conexión según tu configuración de Firebird.
        optionsBuilder.UseFirebird("database=localhost:C:\\Program Files\\Acrom\\Bdr2\\R2.FDB;user=sysdba;password=masterkeyPort=3050;Dialect=3;Charset=ISO8859_1;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;");
    }
}
