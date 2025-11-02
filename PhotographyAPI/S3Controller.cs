using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon; // For RegionEndpoint
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Util.Internal;
using Microsoft.VisualBasic;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;


namespace PhotographyAPI
{
    public class S3Controller
    {
        private static readonly string bucketName = "photographydata";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast2; // Change as needed
        private readonly IAmazonS3 s3Client;

        public S3Controller()
        {
            // AWS Lambda automatically provides credentials from the execution role
            s3Client = new AmazonS3Client(bucketRegion);
        }


        public async Task<JsonElement> WriteTxtFile(Request.WriteTxtFile data)
        {
            if (!Request.Test(data.KeyName, data.FileContent))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.WriteTxtFile response = new Response.WriteTxtFile($"File uploaded to s3://{bucketName}/{data.KeyName}");

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data.FileContent));

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = data.KeyName,
                InputStream = stream,
                ContentType = "text/plain"
            };

            await s3Client.PutObjectAsync(putRequest);
            return response.Respond();
        }

        public async Task<JsonElement> GetKeys(Request.GetKeys data)
        {
            if (!Request.Test(data.PrefixName))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.GetKeys response = new Response.GetKeys($"Files and folders in s3://{bucketName}/{data.PrefixName} have been returned.");

            /*using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data.FileContent));

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = data.KeyName,
                InputStream = stream,
                ContentType = "text/plain"
            };
            await s3Client.PutObjectAsync(putRequest);
            */

            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = data.PrefixName,
                Delimiter = "/" // ensures we get "folders" separately
            };

            var AWSresponse = await s3Client.ListObjectsV2Async(request);

            var folders = AWSresponse.CommonPrefixes; // subfolders under myfolder/
            var files = new List<string>();

            foreach (var obj in AWSresponse.S3Objects)
            {
                // Skip the "folder" itself (which is just a zero-length key)
                if (obj.Key != data.PrefixName)
                {
                    files.Add(obj.Key);
                }
            }

            var safeFolders = folders?.ToArray() ?? Array.Empty<string>();
            var safeFiles = files?.ToArray() ?? Array.Empty<string>();
            response.SetKeyContents(safeFolders, safeFiles);

            return response.Respond();
        }

        public async Task<JsonElement> WriteB64File(Request.WriteB64File data)
        {
            if (!Request.Test(data.KeyName, data.FileContent))
            {
                return Response.Error("Invalid Arguments");
            }

            Response.WriteB64File response = new Response.WriteB64File(
                $"File uploaded to s3://{bucketName}/{data.KeyName}"
            );

            try
            {
                // Decode Base64 string into raw bytes
                byte[] fileBytes = Convert.FromBase64String(data.FileContent);

                using var stream = new MemoryStream(fileBytes);

                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = data.KeyName,
                    InputStream = stream,
                    ContentType = "application/octet-stream" // safer default for binary
                };

                await s3Client.PutObjectAsync(putRequest);
            }
            catch (FormatException)
            {
                return Response.Error("FileContent is not valid Base64");
            }

            return response.Respond();
        }


        public async Task<JsonElement> GetPreSignedURL(Request.GetPreSignedURL data)
        {
            if (!Request.Test(data.KeyName))
            {
                return Response.Error("Invalid Arguments");
            }
            const int exp = 60;

            Response.GetPreSignedURL response = new Response.GetPreSignedURL($"Url for s3://{bucketName}/{data.KeyName} assigned expires in {exp} minuites\nthis is for uploading files only\nPUT to this link to write.");

            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = data.KeyName,
                Verb = HttpVerb.GET, // GET allows uploading
                Expires = DateTime.UtcNow.AddMinutes(exp), // URL valid for 5 minutes
            };

            string url = s3Client.GetPreSignedURL(request);

            response.SetUrl(url);
            return response.Respond();
        }


        public async Task<JsonElement> PutPreSignedURL(Request.PutPreSignedURL data)
        {
            if (!Request.Test(data.KeyName))
            {
                return Response.Error("Invalid Arguments");
            }
            const int exp = 5;

            Response.PutPreSignedURL response = new Response.PutPreSignedURL($"Url for s3://{bucketName}/{data.KeyName} assigned expires in {exp} minuites\nthis is for uploading files only\nPUT to this link to write.");

            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = data.KeyName,
                Verb = HttpVerb.PUT, // Put allows uploading
                Expires = DateTime.UtcNow.AddMinutes(exp), // URL valid for 5 minutes
                ContentType = "application/octet-stream" // optional, match your file type
            };

            string url = s3Client.GetPreSignedURL(request);

            response.SetUrl(url);
            return response.Respond();
        }


        public async Task<JsonElement> ReadFile(Request.ReadFile data)
        {
            if (!Request.Test(data.FileKey))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.ReadFile response = new Response.ReadFile($"Succsesfully read file contents of {data.FileKey}");
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = data.FileKey
                };

                using (var apiResponse = await s3Client.GetObjectAsync(request))
                using (var reader = new StreamReader(apiResponse.ResponseStream))
                {
                    string content = await reader.ReadToEndAsync();
                    response.SetFileContents(content);
                    return response.Respond();
                }
            }
            catch (AmazonS3Exception e)
            {
                return Response.Error($"Error getting object {data.FileKey} from bucket {bucketName}: {e.Message}");
            }
        }




        public async Task<JsonElement> ReadJson(Request.ReadJson data)
        {
            if (!Request.Test(data.FileKey))
            {
                return Response.Error("Invalid Arguments");
            }

            Response.ReadJson response = new Response.ReadJson($"Succsesfully read json contents of {data.FileKey}");

            JsonElement readFileResponce = await this.ReadFile(new Request.ReadFile(data.FileKey));
            var defultData_WriteTxtFile = JsonSerializer.Deserialize<Response.Base>(readFileResponce);
            if (defultData_WriteTxtFile is null)
            {
                return Response.Error("Unexpected null Value");
            }
            else if (defultData_WriteTxtFile.Successful == false)
            {
                return readFileResponce;
            }
            var data_WriteTxtFile = JsonSerializer.Deserialize<Response.ReadFile>(readFileResponce);
            if (data_WriteTxtFile is null)
            {
                return Response.Error("Unexpected null Value");
            }

            using JsonDocument doc = JsonDocument.Parse(data_WriteTxtFile.FileContent);
            JsonElement root = doc.RootElement;
            response.SetJsonContents(root);
            return response.Respond();
        }



        public async Task<JsonElement> CreateFolder(Request.CreateFolder data)
        {
            if (!Request.Test(data.FolderKey))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.CreateFolder response = new Response.CreateFolder($"Folder s3://{bucketName}/{data.FolderKey} created");

            // Ensure folderKey ends with "/"
            if (!data.FolderKey.EndsWith("/"))
                data.FolderKey += "/";

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = data.FolderKey,
                ContentBody = string.Empty // empty object
            };

            await s3Client.PutObjectAsync(request);

            return response.Respond();
        }

        public async Task<JsonElement> CreateFolderRelitivePath(Request.CreateFolderRelitivePath data)
        {
            if (!Request.Test(data.FolderKey, data.User))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.CreateFolderRelitivePath response = new Response.CreateFolderRelitivePath($"Created folder -> {data.FolderKey} succsesfully in both {data.User} files");


            // Do the first
            JsonElement createFolderResponce = await this.CreateFolder(new Request.CreateFolder($"u/{data.User}/Pics/{data.FolderKey}"));
            var defultData_createFolderResponce = JsonSerializer.Deserialize<Response.Base>(createFolderResponce);

            if (defultData_createFolderResponce is null) { return Response.Error("Unexpected null Value"); }
            else if (defultData_createFolderResponce.Successful == false) { return createFolderResponce; }

            var data_createFolder = JsonSerializer.Deserialize<Response.CreateFolder>(createFolderResponce);
            if (data_createFolder is null) { return Response.Error("Unexpected null Value"); }
            // Do the second One
            JsonElement createFolderResponce2 = await this.CreateFolder(new Request.CreateFolder($"LowRez/{data.User}/Pics/{data.FolderKey}"));
            var defultData_createFolderResponce2 = JsonSerializer.Deserialize<Response.Base>(createFolderResponce2);

            if (defultData_createFolderResponce2 is null)
            {
                return Response.Error("Unexpected null Value");
            }
            else if (defultData_createFolderResponce2.Successful == false) { return createFolderResponce2; }

            var data_createFolder2 = JsonSerializer.Deserialize<Response.CreateFolder>(createFolderResponce2);
            if (data_createFolder2 is null) { return Response.Error("Unexpected null Value"); }

            // Did it work
            if (data_createFolder.Successful && data_createFolder2.Successful)
            {
                return response.Respond();
            }
            else
            {
                return Response.Error("Failed to create both folders");
            }
        }




        public async Task<JsonElement> Rename(Request.Rename data)
        {
            if (!Request.Test(data.ObjKey, data.NewObjKey))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.Rename response = new Response.Rename($"Renamed {data.ObjKey} to {data.NewObjKey} succsesfully");

            // Copy the object to the new key
            var copyRequest = new CopyObjectRequest
            {
                SourceBucket = bucketName,
                SourceKey = data.ObjKey,
                DestinationBucket = bucketName,
                DestinationKey = data.NewObjKey
            };

            await s3Client.CopyObjectAsync(copyRequest);

            // Delete the old object
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = data.ObjKey
            };

            await s3Client.DeleteObjectAsync(deleteRequest);

            return response.Respond();
        }

        public async Task<JsonElement> RenameRelitivePath(Request.RenameRelitivePath data)
        {
            if (!Request.Test(data.ObjKey, data.NewObjKey, data.User))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.RenameRelitivePath response = new Response.RenameRelitivePath($"Renamed {data.ObjKey} to {data.NewObjKey} succsesfully in both {data.User} files");


            // Do the first
            JsonElement renameResponce = await this.Rename(new Request.Rename($"u/{data.User}/Pics/{data.ObjKey}", $"u/{data.User}/Pics/{data.NewObjKey}"));
            var defultData_renameResponce = JsonSerializer.Deserialize<Response.Base>(renameResponce);

            if (defultData_renameResponce is null) { return Response.Error("Unexpected null Value"); }
            else if (defultData_renameResponce.Successful == false) { return renameResponce; }

            var data_rename = JsonSerializer.Deserialize<Response.Rename>(renameResponce);
            if (data_rename is null) { return Response.Error("Unexpected null Value"); }
            // Do the second One
            JsonElement renameResponce2 = await this.Rename(new Request.Rename($"LowRez/{data.User}/Pics/{data.ObjKey}", $"LowRez/{data.User}/Pics/{data.NewObjKey}"));
            var defultData_renameResponce2 = JsonSerializer.Deserialize<Response.Base>(renameResponce2);

            if (defultData_renameResponce2 is null)
            {
                return Response.Error("Unexpected null Value");
            }
            else if (defultData_renameResponce2.Successful == false) { return renameResponce2; }

            var data_rename2 = JsonSerializer.Deserialize<Response.Rename>(renameResponce2);
            if (data_rename2 is null) { return Response.Error("Unexpected null Value"); }

            // Did it work
            if (data_rename.Successful && data_rename2.Successful)
            {
                return response.Respond();
            }
            else
            {
                return Response.Error("Failed to rename both");
            }
        }






        public async Task<JsonElement> Delete(Request.Delete data)
        {
            if (!Request.Test(data.KeyOrPrefix))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.Delete response = new Response.Delete();

            var keysToDelete = new List<KeyVersion>();

            // List all objects with this key/prefix
            var listRequest = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = data.KeyOrPrefix
            };

            ListObjectsV2Response listResponse;

            do
            {
                listResponse = await s3Client.ListObjectsV2Async(listRequest);

                if (listResponse.S3Objects is null)
                {
                    return Response.Error($"No objects found with key/prefix '{data.KeyOrPrefix}'");
                }

                foreach (var obj in listResponse.S3Objects)
                {
                    keysToDelete.Add(new KeyVersion { Key = obj.Key });
                }

                listRequest.ContinuationToken = listResponse.NextContinuationToken;

                if (listResponse.IsTruncated is null)
                {
                    return Response.Error("Unexpected Null");
                }
            } while ((bool)listResponse.IsTruncated);

            if (keysToDelete.Count == 0)
            {
                return Response.Error($"No objects found with key/prefix '{data.KeyOrPrefix}'");
            }

            // Delete in batches (S3 supports up to 1000 per request)
            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = keysToDelete
            };

            var deleteResponse = await s3Client.DeleteObjectsAsync(deleteRequest);
            response.SetMessage($"Deleted {deleteResponse.DeletedObjects.Count} object(s) from '{data.KeyOrPrefix}'");
            return response.Respond();
        }


        public async Task<JsonElement> DeleteRelitivePath(Request.DeleteRelitivePath data)
        {
            if (!Request.Test(data.KeyOrPrefix, data.User))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.DeleteRelitivePath response = new Response.DeleteRelitivePath($"Deleted {data.KeyOrPrefix} succsesfully in both {data.User} files");


            // Do the first
            JsonElement deleteResponce = await this.Delete(new Request.Delete($"u/{data.User}/Pics/{data.KeyOrPrefix}"));
            var defultData_deleteResponce = JsonSerializer.Deserialize<Response.Base>(deleteResponce);

            if (defultData_deleteResponce is null) { return Response.Error("Unexpected null Value"); }
            else if (defultData_deleteResponce.Successful == false) { return deleteResponce; }

            var data_delete = JsonSerializer.Deserialize<Response.Delete>(deleteResponce);
            if (data_delete is null) { return Response.Error("Unexpected null Value"); }
            // Do the second One
            JsonElement deleteResponce2 = await this.Delete(new Request.Delete($"LowRez/{data.User}/Pics/{data.KeyOrPrefix}"));
            var defultData_deleteResponce2 = JsonSerializer.Deserialize<Response.Base>(deleteResponce2);

            if (defultData_deleteResponce2 is null)
            {
                return Response.Error("Unexpected null Value");
            }
            else if (defultData_deleteResponce2.Successful == false) { return deleteResponce2; }

            var data_delete2 = JsonSerializer.Deserialize<Response.Delete>(deleteResponce2);
            if (data_delete2 is null) { return Response.Error("Unexpected null Value"); }

            // Did it work
            if (data_delete.Successful && data_delete2.Successful)
            {
                return response.Respond();
            }
            else
            {
                return Response.Error("Failed to delete both");
            }
        }


        public async Task<JsonElement> ChangeCopyright(Request.ChangeCopyright data)
        {
            if (!Request.Test(data.ObjKey, data.CopyrightValue))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.ChangeCopyright response = new Response.ChangeCopyright($"Set copyright value to {data.CopyrightValue} on {data.ObjKey} succsesfully");

            // 1️⃣ Download the image from S3
            var s3Object = await s3Client.GetObjectAsync(bucketName, data.ObjKey);
            using var inputStream = new MemoryStream();
            await s3Object.ResponseStream.CopyToAsync(inputStream);
            inputStream.Position = 0;

            // 2️⃣ Load the image using ImageSharp
            using var image = Image.Load(inputStream); // no out parameter
            var format = image.Metadata.DecodedImageFormat; // get the format separately;

            // 3️⃣ Add or update EXIF copyright
            image.Metadata.ExifProfile ??= new ExifProfile();
            image.Metadata.ExifProfile.SetValue(ExifTag.Copyright, data.CopyrightValue);

            // 4️⃣ Save the modified image to a memory stream
            using var outputStream = new MemoryStream();
            image.Save(outputStream, format);
            outputStream.Position = 0;

            // 5️⃣ Upload the modified image back to S3
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = data.ObjKey,
                InputStream = outputStream,
                ContentType = s3Object.Headers.ContentType
            };

            await s3Client.PutObjectAsync(putRequest);

            return response.Respond();
        }


        public async Task<JsonElement> ChangeCopyrightRelitivePath(Request.ChangeCopyrightRelitivePath data)
        {
            if (!Request.Test(data.ObjKey, data.CopyrightValue, data.User))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.ChangeCopyrightRelitivePath response = new Response.ChangeCopyrightRelitivePath($"Changed copy right for {data.ObjKey} to {data.CopyrightValue} in both of {data.User} files");


            // Do the first
            JsonElement ChangeCopyrightResponce = await this.ChangeCopyright(new Request.ChangeCopyright($"u/{data.User}/Pics/{data.ObjKey}", data.CopyrightValue));
            var defultData_ChangeCopyrightResponce = JsonSerializer.Deserialize<Response.Base>(ChangeCopyrightResponce);

            if (defultData_ChangeCopyrightResponce is null) { return Response.Error("Unexpected null Value"); }
            else if (defultData_ChangeCopyrightResponce.Successful == false) { return ChangeCopyrightResponce; }

            var data_ChangeCopyright = JsonSerializer.Deserialize<Response.ChangeCopyright>(ChangeCopyrightResponce);
            if (data_ChangeCopyright is null) { return Response.Error("Unexpected null Value"); }
            // Do the second One
            JsonElement ChangeCopyrightResponce2 = await this.ChangeCopyright(new Request.ChangeCopyright($"LowRez/{data.User}/Pics/{data.ObjKey}", data.CopyrightValue));
            var defultData_ChangeCopyrightResponce2 = JsonSerializer.Deserialize<Response.Base>(ChangeCopyrightResponce2);

            if (defultData_ChangeCopyrightResponce2 is null)
            {
                return Response.Error("Unexpected null Value");
            }
            else if (defultData_ChangeCopyrightResponce2.Successful == false) { return ChangeCopyrightResponce2; }

            var data_ChangeCopyright2 = JsonSerializer.Deserialize<Response.ChangeCopyright>(ChangeCopyrightResponce2);
            if (data_ChangeCopyright2 is null) { return Response.Error("Unexpected null Value"); }

            // Did it work
            if (data_ChangeCopyright.Successful && data_ChangeCopyright2.Successful)
            {
                return response.Respond();
            }
            else
            {
                return Response.Error("Failed to chance copyright on both");
            }
        }


    }
}
