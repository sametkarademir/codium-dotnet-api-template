using Codium.Template.Domain;
using Codium.Template.Domain.HttpRequestLogs;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class HttpRequestLogConfiguration : IEntityTypeConfiguration<HttpRequestLog>
{
    public void Configure(EntityTypeBuilder<HttpRequestLog> builder)
    {
        builder.ApplyGlobalEntityConfigurations();

        builder.ToTable(ApplicationConsts.DbTablePrefix + "HttpRequestLogs", ApplicationConsts.DbSchema);
        builder.HasIndex(item => item.HttpMethod);
        builder.HasIndex(item => item.RequestPath);
        builder.HasIndex(item => item.ClientIp);
        builder.HasIndex(item => item.DeviceFamily);
        builder.HasIndex(item => item.DeviceModel);
        builder.HasIndex(item => item.OsFamily);
        builder.HasIndex(item => item.OsVersion);
        builder.HasIndex(item => item.BrowserFamily);
        builder.HasIndex(item => item.BrowserVersion);
        builder.HasIndex(item => item.ControllerName);
        builder.HasIndex(item => item.ActionName);
        builder.HasIndex(item => item.CorrelationId);
        builder.HasIndex(item => item.SessionId);
        builder.HasIndex(item => item.SnapshotId);

        builder.Property(item => item.HttpMethod).HasMaxLength(16).IsRequired(false);
        builder.Property(item => item.RequestPath).HasMaxLength(20248).IsRequired(false);
        builder.Property(item => item.QueryString).IsRequired(false);
        builder.Property(item => item.RequestBody).IsRequired(false);
        builder.Property(item => item.RequestHeaders).IsRequired(false);

        builder.Property(item => item.StatusCode).IsRequired(false);

        builder.Property(item => item.RequestTime).IsRequired();
        builder.Property(item => item.ResponseTime).IsRequired();
        builder.Property(item => item.DurationMs).IsRequired(false);

        builder.Property(item => item.ClientIp).HasMaxLength(256).IsRequired(false);
        builder.Property(item => item.UserAgent).HasMaxLength(2048).IsRequired(false);

        builder.Property(item => item.DeviceFamily).HasMaxLength(128).IsRequired(false);
        builder.Property(item => item.DeviceModel).HasMaxLength(128).IsRequired(false);
        builder.Property(item => item.OsFamily).HasMaxLength(128).IsRequired(false);
        builder.Property(item => item.OsVersion).HasMaxLength(128).IsRequired(false);
        builder.Property(item => item.BrowserFamily).HasMaxLength(128).IsRequired(false);
        builder.Property(item => item.BrowserVersion).HasMaxLength(128).IsRequired(false);
        builder.Property(item => item.IsMobile).IsRequired();
        builder.Property(item => item.IsTablet).IsRequired();
        builder.Property(item => item.IsDesktop).IsRequired();

        builder.Property(item => item.ControllerName).HasMaxLength(1024).IsRequired(false);
        builder.Property(item => item.ActionName).HasMaxLength(1024).IsRequired(false);
    }
}