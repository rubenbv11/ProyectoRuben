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
    public class FacturaRepository : GenericRepository<Factura>, IFacturaRepository
    {
        public FacturaRepository(GestioninventarioyserviciosContext context, ILogger<FacturaRepository> logger) : base(context, logger) { }
        public async Task<IEnumerable<Factura>> GetFacturasByClienteIdAsync(int clienteId) =>
            await Query().Where(f => f.ClienteId == clienteId).ToListAsync();
    }
}
