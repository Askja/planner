namespace Services.Interfaces;

using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Enums;

public interface IPlannerService {
    Task<PlannerRowResponse> GetPlannerAsync(IReadOnlyCollection<string>? skuSubNames, IReadOnlyCollection<PlannerLevel>? levels, CancellationToken ct = default);
    Task UpdatePlanningAsync(PlannerRowRequest request, CancellationToken ct = default);
}