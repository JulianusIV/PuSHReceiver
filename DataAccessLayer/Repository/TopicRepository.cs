using DataLayer.JSONObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DataAccessLayer.Repository
{
    public static class TopicRepository
    {
        private static readonly object _lock = new object();

        public static Data Data { get; set; }
        public static Leases Leases { get; set; }

        public static void Load()
        {
            lock (_lock)
            {
                bool exit = false;
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
                    exit = true;
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
                    File.WriteAllText("leases.json", JsonSerializer.Serialize(Leases, new JsonSerializerOptions() { WriteIndented = true }));
                    Console.WriteLine("Created file leases.json, please fill in proper values.");
                    exit = true;
                }
                if (exit)
                    Environment.Exit(0);
            }
        }

        public static void Save(FileNames fileName)
        {
            lock (_lock)
            {
                object data;
                if (fileName == FileNames.data)
                    data = Data;
                else
                    data = Leases;

                File.WriteAllText(fileName.ToString() + ".json", JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true }));
            }
        }
    }

    public enum FileNames
    {
        data,
        leases
    }
}
