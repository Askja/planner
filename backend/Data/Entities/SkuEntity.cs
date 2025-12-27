namespace Data.Entities;

using Abstractions;

public sealed class SkuEntity : BaseEntity {
    public int SkuId { get; set; }

    public string SkuName { get; set; } = string.Empty;

    public List<SkuSubEntity> SkuSubs { get; set; } = [];
}