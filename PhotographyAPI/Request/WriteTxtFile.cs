using System.Text.Json;
using System.Text.Json.Serialization;
namespace PhotographyAPI;

public class WriteTxtFile : IRequest
{
    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("keyName")]
    public string KeyName { get; set; }
    [JsonPropertyName("fileContent")]
    public string FileContent { get; set; }

    
    public WriteTxtFile(string keyName, string fileContent)
    {
        Action = "WriteTxtFile";
        KeyName = keyName;
        FileContent = fileContent;
    }
}