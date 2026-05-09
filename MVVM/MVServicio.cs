using di.proyecto.clase._2025.Frontend.Mensajes;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using ProyectoRuben.Frontend;
using pruebaNavegacion.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ProyectoRuben.MVVM
{
    public class MVServicios : MVBase
    {
        private readonly IServicioRepository _servicioRepository;

        // Fuente completa
        private ObservableCollection<Servicio> _todosLosServicios = new();

        // ── Vista filtrada ────────────────────────────────────────────────────
        private ListCollectionView _listaServiciosView;
        public ListCollectionView ListaServiciosView
        {
            get => _listaServiciosView;
            set => SetProperty(ref _listaServiciosView, value);
        }

        private bool _estaVacio;
        public bool EstaVacio
        {
            get => _estaVacio;
            set => SetProperty(ref _estaVacio, value);
        }

        private int _totalServicios;
        public int TotalServicios
        {
            get => _totalServicios;
            set => SetProperty(ref _totalServicios, value);
        }

        private string _filtroNombre = string.Empty;
        public string FiltroNombre
        {
            get => _filtroNombre;
            set
            {
                if (SetProperty(ref _filtroNombre, value))
                    AplicarFiltro();
            }
        }

        // ── Formulario ────────────────────────────────────────────────────────
        private Servicio _servicioNuevo = new();
        public Servicio ServicioNuevo
        {
            get => _servicioNuevo;
            set => SetProperty(ref _servicioNuevo, value);
        }

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand AgregarServicioCommand { get; }
        public ICommand EditarServicioCommand { get; }
        public ICommand DesactivarServicioCommand { get; }

        // ══════════════════════════════════════════════════════════════════════
        // Constructor
        // ══════════════════════════════════════════════════════════════════════
        public MVServicios(IServicioRepository servicioRepository)
        {
            _servicioRepository = servicioRepository
                ?? throw new ArgumentNullException(nameof(servicioRepository));

            InicializarServicioNuevo();

            AgregarServicioCommand = new RelayCommand(_ => AbrirFormularioNuevo());
            EditarServicioCommand = new RelayCommand(p => AbrirFormularioEdicion(p as Servicio));
            DesactivarServicioCommand = new RelayCommand(async p =>
            {
                if (p is int id) await DesactivarServicio(id);
            });

            _ = CargarServicios();
        }

        // ══════════════════════════════════════════════════════════════════════
        // Carga
        // ══════════════════════════════════════════════════════════════════════
        public async Task CargarServicios()
        {
            try
            {
                var todos = await GetAllAsync(_servicioRepository);
                var activos = todos.Where(s => s.Activo == true).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _todosLosServicios = new ObservableCollection<Servicio>(activos);
                    CrearVista();
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando servicios: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() => EstaVacio = true);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Filtro rápido (sin Refresh — evita freeze)
        // ══════════════════════════════════════════════════════════════════════
        private void AplicarFiltro()
        {
            Application.Current.Dispatcher.InvokeAsync(CrearVista);
        }

        private void CrearVista()
        {
            var filtrado = string.IsNullOrEmpty(_filtroNombre)
                ? _todosLosServicios
                : new ObservableCollection<Servicio>(
                    _todosLosServicios.Where(s =>
                        s.Nombre.IndexOf(_filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0));

            ListaServiciosView = new ListCollectionView(filtrado);
            TotalServicios = filtrado.Count;
            EstaVacio = filtrado.Count == 0;
        }

        // ══════════════════════════════════════════════════════════════════════
        // Formulario nuevo
        // ══════════════════════════════════════════════════════════════════════
        private void InicializarServicioNuevo()
        {
            ServicioNuevo = new Servicio
            {
                Nombre = string.Empty,
                Descripcion = string.Empty,
                Duracion = 30,
                Costo = 0,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
        }

        private void AbrirFormularioNuevo()
        {
            try
            {
                InicializarServicioNuevo();
                var dialogo = new AgregarServicio(this);
                dialogo.ShowDialog();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al abrir formulario: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Guardar NUEVO servicio
        // ══════════════════════════════════════════════════════════════════════
        public async Task<bool> GuardarServicio()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ServicioNuevo.Nombre))
                {
                    MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio.");
                    return false;
                }
                if (ServicioNuevo.Duracion <= 0)
                {
                    MensajeAdvertencia.Mostrar("Validación", "La duración debe ser mayor que 0.");
                    return false;
                }
                if (ServicioNuevo.Costo < 0)
                {
                    MensajeAdvertencia.Mostrar("Validación", "El precio no puede ser negativo.");
                    return false;
                }

                await AddAsync(_servicioRepository, ServicioNuevo);
                SnackbarMessageQueue.Enqueue($"Servicio '{ServicioNuevo.Nombre}' guardado.");
                await CargarServicios();
                InicializarServicioNuevo();
                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al guardar: {ex.Message}");
                return false;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Editar servicio existente
        // ══════════════════════════════════════════════════════════════════════
        private void AbrirFormularioEdicion(Servicio? servicio)
        {
            if (servicio == null) return;

            try
            {
                InicializarServicioNuevo();
                var dialogo = new AgregarServicio(this)
                {
                    ServicioAEditar = servicio
                };
                dialogo.ShowDialog();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al abrir edición: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Actualizar servicio existente
        // ══════════════════════════════════════════════════════════════════════
        public async Task<bool> ActualizarServicio()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ServicioNuevo.Nombre))
                {
                    MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio.");
                    return false;
                }
                if (ServicioNuevo.Duracion <= 0)
                {
                    MensajeAdvertencia.Mostrar("Validación", "La duración debe ser mayor que 0.");
                    return false;
                }

                var tracked = await _servicioRepository.GetByIdAsync(ServicioNuevo.Id);
                if (tracked == null)
                {
                    MensajeError.Mostrar("Error", "Servicio no encontrado.");
                    return false;
                }

                tracked.Nombre = ServicioNuevo.Nombre;
                tracked.Descripcion = ServicioNuevo.Descripcion;
                tracked.Duracion = ServicioNuevo.Duracion;
                tracked.Costo = ServicioNuevo.Costo;

                await UpdateAsync(_servicioRepository, tracked);
                SnackbarMessageQueue.Enqueue($"Servicio '{tracked.Nombre}' actualizado.");
                await CargarServicios();
                InicializarServicioNuevo();
                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al actualizar: {ex.Message}");
                return false;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Desactivar
        // ══════════════════════════════════════════════════════════════════════
        private async Task DesactivarServicio(int servicioId)
        {
            try
            {
                var servicio = await GetByIdAsync(_servicioRepository, servicioId);
                if (servicio == null)
                {
                    MensajeError.Mostrar("Error", "Servicio no encontrado.");
                    return;
                }

                var dialogo = new DialogoEliminar(
                    $"¿Desactivar '{servicio.Nombre}'? Dejará de aparecer en el catálogo.")
                {
                    Owner = Application.Current.MainWindow
                };

                if (dialogo.ShowDialog() == true)
                {
                    servicio.Activo = false;
                    if (await UpdateAsync(_servicioRepository, servicio))
                    {
                        SnackbarMessageQueue.Enqueue($"'{servicio.Nombre}' desactivado.");
                        await CargarServicios();
                    }
                }
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al desactivar: {ex.Message}");
            }
        }
    }
}