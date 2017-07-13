using Serilog;
using Topshelf;

namespace PromBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Setup();
            Log.Logger = Bootstrapper.Container.GetInstance<ILogger>();
            
            HostFactory.Run(x =>
            {
                x.Service<Bot>(bot =>
                {
                    bot.ConstructUsing(() => new Bot());
                    bot.WhenStarted(async b => await b.StartBot());
                    bot.WhenStopped(b => b.StopBot());
                });
                x.StartAutomatically();
                x.RunAsPrompt();
                x.UseSerilog();
                x.SetServiceName("PromB0t");
                x.SetDescription("Prom3theu5 Example Twitch Bot using Command Module system, and TwitchLib by SwiftySpiffy.");
                x.SetInstanceName("PromB0t");
                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(0);
                    r.RestartService(0);
                    r.RestartService(0);
                });
            });
        }
    }
}
