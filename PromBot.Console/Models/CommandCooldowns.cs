using System.Collections.Concurrent;

namespace PromBot.Models.Cooldowns
{
    internal class CommandCooldowns
    {
        public static CommandCooldowns Instance { get; } = new CommandCooldowns();

        private ConcurrentDictionary<string, CommandCooldown> _cooldowns { get; } = new ConcurrentDictionary<string, CommandCooldown>();
        private ConcurrentDictionary<string, CommandCooldown> _modCooldowns { get; } = new ConcurrentDictionary<string, CommandCooldown>();
        private ConcurrentDictionary<string, ActivationCooldown> _activations { get; } = new ConcurrentDictionary<string, ActivationCooldown>();
        private ConcurrentDictionary<string, UserCommandCooldowns> _userCooldowns { get; } = new ConcurrentDictionary<string, UserCommandCooldowns>();

        static CommandCooldowns() { }
        
        public bool ChatCommandAvailable(int seconds, string command)
        {
            CommandCooldown foundCommand = null;
            if (seconds != 0)
            {
                var cooldown = _cooldowns.TryGetValue(command, out foundCommand);
                if (foundCommand != null)
                    return foundCommand.Activate();

                _cooldowns.TryAdd(command, new CommandCooldown(command, seconds));
            }
            return true;
        }

        public bool ModCommandAvailable(int seconds, string command)
        {
            CommandCooldown foundCommand = null;
            if (seconds != 0)
            {
                var cooldown = _modCooldowns.TryGetValue(command, out foundCommand);
                if (foundCommand != null)
                    return foundCommand.Activate();

                _modCooldowns.TryAdd(command, new CommandCooldown(command, seconds));
            }
            return true;
        }

        public bool ActivationAvailable(int seconds, string commandType)
        {
            ActivationCooldown foundCooldown = null;
            if (seconds != 0)
            {
                var cooldown = _activations.TryGetValue(commandType, out foundCooldown);
                if (foundCooldown != null)
                    return foundCooldown.ActivateCooldown();

                _activations.TryAdd(commandType, new ActivationCooldown(seconds));
            }
            return true;
        }

        public bool UserCommandAvailable(string user, int usercooldown)
        {
            UserCommandCooldowns foundUserCooldown = null;
            if (usercooldown != 0)
            {
                var userCooldown = _userCooldowns.TryGetValue(user, out foundUserCooldown);
                if (foundUserCooldown != null)
                   return foundUserCooldown.ActivateUserCooldown();
                
                _userCooldowns.TryAdd(user, new UserCommandCooldowns(user, usercooldown));
            }
            return true;
        }

        public bool UserAndCommandCooldown(string user, string command, int seconds, int usercooldown)
        {
            return UserCommandAvailable(user, usercooldown) && ChatCommandAvailable(seconds, command);
        }
    }
}
