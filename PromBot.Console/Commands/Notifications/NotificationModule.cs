using PromBot.Models;
using PromBot.Modules;

namespace PromBot.CommandModules.Notifications
{
    internal class NotificationModule : ChannelModule
    {
        public NotificationModule()
        {
            commands.Add(new Commands.Twitch(this));
        }
        
        public override string Prefix { get; } = "!";

        public override void Install(ModuleManager manager)
        {
            manager.CreateCommands("", cgb =>
        {
            commands.ForEach(cmd => cmd.Init(cgb));
        });

        }
    }
}
