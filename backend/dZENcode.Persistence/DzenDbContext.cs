using System.Reflection;
using dZENcode.Application.Abstractions;
using dZENcode.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace dZENcode.Persistence;

public class DzenDbContext(DbContextOptions<DzenDbContext> options) : DbContext(options), IApplicationDbContext
{   
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
