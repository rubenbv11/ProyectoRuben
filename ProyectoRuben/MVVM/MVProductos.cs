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
    /// ViewModel para la gestión de productos.
    /// Maneja la carga, filtrado y operaciones CRUD de productos activos.
    /// Incluye lógica visual para detectar stock bajo.
    /// </summary>
    public class MVProductos : MVBase
    {
        private readonly IProductoRepository _productoRepository;

        private ObservableCollection<Producto> _productos;
        public ObservableCollection<Producto> Productos
        {
            get => _productos;
            set => SetProperty(ref _productos, value);
        }

        private ListCollectionView _listaProductosView;
        public ListCollectionView ListaProductosView
        {
            get => _listaProductosView;
            set => SetProperty(ref _listaProductosView, value);
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

        private Producto _productoNuevo;
        public Producto ProductoNuevo
        {
            get => _productoNuevo;
            set => SetProperty(ref _productoNuevo, value);
        }

        public ICommand AgregarProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand DesactivarProductoCommand { get; }

        public MVProductos(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            Productos = new ObservableCollection<Producto>();
            InicializarProductoNuevo();

            AgregarProductoCommand = new RelayCommand(_ => AgregarProducto());
            EditarProductoCommand = new RelayCommand(async (param) => await EditarProducto(param as Producto));
            DesactivarProductoCommand = new RelayCommand(async (param) => await DesactivarProducto((int)param));

            _ = CargarProductos();
        }

        /// <summary>
        /// Carga todos los productos activos de la base de datos.
        /// </summary>
        public async Task CargarProductos()
        {
            try
            {
                var todosLosProductos = await GetAllAsync(_productoRepository);
                var productosActivos = todosLosProductos.Where(p => p.Activo == true).ToList();

                Productos.Clear();
                foreach (var producto in productosActivos)
                {
                    Productos.Add(producto);
                }

                // Crear una ListCollectionView para filtrado
                ListaProductosView = new ListCollectionView(Productos);
                ListaProductosView.Filter = obj =>
                {
                    if (string.IsNullOrEmpty(FiltroNombre))
                        return true;

                    var producto = obj as Producto;
                    return producto != null && producto.Nombre.IndexOf(FiltroNombre, StringComparison.OrdinalIgnoreCase) >= 0;
                };

                EstaVacio = Productos.Count == 0;
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando productos: {ex.Message}");
                EstaVacio = true;
            }
        }

        /// <summary>
        /// Inicializa un nuevo producto con valores por defecto.
        /// </summary>
        private void InicializarProductoNuevo()
        {
            ProductoNuevo = new Producto
            {
                Nombre = string.Empty,
                Descripcion = string.Empty,
                Cantidad = 0,
                StockMinimo = 5,
                StockMaximo = 100,
                Precio = 0,
                Proveedor = string.Empty,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
        }

        /// <summary>
        /// Abre un diálogo para agregar un nuevo producto.
        /// </summary>
        private void AgregarProducto()
        {
            try
            {
                // Aquí se abrirá un diálogo para agregar producto
                // Por ahora, mostramos una notificación
                SnackbarMessageQueue.Enqueue("Función 'Agregar Producto' disponible próximamente.");
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al agregar producto: {ex.Message}");
            }
        }

        /// <summary>
        /// Edita un producto existente.
        /// </summary>
        private async Task EditarProducto(Producto producto)
        {
            if (producto == null)
            {
                MensajeAdvertencia.Mostrar("Advertencia", "Por favor, selecciona un producto para editar.");
                return;
            }

            try
            {
                // Lógica de edición (diálogo, etc.)
                SnackbarMessageQueue.Enqueue($"Editando producto: {producto.Nombre}");
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al editar producto: {ex.Message}");
            }
        }

        /// <summary>
        /// Desactiva un producto (baja lógica, no borra de la BD).
        /// </summary>
        private async Task DesactivarProducto(int productoId)
        {
            try
            {
                var producto = await GetByIdAsync(_productoRepository, productoId);
                if (producto == null)
                {
                    MensajeError.Mostrar("Error", "Producto no encontrado.");
                    return;
                }

                producto.Activo = false;
                await UpdateAsync(_productoRepository, producto);

                SnackbarMessageQueue.Enqueue($"Producto {producto.Nombre} desactivado.");
                await CargarProductos();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al desactivar producto: {ex.Message}");
            }
        }

        /// <summary>
        /// Aplica filtro por nombre en tiempo real.
        /// </summary>
        private void AplicarFiltro()
        {
            ListaProductosView?.Refresh();
        }

        /// <summary>
        /// Determina si un producto tiene stock bajo.
        /// </summary>
        public static bool TieneStockBajo(Producto producto)
        {
            if (producto == null) return false;
            return producto.Cantidad <= (producto.StockMinimo ?? 0);
        }
    }
}
