using System.Text;
using Amazon; // For RegionEndpoint
using Amazon.S3;
using Amazon.S3.Model;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace PhotographyAPI
{
    public class PageEdit
    {
        // Set up importatnt aws data
        private static readonly string bucketName = "photographydata";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast2; // Change as needed
        private static readonly IAmazonS3 s3Client = new AmazonS3Client(bucketRegion);

        // Write a text file
        public static async Task<JsonElement> WriteTxtFile(Request.WriteTxtFile data)
        {
            // Test for appropriot data
            if (!Request.Test(data.User, data.KeyName, data.FileContent))
            {
                return Response.Error("Invalid Arguments");
            }
            // Get paths for aws
            string fullKey = $"u/{data.User}/Pages/{data.KeyName}";
            string nameFromFilePath = fullKey.Split('/').Skip(1).FirstOrDefault() ?? "";
            // Test user credentials
            if (!(await Users.AuthToken(data.TokenedUser, data.Token) && nameFromFilePath == data.TokenedUser))
            {
                return Response.Error($"Invalid Token/User");
            }

            Response.WriteTxtFile response = new Response.WriteTxtFile($"File {data.KeyName} uploaded");

            // Write text file to aws
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data.FileContent));
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = fullKey,
                InputStream = stream,
                ContentType = "text/plain"
            };

            await s3Client.PutObjectAsync(putRequest);
            return response.Respond();
        }

        // Get all files and folders in a user picture folder
        public static async Task<JsonElement> GetKeys(Request.GetKeys data)
        {
            // Test incoming data
            if (!Request.Test(data.User))
            {
                return Response.Error("Invalid Arguments");
            }
            // Get paths for aws
            string fullPrefix = $"u/{data.User}/Pics/{data.PrefixName}";
            string nameFromFilePath = fullPrefix.Split('/').Skip(1).FirstOrDefault() ?? "";
            if (!(await Users.AuthToken(data.TokenedUser, data.Token) && nameFromFilePath == data.TokenedUser))
            {
                return Response.Error($"Invalid Token/User");
            }

            Response.GetKeys response = new Response.GetKeys($"Files and folders in s3://{bucketName}/{data.PrefixName} have been returned.");

            // Create request for s3
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = fullPrefix,
                Delimiter = "/" // ensures we get "folders" separately
            };

            var AWSresponse = await s3Client.ListObjectsV2Async(request);
            
            var folders = new List<string>(); // subfolders under myfolder/
            var files = new List<string>();

            // Add files to correct list
            if (AWSresponse?.S3Objects != null) {
                foreach (var obj in AWSresponse.S3Objects)
                {
                    // Skip the "folder" itself (which is just a zero-length key)
                    if (obj.Key != fullPrefix)
                    {
                        // Only add the relitive user path
                        string[] parts = obj.Key.Split('/');
                        files.Add(string.Join("/", parts.Skip(3)));
                    }
                }
            }

            // Add folders to correct list
            if (AWSresponse?.CommonPrefixes != null)
            {
                foreach (var obj in AWSresponse.CommonPrefixes)
                {
                    // Only add relitive user path
                    string[] parts = obj.Split('/');
                    folders.Add(string.Join("/", parts.Skip(3)));
                }
            }

            // Return information in array format
            var safeFolders = folders?.ToArray() ?? Array.Empty<string>();
            var safeFiles = files?.ToArray() ?? Array.Empty<string>();
            response.SetKeyContents(safeFolders, safeFiles);

            return response.Respond();
        }


        // Get a url for uploading content
        public static async Task<JsonElement> PutPreSignedURL(Request.PutPreSignedURL data)
        {
            // Test incomming data
            if (!Request.Test(data.KeyName, data.User))
            {
                return Response.Error("Invalid Arguments");
            }
            string fullPath = $"u/{data.User}/Pics/{data.KeyName}";
            // Test user credentials
            if (!(await Users.AuthToken(data.TokenedUser, data.Token) && data.User == data.TokenedUser))
            {
                return Response.Error($"Invalid Token/User");
            }

            const int exp = 5; // Experation 5 minutes

            Response.PutPreSignedURL response = new Response.PutPreSignedURL($"Url for s3://{bucketName}/{fullPath} assigned expires in {exp} minuites\nthis is for uploading files only\nPUT to this link to write.");

            // Send request
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = fullPath,
                Verb = HttpVerb.PUT, // Put allows uploading
                Expires = DateTime.UtcNow.AddMinutes(exp), // URL valid for 5 minutes
                ContentType = "application/octet-stream" // optional, match your file type
            };

            string url = s3Client.GetPreSignedURL(request);

            response.SetUrl(url);
            return response.Respond();
        }

        // Create a folder in s3
        public static async Task<bool> CreateFolder(string folderKey)
        {
            try
            {
                // Ensure folderKey ends with "/"
                if (!folderKey.EndsWith("/"))
                    folderKey += "/";

                // Send request to s3
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = folderKey,
                    ContentBody = string.Empty // empty object
                };

                await s3Client.PutObjectAsync(request);

                return true;
            } catch (Exception e)
            {
                return false;
            }

        }

        // Create a folder in both the user relitive path (both full resolution and low resolution directories)
        public static async Task<JsonElement> CreateFolderRelitivePath(Request.CreateFolderRelitivePath data)
        {
            // Test incomming data
            if (!Request.Test(data.FolderKey, data.User))
            {
                return Response.Error("Invalid Arguments");
            }
            // Test credentials && ownership
            if (!(await Users.AuthToken(data.TokenedUser, data.Token) && data.User == data.TokenedUser))
            {
                return Response.Error("Invalid Token/User");
            }

            Response.CreateFolderRelitivePath response = new Response.CreateFolderRelitivePath($"Created folder -> {data.FolderKey} succsesfully in both {data.User} files");

            // Create both folders
            if (!await CreateFolder($"u/{data.User}/Pics/{data.FolderKey}")) { return Response.Error("Folder Creation Failed"); }
            if (!await CreateFolder($"LowRez/{data.User}/Pics/{data.FolderKey}")) { return Response.Error("Folder Creation Failed"); }

            return response.Respond();
        }



        // Rename s3 object
        public static async Task<bool> Rename(string ObjKey, string NewObjKey)
        {
            try
            {
                // Copy the object to the new key
                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    SourceKey = ObjKey,
                    DestinationBucket = bucketName,
                    DestinationKey = NewObjKey
                };

                await s3Client.CopyObjectAsync(copyRequest);

                // Delete the old object
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = ObjKey
                };

                await s3Client.DeleteObjectAsync(deleteRequest);

                return true;
            } catch (Exception e)
            {
                return false;
            }
        }

        // Rename a file in both the user relitive path (both full resolution and low resolution directories)
        public static async Task<JsonElement> RenameRelitivePath(Request.RenameRelitivePath data)
        {
            // Test incomming data
            if (!Request.Test(data.ObjKey, data.NewObjKey, data.User))
            {
                return Response.Error("Invalid Arguments");
            }
            // Test credentials
            if (!(await Users.AuthToken(data.TokenedUser, data.Token) && data.User == data.TokenedUser))
            {
                return Response.Error($"Invalid Token/User");
            }
            Response.RenameRelitivePath response = new Response.RenameRelitivePath($"Renamed {data.ObjKey} to {data.NewObjKey} succsesfully in both {data.User} files");

            // Rename both ojects
            if (!await Rename($"u/{data.User}/Pics/{data.ObjKey}", $"u/{data.User}/Pics/{data.NewObjKey}")) { return Response.Error("Renaming Operation Failed"); }
            if (!await Rename($"LowRez/{data.User}/Pics/{data.ObjKey}", $"LowRez/{data.User}/Pics/{data.NewObjKey}")) { return Response.Error("Renaming Operation Failed"); }

            return response.Respond();
        }


        // Delete s3 filder or file
        public static async Task<bool> Delete(string KeyOrPrefix)
        {
            var keysToDelete = new List<KeyVersion>();

            // List all objects with this key/prefix
            var listRequest = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = KeyOrPrefix
            };

            ListObjectsV2Response listResponse;

            do
            {
                listResponse = await s3Client.ListObjectsV2Async(listRequest);

                if (listResponse.S3Objects is null)
                {
                    return false; // Object dosnt exist
                }

                foreach (var obj in listResponse.S3Objects)
                {
                    keysToDelete.Add(new KeyVersion { Key = obj.Key });
                }

                listRequest.ContinuationToken = listResponse.NextContinuationToken;

                if (listResponse.IsTruncated is null)
                {
                    return false; // ¯\_(ツ)_/¯
                }
            } while ((bool)listResponse.IsTruncated);

            if (keysToDelete.Count == 0)
            {
                return false; // No keys at all ):
            }

            // Delete in batches (S3 supports up to 1000 per request)
            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = keysToDelete
            };

            var deleteResponse = await s3Client.DeleteObjectsAsync(deleteRequest);
            if (deleteResponse.DeletedObjects.Count > 0)
            {
                return true; // Worked Yayyyy
            } else
            {
                return false; // No files deleted
            }
        }


        // Delete a folder or file in both the user relitive path (both full resolution and low resolution directories)
        public static async Task<JsonElement> DeleteRelitivePath(Request.DeleteRelitivePath data)
        {
            // Test incomming arguments
            if (!Request.Test(data.KeyOrPrefix, data.User))
            {
                return Response.Error("Invalid Arguments");
            }
            // Test credentials & Ownership
            if (!(await Users.AuthToken(data.TokenedUser, data.Token) && data.User == data.TokenedUser))
            {
                return Response.Error($"Invalid Token/User");
            }

            Response.DeleteRelitivePath response = new Response.DeleteRelitivePath($"Deleted {data.KeyOrPrefix} succsesfully in both {data.User} files");

            // Delete both files
            if (!await Delete($"u/{data.User}/Pics/{data.KeyOrPrefix}")) { return Response.Error("Failed delete operation"); }
            if (!await Delete($"LowRez/{data.User}/Pics/{data.KeyOrPrefix}")) { return Response.Error("Failed delete operation"); }
            
            return response.Respond();
        }

        // Change copyright on file
        private static async Task<bool> ChangeCopyright(string objKey, string copyrightValue)
        {
            try
            {
                // Download the image from S3
                var s3Object = await s3Client.GetObjectAsync(bucketName, objKey);
                using var inputStream = new MemoryStream();
                await s3Object.ResponseStream.CopyToAsync(inputStream);
                inputStream.Position = 0;

                // Load the image using ImageSharp
                using var image = Image.Load(inputStream);
                var format = image.Metadata.DecodedImageFormat; // Get format

                // Add or update copyright
                image.Metadata.ExifProfile ??= new ExifProfile();
                image.Metadata.ExifProfile.SetValue(ExifTag.Copyright, copyrightValue);

                // Save Image
                using var outputStream = new MemoryStream();
                image.Save(outputStream, format);
                outputStream.Position = 0;

                // Upload image
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = objKey,
                    InputStream = outputStream,
                    ContentType = s3Object.Headers.ContentType
                };

                await s3Client.PutObjectAsync(putRequest);

                return true;
            } catch (Exception e)
            {
                return false;
            }
        }

        // Change copyright data in both file of the user relitive path (both full resolution and low resolution directories)
        public static async Task<JsonElement> ChangeCopyrightRelitivePath(Request.ChangeCopyrightRelitivePath data)
        {
            // Test arguments
            if (!Request.Test(data.ObjKey, data.CopyrightValue, data.User))
            {
                return Response.Error("Invalid Arguments");
            }
            // Test credentials & Ownership
            if (!(await Users.AuthToken(data.TokenedUser, data.Token) && data.User == data.TokenedUser))
            {
                return Response.Error($"Invalid Token/User");
            }
            Response.ChangeCopyrightRelitivePath response = new Response.ChangeCopyrightRelitivePath($"Changed copy right for {data.ObjKey} to {data.CopyrightValue} in both of {data.User} files");

            // Change copyright on both files
            if (!await ChangeCopyright($"u/{data.User}/Pics/{data.ObjKey}", data.CopyrightValue)) { return Response.Error("Failed Copyright change operation"); }
            if (!await ChangeCopyright($"LowRez/{data.User}/Pics/{data.ObjKey}", data.CopyrightValue)) { return Response.Error("Failed Copyright change operation"); }

            return response.Respond();
        }


    }
}
