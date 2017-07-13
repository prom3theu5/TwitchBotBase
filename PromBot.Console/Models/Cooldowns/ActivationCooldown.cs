using System;

namespace PromBot.Models.Cooldowns
{
    public class ActivationCooldown
    {
        private readonly int _seconds;
        private double _startTime;


        public ActivationCooldown(int seconds)
        {
            _seconds = seconds;
            _startTime = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public bool ActivateCooldown()
        {
            if ((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds - _startTime > _seconds)
            {
                _startTime = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
                return true;
            }
            return false;
        }
    }
}
