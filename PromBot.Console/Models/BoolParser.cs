using System;
using System.Collections.Generic;
using System.Linq;

namespace PromBot.Models
{
    public static class BoolParser
    {
        public static bool GetValue(string value)
        {
            return IsTrue(value);
        }

        public static bool IsFalse(string value)
        {
            return !IsTrue(value);
        }

        public static bool CheckBoolValid(string word)
        {
            return ValidWords().Any(c => c.Equals(word, StringComparison.InvariantCultureIgnoreCase));
        }

        public static List<string> ValidWords()
        {
            return new List<string>() { "win","true","yes","false","lose","no" };
        }

        public static bool IsTrue(string value)
        {
            try
            {
                if (value == null)
                {
                    return false;
                }

                value = value.Trim();
                value = value.ToLower();

                if (value == "true")
                {
                    return true;
                }
                if (value == "yes")
                {
                    return true;
                }
                if (value == "win")
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
