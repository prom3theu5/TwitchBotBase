using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PromBot.Modules
{
	public class ModuleService : IService
	{
		public Client Client { get; private set; }

        private static readonly MethodInfo addMethod = typeof(ModuleService).GetTypeInfo().GetDeclaredMethods(nameof(Add)).SingleOrDefault(x => x.IsGenericMethodDefinition && x.GetParameters().Length == 3);

        public IEnumerable<ModuleManager> Modules => _modules.Values;
		private readonly Dictionary<Type, ModuleManager> _modules;

        public ModuleService()
		{
			_modules = new Dictionary<Type, ModuleManager>();
		}

		void IService.Install(Client client)
		{
            Client = client;
        }

        public void Add(IModule instance, string name)
        {
            Type type = instance.GetType();
            addMethod.MakeGenericMethod(type).Invoke(this, new object[] { instance, name });
        }

        public void Add<T>(string name)
            where T : class, IModule, new()
            => Add(new T(), name);

        public void Add<T>(T instance, string name)
			where T : class, IModule
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			if (Client == null)
                throw new InvalidOperationException("Service needs to be added to a TwitchClient before modules can be installed.");

            Type type = typeof(T);
            if (name == null) name = type.Name;
            if (_modules.ContainsKey(type))
                throw new InvalidOperationException("This module has already been added.");

			var manager = new ModuleManager<T>(Client, instance, name);
			_modules.Add(type, manager);
            instance.Install(manager);
        }

        public ModuleManager<T> Get<T>()
            where T : class, IModule
        {
            if (_modules.TryGetValue(typeof(T), out ModuleManager manager))
                return manager as ModuleManager<T>;
            return null;
        }
	}
}
