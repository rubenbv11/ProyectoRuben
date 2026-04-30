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
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(GestioninventarioyserviciosContext context, ILogger<RoleRepository> logger) : base(context, logger) { }
    }
}
