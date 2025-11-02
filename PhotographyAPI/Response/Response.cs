using Amazon.Lambda.Core;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;
using Amazon.S3.Model;
using Amazon.Lambda.APIGatewayEvents;

#pragma warning disable CS8618

namespace PhotographyAPI;

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
    public class WriteTxtFile : Base
    {
        public WriteTxtFile(string message) : base(message) { }
        public WriteTxtFile() : base() { }
    }

    public class WriteB64File : Base
    {
        public WriteB64File(string message) : base(message) { }
        public WriteB64File() : base() { }
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


    public class PutPreSignedURL : Base
    {
        public PutPreSignedURL(string message) : base(message) { }
        public PutPreSignedURL() : base() { }
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

    public class CreateFolder : Base
    {
        public CreateFolder(string message) : base(message) { }
        public CreateFolder() : base() { }
    }

    public class CreateFolderRelitivePath : Base
    {
        public CreateFolderRelitivePath(string message) : base(message) { }
        public CreateFolderRelitivePath() : base() { }
    }

    public class Delete : Base
    {
        public Delete(string message) : base(message) { }
        public Delete() : base() { }
    }

    public class DeleteRelitivePath : Base
    {
        public DeleteRelitivePath(string message) : base(message) { }
        public DeleteRelitivePath() : base() { }
    }

    public class ReadFile : Base
    {
        public ReadFile(string message) : base(message) { }
        public ReadFile() : base() { }
        public new JsonElement Respond()
        {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
        [JsonPropertyName("fileContents")]
        public string FileContent { get; set; }
        public void SetFileContents(string contents)
        {
            FileContent = contents;
        }
    }

    public class GetKeys : Base
    {
        public GetKeys(string message) : base(message) { }
        public GetKeys() : base() { }
        public new JsonElement Respond()
        {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
        [JsonPropertyName("folderContents")]
        public string[] FolderContents { get; set; }
        [JsonPropertyName("fileContents")]
        public string[] FileContents { get; set; }
        public void SetKeyContents(string[] folderContents, string[] fileContents)
        {
            FolderContents = folderContents;
            FileContents = fileContents;
        }
    }


    public class ReadJson : Base
    {
        public ReadJson(string message) : base(message) { }
        public ReadJson() : base() { }
        public new JsonElement Respond()
        {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
        [JsonPropertyName("fileContents")]
        public JsonElement FileContent { get; set; }
        public void SetJsonContents(JsonElement contents)
        {
            FileContent = contents;
        }
    }

    public class Rename : Base
    {
        public Rename(string message) : base(message) { }
        public Rename() : base() { }
    }

    public class RenameRelitivePath : Base
    {
        public RenameRelitivePath(string message) : base(message) { }
        public RenameRelitivePath() : base() { }
    }

    public class ChangeCopyright : Base
    {
        public ChangeCopyright(string message) : base(message) { }
        public ChangeCopyright() : base() { }
    }

    public class ChangeCopyrightRelitivePath : Base
    {
        public ChangeCopyrightRelitivePath(string message) : base(message) { }
        public ChangeCopyrightRelitivePath() : base() { }
    }
}