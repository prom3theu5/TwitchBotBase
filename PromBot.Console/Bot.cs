using Serilog;
using PromBot.Commands;
using PromBot.Modules;
using PromBot.CommandModules.GeneralCommands;
using PromBot.CommandModules.Utilities;
using PromBot.CommandModules.Notifications;
using PromBot.CommandModules.Protections;
using PromBot.CommandModules.Dice;
using System.Threading.Tasks;
using TwitchLib;

namespace PromBot
{
    public class Bot
    {
        public Client Client { get; private set; }

        public const string Channel = ""; // Twitch Channel Here
        public const string BotNickname = ""; // Bot Username Here

        // List of Admin Users used in example commands (Whisper Utilities).
        // Pull these from a Dependancy Injected Service in the Bootstrapper class, cache them - somewhere other than here.
        public static readonly string[] AdminUsers = { Channel, "swiftyspiffy", "prom3theu5" };

        private const string _clientId = ""; // Client Id
        private const string _accessToken = ""; // Access Token
        private string _pass = ""; // OAUTH Password
        private readonly ILogger _logger;

        public Bot()
        {
            _logger = Bootstrapper.Logger;
        }

        public async Task<bool> StartBot()
        {
            await Task.Run(StartConnection);
            return true;
        }

        private Task StartConnection()
        {
            TwitchAPI.Settings.ClientId = _clientId;
            TwitchAPI.Settings.AccessToken = _accessToken;

            return Task.Run(() =>
            {
                Client = new Client(BotNickname, _pass, Channel, _clientId);

                var commandService = new CommandService(new CommandServiceConfigBuilder
                {
                    CustomPrefixHandler = m => 0,
                    ErrorHandler = (s, e) =>
                    {
                        if (string.IsNullOrWhiteSpace(e.Exception?.Message))
                            return;
                        try
                        {
                            Client.TwitchClientMessageQueue.QueueSend(e.Exception.Message);
                            _logger.Error("Error in Command: {Error}", e.Exception.Message);
                        }
                        catch { }
                    },
                    ExecuteHandler = (s,e) =>
                    {
                        _logger.Information("Command Executed: {Command} - Args: {Args} - User: {UserId}:{User}", e.Command.Text, e.Args, e.Message.ChatMessage.UserId, e.Message.ChatMessage.Username);
                    }
                });

                //Start Command Service
                Client.AddService(commandService);
                var modules = Client.AddService(new ModuleService());

                //Add Each Command Module (Group of Commands)
                modules.Add(new GeneralCommandsModule(Client), "GeneralCommands");
                modules.Add(new UtilitiesModule(Client), "Utilities");
                modules.Add(new NotificationModule(Client), "Notifications");
                modules.Add(new ProtectionsModule(Client), "Protections");
                modules.Add(new DiceModule(Client), "Dice");

                Client.ExecuteAndWait(async () =>
                {
                    await Client.Connect();
                });
            });
        }

        public void StopBot()
        {
            Client.Disconnect();
        }

    }
}
