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
    public class MVProductos : MVBase
    {
        private readonly IProductoRepository _productoRepository;

        private ObservableCollection<Producto> _todosActivos = new();
        private ObservableCollection<Producto> _todosArchivados = new();

        // ── Vista ─────────────────────────────────────────────────────────────
        private ListCollectionView _listaProductosView;
        public ListCollectionView ListaProductosView
        {
            get => _listaProductosView;
            set => SetProperty(ref _listaProductosView, value);
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

        private int _totalProductos;
        public int TotalProductos { get => _totalProductos; set => SetProperty(ref _totalProductos, value); }

        private int _totalStockBajo;
        public int TotalStockBajo { get => _totalStockBajo; set => SetProperty(ref _totalStockBajo, value); }

        private bool _hayStockBajo;
        public bool HayStockBajo { get => _hayStockBajo; set => SetProperty(ref _hayStockBajo, value); }

        // ── Filtro ────────────────────────────────────────────────────────────
        private string _filtroNombre = string.Empty;
        public string FiltroNombre
        {
            get => _filtroNombre;
            set { if (SetProperty(ref _filtroNombre, value)) AplicarFiltro(); }
        }

        // ── Formulario ────────────────────────────────────────────────────────
        private Producto _productoNuevo = new();
        public Producto ProductoNuevo { get => _productoNuevo; set => SetProperty(ref _productoNuevo, value); }

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand MostrarActivosCommand { get; }
        public ICommand MostrarArchivadosCommand { get; }
        public ICommand AgregarProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand DesactivarProductoCommand { get; }
        public ICommand ReactivarProductoCommand { get; }

        public MVProductos(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            InicializarProductoNuevo();

            MostrarActivosCommand = new RelayCommand(_ => MostrandoArchivados = false);
            MostrarArchivadosCommand = new RelayCommand(_ => MostrandoArchivados = true);
            AgregarProductoCommand = new RelayCommand(_ => AbrirFormularioNuevo());
            EditarProductoCommand = new RelayCommand(p => AbrirFormularioEdicion(p as Producto));
            DesactivarProductoCommand = new RelayCommand(async p => { if (p is int id) await DesactivarProducto(id); });
            ReactivarProductoCommand = new RelayCommand(async p => { if (p is int id) await ReactivarProducto(id); });

            _ = CargarProductos();
        }

        // ── Carga ─────────────────────────────────────────────────────────────
        public async Task CargarProductos()
        {
            try
            {
                var todos = await GetAllAsync(_productoRepository);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _todosActivos = new ObservableCollection<Producto>(todos.Where(p => p.Activo == true));
                    _todosArchivados = new ObservableCollection<Producto>(todos.Where(p => p.Activo != true));
                    CrearVista();
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando productos: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() => EstaVacio = true);
            }
        }

        private void AplicarFiltro() => Application.Current.Dispatcher.InvokeAsync(CrearVista);

        private void CrearVista()
        {
            var fuente = _mostrandoArchivados ? _todosArchivados : _todosActivos;
            var filtrado = string.IsNullOrEmpty(_filtroNombre)
                ? fuente
                : new ObservableCollection<Producto>(fuente.Where(p =>
                    p.Nombre.IndexOf(_filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0));

            ListaProductosView = new ListCollectionView(filtrado);
            TotalProductos = filtrado.Count;
            EstaVacio = filtrado.Count == 0;
            TotalStockBajo = _todosActivos.Count(p => p.AlertaStock);
            HayStockBajo = TotalStockBajo > 0;
        }

        // ── Formulario ────────────────────────────────────────────────────────
        private void InicializarProductoNuevo()
        {
            ProductoNuevo = new Producto
            {
                Nombre = string.Empty,
                Proveedor = string.Empty,
                Descripcion = string.Empty,
                Precio = 0,
                Cantidad = 0,
                StockMinimo = 5,
                StockMaximo = 100,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
        }

        private void AbrirFormularioNuevo()
        {
            InicializarProductoNuevo();
            new AgregarProducto(this).ShowDialog();
        }

        private void AbrirFormularioEdicion(Producto? p)
        {
            if (p == null) return;
            InicializarProductoNuevo();
            new AgregarProducto(this) { ProductoAEditar = p }.ShowDialog();
        }

        // ── CRUD ──────────────────────────────────────────────────────────────
        public async Task<bool> GuardarProducto()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ProductoNuevo.Nombre))
                { MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio."); return false; }
                await AddAsync(_productoRepository, ProductoNuevo);
                SnackbarMessageQueue.Enqueue($"'{ProductoNuevo.Nombre}' guardado.");
                await CargarProductos(); InicializarProductoNuevo(); return true;
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); return false; }
        }

        public async Task<bool> ActualizarProducto()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ProductoNuevo.Nombre))
                { MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio."); return false; }
                var t = await _productoRepository.GetByIdAsync(ProductoNuevo.Id);
                if (t == null) { MensajeError.Mostrar("Error", "Producto no encontrado."); return false; }
                t.Nombre = ProductoNuevo.Nombre; t.Proveedor = ProductoNuevo.Proveedor;
                t.Precio = ProductoNuevo.Precio; t.Cantidad = ProductoNuevo.Cantidad;
                t.StockMinimo = ProductoNuevo.StockMinimo; t.StockMaximo = ProductoNuevo.StockMaximo;
                await UpdateAsync(_productoRepository, t);
                SnackbarMessageQueue.Enqueue($"'{t.Nombre}' actualizado.");
                await CargarProductos(); InicializarProductoNuevo(); return true;
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); return false; }
        }

        private async Task DesactivarProducto(int id)
        {
            try
            {
                var p = await GetByIdAsync(_productoRepository, id);
                if (p == null) return;
                var d = new DialogoEliminar($"¿Archivar '{p.Nombre}'? Podrás reactivarlo desde Archivados.")
                { Owner = Application.Current.MainWindow };
                if (d.ShowDialog() == true)
                {
                    p.Activo = false;
                    if (await UpdateAsync(_productoRepository, p))
                    { SnackbarMessageQueue.Enqueue($"'{p.Nombre}' archivado."); await CargarProductos(); }
                }
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); }
        }

        private async Task ReactivarProducto(int id)
        {
            try
            {
                var p = await GetByIdAsync(_productoRepository, id);
                if (p == null) return;
                p.Activo = true;
                if (await UpdateAsync(_productoRepository, p))
                { SnackbarMessageQueue.Enqueue($"'{p.Nombre}' reactivado."); await CargarProductos(); }
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); }
        }

        public static bool TieneStockBajo(Producto producto) => producto?.AlertaStock ?? false;
    }
}