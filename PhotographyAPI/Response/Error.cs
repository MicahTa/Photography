using System.Text.Json;
using System.Text.Json.Serialization;
namespace PhotographyAPI;

public class Error : IResponse
{
    [JsonPropertyName("successful")]
    public bool Successful { get; set; } = false;
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }
    public Error(string errorMessage) {
        Successful = false;
        ErrorMessage = errorMessage;
    }
}