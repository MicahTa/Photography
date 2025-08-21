using System.Text.Json;
using System.Text.Json.Serialization;
namespace PhotographyAPI;

interface IResponse
{
    [JsonPropertyName("successful")]
    public bool Successful { get; set; }
}