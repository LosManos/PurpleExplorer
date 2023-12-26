using Configuration;
using Newtonsoft.Json;

namespace PurpleExplorer.Models;

public class ServiceBusConnectionString
{
    public bool UseManagedIdentity { get; set; }

    [JsonConverter(typeof(Base64JsonConverter))]
    public string ConnectionString { get; set; }

    public string Name { get; set; }
}