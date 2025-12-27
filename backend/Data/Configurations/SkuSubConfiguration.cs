namespace Data.Configurations;

using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class SkuSubConfiguration : IEntityTypeConfiguration<SkuSubEntity> {
    public void Configure(EntityTypeBuilder<SkuSubEntity> builder) {
        builder.ToTable("TableSKUSub");

        builder.HasKey(x => x.SkuSubId);

        builder.Property(x => x.SkuSubId).HasColumnName("SKUSubId").ValueGeneratedNever();
        builder.Property(x => x.SkuId).HasColumnName("SKUId").IsRequired();
        builder.Property(x => x.SkuSubName).HasColumnName("SKUSubName").IsRequired().HasMaxLength(200);
        builder.Property(x => x.SkuPrice).HasColumnName("SKUPRICE").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.SkuRatio).HasColumnName("SKURatio").HasPrecision(18, 6).IsRequired();

        builder.HasIndex(x => x.SkuSubName);
    }
}