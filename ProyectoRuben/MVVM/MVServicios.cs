using di.proyecto.clase._2025.Frontend.Mensajes;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using pruebaNavegacion.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace ProyectoRuben.MVVM
{
    /// <summary>
    /// ViewModel para la gestión de servicios.
    /// Maneja la carga, filtrado y operaciones CRUD de servicios activos.
    /// </summary>
    public class MVServicios : MVBase
    {
        private readonly IServicioRepository _servicioRepository;

        private ObservableCollection<Servicio> _servicios;
        public ObservableCollection<Servicio> Servicios
        {
            get => _servicios;
            set => SetProperty(ref _servicios, value);
        }

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

        private string _filtroNombre;
        public string FiltroNombre
        {
            get => _filtroNombre;
            set
            {
                if (SetProperty(ref _filtroNombre, value))
                {
                    AplicarFiltro();
                }
            }
        }

        private Servicio _servicioNuevo;
        public Servicio ServicioNuevo
        {
            get => _servicioNuevo;
            set => SetProperty(ref _servicioNuevo, value);
        }

        public ICommand AgregarServicioCommand { get; }
        public ICommand EditarServicioCommand { get; }
        public ICommand DesactivarServicioCommand { get; }

        public MVServicios(IServicioRepository servicioRepository)
        {
            _servicioRepository = servicioRepository ?? throw new ArgumentNullException(nameof(servicioRepository));
            Servicios = new ObservableCollection<Servicio>();
            InicializarServicioNuevo();

            AgregarServicioCommand = new RelayCommand(_ => AgregarServicio());
            EditarServicioCommand = new RelayCommand(async (param) => await EditarServicio(param as Servicio));
            DesactivarServicioCommand = new RelayCommand(async (param) => await DesactivarServicio((int)param));

            _ = CargarServicios();
        }

        /// <summary>
        /// Carga todos los servicios activos de la base de datos.
        /// </summary>
        public async Task CargarServicios()
        {
            try
            {
                var todosLosServicios = await GetAllAsync(_servicioRepository);
                var serviciosActivos = todosLosServicios.Where(s => s.Activo == true).ToList();

                Servicios.Clear();
                foreach (var servicio in serviciosActivos)
                {
                    Servicios.Add(servicio);
                }

                // Crear una ListCollectionView para filtrado
                ListaServiciosView = new ListCollectionView(Servicios);
                ListaServiciosView.Filter = obj =>
                {
                    if (string.IsNullOrEmpty(FiltroNombre))
                        return true;

                    var servicio = obj as Servicio;
                    return servicio != null && servicio.Nombre.IndexOf(FiltroNombre, StringComparison.OrdinalIgnoreCase) >= 0;
                };

                EstaVacio = Servicios.Count == 0;
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando servicios: {ex.Message}");
                EstaVacio = true;
            }
        }

        /// <summary>
        /// Inicializa un nuevo servicio con valores por defecto.
        /// </summary>
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

        /// <summary>
        /// Abre un diálogo para agregar un nuevo servicio.
        /// </summary>
        private void AgregarServicio()
        {
            try
            {
                // Aquí se abrirá un diálogo para agregar servicio
                // Por ahora, mostramos una notificación
                SnackbarMessageQueue.Enqueue("Función 'Agregar Servicio' disponible próximamente.");
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al agregar servicio: {ex.Message}");
            }
        }

        /// <summary>
        /// Edita un servicio existente.
        /// </summary>
        private async Task EditarServicio(Servicio servicio)
        {
            if (servicio == null)
            {
                MensajeAdvertencia.Mostrar("Advertencia", "Por favor, selecciona un servicio para editar.");
                return;
            }

            try
            {
                // Lógica de edición (diálogo, etc.)
                SnackbarMessageQueue.Enqueue($"Editando servicio: {servicio.Nombre}");
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al editar servicio: {ex.Message}");
            }
        }

        /// <summary>
        /// Desactiva un servicio (baja lógica, no borra de la BD).
        /// </summary>
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

                servicio.Activo = false;
                await UpdateAsync(_servicioRepository, servicio);

                SnackbarMessageQueue.Enqueue($"Servicio {servicio.Nombre} desactivado.");
                await CargarServicios();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al desactivar servicio: {ex.Message}");
            }
        }

        /// <summary>
        /// Aplica filtro por nombre en tiempo real.
        /// </summary>
        private void AplicarFiltro()
        {
            ListaServiciosView?.Refresh();
        }
    }
}
