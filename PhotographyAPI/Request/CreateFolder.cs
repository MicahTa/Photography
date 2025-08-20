using System.Text.Json;
using System.Text.Json.Serialization;
namespace PhotographyAPI;

public class CreateFolder : IRequest
{
    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("folderKey")]
    public string FolderKey { get; set; }

    
    public CreateFolder(string folderKey)
    {
        Action = "CreateFolder";
        FolderKey = folderKey;
    }
}