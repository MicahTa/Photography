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
    public async Task<string> FunctionHandler(JsonElement input, ILambdaContext context)
    {
        try
        {
            var request = JsonSerializer.Deserialize<Base>(input);
            string message;

            // Make Sure Theres a Action
            if (!Request.Test(request.Action))
            {
                return "Invalid Arguments";
            }

            switch (request.Action.ToLower())
            {
                case "writetxtfile":
                    var data_WriteTxtFile = JsonSerializer.Deserialize<WriteTxtFile>(input);
                    message = await S3.WriteTxtFile(new WriteTxtFile(data_WriteTxtFile.KeyName, data_WriteTxtFile.FileContent));
                    return message;

                case "createfolder":
                    var data_CreateFolder = JsonSerializer.Deserialize<CreateFolder>(input);
                    message = await S3.CreateFolder(new CreateFolder(data_CreateFolder.FolderKey));
                    return message;

                case "delete":
                    var data_Delete = JsonSerializer.Deserialize<Delete>(input);
                    message = await S3.Delete(new Delete(data_Delete.KeyOrPrefix));
                    return message;

                default:
                    Console.WriteLine($"Unknown action: {request.Action}");
                    return $"Method \"{request.Action}\" does not exist";
            }
        }
        catch (Exception ex)
        {
            return ($"Error handling request: {ex.Message}");
        }
    }
}


/* 
{
action = ""
Data = {}
}
*/