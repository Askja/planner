namespace Data.Seed;

using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed class DataSeeder(DataContext context, ILogger<DataSeeder> logger) : IDataSeeder {
    public async Task SeedAsync(CancellationToken ct = default) {
        if (await context.Set<SkuEntity>().AnyAsync(ct).ConfigureAwait(false)) {
            logger.LogInformation("Пропускаем вставку данных ибо они уже есть.");

            return;
        }

        logger.LogInformation("Запись данных в БД...");

        List<SkuEntity> skus = [
            new() {
                SkuId = 1,
                SkuName = "Напитки"
            },
            new() {
                SkuId = 2,
                SkuName = "Закуски"
            },
            new() {
                SkuId = 3,
                SkuName = "Хозтовары"
            }
        ];

        List<SkuSubEntity> skuSubs = [
            new() {
                SkuSubId = 101,
                SkuId = 1,
                SkuSubName = "Cola",
                SkuPrice = 2.50m,
                SkuRatio = 1.00m
            },
            new() {
                SkuSubId = 102,
                SkuId = 1,
                SkuSubName = "Orange",
                SkuPrice = 2.10m,
                SkuRatio = 0.90m
            },
            new() {
                SkuSubId = 103,
                SkuId = 1,
                SkuSubName = "Вода",
                SkuPrice = 1.20m,
                SkuRatio = 0.80m
            },
            new() {
                SkuSubId = 201,
                SkuId = 2,
                SkuSubName = "Чипсы",
                SkuPrice = 1.70m,
                SkuRatio = 1.10m
            },
            new() {
                SkuSubId = 202,
                SkuId = 2,
                SkuSubName = "Орешки",
                SkuPrice = 3.40m,
                SkuRatio = 1.00m
            },
            new() {
                SkuSubId = 203,
                SkuId = 2,
                SkuSubName = "Печенье",
                SkuPrice = 2.90m,
                SkuRatio = 0.95m
            },
            new() {
                SkuSubId = 301,
                SkuId = 3,
                SkuSubName = "Мыло",
                SkuPrice = 1.30m,
                SkuRatio = 1.05m
            },
            new() {
                SkuSubId = 302,
                SkuId = 3,
                SkuSubName = "Шампунь",
                SkuPrice = 4.20m,
                SkuRatio = 1.00m
            },
            new() {
                SkuSubId = 303,
                SkuId = 3,
                SkuSubName = "Кондиционер",
                SkuPrice = 5.10m,
                SkuRatio = 0.85m
            }
        ];

        List<HistoryY0Entity> history = skuSubs
        .Select(
            s => {
                decimal units = 10 + s.SkuSubId % 7 * 3;
                decimal amount = units * (s.SkuPrice - 0.10m);

                return new HistoryY0Entity {
                    SkuSubId = s.SkuSubId,
                    Units = units,
                    Amount = amount
                };
            }
        )
        .ToList();

        List<PlanningY1Entity> planning = skuSubs
        .Select(
            s => {
                decimal units = 12 + s.SkuSubId % 5 * 4;
                decimal amount = units * s.SkuPrice;

                return new PlanningY1Entity {
                    SkuSubId = s.SkuSubId,
                    Units = units,
                    Amount = amount
                };
            }
        )
        .ToList();

        await context.Set<SkuEntity>().AddRangeAsync(skus, ct).ConfigureAwait(false);
        await context.Set<SkuSubEntity>().AddRangeAsync(skuSubs, ct).ConfigureAwait(false);
        await context.Set<HistoryY0Entity>().AddRangeAsync(history, ct).ConfigureAwait(false);
        await context.Set<PlanningY1Entity>().AddRangeAsync(planning, ct).ConfigureAwait(false);

        await context.SaveChangesAsync(ct).ConfigureAwait(false);

        logger.LogInformation("Успешная вставка: skus={skus} skuSubs={subs}", skus.Count, skuSubs.Count);
    }
}