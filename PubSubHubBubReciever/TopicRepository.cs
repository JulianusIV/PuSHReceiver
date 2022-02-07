using PubSubHubBubReciever.JSONObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PubSubHubBubReciever
{
    public class TopicRepository
    {
        #region Singleton
        private static readonly object padlock = new object();
        private static TopicRepository _instance;
        public static TopicRepository Instance
        {
            get
            {
                lock (padlock)
                {
                    if (_instance is null)
                        _instance = new TopicRepository();
                    return _instance;
                }
            }
        }

        #endregion
        
        private readonly object _instanceLock = new object();

        public Data Data { get; private set; }
        public Leases Leases { get; private set; }

        public void Load()
        {
            lock (_instanceLock)
            {
                if (File.Exists("data.json"))
                {
                    string dataJson = File.ReadAllText("data.json");
                    Data = JsonSerializer.Deserialize<Data>(dataJson);
                }
                else
                {
                    Data = new Data
                    {
                        Subs = new List<DataSub> { new DataSub() }
                    };
                    File.WriteAllText("data.json", JsonSerializer.Serialize(Data));
                    Console.WriteLine("Created file data.json, please fill in proper values.");
                    Environment.Exit(0);
                }
                if (File.Exists("leases.json"))
                {
                    string leasesJson = File.ReadAllText("leases.json");
                    Leases = JsonSerializer.Deserialize<Leases>(leasesJson);
                }
                else
                {
                    Leases = new Leases()
                    {
                        Subs = new List<LeaseSub> { new LeaseSub() }
                    };
                    File.WriteAllText("leases.json", JsonSerializer.Serialize(Leases));
                    Console.WriteLine("Created file leases.json, please fill in proper values.");
                    Environment.Exit(0);
                }
            }
        }

        public void Save(FileNames fileName)
        {
            lock (_instanceLock)
            {
                object data;
                if (fileName == FileNames.data)
                    data = Data;
                else
                    data = Leases;

                File.WriteAllText(fileName.ToString() + ".json", JsonSerializer.Serialize(data));
            }
        }
    }

    public enum FileNames
    {
        data,
        leases
    }
}
