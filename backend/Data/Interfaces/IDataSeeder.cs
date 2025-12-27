namespace Data.Interfaces;

public interface IDataSeeder {
    Task SeedAsync(CancellationToken ct = default);
}