namespace Services.Services;

using Domain.Models;
using Interfaces;
using Microsoft.Extensions.Options;

public sealed class PlannerMetadataBuilder(IOptions<PlannerOptions> options) : IPlannerMetadataBuilder {
    private readonly PlannerOptions _options = options.Value;

    public IReadOnlyList<PlannerFieldMetadata> Build() {
        List<PlannerFieldMetadata> metadata = [
            new() {
                Key = "level",
                Title = "Уровень",
                DataType = "enum",
                Style = "dim",
                IsEditable = false
            },
            new() {
                Key = "valueType",
                Title = "Тип",
                DataType = "enum",
                Style = "dim",
                IsEditable = false
            },
            new() {
                Key = "skuName",
                Title = "Название",
                DataType = "string",
                Style = "bold",
                IsEditable = false
            },
            new() {
                Key = "skuSubName",
                Title = "Подкатегория",
                DataType = "string",
                Style = "normal",
                IsEditable = false
            },
            new() {
                Key = "historyY0",
                Title = "История Y0",
                DataType = "decimal",
                Style = "normal",
                IsEditable = false
            },
            new() {
                Key = "planningY1",
                Title = "Плановая Y1",
                DataType = "decimal",
                Style = "editable",
                IsEditable = true
            },
            new() {
                Key = "contributionGrowth",
                Title = "Contr. Growth",
                DataType = "decimal",
                Style = "percent",
                IsEditable = false
            }
        ];

        return metadata;
    }
}