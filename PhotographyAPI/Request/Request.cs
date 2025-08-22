using System.Collections.Concurrent;
using Amazon.S3.Model;
using System.Text.Json.Serialization;
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
    }

    public class WriteTxtFile : IRequest
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("keyName")]
        public string KeyName { get; set; }
        [JsonPropertyName("fileContent")]
        public string FileContent { get; set; }
        public WriteTxtFile(string keyName, string fileContent)
        {
            Action = "WriteTxtFile";
            KeyName = keyName;
            FileContent = fileContent;
        }
    }


    public class Delete : IRequest
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("keyOrPrefix")]
        public string KeyOrPrefix { get; set; }
        public Delete(string keyOrPrefix)
        {
            Action = "Delete";
            KeyOrPrefix = keyOrPrefix;
        }
    }


    public class CreateFolder : IRequest
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("folderKey")]
        public string FolderKey { get; set; }


        public CreateFolder(string folderKey)
        {
            Action = "CreateFolder";
            FolderKey = folderKey;
        }
    }



    public class ReadFile : IRequest
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("fileKey")]
        public string FileKey { get; set; }

        public ReadFile(string fileKey)
        {
            Action = "ReadFile";
            FileKey = fileKey;
        }
    }
    

        public class ReadJson : IRequest {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("fileKey")]
        public string FileKey { get; set; }

        public ReadJson(string fileKey)
        {
            Action = "ReadFile";
            FileKey = fileKey;
        }
    }
}