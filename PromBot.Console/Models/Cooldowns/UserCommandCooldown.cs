using System;

namespace PromBot.Models.Cooldowns
{
    public class UserCommandCooldowns
    {
        public string User { get; private set; }
        private readonly int _userCd;
        private double _startTime;

        public string Command { get; private set; }

        public UserCommandCooldowns(string user, int userCd)
        {
            User = user;
            _userCd = userCd;
            _startTime = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public bool ActivateUserCooldown()
        {
            if ((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds - _startTime > _userCd)
            {
                _startTime = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
                return true;
            }
            return false;
        }
    }
}
