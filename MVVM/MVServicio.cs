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

        private ObservableCollection<Servicio> _todosActivos = new();
        private ObservableCollection<Servicio> _todosArchivados = new();

        // ── Vista ─────────────────────────────────────────────────────────────
        private ListCollectionView _listaServiciosView;
        public ListCollectionView ListaServiciosView
        {
            get => _listaServiciosView;
            set => SetProperty(ref _listaServiciosView, value);
        }

        // ── Toggle ────────────────────────────────────────────────────────────
        private bool _mostrandoArchivados;
        public bool MostrandoArchivados
        {
            get => _mostrandoArchivados;
            set { if (SetProperty(ref _mostrandoArchivados, value)) { OnPropertyChanged(nameof(MostrandoActivos)); CrearVista(); } }
        }
        public bool MostrandoActivos => !_mostrandoArchivados;

        // ── Stats ─────────────────────────────────────────────────────────────
        private bool _estaVacio;
        public bool EstaVacio { get => _estaVacio; set => SetProperty(ref _estaVacio, value); }

        private int _totalServicios;
        public int TotalServicios { get => _totalServicios; set => SetProperty(ref _totalServicios, value); }

        // ── Filtro ────────────────────────────────────────────────────────────
        private string _filtroNombre = string.Empty;
        public string FiltroNombre
        {
            get => _filtroNombre;
            set { if (SetProperty(ref _filtroNombre, value)) AplicarFiltro(); }
        }

        // ── Formulario ────────────────────────────────────────────────────────
        private Servicio _servicioNuevo = new();
        public Servicio ServicioNuevo { get => _servicioNuevo; set => SetProperty(ref _servicioNuevo, value); }

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand MostrarActivosCommand { get; }
        public ICommand MostrarArchivadosCommand { get; }
        public ICommand AgregarServicioCommand { get; }
        public ICommand EditarServicioCommand { get; }
        public ICommand DesactivarServicioCommand { get; }
        public ICommand ReactivarServicioCommand { get; }

        public MVServicios(IServicioRepository servicioRepository)
        {
            _servicioRepository = servicioRepository ?? throw new ArgumentNullException(nameof(servicioRepository));
            InicializarServicioNuevo();

            MostrarActivosCommand = new RelayCommand(_ => MostrandoArchivados = false);
            MostrarArchivadosCommand = new RelayCommand(_ => MostrandoArchivados = true);
            AgregarServicioCommand = new RelayCommand(_ => AbrirFormularioNuevo());
            EditarServicioCommand = new RelayCommand(p => AbrirFormularioEdicion(p as Servicio));
            DesactivarServicioCommand = new RelayCommand(async p => { if (p is int id) await DesactivarServicio(id); });
            ReactivarServicioCommand = new RelayCommand(async p => { if (p is int id) await ReactivarServicio(id); });

            _ = CargarServicios();
        }

        // ── Carga ─────────────────────────────────────────────────────────────
        public async Task CargarServicios()
        {
            try
            {
                var todos = await GetAllAsync(_servicioRepository);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _todosActivos = new ObservableCollection<Servicio>(todos.Where(s => s.Activo == true));
                    _todosArchivados = new ObservableCollection<Servicio>(todos.Where(s => s.Activo != true));
                    CrearVista();
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando servicios: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() => EstaVacio = true);
            }
        }

        private void AplicarFiltro() => Application.Current.Dispatcher.InvokeAsync(CrearVista);

        private void CrearVista()
        {
            var fuente = _mostrandoArchivados ? _todosArchivados : _todosActivos;
            var filtrado = string.IsNullOrEmpty(_filtroNombre)
                ? fuente
                : new ObservableCollection<Servicio>(fuente.Where(s =>
                    s.Nombre.IndexOf(_filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0));

            ListaServiciosView = new ListCollectionView(filtrado);
            TotalServicios = filtrado.Count;
            EstaVacio = filtrado.Count == 0;
        }

        // ── Formulario ────────────────────────────────────────────────────────
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
            InicializarServicioNuevo();
            new AgregarServicio(this).ShowDialog();
        }

        private void AbrirFormularioEdicion(Servicio? s)
        {
            if (s == null) return;
            InicializarServicioNuevo();
            new AgregarServicio(this) { ServicioAEditar = s }.ShowDialog();
        }

        // ── CRUD ──────────────────────────────────────────────────────────────
        public async Task<bool> GuardarServicio()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ServicioNuevo.Nombre))
                { MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio."); return false; }
                if (ServicioNuevo.Duracion <= 0)
                { MensajeAdvertencia.Mostrar("Validación", "La duración debe ser mayor que 0."); return false; }
                await AddAsync(_servicioRepository, ServicioNuevo);
                SnackbarMessageQueue.Enqueue($"'{ServicioNuevo.Nombre}' guardado.");
                await CargarServicios(); InicializarServicioNuevo(); return true;
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); return false; }
        }

        public async Task<bool> ActualizarServicio()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ServicioNuevo.Nombre))
                { MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio."); return false; }
                var t = await _servicioRepository.GetByIdAsync(ServicioNuevo.Id);
                if (t == null) { MensajeError.Mostrar("Error", "Servicio no encontrado."); return false; }
                t.Nombre = ServicioNuevo.Nombre; t.Descripcion = ServicioNuevo.Descripcion;
                t.Duracion = ServicioNuevo.Duracion; t.Costo = ServicioNuevo.Costo;
                await UpdateAsync(_servicioRepository, t);
                SnackbarMessageQueue.Enqueue($"'{t.Nombre}' actualizado.");
                await CargarServicios(); InicializarServicioNuevo(); return true;
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); return false; }
        }

        private async Task DesactivarServicio(int id)
        {
            try
            {
                var s = await GetByIdAsync(_servicioRepository, id);
                if (s == null) return;
                var d = new DialogoEliminar($"¿Archivar '{s.Nombre}'? Podrás reactivarlo desde Archivados.")
                { Owner = Application.Current.MainWindow };
                if (d.ShowDialog() == true)
                {
                    s.Activo = false;
                    if (await UpdateAsync(_servicioRepository, s))
                    { SnackbarMessageQueue.Enqueue($"'{s.Nombre}' archivado."); await CargarServicios(); }
                }
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); }
        }

        private async Task ReactivarServicio(int id)
        {
            try
            {
                var s = await GetByIdAsync(_servicioRepository, id);
                if (s == null) return;
                s.Activo = true;
                if (await UpdateAsync(_servicioRepository, s))
                { SnackbarMessageQueue.Enqueue($"'{s.Nombre}' reactivado."); await CargarServicios(); }
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); }
        }
    }
}