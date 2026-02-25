using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GroceryStore.Services;

namespace GroceryStore.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly DataService _dataService;

    [ObservableProperty]
    private string _storeName = string.Empty;

    [ObservableProperty]
    private decimal _taxRate;

    [ObservableProperty]
    private string _currencySymbol = string.Empty;

    [ObservableProperty]
    private int _lowStockThreshold;

    [ObservableProperty]
    private string _saveMessage = string.Empty;

    public SettingsViewModel(DataService dataService)
    {
        _dataService = dataService;
        Refresh();
    }

    public void Refresh()
    {
        StoreName = _dataService.Settings.StoreName;
        TaxRate = _dataService.Settings.TaxRate;
        CurrencySymbol = _dataService.Settings.CurrencySymbol;
        LowStockThreshold = _dataService.Settings.LowStockThreshold;
    }

    [RelayCommand]
    private void SaveSettings()
    {
        _dataService.Settings.StoreName = StoreName;
        _dataService.Settings.TaxRate = TaxRate;
        _dataService.Settings.CurrencySymbol = CurrencySymbol;
        _dataService.Settings.LowStockThreshold = LowStockThreshold;

        _dataService.SaveSettings();
        SaveMessage = "Settings saved successfully!";

        _ = Task.Delay(3000).ContinueWith(_ =>
        {
            SaveMessage = string.Empty;
        });
    }
}
