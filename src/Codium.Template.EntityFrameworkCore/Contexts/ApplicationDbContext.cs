using System.Reflection;
using Codium.Template.Domain.AuditLogs;
using Codium.Template.Domain.EntityPropertyChanges;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Contexts;

public class ApplicationDbContext : DbContext
{
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<EntityPropertyChange> EntityPropertyChanges { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations from the current assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}