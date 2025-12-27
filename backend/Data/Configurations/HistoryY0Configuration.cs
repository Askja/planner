namespace Data.Configurations;

using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class HistoryY0Configuration : IEntityTypeConfiguration<HistoryY0Entity> {
    public void Configure(EntityTypeBuilder<HistoryY0Entity> builder) {
        builder.ToTable("TableHistoryY0");

        builder.HasKey(x => x.SkuSubId);

        builder.Property(x => x.SkuSubId).HasColumnName("SKUSubId").ValueGeneratedNever();
        builder.Property(x => x.Units).HasColumnName("Units").HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("Amount").HasPrecision(18, 6).IsRequired();

        builder.HasOne(x => x.SkuSub).WithOne(x => x.HistoryY0).HasForeignKey<HistoryY0Entity>(x => x.SkuSubId).OnDelete(DeleteBehavior.Cascade);
    }
}