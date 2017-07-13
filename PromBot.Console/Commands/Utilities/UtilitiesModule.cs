using PromBot.Models;
using PromBot.Modules;

namespace PromBot.CommandModules.Utilities
{
    internal class UtilitiesModule : ChannelModule
    {
        public UtilitiesModule()
        {
            commands.Add(new Commands.AdminStats(this));
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
