using MaterialDesignThemes.Wpf;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using ProyectoRuben.MVVM;
using pruebaNavegacion.MVVM;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProyectoRuben.MVVM
{
    public class MVInventario : MVBase
    {
        private readonly IProductoRepository _productoRepository;
        private ObservableCollection<Producto> _listaProductos;

        public ObservableCollection<Producto> ListaProductos
        {
            get => _listaProductos;
            set => SetProperty(ref _listaProductos, value);
        }

        public ICommand ActualizarStockCommand { get; }

        public MVInventario(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
            ActualizarStockCommand = new RelayCommand(async (param) => await GuardarStock((Producto)param));

            CargarInventario();
        }

        private async void CargarInventario()
        {
            try
            {
                var productos = await _productoRepository.GetAllAsync();
                ListaProductos = new ObservableCollection<Producto>(productos);
            }
            catch
            {
                // SI NO HAY BASE DE DATOS, CARGA ESTOS DATOS FALSOS PARA VER EL DISEÑO
                ListaProductos = new ObservableCollection<Producto>
        {
            new Producto { Id = 1, Nombre = "Champú Anticaída", Proveedor = "L'Oréal", Precio = 15.50m, Cantidad = 25, StockMinimo = 5 },
            new Producto { Id = 2, Nombre = "Laca Fijación Fuerte", Proveedor = "Schwarzkopf", Precio = 12.00m, Cantidad = 3, StockMinimo = 5 } // Este saldrá en rojo porque 3 <= 5
        };
                SnackbarMessageQueue.Enqueue("Modo sin conexión: Cargando inventario de prueba");
            }
        }

        private async Task GuardarStock(Producto producto)
        {
            if (producto == null) return;

            // Actualizamos en BD
            await _productoRepository.UpdateAsync(producto);

            // Refrescamos la UI usando Snackbar de MVBase
            SnackbarMessageQueue.Enqueue($"Stock de '{producto.Nombre}' actualizado a {producto.Cantidad}.");
        }
    }
}