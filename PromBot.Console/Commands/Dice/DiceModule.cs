using PromBot.Models;
using PromBot.Modules;

namespace PromBot.CommandModules.Dice
{
    internal class DiceModule : ChannelModule
    {
        public DiceModule(Client client) : base(client)
        {
            commands.Add(new Commands.DiceCommand(this));
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
