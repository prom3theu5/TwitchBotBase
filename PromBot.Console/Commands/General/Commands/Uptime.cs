using System;
using System.Threading.Tasks;
using PromBot.Commands;
using TwitchLib;
using System.Linq;

namespace PromBot.CommandModules.GeneralCommands.Commands
{
    internal class Uptime : ChannelCommand
    {
        public Uptime(ChannelModule module) : base(module)
        { }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "uptime")
                .Description("Show Stream Uptime")
                .Do(DoUptime());
        }

        private Func<CommandEventArgs, Task> DoUptime() =>
            async e => {

                var channel = await Cache.GetOrAddAsync("Broadcaster-Channel-Overview", async ()=> {
                    var foundChannel = await TwitchAPI.Users.v5.GetUserByName(Bot.Channel);
                    return foundChannel.Matches.FirstOrDefault();
                }, TimeSpan.FromMinutes(60));

                if (channel != null)
                {
                    var online = await TwitchAPI.Streams.v5.BroadcasterOnline(channel.Id);
                    if (!online)
                    {
                        await e.Client.SendMessage(".me says Streamer isn't streaming right now Ooops!").ConfigureAwait(false);
                        return;
                    }

                    var uptime = await TwitchAPI.Streams.v5.GetUptime(channel.Id);
                    if (!uptime.HasValue)
                    {
                        await e.Client.SendMessage(".me says Error getting uptime :v").ConfigureAwait(false);
                        return;
                    }
                    await e.Client.SendMessage($".me says Streamer Live for {uptime.Value.Hours} {(uptime.Value.Hours == 1 ? "hour" : "hours")} {uptime.Value.Minutes} {(uptime.Value.Minutes == 1 ? "minute" : "minutes")}.").ConfigureAwait(false);
                }
            };
    }
}
