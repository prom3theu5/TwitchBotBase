using System;

namespace PromBot.CommandModules.Protections.Models
{
   internal class Permit
    {
        private string _username;
        private DateTime _original;
        private int _totalUsages;
        private int _currentUsages = 0;

        public string Username { get { return _username; } }

        public Permit(string username, DateTime original, int totalUsages)
        {
            _username = username;
            _original = original;
            _totalUsages = totalUsages;
        }

        public void Update(DateTime original, int totalUsages)
        {
            _original = original;
            _totalUsages = totalUsages;
            _currentUsages = 0;
        }

        public bool UsePermit()
        {
            TimeSpan difference = DateTime.Now - _original;
            if (difference.Minutes <= 3 && _currentUsages < _totalUsages)
            {
                _currentUsages++;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
