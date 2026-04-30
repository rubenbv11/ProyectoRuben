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
    public class HorarioRepository : GenericRepository<Horario>, IHorarioRepository
    {
        public HorarioRepository(GestioninventarioyserviciosContext context, ILogger<HorarioRepository> logger) : base(context, logger) { }
    }
}
