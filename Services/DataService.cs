using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GroceryStore.Models;

namespace GroceryStore.Services;

public class DataService
{
    private static readonly string AppDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GroceryStore");

    private static readonly string ProductsFile = Path.Combine(AppDataPath, "products.json");
    private static readonly string CategoriesFile = Path.Combine(AppDataPath, "categories.json");
    private static readonly string TransactionsFile = Path.Combine(AppDataPath, "transactions.json");
    private static readonly string SettingsFile = Path.Combine(AppDataPath, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private List<Category> _categories = new();
    private List<Product> _products = new();
    private List<Transaction> _transactions = new();
    private AppSettings _settings = new();

    public List<Category> Categories => _categories;
    public List<Product> Products => _products;
    public List<Transaction> Transactions => _transactions;
    public AppSettings Settings => _settings;

    public event Action? DataChanged;

    public DataService()
    {
        EnsureDirectoryExists();
        LoadAll();
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(AppDataPath))
            Directory.CreateDirectory(AppDataPath);
    }

    private void LoadAll()
    {
        _categories = LoadFromFile<List<Category>>(CategoriesFile) ?? new List<Category>();
        _products = LoadFromFile<List<Product>>(ProductsFile) ?? new List<Product>();
        _transactions = LoadFromFile<List<Transaction>>(TransactionsFile) ?? new List<Transaction>();
        _settings = LoadFromFile<AppSettings>(SettingsFile) ?? new AppSettings();

        if (_categories.Count == 0)
        {
            _categories.Add(new Category { Id = 1, Name = "Fruits & Vegetables" });
            _categories.Add(new Category { Id = 2, Name = "Dairy Products" });
            _categories.Add(new Category { Id = 3, Name = "Beverages" });
            _categories.Add(new Category { Id = 4, Name = "Snacks" });
            _categories.Add(new Category { Id = 5, Name = "Household" });
            SaveCategories();
        }

        UpdateProductCategoryNames();
        UpdateLowStockFlags();
    }

    private T? LoadFromFile<T>(string filePath) where T : class
    {
        try
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading {filePath}: {ex.Message}");
        }
        return null;
    }

    public void SaveCategories()
    {
        SaveToFile(CategoriesFile, _categories);
        DataChanged?.Invoke();
    }

    public void SaveProducts()
    {
        SaveToFile(ProductsFile, _products);
        UpdateLowStockFlags();
        DataChanged?.Invoke();
    }

    public void SaveTransactions()
    {
        SaveToFile(TransactionsFile, _transactions);
        DataChanged?.Invoke();
    }

    public void SaveSettings()
    {
        SaveToFile(SettingsFile, _settings);
        DataChanged?.Invoke();
    }

    private void SaveToFile<T>(string filePath, T data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving {filePath}: {ex.Message}");
        }
    }

    public int GetNextCategoryId() => _categories.Count > 0 ? _categories.Max(c => c.Id) + 1 : 1;
    public int GetNextProductId() => _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
    public int GetNextTransactionId() => _transactions.Count > 0 ? _transactions.Max(t => t.Id) + 1 : 1;

    public void AddCategory(Category category)
    {
        category.Id = GetNextCategoryId();
        _categories.Add(category);
        SaveCategories();
    }

    public void UpdateCategory(Category category)
    {
        var index = _categories.FindIndex(c => c.Id == category.Id);
        if (index >= 0)
        {
            _categories[index] = category;
            SaveCategories();
        }
    }

    public bool CanDeleteCategory(int categoryId)
    {
        return !_products.Any(p => p.CategoryId == categoryId);
    }

    public bool DeleteCategory(int categoryId)
    {
        if (!CanDeleteCategory(categoryId))
            return false;

        _categories.RemoveAll(c => c.Id == categoryId);
        SaveCategories();
        return true;
    }

    public void AddProduct(Product product)
    {
        product.Id = GetNextProductId();
        _products.Add(product);
        SaveProducts();
    }

    public void UpdateProduct(Product product)
    {
        var index = _products.FindIndex(p => p.Id == product.Id);
        if (index >= 0)
        {
            _products[index] = product;
            SaveProducts();
        }
    }

    public bool DeleteProduct(int productId)
    {
        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product == null) return false;

        _products.Remove(product);
        SaveProducts();
        return true;
    }

    public void AddTransaction(Transaction transaction)
    {
        transaction.Id = GetNextTransactionId();
        _transactions.Add(transaction);

        foreach (var item in transaction.Items)
        {
            var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product != null)
            {
                product.Quantity -= item.Quantity;
                if (product.Quantity < 0) product.Quantity = 0;
            }
        }

        SaveTransactions();
        SaveProducts();
    }

    public List<Transaction> GetTransactionsByDateRange(DateTime start, DateTime end)
    {
        return _transactions
            .Where(t => t.Date.Date >= start.Date && t.Date.Date <= end.Date)
            .OrderByDescending(t => t.Date)
            .ToList();
    }

    public List<Product> GetLowStockProducts()
    {
        return _products
            .Where(p => p.Quantity > 0 && p.Quantity < _settings.LowStockThreshold)
            .OrderBy(p => p.Quantity)
            .ToList();
    }

    public decimal GetTodaySales()
    {
        var today = DateTime.Today;
        return _transactions
            .Where(t => t.Date.Date == today)
            .Sum(t => t.TotalAmount);
    }

    public int GetTodayTransactionCount()
    {
        var today = DateTime.Today;
        return _transactions.Count(t => t.Date.Date == today);
    }

    public List<Transaction> GetRecentTransactions(int count = 10)
    {
        return _transactions
            .OrderByDescending(t => t.Date)
            .Take(count)
            .ToList();
    }

    public Dictionary<int, decimal> GetSalesByCategory(DateTime start, DateTime end)
    {
        var result = new Dictionary<int, decimal>();

        var transactions = GetTransactionsByDateRange(start, end);

        foreach (var trans in transactions)
        {
            foreach (var item in trans.Items)
            {
                var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    if (!result.ContainsKey(product.CategoryId))
                        result[product.CategoryId] = 0;
                    result[product.CategoryId] += item.TotalPrice;
                }
            }
        }

        return result;
    }

    public Dictionary<string, decimal> GetTopSellingProducts(DateTime start, DateTime end, int count = 5)
    {
        var result = new Dictionary<string, decimal>();

        var transactions = GetTransactionsByDateRange(start, end);

        foreach (var trans in transactions)
        {
            foreach (var item in trans.Items)
            {
                if (!result.ContainsKey(item.ProductName))
                    result[item.ProductName] = 0;
                result[item.ProductName] += item.Quantity;
            }
        }

        return result
            .OrderByDescending(x => x.Value)
            .Take(count)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    private void UpdateProductCategoryNames()
    {
        foreach (var product in _products)
        {
            var category = _categories.FirstOrDefault(c => c.Id == product.CategoryId);
            product.CategoryName = category?.Name ?? "Unknown";
        }
    }

    private void UpdateLowStockFlags()
    {
        foreach (var product in _products)
        {
            product.IsLowStock = product.Quantity > 0 && product.Quantity < _settings.LowStockThreshold;
        }
    }

    public void RefreshData()
    {
        UpdateProductCategoryNames();
        UpdateLowStockFlags();
        DataChanged?.Invoke();
    }
}
