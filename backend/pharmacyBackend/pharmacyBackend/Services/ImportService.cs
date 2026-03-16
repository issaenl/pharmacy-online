using pharmacyBackend.Data;
using pharmacyBackend.Models;
using ClosedXML.Excel;
using pharmacyBackend.DTO;
using Microsoft.EntityFrameworkCore;

namespace pharmacyBackend.Services
{
    public class ImportService : IImportService
    {
        private readonly AppDbContext _context;

        public ImportService(AppDbContext context)
        {
            _context = context;
        }

        private bool ParseBooleanFlexible(string val)
        {
            var cleanVal = val?.Trim().ToLower() ?? "";
            return cleanVal == "true" || cleanVal == "1" || cleanVal == "да" || cleanVal == "+";
        }

        public async Task<ImportDTO> ImportProductsAsync(IFormFile file)
        {
            var result = new ImportDTO();
            var productsToAdd = new List<Product>();

            var categoriesDict = await _context.Categories
                .GroupBy(c => c.Name.ToLower())
                .ToDictionaryAsync(g => g.Key, g => g.First().Id);

            var existingProductsList = await _context.Products
                .Select(p => p.Name.ToLower() + "|" + p.Manufacturer.ToLower())
                .ToListAsync();

            var existingProducts = existingProductsList.ToHashSet();

            var extension = Path.GetExtension(file.FileName).ToLower();

            using var stream = file.OpenReadStream();

            if (extension == ".csv")
            {
                using var reader = new StreamReader(stream);
                var header = await reader.ReadLineAsync();
                int lineNumber = 1;

                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    line = line.Trim().Trim('"');
                    var values = line.Split(new[] { ',', ';' });
                    ProcessProductRow(values, lineNumber, categoriesDict, existingProducts, productsToAdd, result.Errors);
                }
            }
            else if (extension == ".xlsx" || extension == ".xls")
            {
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1); 
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                int lineNumber = 1;
                foreach (var row in rows)
                {
                    lineNumber++;
                    var values = new string[7];
                    for (int i = 0; i < 7; i++)
                    {
                        values[i] = row.Cell(i + 1).Value.ToString();
                    }

                    ProcessProductRow(values, lineNumber, categoriesDict, existingProducts, productsToAdd, result.Errors);
                }
            }
            else
            {
                result.Errors.Add("Неподдерживаемый формат файла. Используйте CSV или XLSX.");
                return result;
            }

            if (productsToAdd.Any())
            {
                _context.Products.AddRange(productsToAdd);
                await _context.SaveChangesAsync();
                result.AddedCount = productsToAdd.Count;
            }

