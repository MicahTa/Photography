using Amazon.Lambda.Core;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PhotographyAPI;


public class Function
{
    // Load Diffrent Important API Classes
    S3Controller S3 = new S3Controller();
    // Handle API Call
    public async Task<JsonElement> FunctionHandler(JsonElement input, ILambdaContext context)
    {
        try
        {
            var request = JsonSerializer.Deserialize<Request.Base>(input);
            string JsonElement;
            JsonElement message;

            // Make Sure Theres a Action
            if (!Request.Test(request.Action))
            {
                return Response.Error("Invalid Arguments");
            }
            switch (request.Action.ToLower())
            {
                case "writetxtfile":
                    var data_WriteTxtFile = JsonSerializer.Deserialize<Request.WriteTxtFile>(input);
                    message = await S3.WriteTxtFile(new Request.WriteTxtFile(data_WriteTxtFile.KeyName, data_WriteTxtFile.FileContent));
                    return message;

                case "createfolder":
                    var data_CreateFolder = JsonSerializer.Deserialize<Request.CreateFolder>(input);
                    message = await S3.CreateFolder(new Request.CreateFolder(data_CreateFolder.FolderKey));
                    return message;

                case "delete":
                    var data_Delete = JsonSerializer.Deserialize<Request.Delete>(input);
                    message = await S3.Delete(new Request.Delete(data_Delete.KeyOrPrefix));
                    return message;

                default:
                    return Response.Error($"Unknown action: {request.Action}");
            }
        }
        catch (Exception ex)
        {
            return Response.Error($"Error handling request: {ex.Message}");
        }
    }
}


/* 
{
action = ""
Data = {}
}
*/