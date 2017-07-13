using System;
using System.Threading.Tasks;
using PromBot.Commands;
using TwitchLib;
using System.Linq;

namespace PromBot.CommandModules.GeneralCommands.Commands
{
    internal class StreamInfo : ChannelCommand
    {
        public StreamInfo(ChannelModule module) : base(module)
        { }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "title")
                .Description("Display Current Stream information")
                .Do(DoStreamInfo());
        }

        private Func<CommandEventArgs, Task> DoStreamInfo() =>
            async e => {
                var streamInfo = await GetStreamInfo();
                if (streamInfo != null)
                    await e.Client.SendMessage(streamInfo).ConfigureAwait(false);
                else
                    await e.Client.SendMessage("Error retrieving stream info. :v").ConfigureAwait(false);
            };

        private async Task<string> GetStreamInfo()
        {
            var channel = await Cache.GetOrAddAsync("Broadcaster-Channel-Overview", async () => {
                var foundChannel = await TwitchAPI.Users.v5.GetUserByName(Bot.Channel);
                return foundChannel.Matches.FirstOrDefault();
            }, TimeSpan.FromMinutes(60));

            if (channel == null) return null;

            var channelDetails = await Cache.GetOrAddAsync($"Broadcaster-Channel-Details", async () => {
                return await TwitchAPI.Channels.v5.GetChannelByID(channel.Id);
            }, TimeSpan.FromMinutes(1));

            return channelDetails != null ? $".me says Current game being Played: {channelDetails.Game} - {channelDetails.Status}" : null;
        }
    }
}
