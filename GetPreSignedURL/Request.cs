using System.Text.Json;
using System.Text.Json.Serialization;


namespace GetPreSignedURL;
public class Request
{
    static public bool Test(params string[] args)
    {
        foreach (string arg in args)
        {
            if (arg == null || string.IsNullOrEmpty(arg))
            {
                return false;
            }
        }
        return true;
    }
    public class Base
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }
        [JsonPropertyName("CORS")]
        public bool CORS { get; set; } = false;
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("tokenedUser")]
        public string TokenedUser { get; set; }
    }

    public class GetPreSignedURL : Base
    {
        [JsonPropertyName("keyName")]
        public string KeyName { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }

        // Extra options for full rez downloading
        [JsonPropertyName("fullRez")]
        public bool? FullRez { get; set; }
        [JsonPropertyName("page")]
        public string? Page { get; set; }
    }
}