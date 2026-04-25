using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using TaskManagement.API.Infrastructure;
using TaskManagement.API.Services;
using TaskManagement.API.Validators;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

// Регистрация сервисов в контейнере DI.

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskEventNotifier, TaskEventNotifier>();

builder.Services.AddHttpClient("ObserverClient", client =>
{
    var baseUrl = builder.Configuration["Observer:BaseUrl"] ?? "http://localhost:5080";
    client.BaseAddress = new Uri(baseUrl);
});

var otlpEndpoint = builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317";
builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracingBuilder =>
    {
        tracingBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("TaskManagement.API"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddSource("MassTransit")
            .AddOtlpExporter(options => { options.Endpoint = new Uri(otlpEndpoint); });
    });

var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitPort = int.TryParse(builder.Configuration["RabbitMq:Port"], out var parsedRabbitPort)
    ? parsedRabbitPort
    : 5672;
var rabbitUsername = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitPassword = builder.Configuration["RabbitMq:Password"] ?? "guest";
var rabbitMqUiUrl = $"http://{rabbitHost}:15672/";

var rabbitMqAvailable = await RabbitMqAvailabilityProbe.IsReachableAsync(rabbitHost, rabbitPort);

if (rabbitMqAvailable)
{
    builder.Services.AddMassTransit(configurator =>
    {
        configurator.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(new Uri($"rabbitmq://{rabbitHost}:{rabbitPort}/"), host =>
            {
                host.Username(rabbitUsername);
                host.Password(rabbitPassword);
            });

            cfg.ConfigureEndpoints(context);
        });
    });

    builder.Services.AddScoped<ITaskEventPublisher, MassTransitTaskEventPublisher>();
}
else
{
    Log.Warning(
        $"[Warning] RabbitMQ ({rabbitHost}:{rabbitPort}) is unavailable. Running without async messaging.");
    builder.Services.AddScoped<ITaskEventPublisher, NullTaskEventPublisher>();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
    dbContext.Database.Migrate();
}

// Настройка конвейера HTTP-запросов.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskManagement API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation(
        """
        ==================== Локальный запуск ====================
        Сервис: TaskManagement.API
        API: {ApiUrls}
        Swagger: {SwaggerUrls}
        Observer: {ObserverUrl}
        RabbitMQ UI: {RabbitMqUiUrl}
        =========================================================================
        """,
        string.Join(", ", app.Urls),
        string.Join(", ", app.Urls.Select(url => $"{url}/swagger")),
        "http://localhost:5080",
        rabbitMqUiUrl);
});

app.Run();
