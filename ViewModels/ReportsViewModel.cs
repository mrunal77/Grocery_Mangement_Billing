using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GroceryStore.Models;
using GroceryStore.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace GroceryStore.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly DataService _dataService;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private decimal _totalSales;

    [ObservableProperty]
    private int _totalTransactions;

    [ObservableProperty]
    private decimal _averageTransaction;

    [ObservableProperty]
    private ObservableCollection<CategorySalesItem> _categorySales = new();

    [ObservableProperty]
    private ObservableCollection<TopProductItem> _topProducts = new();

    [ObservableProperty]
    private ObservableCollection<Transaction> _transactions = new();

    [ObservableProperty]
    private string _currencySymbol = "$";

    public ReportsViewModel(DataService dataService)
    {
        _dataService = dataService;
        Refresh();
    }

    public void Refresh()
    {
        CurrencySymbol = _dataService.Settings.CurrencySymbol;
        GenerateReport();
    }

    partial void OnStartDateChanged(DateTime value) => GenerateReport();
    partial void OnEndDateChanged(DateTime value) => GenerateReport();

    [RelayCommand]
    private void GenerateReport()
    {
        var transactions = _dataService.GetTransactionsByDateRange(StartDate, EndDate);

        TotalSales = transactions.Sum(t => t.TotalAmount);
        TotalTransactions = transactions.Count;
        AverageTransaction = TotalTransactions > 0 ? TotalSales / TotalTransactions : 0;

        var categorySalesData = _dataService.GetSalesByCategory(StartDate, EndDate);
        CategorySales.Clear();
        foreach (var kvp in categorySalesData)
        {
            var category = _dataService.Categories.FirstOrDefault(c => c.Id == kvp.Key);
            CategorySales.Add(new CategorySalesItem
            {
                CategoryName = category?.Name ?? "Unknown",
                Amount = kvp.Value
            });
        }

        var topProductsData = _dataService.GetTopSellingProducts(StartDate, EndDate, 10);
        TopProducts.Clear();
        foreach (var kvp in topProductsData)
        {
            TopProducts.Add(new TopProductItem
            {
                ProductName = kvp.Key,
                QuantitySold = (int)kvp.Value
            });
        }

        Transactions.Clear();
        foreach (var trans in transactions)
        {
            Transactions.Add(trans);
        }
    }

    [RelayCommand]
    private void ExportToCsv()
    {
        var sb = new StringBuilder();

        sb.AppendLine("Transaction ID,Date,Items,SubTotal,Tax,Total");

        foreach (var trans in Transactions)
        {
            var itemCount = trans.Items.Count;
            sb.AppendLine($"{trans.Id},{trans.Date:yyyy-MM-dd HH:mm},{itemCount},{trans.SubTotal:F2},{trans.TaxAmount:F2},{trans.TotalAmount:F2}");
        }

        var fileName = $"SalesReport_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.csv";
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
        File.WriteAllText(path, sb.ToString());
    }
}

public class CategorySalesItem
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class TopProductItem
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
}
