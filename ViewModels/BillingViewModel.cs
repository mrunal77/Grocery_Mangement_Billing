using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GroceryStore.Models;
using GroceryStore.Services;

namespace GroceryStore.ViewModels;

public partial class BillingViewModel : ViewModelBase
{
    private readonly DataService _dataService;

    [ObservableProperty]
    private ObservableCollection<Product> _availableProducts = new();

    [ObservableProperty]
    private ObservableCollection<CartItem> _cartItems = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private bool _showReceipt;

    [ObservableProperty]
    private Transaction? _lastTransaction;

    [ObservableProperty]
    private string _currencySymbol = "$";

    public BillingViewModel(DataService dataService)
    {
        _dataService = dataService;
        Refresh();
    }

    public void Refresh()
    {
        CurrencySymbol = _dataService.Settings.CurrencySymbol;
        LoadAvailableProducts();
        UpdateTotals();
    }

    private void LoadAvailableProducts()
    {
        AvailableProducts.Clear();
        var products = _dataService.Products.Where(p => p.Quantity > 0);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            products = products.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Barcode.ToLower().Contains(search));
        }

        foreach (var product in products)
        {
            AvailableProducts.Add(product);
        }
    }

    partial void OnSearchTextChanged(string value) => LoadAvailableProducts();

    [RelayCommand]
    private void AddToCart(Product product)
    {
        if (product == null || product.Quantity <= 0) return;

        var existingItem = CartItems.FirstOrDefault(c => c.ProductId == product.Id);
        if (existingItem != null)
        {
            if (existingItem.Quantity < product.Quantity)
            {
                existingItem.Quantity++;
                existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
            }
        }
        else
        {
            CartItems.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = 1,
                TotalPrice = product.Price,
                AvailableQuantity = product.Quantity
            });
        }

        UpdateTotals();
    }

    [RelayCommand]
    private void IncreaseQuantity(CartItem item)
    {
        if (item.Quantity < item.AvailableQuantity)
        {
            item.Quantity++;
            item.TotalPrice = item.Quantity * item.UnitPrice;
            UpdateTotals();
        }
    }

    [RelayCommand]
    private void DecreaseQuantity(CartItem item)
    {
        if (item.Quantity > 1)
        {
            item.Quantity--;
            item.TotalPrice = item.Quantity * item.UnitPrice;
        }
        else
        {
            CartItems.Remove(item);
        }
        UpdateTotals();
    }

    [RelayCommand]
    private void RemoveFromCart(CartItem item)
    {
        CartItems.Remove(item);
        UpdateTotals();
    }

    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        SubTotal = CartItems.Sum(c => c.TotalPrice);
        var taxRate = _dataService.Settings.TaxRate / 100;
        TaxAmount = SubTotal * taxRate;
        GrandTotal = SubTotal + TaxAmount;
    }

    [RelayCommand]
    private void CompleteSale()
    {
        if (CartItems.Count == 0) return;

        var transaction = new Transaction
        {
            Date = DateTime.Now,
            Items = CartItems.Select(c => new TransactionItem
            {
                ProductId = c.ProductId,
                ProductName = c.ProductName,
                Quantity = c.Quantity,
                UnitPrice = c.UnitPrice,
                TotalPrice = c.TotalPrice
            }).ToList(),
            SubTotal = SubTotal,
            TaxAmount = TaxAmount,
            TotalAmount = GrandTotal
        };

        _dataService.AddTransaction(transaction);

        LastTransaction = transaction;
        ShowReceipt = true;
    }

    [RelayCommand]
    private void CloseReceipt()
    {
        ShowReceipt = false;
        CartItems.Clear();
        UpdateTotals();
    }
}

public partial class CartItem : ObservableObject
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }

    [ObservableProperty]
    private int _quantity;

    [ObservableProperty]
    private decimal _totalPrice;

    public int AvailableQuantity { get; set; }
}
