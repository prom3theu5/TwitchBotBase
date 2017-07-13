using TwitchLib.Models.Client;

namespace PromBot.Models
{
    public class Message
    { 
        public MessageState State { get; set; }
        internal int Nonce { get; set; }
        public string Channel { get; private set; }
        public string User { get; private set; }
        public string Target { get; private set; }
        public ChatMessage ChatMessage { get; private set; }
        public string TwitchMessage { get; private set; }

        public Message(string message, string user = Bot.BotNickname, string channel = Bot.Channel, ChatMessage chatmessage = null, string target = null)
        {
            TwitchMessage = message;
            User = user;
            Channel = channel;
            ChatMessage = chatmessage;
            Target = target;
        }
    }

    public enum MessageState : byte
    {
        /// <summary> Message did not originate from this session, or was successfully sent. </summary>
		Normal = 0,
        /// <summary> Message is current queued. </summary>
		Queued,
        /// <summary> Message was deleted before it was sent. </summary>
        Aborted,
        /// <summary> Message failed to be sent. </summary>
		Failed
    }
}
