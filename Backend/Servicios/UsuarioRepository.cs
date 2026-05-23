using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using BCrypt.Net;

namespace pruebaNavegacion.Backend.Servicios
{
    public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
    {
        private readonly ILogger<UsuarioRepository> _logger;

        public UsuarioRepository(GestioninventarioyserviciosContext context, ILogger<UsuarioRepository> logger)
            : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            try
            {
                return await Query(asNoTracking: true)
                    .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por email {Email}", email);
                throw;
            }
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                return await Query(asNoTracking: true)
                    .AnyAsync(u => u.Email == email, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al comprobar existencia de email {Email}", email);
                throw;
            }
        }

        public async Task<bool> LoginAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrEmpty(password))
                return false;

            try
            {
                var user = await Query(asNoTracking: true)
                    .FirstOrDefaultAsync(u => u.Email == usernameOrEmail
                                           || u.Nombre == usernameOrEmail, cancellationToken);

                if (user == null || string.IsNullOrEmpty(user.Contrasena))
                    return false;

                // BCrypt directo — sin reflexión
                bool loginCorrecto = BCrypt.Net.BCrypt.Verify(password, user.Contrasena);

                _logger.LogInformation("Login: usuario={U}, resultado={R}",
                    usernameOrEmail, loginCorrecto);

                if (loginCorrecto) SesionUsuario.IniciarSesion(user);

                return loginCorrecto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante LoginAsync para {User}", usernameOrEmail);
                throw;
            }
        }
    }
}