using PromBot.Models;
using PromBot.Modules;

namespace PromBot.CommandModules.Protections
{
    internal class ProtectionsModule : ChannelModule
    {
        public ProtectionsModule(Client client) : base(client)
        {
            commands.Add(new Commands.Caps(this));
            commands.Add(new Commands.Links(this));
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

