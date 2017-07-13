using System;
using System.Threading.Tasks;
using PromBot.Commands;
using TwitchLib;
using Prom3theu5.AppCache;
using System.Linq;

namespace PromBot.CommandModules.GeneralCommands.Commands
{
    internal class Caster : ChannelCommand
    {
        private readonly IAppCache _cache;

        public Caster(ChannelModule module) : base(module)
        {
            _cache = Bootstrapper.Container.GetInstance<IAppCache>();
        }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "caster")
                   .Alias(Module.Prefix + "shoutout")
                   .Parameter("target", ParameterType.Required)
                   .Description("Shoutout a Caster - Go Give a Follow")
                   .Do(DoCaster());
        }

        private Func<CommandEventArgs, Task> DoCaster() =>
            async e =>
            {
                if (!e.IsAdmin) return;
                var streamer = e.GetArg("target");

                var channel = await _cache.GetOrAddAsync($"User-Overview:{streamer}", async () => {
                    var foundChannel = await TwitchAPI.Users.v5.GetUserByName(streamer);
                    return foundChannel.Matches.FirstOrDefault();
                }, TimeSpan.FromMinutes(60));

                if (channel == null)
                {
                    await e.Client.SendMessage($".me says You should give @{streamer.ToUpper()} a follow over at http://www.twitch.tv/{streamer}").ConfigureAwait(false);
                    return;
                }

                var channelDetails = await _cache.GetOrAddAsync($"Channel-Details:{streamer}", async () => {
                    return await TwitchAPI.Channels.v5.GetChannelByID(channel.Id);
                }, TimeSpan.FromMinutes(5));

                if (channelDetails != null)
                    await e.Client.SendMessage($".me says You should give @{streamer.ToUpper()} a follow over at http://www.twitch.tv/{streamer} - They were last playing: {channelDetails.Game}").ConfigureAwait(false);
                else
                    await e.Client.SendMessage($".me says You should give @{streamer.ToUpper()} a follow over at http://www.twitch.tv/{streamer}").ConfigureAwait(false);
            };
    }
}