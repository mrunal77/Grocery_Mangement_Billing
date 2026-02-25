using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GroceryStore.Models;
using GroceryStore.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace GroceryStore.ViewModels;

public partial class CategoriesViewModel : ViewModelBase
{
    private readonly DataService _dataService;
    private readonly ImportService _importService;

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

    [ObservableProperty]
    private string _importMessage = string.Empty;

    [ObservableProperty]
    private bool _showImportMessage;

    public CategoriesViewModel(DataService dataService)
    {
        _dataService = dataService;
        _importService = new ImportService(dataService);
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

    [RelayCommand]
    private async Task ImportCategories()
    {
        try
        {
            IStorageProvider? storageProvider = null;
            
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                storageProvider = desktop.MainWindow?.StorageProvider;
            }

            if (storageProvider == null)
            {
                ImportMessage = "Cannot open file dialog";
                ShowImportMessage = true;
                return;
            }

            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Import Categories",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } },
                    new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                var (imported, skipped, errors) = _importService.ImportCategories(file.Path.LocalPath);
                
                var message = $"Imported: {imported}, Skipped: {skipped}";
                if (errors.Count > 0)
                {
                    message += $"\nErrors:\n{string.Join("\n", errors.Take(5))}";
                    if (errors.Count > 5) message += $"\n...and {errors.Count - 5} more";
                }

                ImportMessage = message;
                ShowImportMessage = true;
                Refresh();

                await Task.Delay(5000);
                ShowImportMessage = false;
            }
        }
        catch (Exception ex)
        {
            ImportMessage = $"Error: {ex.Message}";
            ShowImportMessage = true;
        }
    }
}

public class CategoryItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}
