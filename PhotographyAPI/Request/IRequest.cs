using System.Text.Json;
using System.Text.Json.Serialization;
namespace PhotographyAPI;

interface IRequest
{
    [JsonPropertyName("action")]
    public string Action { get; set; }
}