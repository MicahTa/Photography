using System.Text.Json;
using System.Text.Json.Serialization;

namespace GetPreSignedURL;
public class Error
{
    [JsonPropertyName("successful")]
    public bool Successful { get; set; } = false;
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }
    public Error(string errorMessage)
    {
        Successful = false;
        ErrorMessage = errorMessage;
    }
}
public class Response
{
    public class Base
    {
        [JsonPropertyName("successful")]
        public bool Successful { get; set; } = true;
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = new List<string>();

        public Base(string message)
        {
            Successful = true;
            Message = message;
        }
        public Base() { }
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

    public class GetPreSignedURL : Base
    {
        public GetPreSignedURL(string message) : base(message) { }
        public GetPreSignedURL() : base() { }
        public new JsonElement Respond()
        {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
        [JsonPropertyName("URL")]
        public string URL { get; set; }
        public void SetUrl(string contents)
        {
            URL = contents;
        }
    }
    public static JsonElement CORS(JsonElement data, int code = 200)
    {

        var response = new Dictionary<string, object>
        {
            ["statusCode"] = 200,
            ["headers"] = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["Access-Control-Allow-Origin"] = "*",
                ["Access-Control-Allow-Headers"] = "Content-Type",
                ["Access-Control-Allow-Methods"] = "OPTIONS,GET,POST,PUT,DELETE"
            },
            ["body"] = data.GetRawText(),
            ["isBase64Encoded"] = false
        };

        string jsonString = JsonSerializer.Serialize(response);
        using var doc = JsonDocument.Parse(jsonString);
        return doc.RootElement.Clone();
    }
    public static JsonElement Error(string errorMessage)
    {
        var response = new Error(errorMessage);
        string jsonString = JsonSerializer.Serialize(response);
        using var doc = JsonDocument.Parse(jsonString);
        return doc.RootElement.Clone();
    }
}
