using Amazon.Lambda.Core;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetPreSignedURL;


public class Function
{
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
                        message = await GetPreSignedURLClass.GetPreSignedURL(data_GetPreSignedURL);
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
    }
}