using dZENcode.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace dZENcode.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Comment> Comments { get;}
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
