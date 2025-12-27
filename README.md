# Planner (сервис «Планирование»)

## Описание
Сервис строит плоскую модель планирования (Planner) на основе иерархии сущностей SKU → SKUSub и двух периодов данных (HistoryY0, PlanningY1). На каждый SKUSubId генерируется 3 строки: UNITS, PRICE, AMOUNT.

Backend предоставляет:
- `GET /api/planner` — рассчитанная модель + metadata (типы/заголовки/стили/IsEditable).
- `PATCH /api/planner` — обновление PlanningY1 (разрешено только на уровне SKUSUB).

Frontend (Vue 3) показывает таблицу, поддерживает rowspan для визуальной группировки и инлайн-редактирование PlanningY1 только там, где это разрешено метаданными.

## Стек
- Backend: .NET 8, EF Core, InMemory DB.
- Frontend: Vue 3 (SPA).
- Логирование: Serilog (конфиг в `Logger/serilog.config.json`)
- DI: DryIoC
- Репозитории: EF Core + базовый репозиторий, регистрация через Scrutor
- Кеш: Snapshot provider (IMemoryCache) + EF 2nd-level cache interceptor

## Быстрый старт

### Требования
- .NET SDK 8
- Node.js 18+ (рекомендуется)

### Backend
Из корня backend

```bash
dotnet restore
dotnet build -c Release
dotnet run --project backend/Api/Api.csproj
