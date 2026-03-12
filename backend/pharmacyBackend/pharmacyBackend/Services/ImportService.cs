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
                    errors.Add($"Строка {lineNumber}: категория '{categoryNameFromFile}' не найдена.");
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
    }
}
