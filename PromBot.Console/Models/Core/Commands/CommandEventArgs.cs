using PromBot.Models;
using System.Linq;
using System;

namespace PromBot.Commands
{
    public class CommandEventArgs : EventArgs
    {
        private readonly string[] _args;

        public Message Message { get; }
        public Command Command { get; }
        public Client Client { get; private set; }

        public CommandEventArgs(Message message, Command command, Client client, string[] args)
        {
            Message = message;
            Command = command;
            Client = client;
            _args = args;
            if (command == null) return;
        }

        public bool IsAdmin {
            get
            {
                if (Message.ChatMessage == null) return false;
                else
                    return Message.ChatMessage.Badges.Any(b => b.Key.Equals("broadcaster", StringComparison.InvariantCultureIgnoreCase)) 
                        || Message.ChatMessage.IsModerator;
            }
        }
        
        public string[] Args => _args;
        public string GetArg(int index) => _args[index];
        public string GetArg(string name) => _args[Command[name].Id];
    }
}
