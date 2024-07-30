using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;
using System.Data;
using System.Data.SqlClient;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class T_ADICIONALES_ABONOSServicios : IT_ADICIONALES_ABONOSServicios
    {
        private readonly AppDbContext _dbcontext;
        public T_ADICIONALES_ABONOSServicios()
        {
            
        }


        public async Task<T_ADICIONALES_ABONOS> Agregar(T_ADICIONALES_ABONOS t_adicionales_abonos)
        {
            using (var _dbcontext = new AppDbContext())
            {
                _dbcontext.T_ADICIONALES_ABONOS.Add(t_adicionales_abonos);
                await _dbcontext.SaveChangesAsync();
                return t_adicionales_abonos;
            }
        }

        public async Task Borrar(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_ADICIONALES_ABONOS.FirstOrDefaultAsync(x => x.ID == ID && x.IDENTIFICADOR == IDENTIFICADOR && x.IDDOCTOR == IDDOCTOR && x.FASE == FASE);
                if (obj != null)
                {
                    _dbcontext.T_ADICIONALES_ABONOS.Remove(obj);
                    await _dbcontext.SaveChangesAsync();
                }
            }
        }

        public async Task<T_ADICIONALES_ABONOS> ConsultarPorId(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_ADICIONALES_ABONOS.FirstOrDefaultAsync(x => x.ID == ID && x.IDENTIFICADOR == IDENTIFICADOR && x.IDDOCTOR == IDDOCTOR && x.FASE == FASE);
                return obj == null ? new T_ADICIONALES_ABONOS() : obj;
            }
        }

        public async Task<int> ConsultarPacientesAbonaronEntreFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_ADICIONALES_ABONOS.Where(x => x.TIPO == 1 && x.FECHA >= fechaInicio && x.FECHA <= fechaFin).CountAsync();
                return obj;
            }
        }

        public async Task<decimal> ConsultarTotalAbonadoEntreFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_ADICIONALES_ABONOS.Where(x => x.TIPO == 1 && x.FECHA >= fechaInicio && x.FECHA <= fechaFin).SumAsync(x => x.VALOR);
                return obj ?? 0;
            }

        }



        public async Task<RespuestasQuerysEstadoCuenta> ConsultarTotalSumaAbonosYDescuentos(int ID, int FASE, int IDDOCTOR)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var abonos = await _dbcontext.T_ADICIONALES_ABONOS
                  .Where(aa => aa.ID == ID && aa.FASE == FASE && aa.IDDOCTOR == IDDOCTOR && aa.TIPO == 1)
                  .SumAsync(aa => aa.VALOR);

                var descuentos = await _dbcontext.T_ADICIONALES_ABONOS
                   .Where(aa => aa.ID == ID && aa.FASE == FASE && aa.IDDOCTOR == IDDOCTOR && aa.TIPO == 3)
                   .SumAsync(aa => aa.VALOR);

                return new RespuestasQuerysEstadoCuenta { ABONOS = abonos, DESCUENTOS = descuentos };
            }
        }



        public async Task<RespuestasQuerysEstadoCuenta> ConsultarValorDescuentoPorIdMaestra(int idMaestra)
        {
            using (var _dbcontext = new AppDbContext())
            {
                if (idMaestra <= 0)
                {
                    return new RespuestasQuerysEstadoCuenta { VALOR = 0, DESCUENTO = 0 };
                }

                var query = from dt in _dbcontext.T_PRESUPUESTO
                            join pm in _dbcontext.T_PRESUPUESTOS_MAESTRA on dt.ID_MAESTRA equals pm.ID
                            where dt.ID_MAESTRA == idMaestra
                            group dt by new { pm.ID, pm.DESCUENTO_PORCENTAJE } into g
                            select new
                            {
                                Valor = (g.Sum(x => x.COSTO ?? 0)),
                                Descuento = ((g.Key.DESCUENTO_PORCENTAJE ?? 0) * g.Sum(x => x.COSTO ?? 0)) / 100
                            };

                var result = await query.FirstOrDefaultAsync();

                return result != null
                    ? new RespuestasQuerysEstadoCuenta { VALOR = result.Valor, DESCUENTO = result.Descuento }
                    : new RespuestasQuerysEstadoCuenta { VALOR = 0, DESCUENTO = 0 };
            }

        }


        public async Task<decimal> ConsultarValorTratamientoSinFinanciar(int ID, int FASE, int IDDOCTOR)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var valor = await (from dt in _dbcontext.T_DEFINICION_TRATAMIENTO
                                   from aa in _dbcontext.T_ADICIONALES_ABONOS
                                   where dt.ID == aa.ID && dt.FASE == aa.FASE && dt.IDDOCTOR == aa.IDDOCTOR
                                   where dt.FECHA_INICIO == aa.FECHA && aa.TIPO == 2
                                   select aa.VALOR ?? 0).SumAsync();

                return valor;
            }
        }

        public async Task<DateTime> ConsultarUltimaFechaAbono(int id, int fase, int idDoctor)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var fechaMaxima = await _dbcontext.T_ADICIONALES_ABONOS
                .Where(a => a.ID == id && a.FASE == fase && a.TIPO == 1 && a.IDDOCTOR == idDoctor)
                .MaxAsync(a => (DateTime?)a.FECHA);

                return fechaMaxima ?? new DateTime();
            }
        }

        private class idFaseDcotor
        {
            public int ID { get; set; }
            public int FASE { get; set; }
            public int IDDOCTOR { get; set; }
        }

        public async Task<List<RespuestaSaldoPorDoctor>> ConsultarSaldoPorDoctor(int idanamnesis)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var resultados = await (
                from p in _dbcontext.T_PRESUPUESTO
                join d in _dbcontext.T_DEFINICION_TRATAMIENTO on p.ID_MAESTRA equals d.IDPRESUPUESTOMAESTRA
                join abonos in (
                    from a in _dbcontext.T_ADICIONALES_ABONOS
                    join dd in _dbcontext.TDATOSDOCTORES on a.RECIBIDO_POR equals dd.ID
                    join dt in _dbcontext.T_DEFINICION_TRATAMIENTO on new { Id = a.ID, FASE = a.FASE ?? 0, IDDOCTOR = a.IDDOCTOR ?? 0 } equals new { Id = dt.ID, FASE = dt.FASE, IDDOCTOR = dt.IDDOCTOR }
                    group a by new { dd.NOMBRE, dt.ID } into g
                    where g.Key.ID == idanamnesis
                    select new { Doctor = g.Key.NOMBRE, AbonosTotales = g.Sum(x => x.VALOR) ?? 0 }
                ) on p.DOCTOR equals abonos.Doctor into abonosGroup
                from abonos in abonosGroup.DefaultIfEmpty()
                where d.ID == idanamnesis
                group new { p, abonos } by new { p.DOCTOR, abonos.AbonosTotales } into g
                select new RespuestaSaldoPorDoctor
                {
                    DOCTOR = g.Key.DOCTOR,
                    VALOR_TOTAL = g.Sum(x => x.p.COSTO ?? 0),
                    ABONOS = g.Key.AbonosTotales
                }
                ).ToListAsync();

                return resultados;
            }
        }


        //public async Task<List<RespuestaSaldoPorDoctor>> ConsultarSaldoPorDoctor(int idanamnesis)
        //{
        //    using (var _dbcontext = new AppDbContext())
        //    {
        //        var resultadosParciales = await (from p in _dbcontext.T_PRESUPUESTO
        //                                         join d in _dbcontext.T_DEFINICION_TRATAMIENTO on p.ID_MAESTRA equals d.IDPRESUPUESTOMAESTRA
        //                                         where d.ID == idanamnesis
        //                                         group p by new { p.DOCTOR, d.ID } into g
        //                                         select new { g.Key.DOCTOR, g.Key.ID, VALOR_TOTAL = g.Sum(x => x.COSTO ?? 0) }
        //                                        ).ToListAsync();

        //        var resultados = new List<RespuestaSaldoPorDoctor>();

        //        foreach (var resultado in resultadosParciales)
        //        {
        //            var abonos = await (from a in _dbcontext.T_ADICIONALES_ABONOS
        //                                join dd in _dbcontext.TDATOSDOCTORES on a.RECIBIDO_POR equals dd.ID
        //                                join dt in _dbcontext.T_DEFINICION_TRATAMIENTO on new idFaseDcotor() { ID = a.ID, FASE = a.FASE ?? 0, IDDOCTOR = a.IDDOCTOR ?? 0 } equals new idFaseDcotor() { ID = dt.ID, FASE = dt.FASE, IDDOCTOR = dt.IDDOCTOR }
        //                                where dd.NOMBRE == resultado.DOCTOR && dt.ID == resultado.ID
        //                                group a by dt.ID into g2
        //                                select g2.Sum(x => x.VALOR) ?? 0
        //                               ).FirstOrDefaultAsync();

        //            resultados.Add(new RespuestaSaldoPorDoctor
        //            {
        //                DOCTOR = resultado.DOCTOR,
        //                VALOR_TOTAL = resultado.VALOR_TOTAL,
        //                ABONOS = abonos
        //            });
        //        }

        //        return resultados;
        //    }
        //}



        //public async Task<List<RespuestaSaldoPorDoctor>> ConsultarSaldoPorDoctor(int idanamnesis)
        //{
        //    using (var _dbcontext = new AppDbContext())
        //    {
        //        var resultados = await (from p in _dbcontext.T_PRESUPUESTO
        //                                join d in _dbcontext.T_DEFINICION_TRATAMIENTO on p.ID_MAESTRA equals d.IDPRESUPUESTOMAESTRA
        //                                where d.ID == idanamnesis
        //                                group p by new { p.DOCTOR, d.ID } into g
        //                                select new RespuestaSaldoPorDoctor
        //                                {
        //                                    DOCTOR = g.Key.DOCTOR,
        //                                    VALOR_TOTAL = g.Sum(x => x.COSTO ?? 0),
        //                                    ABONOS = (
        //                                        from a in _dbcontext.T_ADICIONALES_ABONOS
        //                                        join dd in _dbcontext.TDATOSDOCTORES on a.RECIBIDO_POR equals dd.ID
        //                                        join dt in _dbcontext.T_DEFINICION_TRATAMIENTO on new idFaseDcotor() { ID = a.ID, FASE = a.FASE ?? 0, IDDOCTOR = a.IDDOCTOR ?? 0 } equals new idFaseDcotor() { ID = dt.ID, FASE = dt.FASE, IDDOCTOR = dt.IDDOCTOR }
        //                                        where dd.NOMBRE == g.Key.DOCTOR && dt.ID == g.Key.ID
        //                                        group a by dt.ID into g2
        //                                        select g2.Sum(x => x.VALOR) ?? 0
        //                                   ).FirstOrDefault()
        //                                }).ToListAsync();

        //        return resultados;
        //    }
        //}
        //public async Task<List<RespuestaSaldoPorDoctor>> ConsultarSaldoPorDoctor(int idanamnesis)
        //{
        //    using (var _dbcontext = new AppDbContext())
        //    {
        //        var resultados = await (from p in _dbcontext.T_PRESUPUESTO
        //                                join d in _dbcontext.T_DEFINICION_TRATAMIENTO on p.ID_MAESTRA equals d.IDPRESUPUESTOMAESTRA
        //                                where d.ID == idanamnesis
        //                                group p by new { p.DOCTOR, d.ID } into g
        //                                select new RespuestaSaldoPorDoctor
        //                                {
        //                                    DOCTOR = g.Key.DOCTOR,
        //                                    VALOR_TOTAL = g.Sum(x => x.COSTO ?? 0),
        //                                    ABONOS = (
        //                                        from a in _dbcontext.T_ADICIONALES_ABONOS
        //                                        join dd in _dbcontext.TDATOSDOCTORES on a.RECIBIDO_POR equals dd.ID
        //                                        join dt in _dbcontext.T_DEFINICION_TRATAMIENTO on new idFaseDcotor() { ID = a.ID, FASE =  a.FASE ?? 0, IDDOCTOR = a.IDDOCTOR ?? 0 } equals new idFaseDcotor() { ID = dt.ID, FASE = dt.FASE, IDDOCTOR = dt.IDDOCTOR }
        //                                        where dd.NOMBRE == g.Key.DOCTOR && dt.ID == g.Key.ID
        //                                        group a by new { dd.NOMBRE, dt.ID } into g2
        //                                        select g2.Sum(x => x.VALOR) ?? 0
        //                                   ).FirstOrDefault()


        //                                }).ToListAsync();

        //        return resultados;
        //    }
        //}
        //public async Task<List<RespuestaSaldoPorDoctor>> ConsultarSaldoPorDoctor(int idanamnesis)
        //{
        //    using (var _dbcontext = new AppDbContext())
        //    {
        //        var query = @"SELECT p.DOCTOR, SUM(p.costo) AS Valor_Total, 
        //              (SELECT SUM(a.VALOR) FROM T_ADICIONALES_ABONOS a 
        //               INNER JOIN TDATOSDOCTORES dd ON a.RECIBIDO_POR = dd.ID 
        //               INNER JOIN T_DEFINICION_TRATAMIENTO dt ON a.ID = dt.ID AND a.FASE = dt.FASE AND a.IDDOCTOR = dt.IDDOCTOR 
        //               WHERE dd.NOMBRE = p.DOCTOR 
        //               GROUP BY dt.ID 
        //               HAVING dt.ID = {0}) AS abonos  
        //              FROM T_PRESUPUESTO p 
        //              JOIN T_DEFINICION_TRATAMIENTO d ON p.ID_MAESTRA = d.IDPRESUPUESTOMAESTRA  
        //              WHERE d.ID = {0}
        //              GROUP BY p.DOCTOR, abonos";

        //        var resultados = await _dbcontext.RespuestaSaldoPorDoctor
        //            .FromSqlRaw(query, idanamnesis)
        //            .ToListAsync();

        //        return resultados;
        //    }
        //}


        public async Task<bool> Editar(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE, T_ADICIONALES_ABONOS t_adicionales_abonos)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_ADICIONALES_ABONOS.FirstOrDefaultAsync(x => x.ID == ID && x.IDENTIFICADOR == IDENTIFICADOR && x.IDDOCTOR == IDDOCTOR && x.FASE == FASE);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(t_adicionales_abonos);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface IT_ADICIONALES_ABONOSServicios
    {
        Task<T_ADICIONALES_ABONOS> Agregar(T_ADICIONALES_ABONOS t_adicionales_abonos);
        Task<bool> Editar(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE, T_ADICIONALES_ABONOS t_adicionales_abonos);
        Task<T_ADICIONALES_ABONOS> ConsultarPorId(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE);
        Task<int> ConsultarPacientesAbonaronEntreFechas(DateTime fechaInicio, DateTime fechaFin);
        Task<decimal> ConsultarTotalAbonadoEntreFechas(DateTime fechaInicio, DateTime fechaFin);
        Task<RespuestasQuerysEstadoCuenta> ConsultarTotalSumaAbonosYDescuentos(int ID, int FASE, int IDDOCTOR);
        Task<RespuestasQuerysEstadoCuenta> ConsultarValorDescuentoPorIdMaestra(int idMaestra);
        Task<DateTime> ConsultarUltimaFechaAbono(int id, int fase, int idDoctor);
        Task<List<RespuestaSaldoPorDoctor>> ConsultarSaldoPorDoctor(int idanamnesis);
        Task Borrar(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE);
    }
}
