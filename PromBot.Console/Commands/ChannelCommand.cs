using PromBot.Commands;

namespace PromBot.CommandModules
{
    internal abstract class ChannelCommand
    {

        /// <summary>
        /// Parent module
        /// </summary>
        protected ChannelModule Module { get; }

        /// <summary>
        /// Creates a new instance of twitch command,
        /// use ": base(module)" in the derived class'
        /// constructor to make sure module is assigned
        /// </summary>
        /// <param name="module">Module this command resides in</param>
        protected ChannelCommand(ChannelModule module)
        {
            this.Module = module;
        }
        
        internal abstract void Init(CommandGroupBuilder cgb);
    }
}
