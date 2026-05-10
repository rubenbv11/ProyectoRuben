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
    public class ClienteViewModel
    {
        private readonly Cliente _cliente;
        public ClienteViewModel(Cliente cliente, bool esVip = false)
        { _cliente = cliente; EsVip = esVip; }

        public int Id => _cliente.Id;
        public string Nombre => _cliente.Nombre;
        public string? Telefono => _cliente.Telefono;
        public string? Email => _cliente.Email;
        public string? HistorialCitas => _cliente.HistorialCitas;
        public bool? Activo => _cliente.Activo;
        public bool EsVip { get; }
        public Cliente Modelo => _cliente;
    }

    public class MVClientes : MVBase
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IReservaRepository _reservaRepository;

        private ObservableCollection<ClienteViewModel> _todosActivos = new();
        private ObservableCollection<ClienteViewModel> _todosArchivados = new();

        private ListCollectionView _listaClientesView;
        public ListCollectionView ListaClientesView
        {
            get => _listaClientesView;
            set => SetProperty(ref _listaClientesView, value);
        }

        // ── Toggle ────────────────────────────────────────────────────────────
        private bool _mostrandoArchivados;
        public bool MostrandoArchivados
        {
            get => _mostrandoArchivados;
            set { if (SetProperty(ref _mostrandoArchivados, value)) { OnPropertyChanged(nameof(MostrandoActivos)); CrearVista(); } }
        }
        public bool MostrandoActivos => !_mostrandoArchivados;

        private bool _estaVacio;
        public bool EstaVacio { get => _estaVacio; set => SetProperty(ref _estaVacio, value); }

        private int _totalClientes;
        public int TotalClientes { get => _totalClientes; set => SetProperty(ref _totalClientes, value); }

        private string _filtroNombre = string.Empty;
        public string FiltroNombre
        {
            get => _filtroNombre;
            set { if (SetProperty(ref _filtroNombre, value)) AplicarFiltro(); }
        }

        private Cliente _clienteNuevo = new();
        public Cliente ClienteNuevo { get => _clienteNuevo; set => SetProperty(ref _clienteNuevo, value); }

        public ICommand MostrarActivosCommand { get; }
        public ICommand MostrarArchivadosCommand { get; }
        public ICommand AgregarClienteCommand { get; }
        public ICommand EditarClienteCommand { get; }
        public ICommand DesactivarClienteCommand { get; }
        public ICommand ReactivarClienteCommand { get; }
        public ICommand VerHistorialCommand { get; }

        public MVClientes(IClienteRepository clienteRepository, IReservaRepository reservaRepository)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _reservaRepository = reservaRepository ?? throw new ArgumentNullException(nameof(reservaRepository));

            InicializarClienteNuevo();

            MostrarActivosCommand = new RelayCommand(_ => MostrandoArchivados = false);
            MostrarArchivadosCommand = new RelayCommand(_ => MostrandoArchivados = true);
            AgregarClienteCommand = new RelayCommand(_ => AbrirFormularioNuevoCliente());
            EditarClienteCommand = new RelayCommand(p => AbrirFormularioEdicion(p as ClienteViewModel));
            DesactivarClienteCommand = new RelayCommand(async p => { if (p is int id) await DesactivarCliente(id); });
            ReactivarClienteCommand = new RelayCommand(async p => { if (p is int id) await ReactivarCliente(id); });
            VerHistorialCommand = new RelayCommand(async p => await MostrarHistorial(p as ClienteViewModel));

            _ = CargarClientes();
        }

        public async Task CargarClientes()
        {
            try
            {
                var todos = await GetAllAsync(_clienteRepository);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _todosActivos = new ObservableCollection<ClienteViewModel>(
                        todos.Where(c => c.Activo == true).Select(c => new ClienteViewModel(c)));
                    _todosArchivados = new ObservableCollection<ClienteViewModel>(
                        todos.Where(c => c.Activo != true).Select(c => new ClienteViewModel(c)));
                    CrearVista();
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando clientes: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() => EstaVacio = true);
            }
        }

        private void AplicarFiltro() => Application.Current.Dispatcher.InvokeAsync(CrearVista);

        private void CrearVista()
        {
            var fuente = _mostrandoArchivados ? _todosArchivados : _todosActivos;
            var filtrado = string.IsNullOrEmpty(_filtroNombre)
                ? fuente
                : new ObservableCollection<ClienteViewModel>(fuente.Where(c =>
                    c.Nombre.IndexOf(_filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0));

            ListaClientesView = new ListCollectionView(filtrado);
            TotalClientes = filtrado.Count;
            EstaVacio = filtrado.Count == 0;
        }

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
            InicializarClienteNuevo();
            new AgregarCliente(this).ShowDialog();
        }

        private void AbrirFormularioEdicion(ClienteViewModel? vm)
        {
            if (vm == null) return;
            InicializarClienteNuevo();
            new AgregarCliente(this) { ClienteAEditar = vm }.ShowDialog();
        }

        public async Task<bool> GuardarCliente()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ClienteNuevo.Nombre))
                { MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio."); return false; }
                if (string.IsNullOrWhiteSpace(ClienteNuevo.Contacto))
                    ClienteNuevo.Contacto = ClienteNuevo.Nombre;
                await AddAsync(_clienteRepository, ClienteNuevo);
                SnackbarMessageQueue.Enqueue($"Cliente {ClienteNuevo.Nombre} guardado.");
                await CargarClientes(); InicializarClienteNuevo(); return true;
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); return false; }
        }

        public async Task<bool> ActualizarCliente()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ClienteNuevo.Nombre))
                { MensajeAdvertencia.Mostrar("Validación", "El nombre es obligatorio."); return false; }
                var t = await _clienteRepository.GetByIdAsync(ClienteNuevo.Id);
                if (t == null) { MensajeError.Mostrar("Error", "Cliente no encontrado."); return false; }
                t.Nombre = ClienteNuevo.Nombre; t.Telefono = ClienteNuevo.Telefono;
                t.Email = ClienteNuevo.Email;
                t.Contacto = string.IsNullOrWhiteSpace(ClienteNuevo.Contacto)
                                 ? ClienteNuevo.Nombre : ClienteNuevo.Contacto;
                await UpdateAsync(_clienteRepository, t);
                SnackbarMessageQueue.Enqueue($"Cliente {t.Nombre} actualizado.");
                await CargarClientes(); InicializarClienteNuevo(); return true;
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); return false; }
        }

        private async Task DesactivarCliente(int id)
        {
            try
            {
                var c = await GetByIdAsync(_clienteRepository, id);
                if (c == null) return;
                var d = new DialogoEliminar($"¿Archivar a {c.Nombre}? Podrás reactivarlo desde Archivados.")
                { Owner = Application.Current.MainWindow };
                if (d.ShowDialog() == true)
                {
                    c.Activo = false;
                    if (await UpdateAsync(_clienteRepository, c))
                    { SnackbarMessageQueue.Enqueue($"{c.Nombre} archivado."); await CargarClientes(); }
                }
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); }
        }

        private async Task ReactivarCliente(int id)
        {
            try
            {
                var c = await GetByIdAsync(_clienteRepository, id);
                if (c == null) return;
                c.Activo = true;
                if (await UpdateAsync(_clienteRepository, c))
                { SnackbarMessageQueue.Enqueue($"{c.Nombre} reactivado."); await CargarClientes(); }
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); }
        }

        private async Task MostrarHistorial(ClienteViewModel? vm)
        {
            if (vm == null) { MensajeAdvertencia.Mostrar("Advertencia", "Selecciona un cliente."); return; }
            try
            {
                var reservas = await _reservaRepository
                    .Query(asNoTracking: true, r => r.Servicio, r => r.Empleado)
                    .Where(r => r.ClienteId == vm.Id)
                    .OrderByDescending(r => r.Fecha)
                    .ToListAsync();
                new HistorialCliente(vm.Nombre, reservas)
                { Owner = Application.Current.MainWindow }.ShowDialog();
            }
            catch (Exception ex) { MensajeError.Mostrar("Error", ex.Message); }
        }
    }
}