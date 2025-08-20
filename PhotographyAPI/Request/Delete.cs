using System.Text.Json;
using System.Text.Json.Serialization;
namespace PhotographyAPI;

public class Delete : IRequest
{
    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("keyOrPrefix")]
    public string KeyOrPrefix { get; set; }

    
    public Delete(string keyOrPrefix)
    {
        Action = "Delete";
        KeyOrPrefix = keyOrPrefix;
    }
}