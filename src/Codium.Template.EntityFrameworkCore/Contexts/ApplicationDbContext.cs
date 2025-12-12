using System.Reflection;
using Codium.Template.Domain.AuditLogs;
using Codium.Template.Domain.EntityPropertyChanges;
using Codium.Template.Domain.HttpRequestLogs;
using Codium.Template.Domain.SnapshotAppSettings;
using Codium.Template.Domain.SnapshotLogs;
using Codium.Template.EntityFrameworkCore.EntityConfigurations;
using Microsoft.EntityFrameworkCore;


namespace Codium.Template.EntityFrameworkCore.Contexts;

public class ApplicationDbContext : DbContext
{
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<EntityPropertyChange> EntityPropertyChanges { get; set; }
    public DbSet<HttpRequestLog> HttpRequestLogs { get; set; }
    public DbSet<SnapshotLog> SnapshotLogs { get; set; }
    public DbSet<SnapshotAssemblyConfiguration> SnapshotAssemblyConfigurations { get; set; }
    public DbSet<SnapshotAppSetting> SnapshotAppSettings { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}