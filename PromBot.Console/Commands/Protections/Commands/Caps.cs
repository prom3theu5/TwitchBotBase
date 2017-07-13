﻿using System;
using PromBot.Commands;
using Serilog;
using TwitchLib.Extensions.Client;
using TwitchLib.Models.Client;

namespace PromBot.CommandModules.Protections.Commands
{
    internal class Caps : ChannelCommand
    {
        private Client _client;
        private const int _messageLength = 15;
        private const int _capsPercentage = 80;

        public Caps(ChannelModule module) : base(module)
        { }
        
        internal override void Init(CommandGroupBuilder cgb)
        {
            _client = cgb.Service.Client;
            _client.TwitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;
        }

        private void TwitchClient_OnMessageReceived(object sender, TwitchLib.Events.Client.OnMessageReceivedArgs e)
        {
            ViolatesProtections(e.ChatMessage.Username, e.ChatMessage.IsSubscriber, e.ChatMessage.IsModerator, e.ChatMessage);
        }

        private void ViolatesProtections(string username, bool sub, bool mod, ChatMessage message)
        {
            try
            {
                if (mod) return;
                if (ViolateCapsProtection(message.Message))
                {
                    var reply = $"@{username} Please don't use so many caps.";
                    TimeoutUserExt.TimeoutUser(_client.TwitchClient, username, TimeSpan.FromSeconds(1), message: reply);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error in Caps Protection, Error Timing Out User: {user}, Message: {message}, Error: {error}", message.Username, message.Message, e.Message);
            }
        }

        private bool ViolateCapsProtection(string message)
        {
            if (message.Length >= _messageLength)
            {
                var totalChars = 0;
                var capsCount = 0;
                foreach (var character in message.ToCharArray())
                {
                    if (character == ' ') continue;

                    totalChars++;
                        if (char.IsUpper(character))
                            capsCount++;
                }
                if ((((double)capsCount / totalChars) * 100) > 80)
                    return true;
            }
            return false;
        }
    }
}

