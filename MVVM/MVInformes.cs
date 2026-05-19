using ProyectoRuben.Backen.Modelo;
using ProyectoRuben.Backend.Servicios;
using pruebaNavegacion.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProyectoRuben.MVVM
{
    // ── Modelos auxiliares ────────────────────────────────────────────────────

    public class RankingItem
    {
        public int Posicion { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Importe { get; set; }
        public string ImporteTexto => Importe.ToString("F2", new CultureInfo("es-ES")) + " €";
        public double Porcentaje { get; set; }
    }

    public class PuntoGrafica
    {
        public string Etiqueta { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public double Alto { get; set; }
        public string ValorTexto => Valor.ToString("F0", new CultureInfo("es-ES")) + " €";
    }

    public class FilaExportacion
    {
        public int Id { get; set; }
        public string Fecha { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    /// <summary>
    /// Fila del DataGrid de detalle de facturas en UCInformes.
    /// </summary>
    public class FacturaInformeVista
    {
        public int Id { get; set; }
        public string Fecha { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;

        public string SubtotalTexto => Subtotal.ToString("F2", new CultureInfo("es-ES")) + " €";
        public string TotalTexto => Total.ToString("F2", new CultureInfo("es-ES")) + " €";
        public string DescuentoTexto => Descuento > 0
            ? "-" + Descuento.ToString("F2", new CultureInfo("es-ES")) + " €"
            : "—";
    }

    // ─────────────────────────────────────────────────────────────────────────

    public class MVInformes : MVBase
    {
        private readonly IFacturaRepository _facturaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IReservaRepository _reservaRepository;
        private readonly IServicioRepository _servicioRepository;

        // ── Período ───────────────────────────────────────────────────────────
        public ObservableCollection<string> Periodos { get; } = new()
        {
            "Esta semana", "Este mes", "Este trimestre", "Este año"
        };

        private string _periodoSeleccionado = "Este mes";
        public string PeriodoSeleccionado
        {
            get => _periodoSeleccionado;
            set { if (SetProperty(ref _periodoSeleccionado, value)) _ = CargarDatos(); }
        }

        // ── KPIs ──────────────────────────────────────────────────────────────
        private decimal _ingresosTotales;
        public decimal IngresosTotales
        {
            get => _ingresosTotales;
            set { SetProperty(ref _ingresosTotales, value); OnPropertyChanged(nameof(IngresosTotalesTexto)); }
        }
        public string IngresosTotalesTexto => IngresosTotales.ToString("N2", new CultureInfo("es-ES")) + " €";

        private int _totalFacturas;
        public int TotalFacturas { get => _totalFacturas; set => SetProperty(ref _totalFacturas, value); }

        private decimal _ticketMedio;
        public decimal TicketMedio
        {
            get => _ticketMedio;
            set { SetProperty(ref _ticketMedio, value); OnPropertyChanged(nameof(TicketMedioTexto)); }
        }
        public string TicketMedioTexto => TicketMedio.ToString("F2", new CultureInfo("es-ES")) + " €";

        private int _clientesAtendidos;
        public int ClientesAtendidos { get => _clientesAtendidos; set => SetProperty(ref _clientesAtendidos, value); }

        private string _metodoPagoTop = "-";
        public string MetodoPagoTop { get => _metodoPagoTop; set => SetProperty(ref _metodoPagoTop, value); }

        private decimal _variacionIngresos;
        public decimal VariacionIngresos
        {
            get => _variacionIngresos;
            set
            {
                SetProperty(ref _variacionIngresos, value);
                OnPropertyChanged(nameof(VariacionTexto));
                OnPropertyChanged(nameof(VariacionPositiva));
            }
        }
        public string VariacionTexto => (VariacionIngresos >= 0 ? "+" : "") + VariacionIngresos.ToString("F1", new CultureInfo("es-ES")) + "% vs período anterior";
        public bool VariacionPositiva => VariacionIngresos >= 0;

        // ── Rankings ──────────────────────────────────────────────────────────
        private ObservableCollection<RankingItem> _serviciosTop = new();
        public ObservableCollection<RankingItem> ServiciosTop
        {
            get => _serviciosTop;
            set => SetProperty(ref _serviciosTop, value);
        }

        private ObservableCollection<RankingItem> _clientesTop = new();
        public ObservableCollection<RankingItem> ClientesTop
        {
            get => _clientesTop;
            set => SetProperty(ref _clientesTop, value);
        }

        // ── Desglose método de pago ───────────────────────────────────────────
        private ObservableCollection<RankingItem> _desglosePago = new();
        public ObservableCollection<RankingItem> DesglosePago
        {
            get => _desglosePago;
            set => SetProperty(ref _desglosePago, value);
        }

        // ── Gráfica ───────────────────────────────────────────────────────────
        private ObservableCollection<PuntoGrafica> _puntosGrafica = new();
        public ObservableCollection<PuntoGrafica> PuntosGrafica
        {
            get => _puntosGrafica;
            set => SetProperty(ref _puntosGrafica, value);
        }

        private string _etiquetaGrafica = "Ingresos por semana";
        public string EtiquetaGrafica { get => _etiquetaGrafica; set => SetProperty(ref _etiquetaGrafica, value); }

        // ── TABLA DETALLE FACTURAS ────────────────────────────────────────────
        private System.Collections.Generic.List<FacturaInformeVista> _todasLasFacturas = new();

        private ObservableCollection<FacturaInformeVista> _facturasFiltradas = new();
        public ObservableCollection<FacturaInformeVista> FacturasFiltradas
        {
            get => _facturasFiltradas;
            set => SetProperty(ref _facturasFiltradas, value);
        }

        private string _filtroBusqueda = string.Empty;
        public string FiltroBusqueda
        {
            get => _filtroBusqueda;
            set { if (SetProperty(ref _filtroBusqueda, value)) AplicarFiltroFacturas(); }
        }

        private void AplicarFiltroFacturas()
        {
            var filtro = _filtroBusqueda.Trim();
            var resultado = string.IsNullOrEmpty(filtro)
                ? _todasLasFacturas
                : _todasLasFacturas.Where(f =>
                    f.Cliente.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                    f.MetodoPago.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                    f.Estado.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                    f.Id.ToString().Contains(filtro)).ToList();

            FacturasFiltradas = new ObservableCollection<FacturaInformeVista>(resultado);
        }

        // ── Estado ────────────────────────────────────────────────────────────
        private bool _cargando;
        public bool Cargando { get => _cargando; set => SetProperty(ref _cargando, value); }

        private bool _sinDatos;
        public bool SinDatos { get => _sinDatos; set => SetProperty(ref _sinDatos, value); }

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand RefrescarCommand { get; }
        public ICommand ExportarExcelCommand { get; }
        public ICommand ExportarCsvCommand { get; }

        // ─────────────────────────────────────────────────────────────────────
        public MVInformes(IFacturaRepository facturaRepository,
                          IClienteRepository clienteRepository,
                          IReservaRepository reservaRepository,
                          IServicioRepository servicioRepository)
        {
            _facturaRepository = facturaRepository;
            _clienteRepository = clienteRepository;
            _reservaRepository = reservaRepository;
            _servicioRepository = servicioRepository;

            RefrescarCommand = new RelayCommand(async _ => await CargarDatos());
            ExportarExcelCommand = new RelayCommand(async _ => await ExportarExcel());
            ExportarCsvCommand = new RelayCommand(async _ => await ExportarCsv());

            _ = CargarDatos();
        }

        // ── Rango de fechas ───────────────────────────────────────────────────
        private (DateTime desde, DateTime hasta) ObtenerRango()
        {
            var hoy = DateTime.Today;
            int diasDesdelunes = ((int)hoy.DayOfWeek + 6) % 7;
            return PeriodoSeleccionado switch
            {
                "Esta semana" => (hoy.AddDays(-diasDesdelunes), hoy),
                "Este mes" => (new DateTime(hoy.Year, hoy.Month, 1), hoy),
                "Este trimestre" => (new DateTime(hoy.Year, ((hoy.Month - 1) / 3) * 3 + 1, 1), hoy),
                "Este año" => (new DateTime(hoy.Year, 1, 1), hoy),
                _ => (new DateTime(hoy.Year, hoy.Month, 1), hoy)
            };
        }

        private (DateTime desde, DateTime hasta) ObtenerRangoAnterior()
        {
            var (d, h) = ObtenerRango();
            var duracion = h - d;
            return (d - duracion - TimeSpan.FromDays(1), d - TimeSpan.FromDays(1));
        }

        // ── Carga principal ───────────────────────────────────────────────────
        public async Task CargarDatos()
        {
            try
            {
                Cargando = true;
                SinDatos = false;

                var (desde, hasta) = ObtenerRango();
                var (desdeAnt, hastaAnt) = ObtenerRangoAnterior();

                var todasFacturas = await GetAllAsync(_facturaRepository);
                var todosClientes = await GetAllAsync(_clienteRepository);

                var facturasPeriodo = todasFacturas
                    .Where(f => f.Fecha.Date >= desde && f.Fecha.Date <= hasta
                             && (f.Estado == "Pagada" || string.IsNullOrEmpty(f.Estado)))
                    .ToList();

                var facturasAnteriores = todasFacturas
                    .Where(f => f.Fecha.Date >= desdeAnt && f.Fecha.Date <= hastaAnt
                             && (f.Estado == "Pagada" || string.IsNullOrEmpty(f.Estado)))
                    .ToList();

                if (!facturasPeriodo.Any())
                {
                    SinDatos = true;
                    Cargando = false;
                    return;
                }

                // ── KPIs ──────────────────────────────────────────────────────
                var ingresosActual = facturasPeriodo.Sum(f => f.Total);
                var ingresosAnterior = facturasAnteriores.Sum(f => f.Total);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IngresosTotales = ingresosActual;
                    TotalFacturas = facturasPeriodo.Count;
                    TicketMedio = facturasPeriodo.Count > 0 ? ingresosActual / facturasPeriodo.Count : 0;
                    ClientesAtendidos = facturasPeriodo.Select(f => f.ClienteId).Distinct().Count();
                    MetodoPagoTop = facturasPeriodo
                        .GroupBy(f => f.MetodoPago)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? "-";
                    VariacionIngresos = ingresosAnterior > 0
                        ? Math.Round((ingresosActual - ingresosAnterior) / ingresosAnterior * 100, 1)
                        : 0;
                    OnPropertyChanged(nameof(IngresosTotalesTexto));
                    OnPropertyChanged(nameof(TicketMedioTexto));
                });

                // ── Desglose método de pago ───────────────────────────────────
                var totalGeneral = ingresosActual == 0 ? 1 : ingresosActual;
                var desglose = facturasPeriodo
                    .GroupBy(f => f.MetodoPago)
                    .Select((g, i) => new RankingItem
                    {
                        Posicion = i + 1,
                        Nombre = g.Key,
                        Cantidad = g.Count(),
                        Importe = g.Sum(f => f.Total),
                        Porcentaje = (double)(g.Sum(f => f.Total) / totalGeneral * 100)
                    })
                    .OrderByDescending(x => x.Importe)
                    .ToList();

                // ── Top servicios ─────────────────────────────────────────────
                // Cruza facturas → reserva vinculada → ServicioId
                // así no dependemos de r.Estado == "Completada"
                var todasReservas = await GetAllAsync(_reservaRepository);
                var todosServicios = await GetAllAsync(_servicioRepository);

                var idsReservasFacturadas = facturasPeriodo
                    .Where(f => f.ReservaId.HasValue)
                    .Select(f => f.ReservaId!.Value)
                    .ToHashSet();

                var reservasDelPeriodo = todasReservas
                    .Where(r => r.Fecha.Date >= desde && r.Fecha.Date <= hasta
                             && idsReservasFacturadas.Contains(r.Id))
                    .ToList();

                // Fallback: si las facturas no tienen ReservaId,
                // usamos todas las reservas del período no canceladas
                if (!reservasDelPeriodo.Any())
                {
                    reservasDelPeriodo = todasReservas
                        .Where(r => r.Fecha.Date >= desde && r.Fecha.Date <= hasta
                                 && r.Estado != "Cancelada")
                        .ToList();
                }

                var maxServicios = reservasDelPeriodo.Any()
                    ? reservasDelPeriodo.GroupBy(r => r.ServicioId).Max(g => g.Count()) : 1;

                var topServicios = reservasDelPeriodo
                    .GroupBy(r => r.ServicioId)
                    .Select(g =>
                    {
                        var srv = todosServicios.FirstOrDefault(s => s.Id == g.Key);
                        return new RankingItem
                        {
                            Posicion = 0,
                            Nombre = srv?.Nombre ?? "Desconocido",
                            Cantidad = g.Count(),
                            Importe = srv != null ? srv.Costo * g.Count() : 0,
                            Porcentaje = maxServicios > 0 ? (double)g.Count() / maxServicios * 100 : 0
                        };
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .Take(5)
                    .ToList();

                for (int i = 0; i < topServicios.Count; i++) topServicios[i].Posicion = i + 1;

                // ── Top clientes ──────────────────────────────────────────────
                var maxCliente = facturasPeriodo.Any()
                    ? facturasPeriodo.GroupBy(f => f.ClienteId).Max(g => g.Sum(f => f.Total)) : 1;

                var topClientes = facturasPeriodo
                    .GroupBy(f => f.ClienteId)
                    .Select(g =>
                    {
                        var cli = todosClientes.FirstOrDefault(c => c.Id == g.Key);
                        var totalCli = g.Sum(f => f.Total);
                        return new RankingItem
                        {
                            Posicion = 0,
                            Nombre = cli?.Nombre ?? "Desconocido",
                            Cantidad = g.Count(),
                            Importe = totalCli,
                            Porcentaje = maxCliente > 0 ? (double)(totalCli / maxCliente * 100) : 0
                        };
                    })
                    .OrderByDescending(x => x.Importe)
                    .Take(5)
                    .ToList();

                for (int i = 0; i < topClientes.Count; i++) topClientes[i].Posicion = i + 1;

                // ── Gráfica ───────────────────────────────────────────────────
                var puntos = GenerarPuntosGrafica(facturasPeriodo, desde, hasta);

                // ── Tabla detalle facturas ────────────────────────────────────
                var filasDetalle = facturasPeriodo
                    .OrderByDescending(f => f.Fecha)
                    .Select(f => new FacturaInformeVista
                    {
                        Id = f.Id,
                        Fecha = f.Fecha.ToString("dd/MM/yyyy"),
                        Cliente = todosClientes.FirstOrDefault(c => c.Id == f.ClienteId)?.Nombre ?? "Desconocido",
                        MetodoPago = f.MetodoPago,
                        Subtotal = f.Subtotal ?? f.Total,
                        Descuento = f.Descuento ?? 0,
                        Total = f.Total,
                        Estado = f.Estado ?? "Pagada"
                    })
                    .ToList();

                // ── Actualizar UI ─────────────────────────────────────────────
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ServiciosTop = new ObservableCollection<RankingItem>(topServicios);
                    ClientesTop = new ObservableCollection<RankingItem>(topClientes);
                    DesglosePago = new ObservableCollection<RankingItem>(desglose);
                    PuntosGrafica = new ObservableCollection<PuntoGrafica>(puntos);
                    EtiquetaGrafica = PeriodoSeleccionado is "Este mes" or "Este trimestre"
                        ? "Ingresos por semana" : "Ingresos por día";

                    _todasLasFacturas = filasDetalle;
                    FiltroBusqueda = string.Empty;
                    FacturasFiltradas = new ObservableCollection<FacturaInformeVista>(filasDetalle);
                });
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error cargando informes: {ex.Message}");
            }
            finally
            {
                Cargando = false;
            }
        }

        // ── Generador de gráfica ──────────────────────────────────────────────
        private System.Collections.Generic.List<PuntoGrafica> GenerarPuntosGrafica(
            System.Collections.Generic.List<Factura> facturas, DateTime desde, DateTime hasta)
        {
            var cultura = new CultureInfo("es-ES");
            System.Collections.Generic.List<PuntoGrafica> puntos;

            if (PeriodoSeleccionado == "Esta semana")
            {
                puntos = Enumerable.Range(0, 7).Select(i =>
                {
                    var dia = desde.AddDays(i);
                    var total = facturas.Where(f => f.Fecha.Date == dia).Sum(f => f.Total);
                    return new PuntoGrafica { Etiqueta = dia.ToString("ddd", cultura), Valor = total };
                }).ToList();
            }
            else if (PeriodoSeleccionado == "Este mes")
            {
                puntos = new();
                var semana = 1;
                var inicio = desde;
                while (inicio <= hasta)
                {
                    var fin = inicio.AddDays(6) > hasta ? hasta : inicio.AddDays(6);
                    var total = facturas.Where(f => f.Fecha.Date >= inicio && f.Fecha.Date <= fin).Sum(f => f.Total);
                    puntos.Add(new PuntoGrafica { Etiqueta = $"Sem {semana}", Valor = total });
                    inicio = fin.AddDays(1);
                    semana++;
                }
            }
            else if (PeriodoSeleccionado == "Este trimestre")
            {
                puntos = Enumerable.Range(0, 3).Select(i =>
                {
                    var mes = desde.AddMonths(i);
                    var total = facturas.Where(f => f.Fecha.Month == mes.Month && f.Fecha.Year == mes.Year).Sum(f => f.Total);
                    return new PuntoGrafica { Etiqueta = mes.ToString("MMM", cultura), Valor = total };
                }).ToList();
            }
            else
            {
                puntos = Enumerable.Range(1, 12).Select(m =>
                {
                    var total = facturas.Where(f => f.Fecha.Month == m && f.Fecha.Year == desde.Year).Sum(f => f.Total);
                    return new PuntoGrafica { Etiqueta = new DateTime(desde.Year, m, 1).ToString("MMM", cultura), Valor = total };
                }).ToList();
            }

            var maxVal = puntos.Max(p => p.Valor);
            if (maxVal > 0)
                foreach (var p in puntos)
                    p.Alto = (double)(p.Valor / maxVal) * 200;

            return puntos;
        }

        // ── Exportar CSV ──────────────────────────────────────────────────────
        private async Task ExportarExcel()
        {
            try
            {
                var (desde, hasta) = ObtenerRango();
                var facturas = await GetAllAsync(_facturaRepository);
                var clientes = await GetAllAsync(_clienteRepository);

                var filas = facturas
                    .Where(f => f.Fecha.Date >= desde && f.Fecha.Date <= hasta)
                    .OrderBy(f => f.Fecha)
                    .Select(f => new FilaExportacion
                    {
                        Id = f.Id,
                        Fecha = f.Fecha.ToString("dd/MM/yyyy"),
                        Cliente = clientes.FirstOrDefault(c => c.Id == f.ClienteId)?.Nombre ?? "Desconocido",
                        MetodoPago = f.MetodoPago,
                        Subtotal = f.Subtotal ?? f.Total,
                        Descuento = f.Descuento ?? 0,
                        Total = f.Total,
                        Estado = f.Estado ?? "Pagada"
                    })
                    .ToList();

                var nombreArchivo = $"Informe_{PeriodoSeleccionado.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.csv";
                var ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), nombreArchivo);

                var cultura = new CultureInfo("es-ES");
                var lineas = new System.Collections.Generic.List<string>
                {
                    $"INFORME DE FACTURACIÓN — Peluquería Charo",
                    $"Período:;{PeriodoSeleccionado} ({desde:dd/MM/yyyy} - {hasta:dd/MM/yyyy})",
                    $"Generado:;{DateTime.Now:dd/MM/yyyy HH:mm}",
                    "",
                    "RESUMEN",
                    $"Ingresos totales;{IngresosTotales.ToString("F2", cultura)} €",
                    $"Nº facturas;{TotalFacturas}",
                    $"Ticket medio;{TicketMedio.ToString("F2", cultura)} €",
                    $"Clientes atendidos;{ClientesAtendidos}",
                    $"Método de pago más usado;{MetodoPagoTop}",
                    "",
                    "DETALLE DE FACTURAS",
                    "ID;Fecha;Cliente;Método pago;Subtotal;Descuento;Total;Estado"
                };

                lineas.AddRange(filas.Select(f =>
                    $"{f.Id};{f.Fecha};{f.Cliente};{f.MetodoPago};" +
                    $"{f.Subtotal.ToString("F2", cultura)};{f.Descuento.ToString("F2", cultura)};" +
                    $"{f.Total.ToString("F2", cultura)};{f.Estado}"));

                lineas.Add("");
                lineas.Add($";;TOTALES;;{filas.Sum(f => f.Subtotal).ToString("F2", cultura)};" +
                           $"{filas.Sum(f => f.Descuento).ToString("F2", cultura)};" +
                           $"{filas.Sum(f => f.Total).ToString("F2", cultura)};");

                await File.WriteAllLinesAsync(ruta, lineas, System.Text.Encoding.UTF8);
                SnackbarMessageQueue.Enqueue($"✓ CSV exportado en el escritorio: {nombreArchivo}");
            }
            catch (Exception ex) { SnackbarMessageQueue.Enqueue($"Error al exportar: {ex.Message}"); }
        }

        private async Task ExportarCsv() => await ExportarExcel();
    }
}