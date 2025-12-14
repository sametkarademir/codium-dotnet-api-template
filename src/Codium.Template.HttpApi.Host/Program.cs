using System.Text.Json;
using System.Text.Json.Serialization;
using Codium.Template.Application;
using Codium.Template.EntityFrameworkCore;
using Codium.Template.HttpApi.Attributes;
using Codium.Template.HttpApi.Client;
using Codium.Template.HttpApi.Host.Logging;
using Codium.Template.HttpApi.Host.Middlewares;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationActionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

builder.Services.AddEntityFrameworkCoreService(builder.Configuration);
builder.Services.AddApplicationService(builder.Configuration);
builder.Services.AddHttpApiClientExtensions();

builder.AddSerilogLogging(builder.Configuration);

var app = builder.Build();

app.UseRegisteredMiddlewares();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors();
app.UseRequestLocalization();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseHangfireDashboard(builder.Configuration["Hangfire:Url"], new DashboardOptions
{
    Authorization =
    [
        new HangfireCustomBasicAuthenticationFilter
        {
            User = builder.Configuration["Hangfire:Username"],
            Pass = builder.Configuration["Hangfire:Password"]
        }
    ],
    DisplayStorageConnectionString = false,
    DashboardTitle = builder.Configuration["Hangfire:Title"],
    DarkModeEnabled = true
});

app.Run();