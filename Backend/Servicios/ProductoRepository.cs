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
    public class ProductoRepository : GenericRepository<Producto>, IProductoRepository
    {
        public ProductoRepository(GestioninventarioyserviciosContext context, ILogger<ProductoRepository> logger) : base(context, logger) { }
    }
}
