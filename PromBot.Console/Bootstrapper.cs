using Prom3theu5.AppCache;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using SimpleInjector;


namespace PromBot
{
    public static class Bootstrapper
    {
        public static Container Container;

        public static void Bootstrap()
        {
            Container = new Container();
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            // Logging
            Container.RegisterSingleton<ILogger>(logger);
            
            // Memory Cache
            Container.RegisterSingleton<IAppCache>(new CachingService() { DefaultCacheDuration = 60 });
            
            // or redis :)
            //container.RegisterSingleton<IAppCache>(new CachingService(RedisCache.Default) { DefaultCacheDuration = 60 });
            
            // Register your Currency Interfaces Here - I.e.
            // Container.RegisterSingleton<ICurrencyRepository, CurrencyRepository>();
            Container.Verify();
        }
    }
}
