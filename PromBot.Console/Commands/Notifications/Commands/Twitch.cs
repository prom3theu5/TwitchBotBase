using System.Threading.Tasks;
using PromBot.Commands;
using System;
using Serilog;


namespace PromBot.CommandModules.Notifications.Commands
{
    internal class Twitch : ChannelCommand
    {
        private Client _client;

        public Twitch(ChannelModule module) : base(module)
        { }

        internal override void Init(CommandGroupBuilder cgb)
        {
            _client = cgb.Service.Client;
            _client.TwitchClient.OnNewSubscriber += TwitchClient_OnNewSubscriber;
            _client.TwitchClient.OnReSubscriber += TwitchClient_OnReSubscriber;
        }

        private void TwitchClient_OnReSubscriber(object sender, TwitchLib.Events.Client.OnReSubscriberArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _client.SendMessage($"Thank you {e.ReSubscriber.DisplayName} for your continued support. You have suubscribed for {e.ReSubscriber.Months} consecutive Months.").ConfigureAwait(false);
                }
                catch (Exception ee)
                {
                    Log.Error("Error in On Resub {error}", ee.Message);
                }
            });
        }
        
        private void TwitchClient_OnNewSubscriber(object sender, TwitchLib.Events.Client.OnNewSubscriberArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _client.SendMessage($"Everyone welcome our newest channel subscriber: {e.Subscriber.DisplayName}. Thank you for your support!").ConfigureAwait(false);
                }
                
                catch (Exception ee)
                {
                    Log.Error("Error in New Sub {Error}", ee.Message);
                }
            });
        }
    }
}
