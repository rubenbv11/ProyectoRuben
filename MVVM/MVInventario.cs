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
            ActualizarStockCommand = new RelayCommand(async (param) =>
            {
                if (param is Producto p) await GuardarStock(p);
            });
            _ = InicializarAsync();
        }

        private async Task InicializarAsync()
        {
            try
            {
                var productos = await _productoRepository.GetAllAsync();
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ListaProductos = new ObservableCollection<Producto>(productos);
                });
            }
            catch (Exception ex) when (ex is not OutOfMemoryException
                                           and not StackOverflowException)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ListaProductos = new ObservableCollection<Producto>
                    {
                        new Producto { Id = 1, Nombre = "Champú Anticaída", Cantidad = 25, StockMinimo = 5 }
                    };
                });
                SnackbarMessageQueue.Enqueue($"Sin conexión: {ex.Message}");
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