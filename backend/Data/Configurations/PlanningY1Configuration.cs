namespace Data.Configurations;

using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class PlanningY1Configuration : IEntityTypeConfiguration<PlanningY1Entity> {
    public void Configure(EntityTypeBuilder<PlanningY1Entity> builder) {
        builder.ToTable("TablePlanningY1");

        builder.HasKey(x => x.SkuSubId);

        builder.Property(x => x.SkuSubId).HasColumnName("SKUSubId").ValueGeneratedNever();
        builder.Property(x => x.Units).HasColumnName("Units").HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("Amount").HasPrecision(18, 6).IsRequired();

        builder.HasOne(x => x.SkuSub).WithOne(x => x.PlanningY1).HasForeignKey<PlanningY1Entity>(x => x.SkuSubId).OnDelete(DeleteBehavior.Cascade);
    }
}