using Amazon.Lambda.Core;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MySql.Data.MySqlClient;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Globalization;

namespace GetPreSignedURL;
public class GetPreSignedURLClass
{
    private static readonly string bucketName = "photographydata";
    private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast2; // Change as needed
    // private readonly IAmazonS3 s3Client;
    private static readonly IAmazonS3 s3Client = new AmazonS3Client(bucketRegion);

    public static async Task<JsonElement> GetPreSignedURL(Request.GetPreSignedURL data)
    {
        // Test incomming data
        if (!Request.Test(data.KeyName, data.User))
        {
            return Response.Error("Invalid Arguments");
        }
        // Create full path for S3
        string fullPrefix = $"{((data.FullRez ?? false) ? "u" : "LowRez")}/{data.User}/Pics/{data.KeyName}";

        // Do checks do download full content
        if (data.FullRez ?? false)
        {
            // Check credentials
            if (!(data.TokenedUser == null)){
                if (! await AuthToken(data.TokenedUser, data.Token)) {
                    return Response.Error("Invalid Token");
                }
            }
            // Extra checks if you dont own the page
            if (data.TokenedUser != data.User)
            {
                // Test if the required data for full Resolution downloading is there
                if (!Request.Test(data.Page ?? ""))
                {
                    return Response.Error("Invalid Arguments");
                } else if (!await TestPageDowloadablility(data.Page, data.User, data.TokenedUser))
                {
                    return Response.Error("Permission DENIED");
                }
            }
        }
        //TODO PERMISIONS for all?

        const int exp = 60; // Experation time 60 minuites 

        Response.GetPreSignedURL response = new Response.GetPreSignedURL($"Url for s3://{bucketName}/{fullPrefix} assigned expires in {exp} minuites\nthis is for uploading files only\nPUT to this link to write.");

        // Send request
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = fullPrefix,
            Verb = HttpVerb.GET, // GET allows uploading
            Expires = DateTime.UtcNow.AddMinutes(exp), // URL valid for 5 minutes
        };

        string url = s3Client.GetPreSignedURL(request);

        response.SetUrl(url);
        return response.Respond();
    }

    private static async Task<bool> TestPageDowloadablility(string page, string pageOwner, string user)
    {
        // Create sql connection and query
        string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await GetSQLPassword()};";

        const string query = @"
        SELECT a.id
        FROM authUsers a
        INNER JOIN pageRules p
            ON a.page = p.page
            AND a.user = p.user
        WHERE a.page = @page AND a.user = @pageOwner AND (a.authUser = @user or a.authUser = '*' ) AND p.authDownload = 1;
            ";
        // Connect to sql
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Run query
        await using var command = new MySqlCommand(query, connection);
        command.Parameters.Add("@page", MySqlDbType.VarChar).Value = page;
        command.Parameters.Add("@pageOwner", MySqlDbType.VarChar).Value = pageOwner;
        command.Parameters.Add("@user", MySqlDbType.VarChar).Value = user;
        using var reader = await command.ExecuteReaderAsync();

        // Read all rows
        while (await reader.ReadAsync())
        {
            return true;
        }
        // Nothing return means no user
        return false;
    }

    private static async Task<string> GetSQLPassword()
    {
        string secretName = "PhotographySQL";
        string region = "us-east-2";

        IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

        GetSecretValueRequest request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
        };

        GetSecretValueResponse response;

        try
        {
            response = await client.GetSecretValueAsync(request);
        }
        catch (Exception e)
        {
            // For a list of the exceptions thrown, see
            // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
            throw e;
        }

        string secret = response.SecretString;
        using var doc = JsonDocument.Parse(secret);
        if (doc.RootElement.TryGetProperty("password", out var pwElem))
        {
            string password = pwElem.GetString();
            return password;
        }
        else
        {
            return "";
        }
    }
    public static async Task<bool> AuthToken(string user, string token)
    {
        // Generate SQL Info
        string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await GetSQLPassword()};";
        const string query = @"
            SELECT tokenExpiration
            FROM Photography.users
            WHERE userName = @user AND token = @token;
        ";
        // Connect to sql
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Run query
        await using var command = new MySqlCommand(query, connection);
        command.Parameters.Add("@user", MySqlDbType.VarChar).Value = user;
        command.Parameters.Add("@token", MySqlDbType.VarChar).Value = token;
        using var reader = await command.ExecuteReaderAsync();

        // Read all rows
        while (await reader.ReadAsync())
        {
            // If theres anything to read user & token exists
            // Get experation
            string tokenExpiration = reader["tokenExpiration"].ToString() ?? "";
            DateTime expiration = DateTime.ParseExact(
                tokenExpiration,
                "M/d/yyyy h:mm:ss tt",
                CultureInfo.InvariantCulture
            );

            // Get time
            DateTime time = DateTime.Now;

            // Check if its expired
            if (expiration > time)
            {
                return true; // unexpried
            }
            else
            {
                return false; // Expried
            }
        }
        // Nothing return means no user token match
        return false;
    }
}