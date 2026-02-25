using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GroceryStore.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; } = "piece";
    public string Barcode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [JsonIgnore]
    public string? CategoryName { get; set; }

    [JsonIgnore]
    public bool IsLowStock { get; set; }
}

public class TransactionItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class Transaction
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public List<TransactionItem> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class AppSettings
{
    public string StoreName { get; set; } = "My Grocery Store";
    public decimal TaxRate { get; set; } = 5.0m;
    public string CurrencySymbol { get; set; } = "$";
    public int LowStockThreshold { get; set; } = 10;
}
