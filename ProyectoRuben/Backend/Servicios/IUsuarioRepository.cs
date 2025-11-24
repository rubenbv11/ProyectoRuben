using System.Threading;
using System.Threading.Tasks;
using ProyectoRuben.Backen.Modelo;

namespace pruebaNavegacion.Backend.Servicios
{
    /// <summary>
    /// Repositorio específico para operaciones sobre la entidad <see cref="Usuario"/>.
    /// Extiende <see cref="IGenericRepository{Usuario}"/> con funcionalidad propia de usuarios.
    /// </summary>
    public interface IUsuarioRepository : IGenericRepository<Usuario>
    {
        /// <summary>
        /// Obtiene un usuario por su email (puede devolver null si no existe).
        /// </summary>
        Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Comprueba si existe un usuario con el email especificado.
        /// </summary>
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Intenta autenticar un usuario mediante nombre o email y contraseña.
        /// Devuelve true si las credenciales son válidas.
        /// </summary>
        Task<bool> LoginAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default);
    }
}