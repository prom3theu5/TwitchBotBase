namespace PromBot.Modules
{
    public static class ModuleExtensions
    {
        public static Client UsingModules(this Client client)
        {
            client.AddService(new ModuleService());
            return client;
        }

        public static void AddModule(this Client client, IModule instance, string name = null)
        {
            client.GetService<ModuleService>().Add(instance, name);
        }
        public static void AddModule<T>(this Client client, string name = null)
            where T : class, IModule, new()
        {
            client.GetService<ModuleService>().Add<T>(name);
        }
        public static void AddModule<T>(this Client client, T instance, string name = null)
            where T : class, IModule
        {
            client.GetService<ModuleService>().Add(instance, name);
        }
        public static ModuleManager<T> GetModule<T>(this Client client)
            where T : class, IModule
            => client.GetService<ModuleService>().Get<T>();
    }
}
