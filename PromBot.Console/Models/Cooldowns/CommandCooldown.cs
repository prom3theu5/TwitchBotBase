using System;

namespace PromBot.Models.Cooldowns
{
    public class CommandCooldown
    {
        private string _command;
        private int _seconds;
        private double _startTime;

        public string Command { get { return _command; } }

        public CommandCooldown(string command, int seconds)
        {
            _command = command;
            _seconds = seconds;
            _startTime = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public bool Activate()
        {
            if ((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds - _startTime > _seconds)
            {
                _startTime = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
