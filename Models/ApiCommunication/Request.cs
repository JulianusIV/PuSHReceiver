using System.Reflection.PortableExecutable;

namespace Models.ApiCommunication
{
    public class Request
    {
        public Dictionary<string, string> Headers { get; set; } = new();
        public Dictionary<string, string> QueryParameters { get; set; } = new();
        public string? Body { get; set; }

        public override string ToString()
        {
            var ret = "Headers:\n";
            foreach (var header in Headers)
                ret += $"{header.Key}: {header.Value}\n";
            ret += "Params:\n";
            foreach (var param in QueryParameters)
                ret += $"{param.Key}: {param.Value}\n";
            ret += $"Body:\n{Body}";
            return ret;
        }
    }
}
