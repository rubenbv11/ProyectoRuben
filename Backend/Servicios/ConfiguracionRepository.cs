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
    public class ConfiguracionRepository : GenericRepository<Configuracion>, IConfiguracionRepository
    {
        public ConfiguracionRepository(GestioninventarioyserviciosContext context, ILogger<ConfiguracionRepository> logger) : base(context, logger) { }
        public async Task<string?> GetValorAsync(string clave)
        {
            var c = await Query().FirstOrDefaultAsync(x => x.Clave == clave);
            return c?.Valor;
        }
    }
}
