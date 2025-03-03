using Microsoft.EntityFrameworkCore;
using LoanValuationSystem.Models;
using Microsoft.Extensions.Configuration;

public class LoanDbContext : DbContext
{
    public DbSet<Loan> Loans { get; set; }
    public DbSet<LoanPerformance> LoanPerformance { get; set; }

    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        options.UseSqlServer(configuration.GetConnectionString("AzureSQL"));
    }
}