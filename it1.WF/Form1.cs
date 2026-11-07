using System.Globalization;
using it1.DB;

namespace it1.WF
{
    public partial class Form1 : Form
    {
        private readonly ImportService _import = new(new JsonRentalReader());
        private readonly IRentalRepository _repo = new SqlServerRentalRepository();
        private string? _path;
        private CancellationTokenSource? _cts;

        public Form1()
        {
            InitializeComponent();
            btnSelect.Click += btnSelect_Click;
            btnImport.Click += btnImport_Click;
            btnCancel.Click += btnCancel_Click;
            btnShow.Click += btnShow_Click;
            dataGridView.CellFormatting += dataGridView_CellFormatting;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "JSON files|*.json" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            _path = dlg.FileName;
            lblFile.Text = Path.GetFileName(_path);
        }

        private async void btnImport_Click(object sender, EventArgs e)
        {
            var path = _path;
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Спочатку оберіть JSON-файл.");
                return;
            }

            using var cts = new CancellationTokenSource();
            _cts = cts;
            SetBusy(true, allowCancel: true);

            try
            {
                var progress = new Progress<int>(count => lblStatus.Text = $"Імпортовано хостів: {count}");
                var result = await _import.ImportAsync(path, progress, cts.Token);
                lblStatus.Text = $"Імпорт завершено: хостів {result.Hosts}, об'єктів {result.Properties}, " +
                                 $"бронювань {result.Bookings}, пропущено {result.Skipped}.";
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Імпорт скасовано.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка імпорту", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
                _cts = null;
            }
        }

        private async void btnShow_Click(object sender, EventArgs e)
        {
            var analyticsIndex = cmbAnalytics.SelectedIndex;
            var analyticsName = cmbAnalytics.SelectedItem?.ToString() ?? "Аналітика";
            SetBusy(true);

            try
            {
                var bookings = await _repo.GetAllBookingsAsync();
                if (bookings.Count == 0)
                {
                    ShowNoData("У базі немає бронювань для аналізу.", analyticsName);
                    return;
                }

                var rows = await Task.Run(() => CalculateAnalytics(analyticsIndex, bookings));
                var rowCount = GetRowCount(rows);
                if (rowCount == 0)
                {
                    ShowNoData("За вибраною аналітикою результатів не знайдено.", analyticsName);
                    return;
                }

                dataGridView.DataSource = rows;
                ConfigureColumns();
                lblStatus.Text = $"Показано рядків: {rowCount}. Аналітика: {analyticsName}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка аналітики", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e) => _cts?.Cancel();

        private void SetBusy(bool busy, bool allowCancel = false)
        {
            btnSelect.Enabled = !busy;
            btnImport.Enabled = !busy;
            btnShow.Enabled = !busy;
            cmbAnalytics.Enabled = !busy;
            btnCancel.Enabled = busy && allowCancel;
            progressBar.Visible = busy;
        }

        private void ShowNoData(string message, string analyticsName)
        {
            dataGridView.DataSource = null;
            lblStatus.Text = $"Показано рядків: 0. Аналітика: {analyticsName}.";
            MessageBox.Show(message, "Немає даних", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static object CalculateAnalytics(int analyticsIndex, List<Booking> bookings) => analyticsIndex switch
        {
            0 => RentalAnalytics.FindBookingAnomalies(bookings).ToList(),
            1 => RentalAnalytics.FindDoubleBookings(bookings).ToList(),
            2 => RentalAnalytics.GetRevenueByCity(bookings).ToList(),
            3 => RentalAnalytics.GetHostSummary(bookings).ToList(),
            _ => throw new InvalidOperationException("Оберіть тип аналітики.")
        };

        private static int GetRowCount(object rows) => rows switch
        {
            IReadOnlyCollection<BookingAnomaly> result => result.Count,
            IReadOnlyCollection<DoubleBooking> result => result.Count,
            IReadOnlyCollection<CityRevenueStatistics> result => result.Count,
            IReadOnlyCollection<HostSummary> result => result.Count,
            _ => 0
        };

        private void ConfigureColumns()
        {
            var headers = new Dictionary<string, string>
            {
                [nameof(BookingAnomaly.BookingId)] = "ID бронювання",
                [nameof(BookingAnomaly.PropertyId)] = "ID об'єкта",
                [nameof(BookingAnomaly.City)] = "Місто",
                [nameof(BookingAnomaly.ExpectedCost)] = "Очікувана вартість",
                [nameof(BookingAnomaly.ActualCost)] = "Фактична вартість",
                [nameof(BookingAnomaly.AnomalyType)] = "Тип аномалії",
                [nameof(DoubleBooking.FirstBookingId)] = "Перше бронювання",
                [nameof(DoubleBooking.SecondBookingId)] = "Друге бронювання",
                [nameof(DoubleBooking.OverlapStart)] = "Початок перетину",
                [nameof(DoubleBooking.OverlapEnd)] = "Кінець перетину",
                [nameof(CityRevenueStatistics.BookingCount)] = "Кількість бронювань",
                [nameof(CityRevenueStatistics.TotalRevenue)] = "Загальна виручка",
                [nameof(CityRevenueStatistics.AverageRevenue)] = "Середня виручка",
                [nameof(CityRevenueStatistics.AverageStayNights)] = "Середня тривалість (ночей)",
                [nameof(HostSummary.HostId)] = "ID хоста",
                [nameof(HostSummary.HostName)] = "Хост",
                [nameof(HostSummary.PropertyCount)] = "Кількість об'єктів",
                [nameof(HostSummary.MostProfitablePropertyId)] = "Найприбутковіший об'єкт",
                [nameof(HostSummary.MostProfitablePropertyRevenue)] = "Виручка об'єкта"
            };
            var currency = CultureInfo.GetCultureInfo("en-US");

            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                if (headers.TryGetValue(column.DataPropertyName, out var header))
                {
                    column.HeaderText = header;
                }

                if (column.DataPropertyName is nameof(BookingAnomaly.ExpectedCost)
                    or nameof(BookingAnomaly.ActualCost)
                    or nameof(CityRevenueStatistics.TotalRevenue)
                    or nameof(CityRevenueStatistics.AverageRevenue)
                    or nameof(HostSummary.TotalRevenue)
                    or nameof(HostSummary.MostProfitablePropertyRevenue))
                {
                    column.DefaultCellStyle.Format = "C2";
                    column.DefaultCellStyle.FormatProvider = currency;
                }
                else if (column.DataPropertyName is nameof(DoubleBooking.OverlapStart)
                    or nameof(DoubleBooking.OverlapEnd))
                {
                    column.DefaultCellStyle.Format = "dd.MM.yyyy";
                }
            }
        }

        private void dataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 ||
                dataGridView.Columns[e.ColumnIndex].DataPropertyName != nameof(BookingAnomaly.AnomalyType) ||
                e.Value is not BookingAnomalyType anomalyType)
            {
                return;
            }

            e.Value = anomalyType switch
            {
                BookingAnomalyType.CostMismatch => "Невідповідність вартості",
                BookingAnomalyType.InvalidDates => "Некоректні дати",
                _ => anomalyType.ToString()
            };
            e.FormattingApplied = true;
        }
    }
}