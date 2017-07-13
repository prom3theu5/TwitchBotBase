using System;
using System.Linq;
using System.Diagnostics;
using TwitchLib.Events.Client;

namespace PromBot.CommandModules.Utilities.Commands
{
    internal class AdminStats : ChannelCommand
    {
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramCounter;

        public AdminStats(ChannelModule module) : base(module)
        {
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            TwitchClient.OnWhisperReceived += TwitchWhispers_OnWhisperReceived;
        }

        private void TwitchWhispers_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            if (!Bot.AdminUsers.Any(x => x.Equals(e.WhisperMessage.Username, StringComparison.InvariantCultureIgnoreCase))) return;

            if (e.WhisperMessage.Message.StartsWith("#serverstats", StringComparison.InvariantCultureIgnoreCase))
            {
                SendStats(e);
                return;
            }

            if (e.WhisperMessage.Message.StartsWith("#botsay", StringComparison.InvariantCultureIgnoreCase))
            {
                SayAsBot(e);
                return;
            }

            if (e.WhisperMessage.Message.StartsWith("#botuptime", StringComparison.InvariantCultureIgnoreCase))
            {
                SendUptime(e);
                return;
            }

            if (e.WhisperMessage.Message.StartsWith("#restartbot", StringComparison.InvariantCultureIgnoreCase))
            {
                DoRestartBot(e);
                return;
            }
        }

        #region End Program, Has to be running as a service, and it will auto restart.

        private async void DoRestartBot(OnWhisperReceivedArgs e)
        {
            await Client.SendMessage(Bot.Channel, $"Bot Restart Requested By: @{e.WhisperMessage.Username}. Restarting.").ConfigureAwait(false);
            Environment.Exit(1);
        }
        
        #endregion

        #region Send Uptime

        private async void SendUptime(OnWhisperReceivedArgs e)
        {
            var uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
            await Client.SendWhisper($"{Bot.BotNickname} Uptime: {uptime.Days} Days {uptime.Hours}:{uptime.Minutes}:{uptime.Seconds}", e.WhisperMessage.Username);
        }

        #endregion

        #region Send a Message as the Bot To The Channel

        private async void SayAsBot(OnWhisperReceivedArgs e)
        {
            var parts = e.WhisperMessage.Message.Split(' ');
            if (parts.Length < 2) return;
            var message = e.WhisperMessage.Message.Replace("#botsay", "").Trim();
            await Client.SendMessage(message).ConfigureAwait(false);
       }

        #endregion

        #region Send Server Stats

        private async void SendStats(OnWhisperReceivedArgs e)
        {
            var sender = e.WhisperMessage.Username;
            var ram = Convert.ToInt32(_ramCounter.NextValue()).ToString() + "Mb";
            var cpu = Convert.ToInt32(_cpuCounter.NextValue()).ToString() + "%";
            await Client.SendWhisper($"Current Server Stats: Cpu Usage: {cpu} - Free Memory: {ram}", sender).ConfigureAwait(false);
        }

        #endregion
    }
}
