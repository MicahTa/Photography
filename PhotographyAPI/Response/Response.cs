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
    public class WriteTxtFile : DefultResponse
    {
        public WriteTxtFile(string message) : base(message) { }
        public WriteTxtFile() : base() { }
    }

    public class CreateFolder : DefultResponse
    {
        public CreateFolder(string message) : base(message) { }
        public CreateFolder() : base() { }
    }

    public class Delete : DefultResponse
    {
        public Delete(string message) : base(message) { }
        public Delete() : base() { }
    }
}