using PromBot.Modules;
using System.Collections.Generic;

namespace PromBot.CommandModules
{
    internal abstract class ChannelModule : IModule
    {
        protected readonly HashSet<ChannelCommand> commands = new HashSet<ChannelCommand>();

        public abstract string Prefix { get; }

        public abstract void Install(ModuleManager manager);

        public readonly Client Client;

        public ChannelModule(Client client)
        {
            Client = client;
        }
    }
}
