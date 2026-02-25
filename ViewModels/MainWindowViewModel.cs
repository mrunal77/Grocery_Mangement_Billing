using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GroceryStore.Services;
using GroceryStore.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GroceryStore.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DataService _dataService;

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    [ObservableProperty]
    private string _currentViewTitle = "Dashboard";

    [ObservableProperty]
    private string _storeName = "My Grocery Store";

    [ObservableProperty]
    private string _currentDateTime = DateTime.Now.ToString("dddd, MMMM dd, yyyy HH:mm");

    public DashboardViewModel DashboardViewModel { get; }
    public ProductsViewModel ProductsViewModel { get; }
    public CategoriesViewModel CategoriesViewModel { get; }
    public BillingViewModel BillingViewModel { get; }
    public ReportsViewModel ReportsViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public MainWindowViewModel()
    {
        _dataService = new DataService();
        _dataService.DataChanged += OnDataChanged;

        DashboardViewModel = new DashboardViewModel(_dataService);
        ProductsViewModel = new ProductsViewModel(_dataService);
        CategoriesViewModel = new CategoriesViewModel(_dataService);
        BillingViewModel = new BillingViewModel(_dataService);
        ReportsViewModel = new ReportsViewModel(_dataService);
        SettingsViewModel = new SettingsViewModel(_dataService);

        StoreName = _dataService.Settings.StoreName;
        CurrentViewModel = DashboardViewModel;

        var timer = new System.Timers.Timer(60000);
        timer.Elapsed += (s, e) =>
        {
            CurrentDateTime = DateTime.Now.ToString("dddd, MMMM dd, yyyy HH:mm");
        };
        timer.Start();
    }

    private void OnDataChanged()
    {
        StoreName = _dataService.Settings.StoreName;
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentViewModel = DashboardViewModel;
        CurrentViewTitle = "Dashboard";
        DashboardViewModel.Refresh();
    }

    [RelayCommand]
    private void NavigateToProducts()
    {
        CurrentViewModel = ProductsViewModel;
        CurrentViewTitle = "Products";
        ProductsViewModel.Refresh();
    }

    [RelayCommand]
    private void NavigateToCategories()
    {
        CurrentViewModel = CategoriesViewModel;
        CurrentViewTitle = "Categories";
        CategoriesViewModel.Refresh();
    }

    [RelayCommand]
    private void NavigateToBilling()
    {
        CurrentViewModel = BillingViewModel;
        CurrentViewTitle = "Billing";
        BillingViewModel.Refresh();
    }

    [RelayCommand]
    private void NavigateToReports()
    {
        CurrentViewModel = ReportsViewModel;
        CurrentViewTitle = "Reports";
        ReportsViewModel.Refresh();
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentViewModel = SettingsViewModel;
        CurrentViewTitle = "Settings";
        SettingsViewModel.Refresh();
    }
}
