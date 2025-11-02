using Amazon.Lambda.Core;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetPreSignedURL;
public class Function
    {
        // Load Diffrent Important API Classes
        S3Controller S3 = new S3Controller();
        // Handle API Call
        public async Task<JsonElement> FunctionHandler(JsonElement input, ILambdaContext context)
        {
            //try
            //{
            var request = JsonSerializer.Deserialize<Request.Base>(input);
            JsonElement message;

            // Make Sure Theres a Action
            if (request is null) { message = Response.Error("Invalid Arguments"); }
            else if (!Request.Test(request.Action))
            {
                message = Response.Error("Invalid Arguments");
            }
            else
            {
                switch (request.Action.ToLower())
                {
                    case "getpresignedurl": // Used -- Full Path
                        var data_GetPreSignedURL = JsonSerializer.Deserialize<Request.GetPreSignedURL>(input);
                        if (data_GetPreSignedURL is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.GetPreSignedURL(new Request.GetPreSignedURL(data_GetPreSignedURL.KeyName));
                        break;

                    default:
                        message = Response.Error($"Unknown action: {request.Action}");
                        break;
                }
                if (request.CORS)
                {
                    return Response.CORS(message);
                }
                else
                {
                    return message;
                }
            }
            if (request.CORS)
            {
                return Response.CORS(message);
            }
            else
            {
                return message;
            }
            /*}
            catch (Exception ex)
            {
                JsonElement message;
                message = Response.Error($"Error handling request: {ex.Message}");
                return message;
            }*/
        }
    }
    
interface IRequest
{
    [JsonPropertyName("action")]
    public string Action { get; set; }
}
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
}


public class Error
{
    [JsonPropertyName("successful")]
    public bool Successful { get; set; } = false;
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }
    public Error(string errorMessage)
    {
        Successful = false;
        ErrorMessage = errorMessage;
    }
}
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
}
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
}