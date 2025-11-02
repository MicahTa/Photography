using Amazon.Lambda.Core;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

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
                    case "writetxtfile": // Used -- Full Path
                        var data_WriteTxtFile = JsonSerializer.Deserialize<Request.WriteTxtFile>(input);
                        if (data_WriteTxtFile is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.WriteTxtFile(new Request.WriteTxtFile(data_WriteTxtFile.KeyName, data_WriteTxtFile.FileContent));
                        break;
                    
                    case "getkeys": // Used -- Full Path
                        var data_GetKeys = JsonSerializer.Deserialize<Request.GetKeys>(input);
                        if (data_GetKeys is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.GetKeys(new Request.GetKeys(data_GetKeys.PrefixName));
                        break;

                    case "writeb64file": // // Unused -- Full Path
                        var data_WriteB64File = JsonSerializer.Deserialize<Request.WriteB64File>(input);
                        if (data_WriteB64File is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.WriteB64File(new Request.WriteB64File(data_WriteB64File.KeyName, data_WriteB64File.FileContent));
                        break;

                    case "getpresignedurl": // Used -- Full Path
                        var data_GetPreSignedURL = JsonSerializer.Deserialize<Request.GetPreSignedURL>(input);
                        if (data_GetPreSignedURL is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.GetPreSignedURL(new Request.GetPreSignedURL(data_GetPreSignedURL.KeyName));
                        break;
                    
                    case "putpresignedurl": // Used -- Full Path
                        var data_PutPreSignedURL = JsonSerializer.Deserialize<Request.PutPreSignedURL>(input);
                        if (data_PutPreSignedURL is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.PutPreSignedURL(new Request.PutPreSignedURL(data_PutPreSignedURL.KeyName));
                        break;

                    case "createfolder": // Unused -- Full Path -- TODO create relitive path version to be used
                        var data_CreateFolder = JsonSerializer.Deserialize<Request.CreateFolder>(input);
                        if (data_CreateFolder is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.CreateFolder(new Request.CreateFolder(data_CreateFolder.FolderKey));
                        break;
                    
                    case "createfolderrelitivepath": // Unused -- Full Path -- TODO create relitive path version to be used
                        var data_CreateFolderRelitivePath = JsonSerializer.Deserialize<Request.CreateFolderRelitivePath>(input);
                        if (data_CreateFolderRelitivePath is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.CreateFolderRelitivePath(new Request.CreateFolderRelitivePath(data_CreateFolderRelitivePath.FolderKey, data_CreateFolderRelitivePath.User));
                        break;

                    case "delete": // Unused -- Full Path
                        var data_Delete = JsonSerializer.Deserialize<Request.Delete>(input);
                        if (data_Delete is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.Delete(new Request.Delete(data_Delete.KeyOrPrefix));
                        break;

                    case "deleterelitivepath": // used -- Relitive Path
                        var data_DeleteRelitivePath = JsonSerializer.Deserialize<Request.DeleteRelitivePath>(input);
                        if (data_DeleteRelitivePath is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.DeleteRelitivePath(new Request.DeleteRelitivePath(data_DeleteRelitivePath.KeyOrPrefix, data_DeleteRelitivePath.User));
                        break;

                    case "readfile": // Unused -- Full Path
                        var data_ReadFile = JsonSerializer.Deserialize<Request.ReadFile>(input);
                        if (data_ReadFile is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.ReadFile(new Request.ReadFile(data_ReadFile.FileKey));
                        break;

                    case "readjson": // Used -- Full Path
                        var data_ReadJson = JsonSerializer.Deserialize<Request.ReadJson>(input);
                        if (data_ReadJson is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.ReadJson(new Request.ReadJson(data_ReadJson.FileKey));
                        break;
                    
                    case "rename": // Unused -- Full Path -- TODO Rename folders?
                        var data_Rename = JsonSerializer.Deserialize<Request.Rename>(input);
                        if (data_Rename is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.Rename(new Request.Rename(data_Rename.ObjKey, data_Rename.NewObjKey));
                        break;
                    
                    case "renamerelitivepath": // Used -- Relitive Path
                        var data_RenameRelitivePath = JsonSerializer.Deserialize<Request.RenameRelitivePath>(input);
                        if (data_RenameRelitivePath is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.RenameRelitivePath(new Request.RenameRelitivePath(data_RenameRelitivePath.ObjKey, data_RenameRelitivePath.NewObjKey, data_RenameRelitivePath.User));
                        break;

                    case "changecopyright": // Unused -- Full Path
                        var data_ChangeCopyright = JsonSerializer.Deserialize<Request.ChangeCopyright>(input);
                        if (data_ChangeCopyright is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.ChangeCopyright(new Request.ChangeCopyright(data_ChangeCopyright.ObjKey, data_ChangeCopyright.CopyrightValue));
                        break;
                    
                    case "changecopyrightrelitivepath": // used -- Relitive Path
                        var data_ChangeCopyrightRelitivePath = JsonSerializer.Deserialize<Request.ChangeCopyrightRelitivePath>(input);
                        if (data_ChangeCopyrightRelitivePath is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await S3.ChangeCopyrightRelitivePath(new Request.ChangeCopyrightRelitivePath(data_ChangeCopyrightRelitivePath.ObjKey, data_ChangeCopyrightRelitivePath.CopyrightValue, data_ChangeCopyrightRelitivePath.User));
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