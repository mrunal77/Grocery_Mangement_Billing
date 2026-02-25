using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using GroceryStore.Models;

namespace GroceryStore.Services;

public class ImportService
{
    private readonly DataService _dataService;

    public ImportService(DataService dataService)
    {
        _dataService = dataService;
    }

    public (int imported, int skipped, List<string> errors) ImportProducts(string filePath)
    {
        int imported = 0;
        int skipped = 0;
        var errors = new List<string>();

        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<ProductImportMap>();

            var records = csv.GetRecords<ProductImportRecord>().ToList();

            foreach (var record in records)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(record.Name))
                    {
                        errors.Add($"Skipped row: Product name is empty");
                        skipped++;
                        continue;
                    }

                    var categoryId = 1;
                    if (!string.IsNullOrWhiteSpace(record.Category))
                    {
                        var category = _dataService.Categories.FirstOrDefault(c => 
                            c.Name.Equals(record.Category, StringComparison.OrdinalIgnoreCase));
                        
                        if (category == null)
                        {
                            var newCategory = new Category 
                            { 
                                Name = record.Category,
                                Id = _dataService.GetNextCategoryId()
                            };
                            _dataService.AddCategory(newCategory);
                            categoryId = newCategory.Id;
                        }
                        else
                        {
                            categoryId = category.Id;
                        }
                    }

                    var price = 0m;
                    if (!string.IsNullOrWhiteSpace(record.Price))
                    {
                        if (!decimal.TryParse(record.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
                        {
                            price = 0m;
                        }
                    }

                    var quantity = 0;
                    if (!string.IsNullOrWhiteSpace(record.Quantity))
                    {
                        if (!int.TryParse(record.Quantity, out quantity))
                        {
                            quantity = 0;
                        }
                    }

                    var product = new Product
                    {
                        Name = record.Name,
                        CategoryId = categoryId,
                        Price = price,
                        Quantity = quantity,
                        Unit = string.IsNullOrWhiteSpace(record.Unit) ? "piece" : record.Unit,
                        Barcode = record.Barcode ?? string.Empty,
                        Description = record.Description ?? string.Empty
                    };

                    _dataService.AddProduct(product);
                    imported++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Error importing '{record.Name}': {ex.Message}");
                    skipped++;
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"File error: {ex.Message}");
        }

        return (imported, skipped, errors);
    }

    public (int imported, int skipped, List<string> errors) ImportCategories(string filePath)
    {
        int imported = 0;
        int skipped = 0;
        var errors = new List<string>();

        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<CategoryImportRecord>().ToList();

            foreach (var record in records)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(record.Name))
                    {
                        errors.Add($"Skipped row: Category name is empty");
                        skipped++;
                        continue;
                    }

                    var existingCategory = _dataService.Categories.FirstOrDefault(c => 
                        c.Name.Equals(record.Name, StringComparison.OrdinalIgnoreCase));

                    if (existingCategory != null)
                    {
                        errors.Add($"Skipped '{record.Name}': Category already exists");
                        skipped++;
                        continue;
                    }

                    var category = new Category
                    {
                        Name = record.Name
                    };

                    _dataService.AddCategory(category);
                    imported++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Error importing '{record.Name}': {ex.Message}");
                    skipped++;
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"File error: {ex.Message}");
        }

        return (imported, skipped, errors);
    }
}

public class ProductImportRecord
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Price { get; set; }
    public string? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
}

public class CategoryImportRecord
{
    public string? Name { get; set; }
}

public sealed class ProductImportMap : ClassMap<ProductImportRecord>
{
    public ProductImportMap()
    {
        Map(m => m.Name).Name("Name", "name", "ProductName", "product_name", "PRODUCT_NAME");
        Map(m => m.Category).Name("Category", "category", "CategoryName", "category_name", "CATEGORY");
        Map(m => m.Price).Name("Price", "price", "Price", "PRICE");
        Map(m => m.Quantity).Name("Quantity", "quantity", "Qty", "qty", "QUANTITY", "Stock");
        Map(m => m.Unit).Name("Unit", "unit", "UNIT", "UnitType");
        Map(m => m.Barcode).Name("Barcode", "barcode", "BARCODE", "SKU", "Code");
        Map(m => m.Description).Name("Description", "description", "DESC", "DESCRIPTION");
    }
}
