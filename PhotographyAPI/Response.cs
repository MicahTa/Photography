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




    private class ErrorMsg
    {
        [JsonPropertyName("successful")]
        public bool Successful { get; set; } = false;
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }
        public ErrorMsg(string errorMessage) {
            Successful = false;
            ErrorMessage = errorMessage;
        }
    }

    public static JsonElement Error(string errorMessage)
    {
        var response = new ErrorMsg(errorMessage);
        string jsonString = JsonSerializer.Serialize(response);
        using var doc = JsonDocument.Parse(jsonString);
        return doc.RootElement.Clone();
    }
    public class WriteTxtFile : Base
    {
        public WriteTxtFile(string message) : base(message) { }
        public WriteTxtFile() : base() { }
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


    public class ReadPage : Base
    {
        public ReadPage(string message) : base(message) { }
        public ReadPage() : base() { }
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

    public class RenameRelitivePath : Base
    {
        public RenameRelitivePath(string message) : base(message) { }
        public RenameRelitivePath() : base() { }
    }


    public class ChangeCopyrightRelitivePath : Base
    {
        public ChangeCopyrightRelitivePath(string message) : base(message) { }
        public ChangeCopyrightRelitivePath() : base() { }
    }

    public class DoesUserExist : Base
    {
        public DoesUserExist(string message) : base(message) { }
        public DoesUserExist() : base() { }
        public void SetUserExistance(bool Existance)
        {
            Message = Existance.ToString();
        }
        public new JsonElement Respond()
        {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
    }

    public class GenerateToken : Base
    {
        public GenerateToken(string message) : base(message) { }
        public GenerateToken() : base() { }
        [JsonPropertyName("token")]
        public string Token { get; set; }
        public void SetToken(string token)
        {
            Token = token;
        }
        public new JsonElement Respond()
        {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
    }

    public class CreateUser : Base
    {
        public CreateUser(string message) : base(message) { }
        public CreateUser() : base() { }
        public new JsonElement Respond()
        {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
    }

    public class AddAuthUser : Base
    {
        public AddAuthUser(string message) : base(message) { }
        public AddAuthUser() : base() { }
        public void SetAuthUpdation(bool Existance)
        {
            Message = Existance.ToString();
        }
        public new JsonElement Respond()
        {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
    }

    public class RemoveAuthUser : Base
    {
        public RemoveAuthUser(string message) : base(message) { }
        public RemoveAuthUser() : base() { }
        public void SetAuthUpdation(bool Existance)
        {
            Message = Existance.ToString();
        }
        public new JsonElement Respond()
        {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
    }

    public class ListAuthUsers : Base
    {
        public ListAuthUsers(string message) : base(message) { }
        public ListAuthUsers() : base() { }
        [JsonPropertyName("users")]
        public string[] Users { get; set; }
        public void SetUsers(string[] users)
        {
            Users = users;
        }
        public new JsonElement Respond()
        {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
    }
    public class ChangePageAuth : Base
    {
        public ChangePageAuth(string message) : base(message) { }
        public ChangePageAuth() : base() { }
    }

    public class ListPages : Base
    {
        public ListPages(string message) : base(message) { }
        public ListPages() : base() { }
        public new JsonElement Respond()
        {
            var response = this;
            string jsonString = JsonSerializer.Serialize(response);
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.Clone();
        }
        [JsonPropertyName("projects")]
        public string[] Projects { get; set; }
        public void SetProjectContents(string[] projects)
        {
            Projects = projects;
        }
    }

    public class NewPage : Base
    {
        public NewPage(string message) : base(message) { }
        public NewPage() : base() { }
    }

    public class DeletePage : Base
    {
        public DeletePage(string message) : base(message) { }
        public DeletePage() : base() { }
    }

    public class CopyPage : Base
    {
        public CopyPage(string message) : base(message) { }
        public CopyPage() : base() { }
    }
    public class RenamePage : Base
    {
        public RenamePage(string message) : base(message) { }
        public RenamePage() : base() { }
    }
}