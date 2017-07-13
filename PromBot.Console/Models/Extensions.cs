using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace PromBot.Models
{
    public static class Extensions
    {
        private static Random rng = new Random();

        public static string Scramble(this string word)
        {

            var letters = word.ToArray();
            var count = 0;
            for (var i = 0; i < letters.Length; i++)
            {
                if (letters[i] == ' ')
                    continue;

                count++;
                if (count <= letters.Length / 5)
                    continue;

                if (count % 3 == 0)
                    continue;

                if (letters[i] != ' ')
                    letters[i] = '_';
            }
            return "`" + string.Join(" ", letters) + "`";
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source)
            {
                action(element);
            }
        }

        public static int LevenshteinDistance(this string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (var i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (var j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (var i = 1; i <= n; i++)
            {
                //Step 4
                for (var j = 1; j <= m; j++)
                {
                    // Step 5
                    var cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public static int KiB(this int value) => value * 1024;
        public static int KB(this int value) => value * 1000;

        public static int MiB(this int value) => value.KiB() * 1024;
        public static int MB(this int value) => value.KB() * 1000;

        public static int GiB(this int value) => value.MiB() * 1024;
        public static int GB(this int value) => value.MB() * 1000;

        public static string Matrix(this string s) => string.Join("", s.Select(c => c.ToString() + " ̵̢̬̜͉̞̭̖̰͋̉̎ͬ̔̇̌̀".TrimTo(rng.Next(0, 12), true)));

        public static void Shuffle<T>(this IList<T> list)
        {
            var provider = new RNGCryptoServiceProvider();
            var n = list.Count;
            while (n > 1)
            {
                var box = new byte[(n / Byte.MaxValue) + 1];
                int boxSum;
                do
                {
                    provider.GetBytes(box);
                    boxSum = box.Sum(b => b);
                }
                while (!(boxSum < n * ((Byte.MaxValue * box.Length) / n)));
                var k = (boxSum % n);
                n--;
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static string TrimTo(this string str, int num, bool hideDots = false)
        {
            if (num < 0)
                throw new ArgumentOutOfRangeException(nameof(num), "TrimTo argument cannot be less than 0");
            if (num == 0)
                return string.Empty;
            if (num <= 3)
                return string.Join("", str.Select(c => '.'));
            if (str.Length < num)
                return str;
            return string.Join("", str.Take(num - 3)) + (hideDots ? "" : "...");
        }
    }
}
