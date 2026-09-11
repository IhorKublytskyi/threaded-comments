using System.Reflection;
using dZENcode.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace dZENcode.Persistence;

public class DzenDbContext(DbContextOptions<DzenDbContext> options) : DbContext(options)
{   
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
