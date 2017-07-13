using Prom3theu5.AppCache;
using PromBot.Commands;
using Serilog;
using TwitchLib;

namespace PromBot.CommandModules
{
    internal abstract class ChannelCommand
    {
        /// <summary>
        /// Parent module
        /// </summary>
        protected ChannelModule Module { get; }
        protected Client Client { get; }
        protected TwitchClient TwitchClient { get; }
        internal readonly ILogger Logger;
        internal readonly IAppCache Cache;

        /// <summary>
        /// Creates a new instance of twitch command,
        /// use ": base(module)" in the derived class'
        /// constructor to make sure module is assigned
        /// </summary>
        /// <param name="module">Module this command resides in</param>
        protected ChannelCommand(ChannelModule module)
        {
            Module = module;
            Logger = Bootstrapper.Logger;
            Cache = Bootstrapper.Cache;
            Client = module.Client;
            TwitchClient = Client.TwitchClient;
        }

        internal virtual void Init(CommandGroupBuilder cgb)
        { }
    }
}
