using NetDocsFileQueueFlusher;
using NetDocsFileQueueFlusher.Helpers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddTransient<IRestHelper, RestHelper>();
    })
    .Build();

await host.RunAsync();
