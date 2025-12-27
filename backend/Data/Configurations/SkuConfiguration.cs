namespace Data.Configurations;

using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class SkuConfiguration : IEntityTypeConfiguration<SkuEntity> {
    public void Configure(EntityTypeBuilder<SkuEntity> builder) {
        builder.ToTable("TableSKU");

        builder.HasKey(x => x.SkuId);

        builder.Property(x => x.SkuId).HasColumnName("SKUId").ValueGeneratedNever();
        builder.Property(x => x.SkuName).HasColumnName("SKUName").IsRequired().HasMaxLength(200);

        builder.HasMany(x => x.SkuSubs).WithOne(x => x.Sku).HasForeignKey(x => x.SkuId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SkuName);
    }
}