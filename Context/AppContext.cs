using System.Data.SqlClient;
using _1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace _1.Context;

public class AppLicationContext : DbContext
{
    private readonly string _connectionString = new SqlConnectionStringBuilder
    {
        DataSource = ".",
        InitialCatalog = "TestSetSelector",
        IntegratedSecurity = true,
        MultipleActiveResultSets = true,
        TrustServerCertificate = true
    }.ToString();

    public DbSet<Employee> Employees { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<Test1> Test1s { get; set; }
    public DbSet<Test2> Test2s { get; set; }
    public DbSet<UnboundClass> UnboundClasses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
    }
}