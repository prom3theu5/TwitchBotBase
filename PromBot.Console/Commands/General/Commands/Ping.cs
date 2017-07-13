using System;
using System.Threading.Tasks;
using PromBot.Commands;

namespace PromBot.CommandModules.GeneralCommands.Commands
{
    internal class PingCommand : ChannelCommand
    {
        public PingCommand(ChannelModule module) : base(module)
        { }

        internal override void Init(CommandGroupBuilder builder)
        {
            builder.CreateCommand(Module.Prefix + "ping")
                .Description("Send back Server Time, and EST time")
                .Do(DoPing());
        }

        private Func<CommandEventArgs, Task> DoPing() =>
            async e => {
                if (e.IsAdmin)
                {
                    var now = DateTime.Now;
                    var easternStandard = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    var estTime = TimeZoneInfo.ConvertTime(now, TimeZoneInfo.Local, easternStandard);
                    var timeString = $".me Servertime: {now.ToString()} {TimeZoneInfo.Local.DisplayName}, EST: {estTime} {easternStandard.DisplayName}";
                    await e.Client.SendMessage(timeString).ConfigureAwait(false);
                }
            };
        
    }
}
