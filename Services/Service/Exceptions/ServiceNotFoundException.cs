namespace Services.Service.Exceptions
{
    public class ServiceNotFoundException : Exception
    {
        public string ServiceName { get; private set; }

        public ServiceNotFoundException(string name) : base()
            => ServiceName = name;

        public ServiceNotFoundException(string message, string name) : base(message)
            => ServiceName = name;

        public ServiceNotFoundException(string message, Exception innerException, string name) : base(message, innerException)
            => ServiceName = name;
    }
}
