using System.Collections.Concurrent;
using Amazon.S3.Model;
using System.Text.Json.Serialization;

#pragma warning disable CS8618

namespace PhotographyAPI;

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

    public class Base : IRequest
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

    public class WriteTxtFile : Base
    {
        [JsonPropertyName("keyName")]
        public string KeyName { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }
        [JsonPropertyName("fileContent")]
        public string FileContent { get; set; }
    }

    public class GetKeys : Base
    {
        [JsonPropertyName("prefixName")]
        public string PrefixName { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }
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

    public class PutPreSignedURL : Base
    {
        [JsonPropertyName("keyName")]
        public string KeyName { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }
    }



    public class DeleteRelitivePath : Base
    {
        [JsonPropertyName("keyOrPrefix")]
        public string KeyOrPrefix { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }
    }


    public class CreateFolderRelitivePath : Base
    {
        [JsonPropertyName("folderKey")]
        public string FolderKey { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }
    }
    public class ReadPage : Base
    {
        [JsonPropertyName("user")]
        public string User { get; set; }
        [JsonPropertyName("page")]
        public string Page { get; set; }
    }

    public class RenameRelitivePath : Base
    {
        [JsonPropertyName("objKey")]
        public string ObjKey { get; set; }
        [JsonPropertyName("newObjKey")]
        public string NewObjKey { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }
    }


    public class ChangeCopyrightRelitivePath : Base
    {
        [JsonPropertyName("objKey")]
        public string ObjKey { get; set; }
        [JsonPropertyName("copyrightValue")]
        public string CopyrightValue { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }
    }


    public class CreateUser : Base
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
    public class AddAuthUser : Base
    {
        [JsonPropertyName("user")]
        public string User { get; set; }
        [JsonPropertyName("page")]
        public string Page { get; set; }
    }
    public class RemoveAuthUser : Base
    {
        [JsonPropertyName("user")]
        public string User { get; set; }
        [JsonPropertyName("page")]
        public string Page { get; set; }
    }
    public class ListAuthUsers : Base
    {
        [JsonPropertyName("page")]
        public string Page { get; set; }
    }
    public class DoesUserExist : Base
    {
        [JsonPropertyName("user")]
        public string User { get; set; }

        public DoesUserExist(string user)
        {
            Action = "DoesUserExist";
            User = user;
        }
    }

    public class GenerateToken : Base
    {
        [JsonPropertyName("user")]
        public string User { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
    public class ChangePageAuth : Base
    {
        [JsonPropertyName("page")]
        public string Page { get; set; }
        [JsonPropertyName("open")]
        public bool Open { get; set; }
        [JsonPropertyName("download")]
        public bool Download { get; set; }
    }
    public class ListPages : Base {}

    public class NewPage : Base
    {
        [JsonPropertyName("pageName")]
        public string PageName { get; set; }
    }
    public class DeletePage : Base
    {
        [JsonPropertyName("page")]
        public string Page { get; set; }
    }

    public class CopyPage : Base
    {
        [JsonPropertyName("page")]
        public string Page { get; set; }
    }

    public class RenamePage : Base
    {
        [JsonPropertyName("page")]
        public string Page { get; set; }
        [JsonPropertyName("newName")]
        public string NewName { get; set; }
    }
}