using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TwitterBot;

static void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        builder.AddDebug();
    });

    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false)
        .AddEnvironmentVariables()
        .Build();

    services.Configure<AppSettings>(configuration.GetSection("App"));

    services.AddSingleton<IBot, HorrorBot>();
    services.AddSingleton<IBot, PsychologyBot>();
    services.AddTransient<MyApp>();
}

var services = new ServiceCollection();
ConfigureServices(services);

using var serviceProvider = services.BuildServiceProvider();

await serviceProvider.GetService<MyApp>().Run(args);