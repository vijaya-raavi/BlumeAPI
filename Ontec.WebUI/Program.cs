using Serilog;
using Serilog.Events;

namespace Ontec.WebUI;
public class Program
{
    public async static Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();
        try
        {
            var host = CreateHostBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()).Build();  
            
            await host.RunAsync();

        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host termincated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }

    }

    public static IHostBuilder CreateHostBuilder(string[] args)=> 
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }); 
}