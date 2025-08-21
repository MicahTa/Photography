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
            if (!Request.Test(data.KeyName, data.FileContent)) {
                return Response.Error("Invalid Arguments");
            }

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data.FileContent));

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = data.KeyName,
                InputStream = stream,
                ContentType = "text/plain"
            };

            await s3Client.PutObjectAsync(putRequest);
            Response.WriteTxtFile response = new Response.WriteTxtFile($"File uploaded to s3://{bucketName}/{data.KeyName}");
            return response.Respond();
            // $"File uploaded to s3://{bucketName}/{data.KeyName}";
        }


        /*public async Task<string> CreateFolder(CreateFolder data)
        {
            if (!Request.Test(data.FolderKey)) {
                return "Invalid Arguments";
            }

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

            return $"Folder s3://{bucketName}/{data.FolderKey} created";
        }



        public async Task<string> Delete(Delete data) {
            if (!Request.Test(data.KeyOrPrefix)) {
                return "Invalid Arguments";
            }


            var keysToDelete = new List<KeyVersion>();

            // List all objects with this key/prefix
            var listRequest = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = data.KeyOrPrefix
            };

            ListObjectsV2Response listResponse;
            do {
                listResponse = await s3Client.ListObjectsV2Async(listRequest);

                if (listResponse.S3Objects is null) {
                    return ($"No objects found with key/prefix '{data.KeyOrPrefix}'");
                }

                foreach (var obj in listResponse.S3Objects)
                {
                    keysToDelete.Add(new KeyVersion { Key = obj.Key });
                }

                listRequest.ContinuationToken = listResponse.NextContinuationToken;
            } while ((bool)listResponse.IsTruncated);

            if (keysToDelete.Count == 0) {
                return ($"No objects found with key/prefix '{data.KeyOrPrefix}'");
            }

            // Delete in batches (S3 supports up to 1000 per request)
            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = keysToDelete
            };

            var deleteResponse = await s3Client.DeleteObjectsAsync(deleteRequest);
            return ($"Deleted {deleteResponse.DeletedObjects.Count} object(s) from '{data.KeyOrPrefix}'");
        }*/


    }
}
