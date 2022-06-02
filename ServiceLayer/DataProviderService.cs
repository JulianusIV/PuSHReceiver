using Data.JSONObjects;
using Services;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ServiceLayer
{
    public class DataProviderService : IDataProviderService
    {
        public Data.JSONObjects.Data? Data { get; set; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Load()
        {
            bool exit = false;
            if (File.Exists("data.json"))
            {
                string dataJson = File.ReadAllText("data.json");
                var data = JsonSerializer.Deserialize<Data.JSONObjects.Data>(dataJson);
                if (data is null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Something went wrong while reading your file, check to make sure it is not empty!");
                    exit = true;
                }
                else
                    Data = data;
            }
            else
            {
                Data = new()
                {
                    Subs = new List<DataSub> { new DataSub() }
                };
                File.WriteAllText("data.json", JsonSerializer.Serialize(Data));
                Console.WriteLine("Created file data.json, please fill in proper values.");
                exit = true;
            }
            if (exit)
                Environment.Exit(0);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
            => File.WriteAllText("data.json", JsonSerializer.Serialize(Data, new JsonSerializerOptions() { WriteIndented = true }));
    }
}
