using System.Reflection;
using Codium.Template.Domain.AuditLogs;
using Codium.Template.Domain.ConfirmationCodes;
using Codium.Template.Domain.EntityPropertyChanges;
using Codium.Template.Domain.HttpRequestLogs;
using Codium.Template.Domain.Permissions;
using Codium.Template.Domain.RefreshTokens;
using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.SnapshotAppSettings;
using Codium.Template.Domain.SnapshotAssemblies;
using Codium.Template.Domain.SnapshotLogs;
using Codium.Template.Domain.UserRoles;
using Codium.Template.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Contexts;

public class ApplicationDbContext : DbContext
{
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<EntityPropertyChange> EntityPropertyChanges { get; set; }
    public DbSet<HttpRequestLog> HttpRequestLogs { get; set; }
    public DbSet<SnapshotLog> SnapshotLogs { get; set; }
    public DbSet<SnapshotAssembly> SnapshotAssemblies { get; set; }
    public DbSet<SnapshotAppSetting> SnapshotAppSettings { get; set; }
    public DbSet<ConfirmationCode> ConfirmationCodes { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<User> Users { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}