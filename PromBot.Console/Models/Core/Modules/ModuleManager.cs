using PromBot.Commands;
using Nito.AsyncEx;
using System;

namespace PromBot.Modules
{
    public class ModuleManager<T> : ModuleManager
        where T : class, IModule
    {
        public new T Instance => base.Instance as T;

        internal ModuleManager(Client client, T instance, string name)
            : base(client, instance, name)
        {
        }
    }

    public class ModuleManager
	{
        private readonly AsyncLock _lock;

        public Client Client { get; }
        public IModule Instance { get; }
        public string Name { get; }
		public string Id { get; }
        
		internal ModuleManager(Client client, IModule instance, string name)
		{
            Client = client;
            Instance = instance;
            Name = name;

            Id = name.ToLowerInvariant();
            _lock = new AsyncLock();
		}

		public void CreateCommands(string prefix, Action<CommandGroupBuilder> config)
		{
			var commandService = Client.GetService<CommandService>();
			commandService.CreateGroup(prefix, x =>
			{
				x.Category(Name);
				config(x);
            });

		}
	}
}
