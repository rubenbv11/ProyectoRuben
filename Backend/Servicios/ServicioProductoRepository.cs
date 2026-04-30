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
    public class ServicioProductoRepository : GenericRepository<ServicioProducto>, IServicioProductoRepository
    {
        public ServicioProductoRepository(GestioninventarioyserviciosContext context, ILogger<ServicioProductoRepository> logger) : base(context, logger) { }
        public async Task<IEnumerable<Producto>> GetProductosByServicioIdAsync(int servicioId) =>
            await Query().Include(sp => sp.Producto).Where(sp => sp.ServicioId == servicioId).Select(sp => sp.Producto).ToListAsync();
    }
}
