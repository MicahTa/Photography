using Amazon.Lambda.Core;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PhotographyAPI;


public class Function
{
    // Handle API Call
    public async Task<JsonElement> FunctionHandler(JsonElement input, ILambdaContext context)
    {
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
                        message = await PageEdit.WriteTxtFile(data_WriteTxtFile);
                        break;
                    
                    case "getkeys": // Used -- Full Path
                        var data_GetKeys = JsonSerializer.Deserialize<Request.GetKeys>(input);
                        if (data_GetKeys is null) { message = Response.Error("Invalid Arguments"); break; }
                        if (data_GetKeys.PrefixName is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await PageEdit.GetKeys(data_GetKeys);
                        break;

                    case "getpresignedurl": // Used -- Full Path
                        var data_GetPreSignedURL = JsonSerializer.Deserialize<Request.GetPreSignedURL>(input);
                        if (data_GetPreSignedURL is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Pages.GetPreSignedURL(data_GetPreSignedURL);
                        break;
                    
                    case "putpresignedurl": // Used -- Full Path
                        var data_PutPreSignedURL = JsonSerializer.Deserialize<Request.PutPreSignedURL>(input);
                        if (data_PutPreSignedURL is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await PageEdit.PutPreSignedURL(data_PutPreSignedURL);
                        break;
                    
                    case "createfolderrelitivepath": // Used -- Relitive Path 
                        var data_CreateFolderRelitivePath = JsonSerializer.Deserialize<Request.CreateFolderRelitivePath>(input);
                        if (data_CreateFolderRelitivePath is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await PageEdit.CreateFolderRelitivePath(data_CreateFolderRelitivePath);
                        break;

                    case "deleterelitivepath": // used -- Relitive Path
                        var data_DeleteRelitivePath = JsonSerializer.Deserialize<Request.DeleteRelitivePath>(input);
                        if (data_DeleteRelitivePath is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await PageEdit.DeleteRelitivePath(data_DeleteRelitivePath);
                        break;

                    case "readpage": // Used -- Full Path
                        var data_ReadPage = JsonSerializer.Deserialize<Request.ReadPage>(input);
                        if (data_ReadPage is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Pages.ReadPage(data_ReadPage);
                        break;
                    
                    case "renamerelitivepath": // Used -- Relitive Path
                        var data_RenameRelitivePath = JsonSerializer.Deserialize<Request.RenameRelitivePath>(input);
                        if (data_RenameRelitivePath is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await PageEdit.RenameRelitivePath(data_RenameRelitivePath);
                        break;
                    
                    case "changecopyrightrelitivepath": // used -- Relitive Path
                        var data_ChangeCopyrightRelitivePath = JsonSerializer.Deserialize<Request.ChangeCopyrightRelitivePath>(input);
                        if (data_ChangeCopyrightRelitivePath is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await PageEdit.ChangeCopyrightRelitivePath(data_ChangeCopyrightRelitivePath);
                        break;
                        
                    case "createuser": // used
                        var data_CreateUser = JsonSerializer.Deserialize<Request.CreateUser>(input);
                        if (data_CreateUser is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Users.CreateUser(data_CreateUser);
                        break;
                    
                    case "doesuserexist": // unused
                        var data_DoesUserExist = JsonSerializer.Deserialize<Request.DoesUserExist>(input);
                        if (data_DoesUserExist is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Users.DoesUserExist(data_DoesUserExist);
                        break;
                    
                    case "addauthuser": // used
                        var data_AddAuthUser = JsonSerializer.Deserialize<Request.AddAuthUser>(input);
                        if (data_AddAuthUser is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Users.AddAuthUser(data_AddAuthUser);
                        break;
                    
                    case "removeauthuser": // used
                        var data_RemoveAuthUser = JsonSerializer.Deserialize<Request.RemoveAuthUser>(input);
                        if (data_RemoveAuthUser is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Users.RemoveAuthUser(data_RemoveAuthUser);
                        break;
                        
                    case "listauthusers": // used
                        var data_ListAuthUsers = JsonSerializer.Deserialize<Request.ListAuthUsers>(input);
                        if (data_ListAuthUsers is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Users.ListAuthUsers(data_ListAuthUsers);
                        break;

                    case "generatetoken": // used
                        var data_GenerateToken = JsonSerializer.Deserialize<Request.GenerateToken>(input);
                        if (data_GenerateToken is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Users.GenerateToken(data_GenerateToken);
                        break;
                        
                    case "changepageauth": // used
                        var data_ChangePageAuth = JsonSerializer.Deserialize<Request.ChangePageAuth>(input);
                        if (data_ChangePageAuth is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Users.ChangePageAuth(data_ChangePageAuth);
                        break;

                    case "listprojects": // used
                        var data_ListPages = JsonSerializer.Deserialize<Request.ListPages>(input);
                        if (data_ListPages is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Pages.ListPages(data_ListPages);
                        break;
                    
                    case "newpage": // used
                        var data_NewPage = JsonSerializer.Deserialize<Request.NewPage>(input);
                        if (data_NewPage is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Pages.NewPage(data_NewPage);
                        break;

                    case "deletepage": // used
                        var data_DeletePage = JsonSerializer.Deserialize<Request.DeletePage>(input);
                        if (data_DeletePage is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Pages.DeletePage(data_DeletePage);
                        break;
                    
                    case "copypage": // used
                        var data_CopyPage = JsonSerializer.Deserialize<Request.CopyPage>(input);
                        if (data_CopyPage is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Pages.CopyPage(data_CopyPage);
                        break;
                    
                    case "renamepage": // used
                        var data_RenamePage = JsonSerializer.Deserialize<Request.RenamePage>(input);
                        if (data_RenamePage is null) { message = Response.Error("Invalid Arguments"); break; }
                        message = await Pages.RenamePage(data_RenamePage);
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