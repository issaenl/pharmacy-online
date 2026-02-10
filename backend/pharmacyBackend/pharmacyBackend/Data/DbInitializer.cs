using pharmacyBackend.Models;
using System.Diagnostics;

namespace pharmacyBackend.Data
{
    public class DbInitializer
    {

        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.Categories.Any())
            {
                var categories = new Category[]
                {
                    new Category { Name = "Антибиотики", Description = "Противомикробные препараты" },
                    new Category { Name = "Антидепрессанты", Description = "При депрессии" },
                    new Category { Name = "Антисептики", Description = "Обеззараживающие средства" },
                    new Category { Name = "Борьба с вредными привычками", Description = "Для отказа от курения/алкоголя" },
                    new Category { Name = "Витамины", Description = "Лекарственные витамины" },
                    new Category { Name = "Гели и мази от боли в суставах, мышцах", Description = "Наружные средства" },
                    new Category { Name = "Глазные капли от сухости", Description = "Увлажняющие капли" },
                    new Category { Name = "Жаропонижающие", Description = "Снижают температуру" },
                    new Category { Name = "Желчегонные", Description = "Для оттока желчи" },
                    new Category { Name = "Контрацептивы", Description = "Противозачаточные средства" },
                    new Category { Name = "Кортикостероиды", Description = "Гормональные препараты" },
                    new Category { Name = "Кровоостанавливающие препараты", Description = "Гемостатики" },
                    new Category { Name = "Минеральные добавки", Description = "Минералы" },
                    new Category { Name = "Мочегонные", Description = "Диуретики" },
                    new Category { Name = "Обезболивающие и спазмолитики", Description = "Средства от боли и спазмов" },
                    new Category { Name = "Препараты для иммунитета", Description = "Иммуномодуляторы" },
                    new Category { Name = "Препараты для лечения акне", Description = "От угревой сыпи" },
                    new Category { Name = "Препараты для лечения бронхиальной астмы", Description = "При астме" },
                    new Category { Name = "Препараты для лечения заболеваний уха", Description = "Ототологические средства" },
                    new Category { Name = "Препараты для лечения простатита", Description = "При воспалении простаты" },
                    new Category { Name = "Препараты для лечения псориаза и дерматита", Description = "При кожных заболеваниях" },
                    new Category { Name = "Препараты для лечения ран, ожогов и пролежней", Description = "Для заживления повреждений кожи" },
                    new Category { Name = "Препараты для лечения сахарного диабета", Description = "Антидиабетические" },
                    new Category { Name = "Препараты для лечения урологических заболеваний", Description = "Для мочеполовой системы" },
                    new Category { Name = "Препараты для мозгового кровообращения", Description = "Ноотропы, цереброваскулярные" },
                    new Category { Name = "Препараты для повышения потенции", Description = "Для эректильной функции" },
                    new Category { Name = "Препараты для похудения", Description = "Для снижения веса" },
                    new Category { Name = "Препараты для снижения холестерина", Description = "Гиполипидемические" },
                    new Category { Name = "Препараты для суставов", Description = "Хондропротекторы и др." },
                    new Category { Name = "Препараты от аллергии", Description = "Антигистаминные" },
                    new Category { Name = "Препараты от варикоза, венотоники", Description = "Для венозного кровотока" },
                    new Category { Name = "Препараты от вшей", Description = "Педикулициды" },
                    new Category { Name = "Препараты от выпадения волос", Description = "Трихологические" },
                    new Category { Name = "Препараты от высокого давления", Description = "Антигипертензивные" },
                    new Category { Name = "Препараты от геморроя", Description = "Противогеморроидальные" },
                    new Category { Name = "Препараты от глистов", Description = "Антигельминтные" },
                    new Category { Name = "Препараты от головной боли и мигрени", Description = "Цефалгические средства" },
                    new Category { Name = "Препараты от кашля", Description = "Противокашлевые, отхаркивающие" },
                    new Category { Name = "Препараты от молочницы", Description = "Против кандидоза" },
                    new Category { Name = "Препараты от насморка и заложенности носа", Description = "Деконгестанты" },
                    new Category { Name = "Препараты от подагры", Description = "Урикозурические, урикодепрессивные" },
                    new Category { Name = "Препараты от простуды и гриппа", Description = "Противовирусные, симптоматические" },
                    new Category { Name = "Препараты от тошноты и рвоты", Description = "Антеметики" },
                    new Category { Name = "Препараты от тромбов (антикоагулянты)", Description = "Антитромботические" },
                    new Category { Name = "Препараты от укачивания", Description = "При кинетозах" },
                    new Category { Name = "Препараты от чесотки", Description = "Скабициды" },
                    new Category { Name = "Препараты от шрамов и рубцов", Description = "Келоидолитические" },
                    new Category { Name = "Препараты при боли в горле", Description = "Антисептики, анестетики для горла" },
                    new Category { Name = "Препараты при вздутии живота", Description = "Ветрогонные" },
                    new Category { Name = "Препараты при герпесе", Description = "Противогерпетические" },
                    new Category { Name = "Препараты при диарее, кишечных расстройствах", Description = "Антидиарейные" },
                    new Category { Name = "Препараты при заболеваниях печени", Description = "Гепатопротекторы" },
                    new Category { Name = "Препараты при изжоге", Description = "Антациды, антисекреторные" },
                    new Category { Name = "Препараты при менопаузе", Description = "Заместительная гормональная терапия" },
                    new Category { Name = "Препараты при язве и гастрите", Description = "Гастропротекторы" },
                    new Category { Name = "При заболеваниях глаз, век", Description = "Офтальмологические" },
                    new Category { Name = "Пробиотики и пребиотики", Description = "Для микрофлоры кишечника" },
                    new Category { Name = "Противогрибковые препараты", Description = "Антимикотики" },
                    new Category { Name = "Слабительные препараты", Description = "Лаксативы" },
                    new Category { Name = "Снотворные и успокоительные средства", Description = "Седативные, гипнотики" },
                    new Category { Name = "Ферменты, препараты для улучшения пищеварения", Description = "Энзимы" },
                    new Category { Name = "Обезболивающие", Description = "Средства от боли и жара" },
                    new Category { Name = "Витамины", Description = "Для укрепления иммунитета" }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }


            if (!context.Pharmacies.Any())
            {
                var pharmacies = new Pharmacy[]
                {
                    new Pharmacy { Name = "Планета здоровья Аптека №1", Address = "пр-т Независимости, д. 18",
                        District = "Минск", Phone = "+375 (17) 3521781"},
                    new Pharmacy { Name = "Планета Здоровья Аптека №14", Address = "ул. П. Глебки, д. 2",
                        District = "Минск", Phone = "+375 (17) 3609006"}
                };

                context.Pharmacies.AddRange(pharmacies);
                context.SaveChanges();
            }

            if (!context.Products.Any(p => p.Name == "Канефрон форте"))
            {
                var urologyCat = context.Categories.First(c => c.Name == "Препараты для лечения урологических заболеваний");
                var coughCat = context.Categories.First(c => c.Name == "Препараты от кашля");
                var vitaminsCat = context.Categories.First(c => c.Name == "Витамины");
                var probioticCat = context.Categories.First(c => c.Name == "Препараты при диарее, кишечных расстройствах");
                var pharmacy1 = context.Pharmacies.First(p => p.Name == "Планета здоровья Аптека №1");
                var pharmacy2 = context.Pharmacies.First(p => p.Name == "Планета Здоровья Аптека №14");

                var products = new Product[]
                {
                    new Product
                    {
                        Name = "Канефрон форте",
                        Manufacturer = "Бионорика",
                        Country = "Германия",
                        IsPrescription = false,
                        DosageForm = "таблетки ×30",
                        ExpirationDate = new DateOnly(2027, 12, 1),
                        IsActive = true,
                        CategoryId = urologyCat.Id
                    },
                    new Product
                    {
                        Name = "Бронхипрет",
                        Manufacturer = "Бионорика",
                        Country = "Германия",
                        IsPrescription = false,
                        DosageForm = "раствор 100 мл ×1",
                        ExpirationDate = new DateOnly(2026, 6, 1),
                        IsActive = true,
                        CategoryId = coughCat.Id
                    },
                    new Product
                    {
                        Name = "Магне b6",
                        Manufacturer = "Санофи",
                        Country = "Франция",
                        IsPrescription = false,
                        DosageForm = "таблетки ×60",
                        ExpirationDate = new DateOnly(2027, 3, 1),
                        IsActive = true,
                        CategoryId = vitaminsCat.Id
                    },
                    new Product
                    {
                        Name = "Энтерол",
                        Manufacturer = "Биокодекс",
                        Country = "Франция",
                        IsPrescription = false,
                        DosageForm = "капсулы 250 мг ×30",
                        ExpirationDate = new DateOnly(2026, 9, 1),
                        IsActive = true,
                        CategoryId = probioticCat.Id
                    }
                };

                context.Products.AddRange(products);
                context.SaveChanges();

                var canephron = context.Products.First(p => p.Name == "Канефрон форте");
                var bronchipret = context.Products.First(p => p.Name == "Бронхипрет");
                var magneB6 = context.Products.First(p => p.Name == "Магне b6");
                var enterol = context.Products.First(p => p.Name == "Энтерол");

                var stocks = new Stock[]
                {
                    new Stock { ProductId = canephron.Id, PharmacyId = pharmacy1.Id, Price = 27.34m, Quantity = 10 },
                    new Stock { ProductId = canephron.Id, PharmacyId = pharmacy2.Id, Price = 32.01m, Quantity = 5 },
                    new Stock { ProductId = bronchipret.Id, PharmacyId = pharmacy1.Id, Price = 22.50m, Quantity = 8 },
                    new Stock { ProductId = bronchipret.Id, PharmacyId = pharmacy2.Id, Price = 24.90m, Quantity = 3 },
                    new Stock { ProductId = magneB6.Id, PharmacyId = pharmacy1.Id, Price = 27.90m, Quantity = 6 },
                    new Stock { ProductId = magneB6.Id, PharmacyId = pharmacy2.Id, Price = 29.50m, Quantity = 4 },
                    new Stock { ProductId = enterol.Id, PharmacyId = pharmacy1.Id, Price = 26.22m, Quantity = 12 },
                    new Stock { ProductId = enterol.Id, PharmacyId = pharmacy2.Id, Price = 28.00m, Quantity = 7 }
                };

                context.Stocks.AddRange(stocks);
                context.SaveChanges();
            }
        }
    }
}
