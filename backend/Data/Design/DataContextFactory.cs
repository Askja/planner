namespace Data.Design;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class DataContextFactory : IDesignTimeDbContextFactory<DataContext> {
    public DataContext CreateDbContext(string[] args) {
        DbContextOptionsBuilder<DataContext> optionsBuilder = new();

        optionsBuilder.UseInMemoryDatabase("DesignTimeDb");

        return new(optionsBuilder.Options);
    }
}