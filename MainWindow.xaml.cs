using System.Globalization;
using System.Windows;
using FlowerStore.Models;
using FlowerStore.Services;

namespace FlowerStore;

public partial class MainWindow : Window
{
    private readonly FlowerStoreService _service = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadReferenceData();
            LoadFlowers();
            LoadStatistics();
            SetStatus("Đã tải dữ liệu từ cơ sở dữ liệu.");
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void ShowFlowers_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(LoadFlowers);
    }

    private void ShowOrders_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(LoadOrders);
    }

    private void RefreshData_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            LoadReferenceData();
            LoadFlowers();
            LoadStatistics();
            SetStatus("Đã làm mới danh sách hoa, đơn hàng và thống kê.");
        });
    }

    private void AddFlower_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            var flower = new Flower
            {
                Id = ParseInt(FlowerIdTextBox.Text, "Mã hoa"),
                Name = FlowerNameTextBox.Text.Trim(),
                Price = ParseDouble(FlowerPriceTextBox.Text, "Giá bán"),
                Quantity = ParseInt(FlowerQuantityTextBox.Text, "Tồn kho")
            };

            _service.AddFlower(flower);
            ClearAddFlowerInputs();
            LoadReferenceData();
            LoadFlowers();
            SetStatus($"Đã thêm hoa {flower.Name}.");
        });
    }

    private void CreateOrder_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            var request = new CreateOrderRequest
            {
                CustomerId = GetSelectedId(CustomerComboBox.SelectedValue, "khách hàng"),
                FlowerId = GetSelectedId(OrderFlowerComboBox.SelectedValue, "hoa"),
                Quantity = ParseInt(OrderQuantityTextBox.Text, "Số lượng"),
                OrderDate = OrderDatePicker.SelectedDate ?? DateTime.Today
            };

            _service.CreateOrder(request);
            ClearCreateOrderInputs();
            LoadReferenceData();
            LoadOrders();
            LoadStatistics();
            SetStatus("Đã tạo đơn hàng mới.");
        });
    }

    private void UpdateOrderDate_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            var orderId = GetSelectedId(OrderComboBox.SelectedValue, "đơn hàng");
            var newDate = UpdateOrderDatePicker.SelectedDate
                ?? throw new InvalidOperationException("Bạn chưa chọn ngày mới cho đơn hàng.");

            _service.UpdateOrderDate(orderId, newDate);
            LoadReferenceData();
            LoadOrders();
            SetStatus($"Đã cập nhật ngày cho đơn hàng #{orderId}.");
        });
    }

    private void OrderComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (OrderComboBox.SelectedItem is OrderSummary order)
        {
            UpdateOrderDatePicker.SelectedDate = order.OrderDate;
        }
    }

    private void ShowStatistics_Click(object sender, RoutedEventArgs e)
    {
        ExecuteUiAction(() =>
        {
            LoadStatistics();
            ViewTitleTextBlock.Text = "Thống kê hoa";
            dataGrid.ItemsSource = BuildStatisticRows();
            SetStatus("Đã hiển thị thống kê hoa bán nhiều và doanh thu cao.");
        });
    }

    private void LoadReferenceData()
    {
        var customers = _service.GetCustomers();
        var flowers = _service.GetFlowers();
        var orders = _service.GetOrders();

        CustomerComboBox.ItemsSource = customers;
        OrderFlowerComboBox.ItemsSource = flowers;
        OrderComboBox.ItemsSource = orders;

        if (customers.Count > 0 && CustomerComboBox.SelectedIndex < 0)
        {
            CustomerComboBox.SelectedIndex = 0;
        }

        if (flowers.Count > 0 && OrderFlowerComboBox.SelectedIndex < 0)
        {
            OrderFlowerComboBox.SelectedIndex = 0;
        }

        if (orders.Count > 0)
        {
            OrderComboBox.SelectedIndex = 0;
        }

        OrderDatePicker.SelectedDate ??= DateTime.Today;
        UpdateOrderDatePicker.SelectedDate ??= DateTime.Today;
    }

    private void LoadFlowers()
    {
        ViewTitleTextBlock.Text = "Danh sách hoa";
        dataGrid.ItemsSource = _service.GetFlowers();
        SetStatus("Đang hiển thị danh sách hoa.");
    }

    private void LoadOrders()
    {
        ViewTitleTextBlock.Text = "Danh sách đơn hàng";
        dataGrid.ItemsSource = _service.GetOrders();
        SetStatus("Đang hiển thị danh sách đơn hàng.");
    }

    private void LoadStatistics()
    {
        var topSelling = _service.GetTopSellingFlower();
        var highestRevenue = _service.GetHighestRevenueFlower();

        TopSellingFlowerTextBlock.Text = FormatStatistic(topSelling);
        HighestRevenueFlowerTextBlock.Text = FormatStatistic(highestRevenue);
    }

    private static string FormatStatistic(FlowerStatistic? statistic)
    {
        if (statistic is null)
        {
            return "Chưa có dữ liệu bán hàng.";
        }

        return string.Format(
            CultureInfo.InvariantCulture,
            "#{0} - {1} | Đã bán: {2} | Doanh thu: {3:N0}",
            statistic.FlowerId,
            statistic.FlowerName,
            statistic.TotalSold,
            statistic.TotalRevenue);
    }

    private List<object> BuildStatisticRows()
    {
        var topSelling = _service.GetTopSellingFlower();
        var highestRevenue = _service.GetHighestRevenueFlower();

        return
        [
            new
            {
                LoaiThongKe = "Hoa bán nhiều nhất",
                MaHoa = topSelling?.FlowerId ?? 0,
                TenHoa = topSelling?.FlowerName ?? "Không có",
                TongSoLuong = topSelling?.TotalSold ?? 0,
                TongDoanhThu = topSelling?.TotalRevenue ?? 0
            },
            new
            {
                LoaiThongKe = "Hoa kiếm nhiều tiền nhất",
                MaHoa = highestRevenue?.FlowerId ?? 0,
                TenHoa = highestRevenue?.FlowerName ?? "Không có",
                TongSoLuong = highestRevenue?.TotalSold ?? 0,
                TongDoanhThu = highestRevenue?.TotalRevenue ?? 0
            }
        ];
    }

    private void ExecuteUiAction(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private static int ParseInt(string? input, string fieldName)
    {
        if (int.TryParse(input, out var value))
        {
            return value;
        }

        throw new InvalidOperationException($"{fieldName} phải là số nguyên hợp lệ.");
    }

    private static double ParseDouble(string? input, string fieldName)
    {
        if (double.TryParse(input, out var value))
        {
            return value;
        }

        if (double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value))
        {
            return value;
        }

        throw new InvalidOperationException($"{fieldName} phải là số hợp lệ.");
    }

    private static int GetSelectedId(object? selectedValue, string fieldName)
    {
        if (selectedValue is int id && id > 0)
        {
            return id;
        }

        throw new InvalidOperationException($"Bạn chưa chọn {fieldName}.");
    }

    private void ClearAddFlowerInputs()
    {
        FlowerIdTextBox.Clear();
        FlowerNameTextBox.Clear();
        FlowerPriceTextBox.Clear();
        FlowerQuantityTextBox.Clear();
    }

    private void ClearCreateOrderInputs()
    {
        OrderQuantityTextBox.Clear();
        OrderDatePicker.SelectedDate = DateTime.Today;
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }

    private void ShowError(Exception ex)
    {
        SetStatus(ex.Message);
        MessageBox.Show(ex.Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
