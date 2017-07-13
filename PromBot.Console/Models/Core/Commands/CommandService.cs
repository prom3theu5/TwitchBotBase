using PromBot.Models;
using Serilog;
using System;
using System.Collections.Generic;

namespace PromBot.Commands
{
	public partial class CommandService : IService
    {
        private readonly List<Command> _allCommands;
        private readonly Dictionary<string, CommandMap> _categories;
        private readonly CommandMap _map; //Command map stores all commands by their input text, used for fast resolving and parsing

        public CommandServiceConfig Config { get; }
        public CommandGroupBuilder Root { get; }
        public Client Client { get; private set; }

		//AllCommands store a flattened collection of all commands
		public IEnumerable<Command> AllCommands => _allCommands;		
		//Groups store all commands by their module, used for more informative help
		internal IEnumerable<CommandMap> Categories => _categories.Values;

        public event EventHandler<CommandEventArgs> CommandExecuted = delegate { };
        public event EventHandler<CommandErrorEventArgs> CommandErrored = delegate { };

        private void OnCommand(CommandEventArgs args)
            => CommandExecuted(this, args);
        private void OnCommandError(CommandErrorType errorType, CommandEventArgs args, Exception ex = null)
            => CommandErrored(this, new CommandErrorEventArgs(errorType, args, Client, ex));
        
        public CommandService(CommandServiceConfigBuilder builder)
            : this(builder.Build())
        {
            if (builder.ExecuteHandler != null)
            {
                CommandExecuted += builder.ExecuteHandler;
            }
            if (builder.ErrorHandler != null)
                CommandErrored += builder.ErrorHandler;
        }

        public CommandService(CommandServiceConfig config)
		{
            Config = config;

			_allCommands = new List<Command>();
			_map = new CommandMap();
			_categories = new Dictionary<string, CommandMap>();
            Root = new CommandGroupBuilder(this);
		}

		void IService.Install(Client client)
		{
            Client = client;

            client.TwitchClient.OnMessageReceived += async (s, e) =>
            {
                if (_allCommands.Count == 0)  return;

                var msg = new Message(e.ChatMessage.Message, e.ChatMessage.Username, e.ChatMessage.Channel, e.ChatMessage);
                if (msg.TwitchMessage.Length == 0) return;

                string cmdMsg = null;

                //Check for command char
                if (Config.PrefixChar.HasValue)
                {
                    if (msg.TwitchMessage[0] == Config.PrefixChar.Value)
                        cmdMsg = msg.TwitchMessage.Substring(1);
                }

                //Check using custom activator
                if (cmdMsg == null && Config.CustomPrefixHandler != null)
                {
                    int index = Config.CustomPrefixHandler(msg);
                    if (index >= 0)
                        cmdMsg = msg.TwitchMessage.Substring(index);
                }
                
                if (cmdMsg == null) return;

                //Parse command
                CommandParser.ParseCommand(cmdMsg, _map, out IEnumerable<Command> commands, out int argPos);
                if (commands == null)
				{
					CommandEventArgs errorArgs = new CommandEventArgs(msg, null, Client, null);
					OnCommandError(CommandErrorType.UnknownCommand, errorArgs);
					return;
				}
				else
				{
					foreach (var command in commands)
					{
                        //Parse arguments
                        var error = CommandParser.ParseArgs(cmdMsg, argPos, command, out string[] args);
                        if (error != null)
						{
							if (error == CommandErrorType.BadArgCount)
								continue;
							else
							{
								var errorArgs = new CommandEventArgs(msg, command, Client, null);
								OnCommandError(error.Value, errorArgs);
								return;
							}
						}

						var eventArgs = new CommandEventArgs(msg, command, Client, args);
                        
						// Run the command
						try
						{
							OnCommand(eventArgs);
							await command.Run(eventArgs).ConfigureAwait(false);
						}
						catch (Exception ex)
						{
							OnCommandError(CommandErrorType.Exception, eventArgs, ex);
						}
						return;
					}
					var errorArgs2 = new CommandEventArgs(msg, null, Client, null);
					OnCommandError(CommandErrorType.BadArgCount, errorArgs2);
				}
            };
        }
        
		public void CreateGroup(string cmd, Action<CommandGroupBuilder> config = null) => Root.CreateGroup(cmd, config);
		public CommandBuilder CreateCommand(string cmd) => Root.CreateCommand(cmd);

		internal void AddCommand(Command command)
		{
			_allCommands.Add(command);

            //Get category
            string categoryName = command.Category ?? "";
            if (!_categories.TryGetValue(categoryName, out CommandMap category))
			{
				category = new CommandMap();
				_categories.Add(categoryName, category);
			}

			//Add main command
			category.AddCommand(command.Text, command, false);
            _map.AddCommand(command.Text, command, false);

			//Add aliases
			foreach (var alias in command.Aliases)
			{
				category.AddCommand(alias, command, true);
				_map.AddCommand(alias, command, true);
			}
		}

        internal void RemoveCommand(Command command)
        {
            try
            {
                _allCommands.Remove(command);

                var cat = _categories[command.Category];
                cat.Items.Remove(command.Text.ToLower());

                foreach (var item in command.Aliases)
                {
                    cat.Items.Remove(item.ToLower());
                    _map.Items.Remove(item.ToLower());
                }

                _map.Items.Remove(command.Text.ToLower());
            }
            catch (Exception ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
            }
        }
      
    }
}
