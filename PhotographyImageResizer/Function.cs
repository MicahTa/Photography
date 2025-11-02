using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing; // For Resize
using System.IO;
using System.Threading.Tasks;
using System;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PhotographyImageResizer
{
    public class Function
    {
        private readonly IAmazonS3 _s3Client;

        public Function()
        {
            _s3Client = new AmazonS3Client();
        }

        // Lambda entry point
        public async Task<int> FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            foreach (var record in evnt.Records)
            {
                var s3 = record.S3;
                string bucketName = s3.Bucket.Name;
                string objectKey = WebUtility.UrlDecode(s3.Object.Key);

                Console.WriteLine(objectKey);

                // Only handle if it's an image
                string[] extensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".tiff" };
                bool isImage = extensions.Any(ext =>
                    objectKey.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
                if (!isImage) { return 0; }

                try
                {
                    // Download the original image
                    using var response = await _s3Client.GetObjectAsync(bucketName, objectKey);
                    await using var responseStream = response.ResponseStream;
                    using var image = await Image.LoadAsync(responseStream);

                    // Resize (50% size for example, adjust as needed)
                    image.Mutate(x => x.Resize(image.Width / 10, image.Height / 10));

                    // Save resized image to memory
                    await using var outStream = new MemoryStream();
                    await image.SaveAsJpegAsync(outStream);
                    outStream.Position = 0;

                    // Replace "Pics/" with "LowRezPics/"
                    string lowRezKey = WebUtility.UrlDecode("LowRez/" + objectKey.Substring(2));

                    //string lowRezKey = objectKey.Replace("u/", "LowRez/");

                    // Upload low-resolution image
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = lowRezKey,
                        InputStream = outStream,
                        ContentType = "image/jpeg"
                    };

                    await _s3Client.PutObjectAsync(putRequest);

                    context.Logger.LogInformation($"Created low-res copy: {lowRezKey}");
                    return 1;
                }
                catch (Exception e)
                {
                    context.Logger.LogError($"Error processing {objectKey}: {e}");
                    return 0;
                }
            }
            return 0;
        }
    }
}
