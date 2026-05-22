using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using pruebaNavegacion.Backend.Servicios;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using System.Linq;

namespace ProyectoRuben.MVVM
{
    public class MVAdministracion : INotifyPropertyChanged
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPermisoRepository _permisoRepository;
        private readonly IRolesPermisosRepository _rolesPermisosRepository;

        // ── Listas ────────────────────────────────────────────────────────────
        public ObservableCollection<Usuario> ListaUsuarios { get; } = new();
        public ObservableCollection<Permiso> ListaPermisos { get; } = new();
        public ObservableCollection<PermisoViewModel> PermisosDelRol { get; } = new();

        // ── Usuario seleccionado ──────────────────────────────────────────────
        private Usuario? _usuarioSeleccionado;
        public Usuario? UsuarioSeleccionado
        {
            get => _usuarioSeleccionado;
            set
            {
                _usuarioSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HayUsuarioSeleccionado));
                _ = CargarPermisosDelRolAsync();
            }
        }

        public bool HayUsuarioSeleccionado => _usuarioSeleccionado != null;

        // ── Formulario nuevo usuario ──────────────────────────────────────────
        private string _nuevoNombre = "";
        public string NuevoNombre
        {
            get => _nuevoNombre;
            set { _nuevoNombre = value; OnPropertyChanged(); }
        }

        private string _nuevoEmail = "";
        public string NuevoEmail
        {
            get => _nuevoEmail;
            set { _nuevoEmail = value; OnPropertyChanged(); }
        }

        private string _nuevoTelefono = "";
        public string NuevoTelefono
        {
            get => _nuevoTelefono;
            set { _nuevoTelefono = value; OnPropertyChanged(); }
        }

        private string _nuevoContrasena = "";
        public string NuevoContrasena
        {
            get => _nuevoContrasena;
            set { _nuevoContrasena = value; OnPropertyChanged(); }
        }

        private string _nuevoRol = "Empleado";
        public string NuevoRol
        {
            get => _nuevoRol;
            set { _nuevoRol = value; OnPropertyChanged(); }
        }

        // ── Estado UI ─────────────────────────────────────────────────────────
        private string _mensaje = "";
        public string Mensaje
        {
            get => _mensaje;
            set { _mensaje = value; OnPropertyChanged(); }
        }

        private bool _cargando;
        public bool Cargando
        {
            get => _cargando;
            set { _cargando = value; OnPropertyChanged(); }
        }

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand CargarCommand { get; }
        public ICommand CrearUsuarioCommand { get; }
        public ICommand ToggleActivoCommand { get; }
        public ICommand GuardarPermisosCommand { get; }
        public ICommand SeleccionarUsuarioCommand { get; }
        public ICommand SeleccionarRolCommand { get; }

        public MVAdministracion(IUsuarioRepository usuarioRepository,
                                IPermisoRepository permisoRepository,
                                IRolesPermisosRepository rolesPermisosRepository)
        {
            _usuarioRepository = usuarioRepository;
            _permisoRepository = permisoRepository;
            _rolesPermisosRepository = rolesPermisosRepository;

            CargarCommand = new RelayCommand(async _ => await CargarAsync());
            CrearUsuarioCommand = new RelayCommand(async _ => await CrearUsuarioAsync());
            ToggleActivoCommand = new RelayCommand(async u => await ToggleActivoAsync(u as Usuario));
            GuardarPermisosCommand = new RelayCommand(async _ => await GuardarPermisosAsync());

            SeleccionarUsuarioCommand = new RelayCommand(u =>
            {
                if (u is Usuario usuario)
                    UsuarioSeleccionado = usuario;
            });

            SeleccionarRolCommand = new RelayCommand(rol =>
            {
                if (rol is string r)
                    NuevoRol = r;
            });
        }

        // ── Carga inicial ─────────────────────────────────────────────────────
        public async Task CargarAsync()
        {
            Cargando = true;
            Mensaje = "";
            try
            {
                var usuarios = await _usuarioRepository.GetAllAsync();
                ListaUsuarios.Clear();
                foreach (var u in usuarios.OrderBy(u => u.Nombre))
                    ListaUsuarios.Add(u);

                var permisos = await _permisoRepository.GetAllAsync();
                ListaPermisos.Clear();
                foreach (var p in permisos)
                    ListaPermisos.Add(p);
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al cargar: {ex.Message}";
            }
            finally { Cargando = false; }
        }

        // ── Cargar permisos del rol del usuario seleccionado ──────────────────
        private async Task CargarPermisosDelRolAsync()
        {
            PermisosDelRol.Clear();
            if (_usuarioSeleccionado?.RolId == null) return;

            try
            {
                var permisosDelRol = await _rolesPermisosRepository
                    .GetPermisosByRoleIdAsync(_usuarioSeleccionado.RolId.Value);
                var idsConPermiso = permisosDelRol.Select(p => p.Id).ToHashSet();

                foreach (var permiso in ListaPermisos)
                {
                    PermisosDelRol.Add(new PermisoViewModel
                    {
                        Permiso = permiso,
                        Concedido = idsConPermiso.Contains(permiso.Id)
                    });
                }
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al cargar permisos: {ex.Message}";
            }
        }

        // ── Crear nuevo usuario ───────────────────────────────────────────────
        private async Task CrearUsuarioAsync()
        {
            if (string.IsNullOrWhiteSpace(NuevoNombre) ||
                string.IsNullOrWhiteSpace(NuevoContrasena))
            {
                Mensaje = "⚠ El nombre y la contraseña son obligatorios.";
                return;
            }

            Cargando = true;
            try
            {
                var nuevoUsuario = new Usuario
                {
                    Nombre = NuevoNombre.Trim(),
                    Email = NuevoEmail.Trim(),
                    Telefono = NuevoTelefono.Trim(),
                    Contrasena = NuevoContrasena,
                    Rol = NuevoRol,
                    RolId = NuevoRol == "Administrador" ? 1 : 2,
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };

                await _usuarioRepository.AddAsync(nuevoUsuario);

                NuevoNombre = NuevoEmail = NuevoTelefono = NuevoContrasena = "";
                NuevoRol = "Empleado";

                Mensaje = "✅ Usuario creado correctamente.";
                await CargarAsync();
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al crear usuario: {ex.Message}";
            }
            finally { Cargando = false; }
        }

        // ── Activar / desactivar usuario ──────────────────────────────────────
        private async Task ToggleActivoAsync(Usuario? usuario)
        {
            if (usuario == null) return;

            if (usuario.Id == SesionUsuario.UsuarioActual?.Id)
            {
                Mensaje = "⚠ No puedes desactivar tu propia cuenta.";
                return;
            }

            try
            {
                usuario.Activo = !(usuario.Activo ?? false);
                await _usuarioRepository.UpdateAsync(usuario);
                Mensaje = $"Usuario {usuario.Nombre} {(usuario.Activo == true ? "activado ✅" : "desactivado ⛔")}.";
                await CargarAsync();
            }
            catch (Exception ex)
            {
                Mensaje = $"Error: {ex.Message}";
            }
        }

        // ── Guardar permisos del rol ──────────────────────────────────────────
        private async Task GuardarPermisosAsync()
        {
            if (_usuarioSeleccionado?.RolId == null)
            {
                Mensaje = "⚠ Selecciona un usuario primero.";
                return;
            }

            Cargando = true;
            try
            {
                int rolId = _usuarioSeleccionado.RolId.Value;

                var actuales = await _rolesPermisosRepository.GetAllAsync();
                var delRol = actuales.Where(rp => rp.RolesId == rolId).ToList();

                foreach (var rp in delRol)
                    await _rolesPermisosRepository.RemoveAsync(rp);

                foreach (var pvm in PermisosDelRol.Where(p => p.Concedido))
                {
                    await _rolesPermisosRepository.AddAsync(new RolesPermisos
                    {
                        RolesId = rolId,
                        PermisosId = pvm.Permiso.Id
                    });
                }

                Mensaje = "✅ Permisos guardados correctamente.";
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al guardar permisos: {ex.Message}";
            }
            finally { Cargando = false; }
        }

        // ── INotifyPropertyChanged ────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class PermisoViewModel : INotifyPropertyChanged
    {
        public Permiso Permiso { get; set; } = null!;

        private bool _concedido;
        public bool Concedido
        {
            get => _concedido;
            set { _concedido = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}