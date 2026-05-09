using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.EntityFrameworkCore;
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
    // ══════════════════════════════════════════════════════════════════════════
    // Wrapper que expone EsVip sin modificar la entidad de EF Core
    // ══════════════════════════════════════════════════════════════════════════
    public class ClienteViewModel
    {
        private readonly Cliente _cliente;

        public ClienteViewModel(Cliente cliente, bool esVip = false)
        {
            _cliente = cliente;
            EsVip = esVip;
        }

        public int Id => _cliente.Id;
        public string Nombre => _cliente.Nombre;
        public string? Telefono => _cliente.Telefono;
        public string? Email => _cliente.Email;
        public string? HistorialCitas => _cliente.HistorialCitas;
        public bool? Activo => _cliente.Activo;
        public bool EsVip { get; }
        public Cliente Modelo => _cliente;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ViewModel principal
    // ══════════════════════════════════════════════════════════════════════════
    public class MVClientes : MVBase
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IReservaRepository _reservaRepository;

        // Fuente completa (nunca se filtra, solo se lee)
        private ObservableCollection<ClienteViewModel> _todoLosClientes = new();

        // ── Vista filtrada (bound al ItemsControl) ────────────────────────────
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

        private int _totalClientes;
        public int TotalClientes
        {
            get => _totalClientes;
            set => SetProperty(ref _totalClientes, value);
        }

        // ── Filtro: usa un timer para no filtrar en cada tecla ────────────────
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
        private Cliente _clienteNuevo = new();
        public Cliente ClienteNuevo
        {
            get => _clienteNuevo;
            set => SetProperty(ref _clienteNuevo, value);
        }

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand AgregarClienteCommand { get; }
        public ICommand EditarClienteCommand { get; }
        public ICommand DesactivarClienteCommand { get; }
        public ICommand VerHistorialCommand { get; }

        // ══════════════════════════════════════════════════════════════════════
        // Constructor
        // ══════════════════════════════════════════════════════════════════════
        public MVClientes(IClienteRepository clienteRepository,
                          IReservaRepository reservaRepository)
        {
            _clienteRepository = clienteRepository
                ?? throw new ArgumentNullException(nameof(clienteRepository));
            _reservaRepository = reservaRepository
                ?? throw new ArgumentNullException(nameof(reservaRepository));

            InicializarClienteNuevo();

            AgregarClienteCommand = new RelayCommand(_ => AbrirFormularioNuevoCliente());
            EditarClienteCommand = new RelayCommand(p => AbrirFormularioEdicion(p as ClienteViewModel));
            DesactivarClienteCommand = new RelayCommand(async p =>
            {
                if (p is int id) await DesactivarCliente(id);
            });
            VerHistorialCommand = new RelayCommand(async p =>
                await MostrarHistorial(p as ClienteViewModel));

            _ = CargarClientes();
        }

        // ══════════════════════════════════════════════════════════════════════
        // Carga de datos
        // ══════════════════════════════════════════════════════════════════════
        public async Task CargarClientes()
        {
            try
            {
                var todos = await GetAllAsync(_clienteRepository);
                var activos = todos.Where(c => c.Activo == true).ToList();

                var wrappers = activos
                    .Select(c => new ClienteViewModel(c))
                    .ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _todoLosClientes = new ObservableCollection<ClienteViewModel>(wrappers);

                    // Crear la vista con el filtro ya aplicado
                    CrearVista();
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando clientes: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() => EstaVacio = true);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Filtro rápido: sustituye la colección visible, no hace Refresh()
        // Esto evita el freeze de 2 segundos con muchos clientes
        // ══════════════════════════════════════════════════════════════════════
        private void AplicarFiltro()
        {
            // Usar Dispatcher para no bloquear el hilo de UI desde el setter
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                CrearVista();
            });
        }

        private void CrearVista()
        {
            // Filtrar en memoria sobre la colección ligera
            var filtrado = string.IsNullOrEmpty(_filtroNombre)
                ? _todoLosClientes
                : new ObservableCollection<ClienteViewModel>(
                    _todoLosClientes.Where(c =>
                        c.Nombre.IndexOf(_filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0));

            // Asignar una nueva vista — más rápido que Refresh() en colecciones grandes
            ListaClientesView = new ListCollectionView(filtrado);
            TotalClientes = filtrado.Count;
            EstaVacio = filtrado.Count == 0;
        }

        // ══════════════════════════════════════════════════════════════════════
        // Formulario nuevo cliente
        // ══════════════════════════════════════════════════════════════════════
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

        private void AbrirFormularioNuevoCliente()
        {
            try
            {
                InicializarClienteNuevo();
                var dialogo = new AgregarCliente(this);
                dialogo.ShowDialog();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al abrir formulario: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Guardar NUEVO cliente
        // ══════════════════════════════════════════════════════════════════════
        public async Task<bool> GuardarCliente()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ClienteNuevo.Nombre))
                {
                    MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio.");
                    return false;
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
                MensajeError.Mostrar("Error", $"Error al guardar: {ex.Message}");
                return false;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Editar cliente existente
        // ══════════════════════════════════════════════════════════════════════
        private void AbrirFormularioEdicion(ClienteViewModel? vm)
        {
            if (vm == null) return;

            try
            {
                InicializarClienteNuevo();
                var dialogo = new AgregarCliente(this)
                {
                    ClienteAEditar = vm
                };
                dialogo.ShowDialog();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al abrir edición: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Actualizar cliente existente
        // ══════════════════════════════════════════════════════════════════════
        public async Task<bool> ActualizarCliente()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ClienteNuevo.Nombre))
                {
                    MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio.");
                    return false;
                }

                var clienteTracked = await _clienteRepository.GetByIdAsync(ClienteNuevo.Id);
                if (clienteTracked == null)
                {
                    MensajeError.Mostrar("Error", "Cliente no encontrado.");
                    return false;
                }

                clienteTracked.Nombre = ClienteNuevo.Nombre;
                clienteTracked.Telefono = ClienteNuevo.Telefono;
                clienteTracked.Email = ClienteNuevo.Email;
                clienteTracked.Contacto = string.IsNullOrWhiteSpace(ClienteNuevo.Contacto)
                                              ? ClienteNuevo.Nombre
                                              : ClienteNuevo.Contacto;

                await UpdateAsync(_clienteRepository, clienteTracked);
                SnackbarMessageQueue.Enqueue($"Cliente {clienteTracked.Nombre} actualizado.");
                await CargarClientes();
                InicializarClienteNuevo();
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

                var dialogo = new DialogoEliminar(
                    $"¿Desactivar a {cliente.Nombre}? Dejará de aparecer en la lista.")
                {
                    Owner = Application.Current.MainWindow
                };

                if (dialogo.ShowDialog() == true)
                {
                    cliente.Activo = false;
                    if (await UpdateAsync(_clienteRepository, cliente))
                    {
                        SnackbarMessageQueue.Enqueue($"{cliente.Nombre} desactivado.");
                        await CargarClientes();
                    }
                }
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al desactivar: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // Historial real con ventana dedicada
        // ══════════════════════════════════════════════════════════════════════
        private async Task MostrarHistorial(ClienteViewModel? vm)
        {
            if (vm == null)
            {
                MensajeAdvertencia.Mostrar("Advertencia", "Selecciona un cliente.");
                return;
            }

            try
            {
                // Cargar reservas con navegación a Servicio y Empleado
                var reservas = await _reservaRepository
                    .Query(asNoTracking: true,
                           r => r.Servicio,
                           r => r.Empleado)
                    .Where(r => r.ClienteId == vm.Id)
                    .OrderByDescending(r => r.Fecha)
                    .ToListAsync();

                // Abrir ventana dedicada grande
                var ventana = new HistorialCliente(vm.Nombre, reservas)
                {
                    Owner = Application.Current.MainWindow
                };
                ventana.ShowDialog();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("Error", $"Error al cargar historial: {ex.Message}");
            }
        }
    }
}