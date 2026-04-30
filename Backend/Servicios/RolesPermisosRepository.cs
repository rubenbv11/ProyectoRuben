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
    public class RolesPermisosRepository : GenericRepository<RolesPermisos>, IRolesPermisosRepository
    {
        public RolesPermisosRepository(GestioninventarioyserviciosContext context, ILogger<RolesPermisosRepository> logger) : base(context, logger) { }
        public async Task<IEnumerable<Permiso>> GetPermisosByRoleIdAsync(int roleId) =>
            await _context.RolesPermisos.Where(rp => rp.RolesId == roleId).Select(rp => rp.Permisos).ToListAsync();
    }
}