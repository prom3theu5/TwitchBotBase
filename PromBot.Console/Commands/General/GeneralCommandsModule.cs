using PromBot.Models;
using PromBot.Modules;

namespace PromBot.CommandModules.GeneralCommands
{
    internal class GeneralCommandsModule : ChannelModule
    {
        public GeneralCommandsModule(Client client): base(client)
        {
            commands.Add(new Commands.Caster(this));
            commands.Add(new Commands.StreamInfo(this));
            commands.Add(new Commands.Uptime(this));
            commands.Add(new Commands.PingCommand(this));
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

