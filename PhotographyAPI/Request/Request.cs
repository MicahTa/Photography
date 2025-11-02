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
    }

    public class WriteTxtFile : Base
    {
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

    public class GetKeys : Base
    {
        [JsonPropertyName("prefixName")]
        public string PrefixName { get; set; }
        public GetKeys(string prefixName)
        {
            Action = "GetKeys";
            PrefixName = prefixName;
        }
    }

    public class WriteB64File : Base
    {
        [JsonPropertyName("keyName")]
        public string KeyName { get; set; }
        [JsonPropertyName("fileContent")]
        public string FileContent { get; set; }
        public WriteB64File(string keyName, string fileContent)
        {
            Action = "writeb64file";
            KeyName = keyName;
            FileContent = fileContent;
        }
    }

    public class GetPreSignedURL : Base
    {
        [JsonPropertyName("keyName")]
        public string KeyName { get; set; }
        public GetPreSignedURL(string keyName)
        {
            Action = "getpresignedurl";
            KeyName = keyName;
        }
    }

    public class PutPreSignedURL : Base
    {
        [JsonPropertyName("keyName")]
        public string KeyName { get; set; }
        public PutPreSignedURL(string keyName)
        {
            Action = "putpresignedurl";
            KeyName = keyName;
        }
    }


    public class Delete : Base
    {
        [JsonPropertyName("keyOrPrefix")]
        public string KeyOrPrefix { get; set; }
        public Delete(string keyOrPrefix)
        {
            Action = "Delete";
            KeyOrPrefix = keyOrPrefix;
        }
    }


    public class DeleteRelitivePath : Base
    {
        [JsonPropertyName("keyOrPrefix")]
        public string KeyOrPrefix { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }

        public DeleteRelitivePath(string keyOrPrefix, string user)
        {
            Action = "RenameRelitivePath";
            KeyOrPrefix = keyOrPrefix;
            User = user;
        }
    }


    public class CreateFolder : Base
    {
        [JsonPropertyName("folderKey")]
        public string FolderKey { get; set; }


        public CreateFolder(string folderKey)
        {
            Action = "CreateFolder";
            FolderKey = folderKey;
        }
    }

    public class CreateFolderRelitivePath : Base
    {
        [JsonPropertyName("folderKey")]
        public string FolderKey { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }


        public CreateFolderRelitivePath(string folderKey, string user)
        {
            Action = "CreateFolderRelitivePath";
            FolderKey = folderKey;
            User = user;
        }
    }



    public class ReadFile : Base
    {
        [JsonPropertyName("fileKey")]
        public string FileKey { get; set; }

        public ReadFile(string fileKey)
        {
            Action = "ReadFile";
            FileKey = fileKey;
        }
    }


    public class ReadJson : Base
    {
        [JsonPropertyName("fileKey")]
        public string FileKey { get; set; }

        public ReadJson(string fileKey)
        {
            Action = "ReadFile";
            FileKey = fileKey;
        }
    }

    public class Rename : Base
    {
        [JsonPropertyName("objKey")]
        public string ObjKey { get; set; }
        [JsonPropertyName("newObjKey")]
        public string NewObjKey { get; set; }

        public Rename(string objKey, string newObjKey)
        {
            Action = "Rename";
            ObjKey = objKey;
            NewObjKey = newObjKey;
        }
    }

    public class RenameRelitivePath : Base
    {
        [JsonPropertyName("objKey")]
        public string ObjKey { get; set; }
        [JsonPropertyName("newObjKey")]
        public string NewObjKey { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }

        public RenameRelitivePath(string objKey, string newObjKey, string user)
        {
            Action = "RenameRelitivePath";
            ObjKey = objKey;
            NewObjKey = newObjKey;
            User = user;
        }
    }

    public class ChangeCopyright : Base
    {
        [JsonPropertyName("objKey")]
        public string ObjKey { get; set; }
        [JsonPropertyName("copyrightValue")]
        public string CopyrightValue { get; set; }

        public ChangeCopyright(string objKey, string copyrightValue)
        {
            Action = "ChangeCopyright";
            ObjKey = objKey;
            CopyrightValue = copyrightValue;
        }
    }

    public class ChangeCopyrightRelitivePath : Base
    {
        [JsonPropertyName("objKey")]
        public string ObjKey { get; set; }
        [JsonPropertyName("copyrightValue")]
        public string CopyrightValue { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }

        public ChangeCopyrightRelitivePath(string objKey, string copyrightValue, string user)
        {
            Action = "ChangeCopyrightRelitivePath";
            ObjKey = objKey;
            CopyrightValue = copyrightValue;
            User = user;
        }
    }
}