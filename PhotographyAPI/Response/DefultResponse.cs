using System.Text.Json;
using System.Text.Json.Serialization;
namespace PhotographyAPI;


public class DefultResponse
{
    [JsonPropertyName("successful")]
    public bool Successful { get; set; } = true;
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("warnings")]
    public new List<string> Warnings { get; set; } = new List<string>();

    public DefultResponse(string message)
    {
        Successful = true;
        Message = message;
    }
    public DefultResponse() { }
    public JsonElement Respond()
    {
        var response = this;
        string jsonString = JsonSerializer.Serialize(response);
        using var doc = JsonDocument.Parse(jsonString);
        return doc.RootElement.Clone();
    }
    /*public JsonElement Respond(object jsonKey)
    {
        var response = jsonKey;
        string jsonString = JsonSerializer.Serialize(response);
        using var doc = JsonDocument.Parse(jsonString);
        return doc.RootElement.Clone();
    }*/
    public void SetMessage(string message)
    {
        Message = message;
    }
    public void AddWarning(string message)
    {
        Warnings.Append(message);
    }
}