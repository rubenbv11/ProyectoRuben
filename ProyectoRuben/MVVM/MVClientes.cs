using di.proyecto.clase._2025.Frontend.Mensajes ;
using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
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

        public ICommand AgregarClienteCommand { get; }
        public ICommand EditarClienteCommand { get; }
        public ICommand DesactivarClienteCommand { get; }
        public ICommand VerHistorialCommand { get; }

        public MVClientes(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            Clientes = new ObservableCollection<Cliente>();

            AgregarClienteCommand = new RelayCommand(_ => AgregarCliente());
            EditarClienteCommand = new RelayCommand(async (param) => await EditarCliente(param as Cliente));
            DesactivarClienteCommand = new RelayCommand(async (param) => await DesactivarCliente((int)param));
            VerHistorialCommand = new RelayCommand(param => VerHistorial(param as Cliente));

            _ = CargarClientes();
        }

        /// <summary>
        /// Carga todos los clientes activos de la base de datos.
        /// </summary>
        public async Task CargarClientes()
        {
            try
            {
                var todosLosClientes = await GetAllAsync(_clienteRepository);
                var clientesActivos = todosLosClientes.Where(c => c.Activo == true).ToList();

                Clientes.Clear();
                foreach (var cliente in clientesActivos)
                {
                    Clientes.Add(cliente);
                }

                // Crear una ListCollectionView para filtrado
                ListaClientesView = new ListCollectionView(Clientes);
                ListaClientesView.Filter = obj =>
                {
                    if (string.IsNullOrEmpty(FiltroNombre))
                        return true;

                    var cliente = obj as Cliente;
                    return cliente != null && cliente.Nombre.IndexOf(FiltroNombre, StringComparison.OrdinalIgnoreCase) >= 0;
                };

                EstaVacio = Clientes.Count == 0;
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando clientes: {ex.Message}");
                EstaVacio = true;
            }
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
                // Aquí se abrirá un diálogo para agregar cliente
                // Por ahora, mostramos una notificación
                SnackbarMessageQueue.Enqueue("Función 'Agregar Cliente' disponible próximamente.");

                // En el futuro:
                // var dialogo = new AgregarClienteDialog();
                // if (dialogo.ShowDialog() == true)
                // {
                //     await CargarClientes();
                // }
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al agregar cliente: {ex.Message}");
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