            return result;
        }

        private void ProcessProductRow(
            string[] values,
            int lineNumber,
            Dictionary<string, int> categoriesDict,
            HashSet<string> existingProducts,
            List<Product> productsToAdd,
            List<string> errors)
        {
            if (values.Length < 7)
            {
                errors.Add($"Строка {lineNumber}: недостаточно колонок.");
                return;
            }

            try
            {
                var name = values[0].Trim();
                var manufacturer = values[1].Trim();
                var categoryNameFromFile = values[5].Trim();

                var uniqueKey = $"{name.ToLower()}|{manufacturer.ToLower()}";
                if (existingProducts.Contains(uniqueKey))
                {
                    errors.Add($"Строка {lineNumber}: Товар {name} уже существует (дубликат).");
                    return;
                }

                if (!categoriesDict.TryGetValue(categoryNameFromFile.ToLower(), out int categoryId))
                {
                    errors.Add($"Строка {lineNumber}: категория {categoryNameFromFile} не найдена.");
                    return;
                }

                var product = new Product
                {
                    Name = name,
                    Manufacturer = manufacturer,
                    Country = values[2].Trim(),
                    IsPrescription = ParseBooleanFlexible(values[3]),
                    DosageForm = values[4].Trim(),
                    CategoryId = categoryId,
                    IsActive = ParseBooleanFlexible(values[6])
                };

                productsToAdd.Add(product);
                existingProducts.Add(uniqueKey);
            }
            catch (Exception ex)
            {
                errors.Add($"Строка {lineNumber}: ошибка форматов данных ({ex.Message}).");
            }
        }

        public async Task<ImportDTO> ImportStocksAsync(IFormFile file)
        {
            var result = new ImportDTO();
            var stocksToAdd = new List<Stock>();

            var pharmaciesDict = await _context.Pharmacies
                .GroupBy(p => p.Name.ToLower())
                .ToDictionaryAsync(g => g.Key, g => g.First().Id);

            var productsDict = await _context.Products
                .GroupBy(p => p.Name.ToLower())
                .ToDictionaryAsync(g => g.Key, g => g.First().Id);

            var existingStocksList = await _context.Stocks
                .Select(s => s.PharmacyId + "_" + s.ProductId)
                .ToListAsync();
            var existingStocks = existingStocksList.ToHashSet();

            var extension = Path.GetExtension(file.FileName).ToLower();
            using var stream = file.OpenReadStream();

            if (extension == ".csv")
            {
                using var reader = new StreamReader(stream);
                await reader.ReadLineAsync(); 
                int lineNumber = 1;

                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    line = line.Trim().Trim('"');
                    var values = line.Split(new[] { ',', ';' });
                    ProcessStockRow(values, lineNumber, pharmaciesDict, productsDict, existingStocks, stocksToAdd, result.Errors);
                }
            }
            else if (extension == ".xlsx" || extension == ".xls")
            {
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                int lineNumber = 1;
                foreach (var row in rows)
                {
                    lineNumber++;
                    var values = new string[4];
                    for (int i = 0; i < 4; i++) values[i] = row.Cell(i + 1).Value.ToString();

                    ProcessStockRow(values, lineNumber, pharmaciesDict, productsDict, existingStocks, stocksToAdd, result.Errors);
                }
            }
            else
            {
                result.Errors.Add("Неподдерживаемый формат файла.");
                return result;
            }

            if (stocksToAdd.Any())
            {
                _context.Stocks.AddRange(stocksToAdd);
                await _context.SaveChangesAsync();
                result.AddedCount = stocksToAdd.Count;
            }

            return result;
        }

        private void ProcessStockRow(string[] values, int lineNumber,
            Dictionary<string, int> pharmaciesDict, Dictionary<string, int> productsDict,
            HashSet<string> existingStocks, List<Stock> stocksToAdd, List<string> errors)
        {
            if (values.Length < 4)
            {
                errors.Add($"Строка {lineNumber}: недостаточно колонок (ожидается 4).");
                return;
            }

            try
            {
                var pharmacyName = values[0].Trim().ToLower();
                var productName = values[1].Trim().ToLower();

                if (!pharmaciesDict.TryGetValue(pharmacyName, out int pharmacyId))
                {
                    errors.Add($"Строка {lineNumber}: Аптека {values[0]} не найдена.");
                    return;
                }

                if (!productsDict.TryGetValue(productName, out int productId))
                {
                    errors.Add($"Строка {lineNumber}: Товар {values[1]} не найден.");
                    return;
                }

                var uniqueKey = $"{pharmacyId}_{productId}";
                if (existingStocks.Contains(uniqueKey))
                {
                    errors.Add($"Строка {lineNumber}: Запас для товара {values[1]} в аптеке {values[0]} уже существует.");
                    return;
                }

                var stock = new Stock
                {
                    PharmacyId = pharmacyId,
                    ProductId = productId,
                    Quantity = int.Parse(values[2].Trim()),
                    Price = decimal.Parse(values[3].Trim().Replace(".", ",")),
                    LastUpdate = DateTime.UtcNow
                };

                stocksToAdd.Add(stock);
                existingStocks.Add(uniqueKey);
            }
            catch (Exception ex)
            {
                errors.Add($"Строка {lineNumber}: ошибка форматов данных ({ex.Message}).");
            }
        }

        public async Task<ImportDTO> ImportCategoriesAsync(IFormFile file)
        {
            var result = new ImportDTO();
            var categoriesToAdd = new List<Category>();

            var existingCategoriesList = await _context.Categories
                .Select(c => c.Name.ToLower())
                .ToListAsync();
            var existingCategories = existingCategoriesList.ToHashSet();

            var existingTagsDict = await _context.Tags
                .ToDictionaryAsync(t => t.Name.ToLower(), t => t);

            var extension = Path.GetExtension(file.FileName).ToLower();
            using var stream = file.OpenReadStream();

            if (extension == ".csv")
            {
                using var reader = new StreamReader(stream);
                await reader.ReadLineAsync();
                int lineNumber = 1;

                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    line = line.Trim().Trim('"');
                    var values = line.Split(new[] { ';', '\t' });
                    ProcessCategoryRow(values, lineNumber, existingCategories, existingTagsDict, categoriesToAdd, result.Errors);
                }
            }
            else if (extension == ".xlsx" || extension == ".xls")
            {
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                int lineNumber = 1;
                foreach (var row in rows)
                {
                    lineNumber++;
                    var values = new string[3];
                    for (int i = 0; i < 3; i++) values[i] = row.Cell(i + 1).Value.ToString();

                    ProcessCategoryRow(values, lineNumber, existingCategories, existingTagsDict, categoriesToAdd, result.Errors);
                }
            }
            else
            {
                result.Errors.Add("Неподдерживаемый формат файла.");
                return result;
            }

            if (categoriesToAdd.Any())
            {
                _context.Categories.AddRange(categoriesToAdd);
                await _context.SaveChangesAsync();
                result.AddedCount = categoriesToAdd.Count;
            }

            return result;
        }

        private void ProcessCategoryRow(string[] values, int lineNumber,
            HashSet<string> existingCategories, Dictionary<string, Tag> existingTagsDict,
            List<Category> categoriesToAdd, List<string> errors)
        {
            if (values.Length < 1 || string.IsNullOrWhiteSpace(values[0]))
            {
                errors.Add($"Строка {lineNumber}: пропущено название категории.");
                return;
            }

            try
            {
                var name = values[0].Trim();
                if (existingCategories.Contains(name.ToLower()))
                {
                    errors.Add($"Строка {lineNumber}: Категория {name} уже существует.");
                    return;
                }

                var description = values.Length > 1 ? values[1].Trim() : null;
                var tagsString = values.Length > 2 ? string.Join(",", values.Skip(2)) : "";

                var category = new Category
                {
                    Name = name,
                    Description = description,
                    CategoryTags = new List<CategoryTag>()
                };

                if (!string.IsNullOrWhiteSpace(tagsString))
                {
                    var tagNames = tagsString.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(t => t.Trim().ToLower())
                                             .Where(t => !string.IsNullOrEmpty(t))
                                             .Distinct();

                    foreach (var tagName in tagNames)
                    {
                        if (!existingTagsDict.TryGetValue(tagName, out var tag))
                        {
                            tag = new Tag { Name = tagName };
                            existingTagsDict[tagName] = tag;
                        }
                        category.CategoryTags.Add(new CategoryTag { Tag = tag });
                    }
                }

                categoriesToAdd.Add(category);
                existingCategories.Add(name.ToLower());
            }
            catch (Exception ex)
            {
                errors.Add($"Строка {lineNumber}: ошибка обработки данных ({ex.Message}).");
            }
        }
    }
}
