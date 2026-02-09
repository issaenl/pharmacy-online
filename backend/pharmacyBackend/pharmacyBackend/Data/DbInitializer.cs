using pharmacyBackend.Models;

namespace pharmacyBackend.Data
{
    public class DbInitializer
    {

        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Categories.Any()) return;

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
    }
}
