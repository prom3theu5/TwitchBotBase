using System.Threading.Tasks;
using System;

namespace PromBot.CommandModules.Notifications.Commands
{
    internal class Twitch : ChannelCommand
    {
        public Twitch(ChannelModule module) : base(module)
        {
            TwitchClient.OnNewSubscriber += TwitchClient_OnNewSubscriber;
            TwitchClient.OnReSubscriber += TwitchClient_OnReSubscriber;
        }

        private void TwitchClient_OnReSubscriber(object sender, TwitchLib.Events.Client.OnReSubscriberArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Client.SendMessage($"Thank you {e.ReSubscriber.DisplayName} for your continued support. You have suubscribed for {e.ReSubscriber.Months} consecutive Months.").ConfigureAwait(false);
                }
                catch (Exception ee)
                {
                    Logger.Error("Error in On Resub {error}", ee.Message);
                }
            });
        }
        
        private void TwitchClient_OnNewSubscriber(object sender, TwitchLib.Events.Client.OnNewSubscriberArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Client.SendMessage($"Everyone welcome our newest channel subscriber: {e.Subscriber.DisplayName}. Thank you for your support!").ConfigureAwait(false);
                }
                
                catch (Exception ee)
                {
                    Logger.Error("Error in New Sub {Error}", ee.Message);
                }
            });
        }
    }
}
