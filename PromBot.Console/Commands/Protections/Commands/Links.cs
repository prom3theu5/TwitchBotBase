using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PromBot.CommandModules.Protections.Models;
using PromBot.Commands;
using Serilog;
using TwitchLib.Extensions.Client;
using TwitchLib.Models.Client;

namespace PromBot.CommandModules.Protections.Commands
{
    internal class Links : ChannelCommand
    {
        private List<string> _topLevelDomains;
        private readonly List<Permit> _permits;
        private Client _client;

        public Links(ChannelModule module) : base(module)
        {
            _permits = new List<Permit>();
            _topLevelDomains = DownloadTopLevelDomains();
        }

        private List<string> DownloadTopLevelDomains()
        {
            try
            {
                using (var wb = new WebClient())
                {
                    var tlds = new List<string>();
                    var cnts = wb.DownloadString(new Uri("https://data.iana.org/TLD/tlds-alpha-by-domain.txt"));
                    var i = 0;
                    using (var reader = new StringReader(cnts))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (i != 0)
                                tlds.Add(line.ToLower());
                            i++;
                        }
                    }
                    return tlds;
                }
            }
            catch (Exception e)
            {
                Log.Error("Error in Link Protection, Loading Top Level Domains: {error}", e.Message);
                return null;
            }
        }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "permit")
               .Description("Permit viewer to Post Link")
               .Parameter("viewer", ParameterType.Required)
               .Parameter("number", ParameterType.Optional)
               .Do(DoPermit());

            _client = cgb.Service.Client;
            _client.TwitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;
        }

        private void TwitchClient_OnMessageReceived(object sender, TwitchLib.Events.Client.OnMessageReceivedArgs e)
        {
            if (_topLevelDomains != null)
                ViolatesProtections(e.ChatMessage.Username, e.ChatMessage.IsSubscriber, e.ChatMessage.IsModerator, e.ChatMessage);
        }

        private void ViolatesProtections(string username, bool sub, bool mod, ChatMessage message)
        {
            try
            {
                if (mod) return;
                string reply;
                if (!sub)
                {
                    if (ViolateLinkProtection(message.Message) && !LinkPermitExists(username))
                    {
                        reply = $"@{username} Please get permission before posting links.";
                        TimeoutUserExt.TimeoutUser(_client.TwitchClient, username, TimeSpan.FromSeconds(1), message: reply);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error in Link Protection, Error Timing Out User: {user}, Message: {message}, Error: {error}",message.Username, message.Message, e.Message);
            }
        }

        private bool LinkPermitExists(string username)
        {
            return _permits.Where(p => p.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase)).Select(p => p.UsePermit()).FirstOrDefault();
        }

        private bool ViolateLinkProtection(string message)
        {
            foreach (var word in message.Split(' '))
            {
                if (word.Contains('.'))
                {
                    var parts = word.Split('.');
                    if (parts[parts.Length - 1].Contains('/'))
                        if (_topLevelDomains.Contains(parts[parts.Length - 1].Split('/')[0]))
                            return true;
                        else
                        if (_topLevelDomains.Contains(parts[parts.Length - 1]) && parts[0].Length > 0)
                            return true;
                }
            }
            return false;
        }

        private Func<CommandEventArgs, Task> DoPermit() =>
            async (e) =>
            {
                if (e.IsAdmin)
                {
                    var target = e.GetArg("viewer");
                    var usages = 1;
                    var found = false;
                    try
                    {
                        if (!string.IsNullOrEmpty(e.GetArg("number")))
                            usages = int.Parse(e.GetArg("number"));

                        foreach (var item in _permits)
                        {
                            if (item.Username.Equals(target, StringComparison.InvariantCultureIgnoreCase))
                            {
                                found = true;
                                item.Update(DateTime.Now, usages);
                            }
                        }
                        if (!found)
                        {
                            _permits.Add(new Permit(target, DateTime.Now, usages));
                        }
                        await e.Client.SendMessage(usages == 1
                            ? $".me says You are permitted to post 1 link, @{target}"
                            : $".me says You are permitted to post {usages} links, @{target}").ConfigureAwait(false);
                    }
                    catch
                    {
                        Log.Error("Error in Link Protection, Adding Permit: {error}", e.Message);
                        await e.Client.SendMessage("Usage: !permit <user> <numberoflinks>").ConfigureAwait(false);
                    }
                }
            };
    }
}
