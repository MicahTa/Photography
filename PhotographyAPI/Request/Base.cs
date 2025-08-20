using System.Text.Json;
using System.Text.Json.Serialization;
namespace PhotographyAPI;

public class Base : IRequest
{
    [JsonPropertyName("action")]
    public string Action { get; set; }
}