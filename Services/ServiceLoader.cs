using Services.Exceptions;
using Services.Interfaces;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Services
{
    public class ServiceLoader
    {
        private readonly List<IService> _services = new();

        public ServiceLoader()
        {
#if DEBUG
            LoadDlls(@"bin\Debug\net6.0\");
#else
            LoadDlls("."); 
#endif

            _services = GetServices();
        }

        private List<IService> GetServices(List<IService>? services = null)
        {
            if (services is null)
                services = new();

            Type serviceType = typeof(IService);

            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(assemblyType => serviceType.IsAssignableFrom(assemblyType) && assemblyType.IsClass)
                .ToArray();

            foreach (var type in types)
            {
                IService? service = (IService?)Activator.CreateInstance(type);
                if (service is not null)
                    services.Add(service);
                Console.WriteLine($"Loaded Service {type.Name}.");
            }
            return services;
        }

        private void LoadDlls(string path)
        {
            if (Directory.Exists(path))
                Directory.GetFiles(path).ToList().ForEach(x =>
                {
                    if (x.EndsWith(".dll"))
                        Assembly.LoadFile(Path.GetFullPath(x));
                });
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public T ResolveService<T>() where T : IService
        {
            T? retVal = (T?)_services.SingleOrDefault(x => x.GetType().IsAssignableTo(typeof(T)));
            if (retVal is null)
                throw new ServiceNotFoundException(typeof(T).Name);
            return retVal;
        }
    }
}
