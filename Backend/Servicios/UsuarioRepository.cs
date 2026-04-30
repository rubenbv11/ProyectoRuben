using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProyectoRuben.Backen.Modelo;

namespace pruebaNavegacion.Backend.Servicios
{
    /// <summary>
    /// Implementación de <see cref="IUsuarioRepository"/> basada en <see cref="GenericRepository{T}"/>.
    /// Proporciona consultas específicas para <see cref="Usuario"/>.
    /// </summary>
    public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
    {
        private readonly ILogger<UsuarioRepository> _logger;

        public UsuarioRepository(GestioninventarioyserviciosContext context, ILogger<UsuarioRepository> logger)
            : base(context, logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<bool> LoginAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrEmpty(password))
                return false;

            try
            {
                var user = await Query(asNoTracking: true)
                    .FirstOrDefaultAsync(u => u.Email == usernameOrEmail || u.Nombre == usernameOrEmail, cancellationToken);

                if (user == null || string.IsNullOrEmpty(user.Contrasena))
                    return false;

                var storedHash = user.Contrasena;

                // Intentar verificar con BCrypt si la librería está presente en runtime (sin dependencia directa)
                try
                {
                    var bcryptType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a =>
                        {
                            try { return a.GetTypes(); }
                            catch { return Array.Empty<Type>(); }
                        })
                        .FirstOrDefault(t => t.FullName == "BCrypt.Net.BCrypt");

                    if (bcryptType != null)
                    {
                        var verifyMethod = bcryptType.GetMethod("Verify", new[] { typeof(string), typeof(string) });
                        if (verifyMethod != null)
                        {
                            var result = (bool)verifyMethod.Invoke(null, new object[] { password, storedHash })!;
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "BCrypt verification attempt failed; falling back to plain compare.");
                }

                // Fallback: comparación directa (útil en entornos de desarrollo o hashes no presentes)
                return string.Equals(storedHash, password, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante LoginAsync para {User}", usernameOrEmail);
                throw;
            }
        }
    }
}