using Amazon.Lambda.Core;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;
using Amazon.S3.Model;
namespace PhotographyAPI;

public class Response
{
    public static JsonElement Error(string errorMessage)
    {
        var response = new Error(errorMessage);
        string jsonString = JsonSerializer.Serialize(response);
        using var doc = JsonDocument.Parse(jsonString);
        return doc.RootElement.Clone();
    }
    public class WriteTxtFile : IResponse
    {
        [JsonPropertyName("successful")]
        public bool Successful { get; set; } = false;
        [JsonPropertyName("message")]
        public string Message { get; set; }
        public WriteTxtFile(string message)
        {
            Successful = true;
            Message = message;
        }
        public JsonElement Respond() {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
    }
}