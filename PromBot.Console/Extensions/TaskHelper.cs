using System;
using System.Threading.Tasks;

namespace PromBot
{
    internal static class TaskHelper
    {
        public static Task CompletedTask => Task.Delay(0);

        public static Func<Task> ToAsync(Action action)
        {
            return () =>
            {
                action(); return CompletedTask;
            };
        }
        public static Func<T, Task> ToAsync<T>(Action<T> action)
        {
            return x =>
            {
                action(x); return CompletedTask;
            };
        }
    }
}