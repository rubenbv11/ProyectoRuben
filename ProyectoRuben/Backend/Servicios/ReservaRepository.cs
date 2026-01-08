using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProyectoRuben.Backen.Modelo;
using pruebaNavegacion.Backend.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoRuben.Backend.Servicios
{
    public class ReservaRepository : GenericRepository<Reserva>, IReservaRepository
    {
        public ReservaRepository(GestioninventarioyserviciosContext context, ILogger<ReservaRepository> logger) : base(context, logger) { }
        public async Task<IEnumerable<Reserva>> GetReservasByFechaAsync(DateTime fecha) =>
            await Query().Include(r => r.Cliente).Include(r => r.Servicio).Where(r => r.Fecha.Date == fecha.Date).ToListAsync();
    }
}
