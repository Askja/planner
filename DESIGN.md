## Цель
Реализовать сервис, который строит плоскую модель планирования из иерархии сущностей и поддерживает:
- добавление ~30 дополнительных расчетных колонок (аналогов HistoryY0) без переписывания всего кода;
- добавление новых уровней детализации без переписывания всего кода;
- чистый код/DI/тестируемость.

## Данные и уровни
Сущности:
- SKU (SKUId, SKUName)
- SKUSub (SKUSubId, SKUId, SKUSubName, SKUPRICE, SKURatio)
- HistoryY0 / PlanningY1: (SKUSubId, Units, Amount)

Уровни модели:
- TOTAL, SKU, SKUSUB

ValueType:
- UNITS, PRICE, AMOUNT (3 строки на каждый SKUSubId)

## Механизм расчета
Расчет “снизу вверх”, с учетом фильтров: если SKUSubId отфильтрован, он исключается из расчетов всех уровней.

Основные формулы:

SKUSUB:
- PRICE (PlanningY1) = SKUPRICE
- PRICE (HistoryY0) = AMOUNT(HistoryY0) / UNITS(HistoryY0)
- AMOUNT = UNITS * PRICE
- ContributionGrowth = (PlanningY1[SKUSUB] - HistoryY0[SKUSUB]) / HistoryY0[SKU]

SKU:
- AMOUNT = сумма AMOUNT всех SKUSUB
- UNITS = сумма (UNITS * SKURatio) всех SKUSUB
- PRICE = AMOUNT / UNITS
- ContributionGrowth = (PlanningY1[SKU] - HistoryY0[SKU]) / HistoryY0[TOTAL]

TOTAL:
- AMOUNT/UNITS = сумма по всем SKU
- PRICE = AMOUNT(TOTAL) / UNITS(TOTAL)
- ContributionGrowth = (PlanningY1[TOTAL] - HistoryY0[TOTAL]) / HistoryY0[TOTAL]

## Архитектура (слои)
- Api: HTTP endpoints.
- Domain: модели/DTO
- Services: расчёты, формулы.
- Data: EF Core, сущности, конфигурации, репозитории, провайдер снапшотов.
- Logger: Serilog-конфигурация, middleware (correlationId, exception handler, slow request log).

## Расширяемость: +30 колонок (периоды)
Подход: “таблица значений” вместо “30 раз скопировать формулу”.

Идея:
1) Внутри домена описывается набор периодов/колонок как конфиг/enum:
   - например: PeriodId = HistoryY0, PlanningY1, HistoryY-1, HistoryY-2, …
2) Для каждой колонки определяются источники и правила расчета (Strategy/Policy).
3) Движок расчета работает с коллекцией “колонка → значения”, а не с жестко закодированными свойствами.

Пример интерфейса (концептуально):
- `IColumnCalculator` — считает конкретную “колонку периода”.
- `ILevelAggregator` — агрегирует снизу вверх на уровень SKU/TOTAL.
- `IValueTypeCalculator` — вычисляет UNITS/PRICE/AMOUNT.

Добавление новой колонки:
- добавить новый `PeriodDefinition` (ключ + источники данных);
- зарегистрировать калькулятор/правила (без изменения существующих).

## Расширяемость: новый уровень детализации
Подход: цепочка агрегации по уровням.

Сейчас уровни: SKUSUB → SKU → TOTAL.
Чтобы добавить новый уровень (например, “Brand” между SKU и TOTAL):
- добавить enum `Level.Brand`;
- реализовать агрегатор `BrandAggregator : ILevelAggregator`, который знает, как собирать Brand из SKU;
- зарегистрировать агрегатор в пайплайн.

Движок расчетов вызывает агрегаторы по порядку уровней (конфигурируемый список уровней).

## Кеширование
Слои:
1) Snapshot cache (IMemoryCache): хранит “снапшот исходных таблиц” (sku, skuSub, history, planning).
2) EF 2nd-level cache interceptor: кеширование запросов репозиториев (опционально поверх снапшота).

Инвалидация:
- после успешного PATCH: инвалидируем снапшот (и при необходимости чистим EF cache).

Параллельность:
- построение снапшота под semaphore gate (один прогрев).

## Контракты API и UI
GET возвращает:
- `data` + `metadata` (включая `isEditable`).
Frontend использует metadata, чтобы включать редактирование только там, где это разрешено.

PATCH ограничен:
- редактирование только `PlanningY1` на уровне `SKUSUB`.

## Тестируемость
Минимально: 3–4 unit-теста на ключевые формулы (PRICE/AMOUNT/ContributionGrowth на разных уровнях).

## Ошибки и логирование
- глобальный middleware для исключений (одна точка правды)
- CorrelationId в контексте запроса
- Serilog через конфиг `Logger/serilog.config.json` (fallback при отсутствии)