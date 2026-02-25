using CommunityToolkit.Mvvm.ComponentModel;
using GroceryStore.Models;
using GroceryStore.Services;
using System.Collections.ObjectModel;

namespace GroceryStore.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly DataService _dataService;

    [ObservableProperty]
    private int _totalProducts;

    [ObservableProperty]
    private int _totalCategories;

    [ObservableProperty]
    private decimal _todaySales;

    [ObservableProperty]
    private int _todayTransactions;

    [ObservableProperty]
    private ObservableCollection<Product> _lowStockProducts = new();

    [ObservableProperty]
    private ObservableCollection<Transaction> _recentTransactions = new();

    public DashboardViewModel(DataService dataService)
    {
        _dataService = dataService;
        Refresh();
    }

    public void Refresh()
    {
        TotalProducts = _dataService.Products.Count;
        TotalCategories = _dataService.Categories.Count;
        TodaySales = _dataService.GetTodaySales();
        TodayTransactions = _dataService.GetTodayTransactionCount();

        LowStockProducts.Clear();
        foreach (var product in _dataService.GetLowStockProducts())
        {
            LowStockProducts.Add(product);
        }

        RecentTransactions.Clear();
        foreach (var trans in _dataService.GetRecentTransactions(10))
        {
            RecentTransactions.Add(trans);
        }
    }
}
