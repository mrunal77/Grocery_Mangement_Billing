using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GroceryStore.Models;
using GroceryStore.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace GroceryStore.ViewModels;

public partial class ProductsViewModel : ViewModelBase
{
    private readonly DataService _dataService;

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private bool _isDialogOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private Category? _editCategory;

    [ObservableProperty]
    private decimal _editPrice;

    [ObservableProperty]
    private int _editQuantity;

    [ObservableProperty]
    private string _editUnit = "piece";

    [ObservableProperty]
    private string _editBarcode = string.Empty;

    [ObservableProperty]
    private string _editDescription = string.Empty;

    [ObservableProperty]
    private string _dialogError = string.Empty;

    public string[] Units { get; } = { "kg", "g", "L", "mL", "piece", "dozen", "pack" };

    public ProductsViewModel(DataService dataService)
    {
        _dataService = dataService;
        Refresh();
    }

    public void Refresh()
    {
        Categories.Clear();
        foreach (var category in _dataService.Categories)
        {
            Categories.Add(category);
        }

        FilterProducts();
    }

    private void FilterProducts()
    {
        Products.Clear();
        var filtered = _dataService.Products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Barcode.ToLower().Contains(search));
        }

        if (SelectedCategory != null)
        {
            filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);
        }

        foreach (var product in filtered)
        {
            Products.Add(product);
        }
    }

    partial void OnSearchTextChanged(string value) => FilterProducts();
    partial void OnSelectedCategoryChanged(Category? value) => FilterProducts();

    [RelayCommand]
    private void OpenAddDialog()
    {
        IsEditing = false;
        EditName = string.Empty;
        EditCategory = Categories.FirstOrDefault();
        EditPrice = 0;
        EditQuantity = 0;
        EditUnit = "piece";
        EditBarcode = string.Empty;
        EditDescription = string.Empty;
        DialogError = string.Empty;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void OpenEditDialog()
    {
        if (SelectedProduct == null) return;

        IsEditing = true;
        EditName = SelectedProduct.Name;
        EditCategory = Categories.FirstOrDefault(c => c.Id == SelectedProduct.CategoryId);
        EditPrice = SelectedProduct.Price;
        EditQuantity = SelectedProduct.Quantity;
        EditUnit = SelectedProduct.Unit;
        EditBarcode = SelectedProduct.Barcode;
        EditDescription = SelectedProduct.Description;
        DialogError = string.Empty;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void SaveProduct()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            DialogError = "Product name is required";
            return;
        }

        if (EditCategory == null)
        {
            DialogError = "Please select a category";
            return;
        }

        if (EditPrice < 0)
        {
            DialogError = "Price cannot be negative";
            return;
        }

        if (EditQuantity < 0)
        {
            DialogError = "Quantity cannot be negative";
            return;
        }

        if (IsEditing && SelectedProduct != null)
        {
            SelectedProduct.Name = EditName;
            SelectedProduct.CategoryId = EditCategory.Id;
            SelectedProduct.Price = EditPrice;
            SelectedProduct.Quantity = EditQuantity;
            SelectedProduct.Unit = EditUnit;
            SelectedProduct.Barcode = EditBarcode;
            SelectedProduct.Description = EditDescription;
            _dataService.UpdateProduct(SelectedProduct);
        }
        else
        {
            var product = new Product
            {
                Name = EditName,
                CategoryId = EditCategory.Id,
                Price = EditPrice,
                Quantity = EditQuantity,
                Unit = EditUnit,
                Barcode = EditBarcode,
                Description = EditDescription
            };
            _dataService.AddProduct(product);
        }

        IsDialogOpen = false;
        Refresh();
    }

    [RelayCommand]
    private void CancelDialog()
    {
        IsDialogOpen = false;
    }

    [RelayCommand]
    private void DeleteProduct()
    {
        if (SelectedProduct == null) return;

        _dataService.DeleteProduct(SelectedProduct.Id);
        Refresh();
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SearchText = string.Empty;
        SelectedCategory = null;
    }
}
