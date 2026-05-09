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

        // Fuente completa
        private ObservableCollection<Producto> _todosLosProductos = new();

        // ── Vista filtrada ────────────────────────────────────────────────────
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

        private int _totalProductos;
        public int TotalProductos
        {
            get => _totalProductos;
            set => SetProperty(ref _totalProductos, value);
        }

        private int _totalStockBajo;
        public int TotalStockBajo
        {
            get => _totalStockBajo;
            set => SetProperty(ref _totalStockBajo, value);
        }

        private bool _hayStockBajo;
        public bool HayStockBajo
        {
            get => _hayStockBajo;
            set => SetProperty(ref _hayStockBajo, value);
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
        private Producto _productoNuevo = new();
        public Producto ProductoNuevo
        {
            get => _productoNuevo;
            set => SetProperty(ref _productoNuevo, value);
        }

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand AgregarProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand DesactivarProductoCommand { get; }

        // ══════════════════════════════════════════════════════════════════════
        // Constructor
        // ══════════════════════════════════════════════════════════════════════
        public MVProductos(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository
                ?? throw new ArgumentNullException(nameof(productoRepository));

            InicializarProductoNuevo();

            AgregarProductoCommand = new RelayCommand(_ => AbrirFormularioNuevo());
            EditarProductoCommand = new RelayCommand(p => AbrirFormularioEdicion(p as Producto));
            DesactivarProductoCommand = new RelayCommand(async p =>
            {
                if (p is int id) await DesactivarProducto(id);
            });

            _ = CargarProductos();
        }

        // ══════════════════════════════════════════════════════════════════════
        // Carga
        // ══════════════════════════════════════════════════════════════════════
        public async Task CargarProductos()
        {
            try
            {
                var todos = await GetAllAsync(_productoRepository);
                var activos = todos.Where(p => p.Activo == true).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _todosLosProductos = new ObservableCollection<Producto>(activos);
                    CrearVista();
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando productos: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() => EstaVacio = true);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Filtro rápido sin freeze
        // ══════════════════════════════════════════════════════════════════════
        private void AplicarFiltro()
        {
            Application.Current.Dispatcher.InvokeAsync(CrearVista);
        }

        private void CrearVista()
        {
            var filtrado = string.IsNullOrEmpty(_filtroNombre)
                ? _todosLosProductos
                : new ObservableCollection<Producto>(
                    _todosLosProductos.Where(p =>
                        p.Nombre.IndexOf(_filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0));

            ListaProductosView = new ListCollectionView(filtrado);
            TotalProductos = filtrado.Count;
            EstaVacio = filtrado.Count == 0;

            // Stats de stock bajo
            TotalStockBajo = filtrado.Count(p => p.AlertaStock);
            HayStockBajo = TotalStockBajo > 0;
        }

        // ══════════════════════════════════════════════════════════════════════
        // Formulario nuevo
        // ══════════════════════════════════════════════════════════════════════
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
            try
            {
                InicializarProductoNuevo();
                var dialogo = new AgregarProducto(this);
                dialogo.ShowDialog();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al abrir formulario: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Guardar NUEVO producto
        // ══════════════════════════════════════════════════════════════════════
        public async Task<bool> GuardarProducto()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ProductoNuevo.Nombre))
                {
                    MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio.");
                    return false;
                }
                if (ProductoNuevo.Precio < 0)
                {
                    MensajeAdvertencia.Mostrar("Validación", "El precio no puede ser negativo.");
                    return false;
                }
                if (ProductoNuevo.Cantidad < 0)
                {
                    MensajeAdvertencia.Mostrar("Validación", "La cantidad no puede ser negativa.");
                    return false;
                }

                await AddAsync(_productoRepository, ProductoNuevo);
                SnackbarMessageQueue.Enqueue($"Producto '{ProductoNuevo.Nombre}' guardado.");
                await CargarProductos();
                InicializarProductoNuevo();
                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al guardar: {ex.Message}");
                return false;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Editar producto existente
        // ══════════════════════════════════════════════════════════════════════
        private void AbrirFormularioEdicion(Producto? producto)
        {
            if (producto == null) return;

            try
            {
                InicializarProductoNuevo();
                var dialogo = new AgregarProducto(this)
                {
                    ProductoAEditar = producto
                };
                dialogo.ShowDialog();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al abrir edición: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Actualizar producto existente
        // ══════════════════════════════════════════════════════════════════════
        public async Task<bool> ActualizarProducto()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ProductoNuevo.Nombre))
                {
                    MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio.");
                    return false;
                }

                var tracked = await _productoRepository.GetByIdAsync(ProductoNuevo.Id);
                if (tracked == null)
                {
                    MensajeError.Mostrar("Error", "Producto no encontrado.");
                    return false;
                }

                tracked.Nombre = ProductoNuevo.Nombre;
                tracked.Proveedor = ProductoNuevo.Proveedor;
                tracked.Precio = ProductoNuevo.Precio;
                tracked.Cantidad = ProductoNuevo.Cantidad;
                tracked.StockMinimo = ProductoNuevo.StockMinimo;
                tracked.StockMaximo = ProductoNuevo.StockMaximo;

                await UpdateAsync(_productoRepository, tracked);
                SnackbarMessageQueue.Enqueue($"Producto '{tracked.Nombre}' actualizado.");
                await CargarProductos();
                InicializarProductoNuevo();
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

                var dialogo = new DialogoEliminar(
                    $"¿Desactivar '{producto.Nombre}'? Dejará de aparecer en el catálogo.")
                {
                    Owner = Application.Current.MainWindow
                };

                if (dialogo.ShowDialog() == true)
                {
                    producto.Activo = false;
                    if (await UpdateAsync(_productoRepository, producto))
                    {
                        SnackbarMessageQueue.Enqueue($"'{producto.Nombre}' desactivado.");
                        await CargarProductos();
                    }
                }
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al desactivar: {ex.Message}");
            }
        }

        // Método legacy para compatibilidad con UCInventario
        public static bool TieneStockBajo(Producto producto)
            => producto?.AlertaStock ?? false;
    }
}