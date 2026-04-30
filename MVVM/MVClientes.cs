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
    /// <summary>
    /// ViewModel para la gestión de clientes.
    /// Maneja la carga, filtrado y operaciones CRUD de clientes activos.
    /// </summary>
    public class MVClientes : MVBase
    {
        private readonly IClienteRepository _clienteRepository;

        private ObservableCollection<Cliente> _clientes;
        public ObservableCollection<Cliente> Clientes
        {
            get => _clientes;
            set => SetProperty(ref _clientes, value);
        }

        private ListCollectionView _listaClientesView;
        public ListCollectionView ListaClientesView
        {
            get => _listaClientesView;
            set => SetProperty(ref _listaClientesView, value);
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

        private Cliente _clienteNuevo;
        public Cliente ClienteNuevo
        {
            get => _clienteNuevo;
            set => SetProperty(ref _clienteNuevo, value);
        }

        public ICommand AgregarClienteCommand { get; }
        public ICommand EditarClienteCommand { get; }
        public ICommand DesactivarClienteCommand { get; }
        public ICommand VerHistorialCommand { get; }

        public MVClientes(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            Clientes = new ObservableCollection<Cliente>();
            InicializarClienteNuevo();

            AgregarClienteCommand = new RelayCommand(_ => AgregarCliente());
            EditarClienteCommand = new RelayCommand(async (param) => await EditarCliente(param as Cliente));
            DesactivarClienteCommand = new RelayCommand(async (param) =>
            {
                if (param is int clienteId)
                    await DesactivarCliente(clienteId);
            });
            VerHistorialCommand = new RelayCommand(param => VerHistorial(param as Cliente));

            _ = CargarClientes();
        }

        /// <summary>
        /// Carga todos los clientes activos de la base de datos.
        /// Las actualizaciones de colección se ejecutan en el hilo de la UI para evitar fallos de dispatcher.
        /// </summary>
        public async Task CargarClientes()
        {
            try
            {
                var todosLosClientes = await GetAllAsync(_clienteRepository);
                var clientesActivos = todosLosClientes.Where(c => c.Activo == true).ToList();

                // Actualizar la colección en el hilo de la UI (ListCollectionView tiene afinidad de dispatcher)
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Clientes = new ObservableCollection<Cliente>(clientesActivos);

                    ListaClientesView = new ListCollectionView(Clientes);
                    ListaClientesView.Filter = obj =>
                    {
                        if (string.IsNullOrEmpty(FiltroNombre))
                            return true;
                        var c = obj as Cliente;
                        return c != null && c.Nombre.IndexOf(FiltroNombre, StringComparison.OrdinalIgnoreCase) >= 0;
                    };

                    EstaVacio = Clientes.Count == 0;
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando clientes: {ex.Message}");
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => EstaVacio = true);
            }
        }

        /// <summary>
        /// Inicializa un nuevo cliente con valores por defecto.
        /// </summary>
        private void InicializarClienteNuevo()
        {
            ClienteNuevo = new Cliente
            {
                Nombre = string.Empty,
                Telefono = string.Empty,
                Email = string.Empty,
                Contacto = string.Empty,
                Activo = true,
                FechaRegistro = DateTime.Now
            };
        }

        /// <summary>
        /// Aplica el filtro de búsqueda por nombre.
        /// </summary>
        private void AplicarFiltro()
        {
            ListaClientesView?.Refresh();
            EstaVacio = ListaClientesView?.Count == 0;
        }

        /// <summary>
        /// Abre un diálogo para agregar un nuevo cliente.
        /// </summary>
        private void AgregarCliente()
        {
            try
            {
                var dialogo = new AgregarCliente(this);
                dialogo.ShowDialog();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al abrir formulario de cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Guarda un nuevo cliente en la base de datos.
        /// </summary>
        // MVClientes.cs — GuardarCliente debe retornar bool:
        public async Task<bool> GuardarCliente()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ClienteNuevo.Nombre))
                {
                    MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio.");
                    return false; // ← indica fallo sin excepción
                }
                if (string.IsNullOrWhiteSpace(ClienteNuevo.Contacto))
                    ClienteNuevo.Contacto = ClienteNuevo.Nombre;

                await AddAsync(_clienteRepository, ClienteNuevo);
                SnackbarMessageQueue.Enqueue($"Cliente {ClienteNuevo.Nombre} guardado.");
                await CargarClientes();
                InicializarClienteNuevo();
                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al guardar cliente: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Edita un cliente existente.
        /// </summary>
        private async Task EditarCliente(Cliente cliente)
        {
            if (cliente == null)
            {
                MensajeAdvertencia.Mostrar("Advertencia", "Por favor, selecciona un cliente para editar.");
                return;
            }

            try
            {
                // Aquí se abrirá un diálogo para editar cliente
                // Por ahora, mostramos una notificación
                SnackbarMessageQueue.Enqueue($"Editando cliente: {cliente.Nombre}");

                // En el futuro:
                // var dialogo = new AgregarClienteDialog(cliente);
                // if (dialogo.ShowDialog() == true)
                // {
                //     await CargarClientes();
                // }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al editar cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Desactiva un cliente (baja lógica, no borra de la BD).
        /// </summary>
        private async Task DesactivarCliente(int clienteId)
        {
            try
            {
                var cliente = await GetByIdAsync(_clienteRepository, clienteId);
                if (cliente == null)
                {
                    MensajeError.Mostrar("Error", "Cliente no encontrado.");
                    return;
                }

                var resultado = MessageBox.Show(
                    $"¿Estás seguro de que deseas desactivar a {cliente.Nombre}? Esta acción no se puede deshacer fácilmente.",
                    "Confirmar desactivación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resultado == System.Windows.MessageBoxResult.Yes)
                {
                    cliente.Activo = false;
                    if (await UpdateAsync(_clienteRepository, cliente))
                    {
                        SnackbarMessageQueue.Enqueue($"Cliente {cliente.Nombre} desactivado correctamente.");
                        await CargarClientes();
                    }
                }
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al desactivar cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Muestra el historial de citas de un cliente.
        /// </summary>
        private void VerHistorial(Cliente cliente)
        {
            if (cliente == null)
            {
                MensajeAdvertencia.Mostrar("Advertencia", "Por favor, selecciona un cliente.");
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(cliente.HistorialCitas))
                {
                    MensajeInformacion.Mostrar(
                        "Historial Vacio",
                        $"El cliente {cliente.Nombre} no tiene historial de citas registrado.");
                }
                else
                {
                    MensajeInformacion.Mostrar(
                        "Historial de Citas",
                        $"Historial de {cliente.Nombre}:\n\n{cliente.HistorialCitas}");
                }
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al mostrar historial: {ex.Message}");
            }
        }
    }
}
