using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GroceryStore.Models;
using GroceryStore.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace GroceryStore.ViewModels;

public partial class CategoriesViewModel : ViewModelBase
{
    private readonly DataService _dataService;

    [ObservableProperty]
    private ObservableCollection<CategoryItem> _categories = new();

    [ObservableProperty]
    private CategoryItem? _selectedCategory;

    [ObservableProperty]
    private bool _isDialogOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _dialogError = string.Empty;

    [ObservableProperty]
    private string _deleteError = string.Empty;

    public CategoriesViewModel(DataService dataService)
    {
        _dataService = dataService;
        Refresh();
    }

    public void Refresh()
    {
        Categories.Clear();
        foreach (var category in _dataService.Categories)
        {
            var productCount = _dataService.Products.Count(p => p.CategoryId == category.Id);
            Categories.Add(new CategoryItem
            {
                Id = category.Id,
                Name = category.Name,
                ProductCount = productCount
            });
        }
    }

    [RelayCommand]
    private void OpenAddDialog()
    {
        IsEditing = false;
        EditName = string.Empty;
        DialogError = string.Empty;
        DeleteError = string.Empty;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void OpenEditDialog()
    {
        if (SelectedCategory == null) return;

        IsEditing = true;
        EditName = SelectedCategory.Name;
        DialogError = string.Empty;
        DeleteError = string.Empty;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void SaveCategory()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            DialogError = "Category name is required";
            return;
        }

        if (IsEditing && SelectedCategory != null)
        {
            _dataService.UpdateCategory(new Category
            {
                Id = SelectedCategory.Id,
                Name = EditName
            });
        }
        else
        {
            _dataService.AddCategory(new Category
            {
                Name = EditName
            });
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
    private void DeleteCategory()
    {
        if (SelectedCategory == null) return;

        if (!_dataService.CanDeleteCategory(SelectedCategory.Id))
        {
            DeleteError = "Cannot delete category with existing products";
            return;
        }

        _dataService.DeleteCategory(SelectedCategory.Id);
        Refresh();
    }
}

public class CategoryItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}
