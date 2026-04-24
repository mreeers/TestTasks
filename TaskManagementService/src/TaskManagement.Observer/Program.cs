using MassTransit;
using Serilog;
using TaskManagement.Observer.Infrastructure;
using TaskManagement.Observer.Consumers;

var builder = WebApplication.CreateBuilder(args);
var observerLogDirectory = Path.Combine("logs", DateTime.Now.ToString("dd-MM-yyyy"));
Directory.CreateDirectory(observerLogDirectory);
var observerLogFilePath = Path.Combine(observerLogDirectory, "observer.log");
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.File(
            observerLogFilePath,
            shared: true);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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
        configurator.AddConsumer<TaskChangedEventConsumer>();

        configurator.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(new Uri($"rabbitmq://{rabbitHost}:{rabbitPort}/"), host =>
            {
                host.Username(rabbitUsername);
                host.Password(rabbitPassword);
            });

            cfg.ReceiveEndpoint("task-changed-events", endpoint =>
            {
                endpoint.ConfigureConsumer<TaskChangedEventConsumer>(context);
            });
        });
    });
}
else
{
    Log.Warning(
        $"[Warning] RabbitMQ ({rabbitHost}:{rabbitPort}) is unavailable. Running observer in HTTP-only mode.");
}

var app = builder.Build();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation(
        """
        ==================== Локальный запуск ====================
        Сервис: TaskManagement.Observer
        Observer: {ObserverUrls}
        API Swagger: {ApiSwaggerUrl}
        RabbitMQ UI: {RabbitMqUiUrl}
        =========================================================================
        """,
        string.Join(", ", app.Urls),
        "http://localhost:5000/swagger",
        rabbitMqUiUrl);
});

app.Run();
