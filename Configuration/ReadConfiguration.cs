namespace Configuration
{
    public class ReadConfiguration
    {
        public static Config Config
        {
            get
            {
                _config ??= CreateConfig();
                return _config;
            }
        }


        private static Config? _config;

        private static Config CreateConfig()
        {
            //if settings.env file is found existing envvars are overwritten
            //TODO: find a better way to do this...
            if (File.Exists("settings.env"))
                foreach (var line in File.ReadAllLines("settings.env"))
                    Environment.SetEnvironmentVariable(line[..line.IndexOf('=')], line[(line.IndexOf('=') + 1)..]);

            var callbackUrl = Environment.GetEnvironmentVariable("CALLBACKURL");
            var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING");

            if (callbackUrl is null || connectionString is null)
                throw new Exception("Something, somewhere went so incredibly wrong that I honestly fail to describe it...");

            return new(callbackUrl, connectionString);
        }
    }
}