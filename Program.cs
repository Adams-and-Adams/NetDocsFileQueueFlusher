using NetDocsFileQueueFlusher;
using NetDocsFileQueueFlusher.FileLogging;
using NetDocsFileQueueFlusher.Helpers;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddTransient<IRestHelper, RestHelper>();
    })
    .ConfigureLogging((context, logging) => 
    {
        logging.AddFileLogger(options => 
        {
            context.Configuration.GetSection("Logging").GetSection("LoggerFile").GetSection("Options").Bind(options);
        });
    })
    .Build();

await host.RunAsync();
