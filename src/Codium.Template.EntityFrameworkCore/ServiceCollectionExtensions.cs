using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Extensions;
using Codium.Template.EntityFrameworkCore.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Codium.Template.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkCoreService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<NpgsqlDataSource>(opt =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Default"));
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();
            
            return dataSource;
        });

        services.AddDbContext<ApplicationDbContext>((serviceProvider, opt) =>
        {
            var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
            opt.UseNpgsql(dataSource, builder =>
            {
                builder.CommandTimeout(30);
            });
            opt.UseEntityMetadataTracking();
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHostedService<DbMigrationInitializer>();

        return services;
    }
}