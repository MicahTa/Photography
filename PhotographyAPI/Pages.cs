using Amazon; // For RegionEndpoint
using Amazon.S3;
using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Text;
using Amazon.S3.Model;
using SixLabors.ImageSharp;

namespace PhotographyAPI
{
    public class Pages
    {
        private static readonly string bucketName = "photographydata";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast2; // Change as needed
        private static readonly IAmazonS3 s3Client = new AmazonS3Client(bucketRegion);

        public static async Task<JsonElement> ListPages(Request.ListPages data)
        {
            // Test incoming data
            if (!Request.Test(data.TokenedUser))
            {
                return Response.Error("Invalid Arguments");
            }
            // Get paths for aws
            string fullPrefix = $"u/{data.TokenedUser}/Pages/";
            if (!await Users.AuthToken(data.TokenedUser, data.Token))
            {
                return Response.Error($"Invalid Token/User");
            }

            Response.ListPages response = new Response.ListPages($"Projects have been returned");

            // Create request for s3
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = fullPrefix,
                Delimiter = "/" // ensures we get "folders" separately
            };

            var AWSresponse = await s3Client.ListObjectsV2Async(request);
            
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

            // Return information in array format
            var safeFiles = files?.ToArray() ?? Array.Empty<string>();
            response.SetProjectContents(safeFiles);

            return response.Respond();
        }


        // Rename a page
        public static async Task<JsonElement> RenamePage(Request.RenamePage data)
        {
            // Test incoming data
            if (!Request.Test(data.TokenedUser, data.Page, data.NewName))
            {
                return Response.Error("Invalid Arguments");
            }
            if (!await Users.AuthToken(data.TokenedUser, data.Token))
            {
                return Response.Error($"Invalid Token/User");
            }
            Response.RenamePage response = new Response.RenamePage($"Page Renamed");

            // Create sql query
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await Users.GetSQLPassword()};";
            const string query = @"
            UPDATE `Photography`.`pageRules`
            SET `page` = @newPageName
            WHERE (user = @user AND page = @page);
            ";

            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                // Run query
                    await using var command = new MySqlCommand(query, connection);
                    command.Parameters.Add("@page", MySqlDbType.VarChar).Value = data.Page;
                    command.Parameters.Add("@user", MySqlDbType.VarChar).Value = data.TokenedUser;
                    command.Parameters.Add("@newPageName", MySqlDbType.VarChar).Value = data.NewName;
                    int rows = await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                return Response.Error("Unexpected Error");
            }

            await PageEdit.Rename($"u/{data.TokenedUser}/Pages/{data.Page}", $"u/{data.TokenedUser}/Pages/{data.NewName}");
            return response.Respond();
        }

        // Delete page
        public static async Task<JsonElement> DeletePage(Request.DeletePage data)
        {
            // Test incoming data
            if (!Request.Test(data.TokenedUser, data.Page))
            {
                return Response.Error("Invalid Arguments");
            }
            if (!await Users.AuthToken(data.TokenedUser, data.Token))
            {
                return Response.Error($"Invalid Token/User");
            }

            Response.DeletePage response = new Response.DeletePage($"Page Deleted");

            // Query for page rules
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await Users.GetSQLPassword()};";
            const string query = @"
            DELETE FROM `Photography`.`pageRules`
            WHERE (page = @page AND user = @user);
            ";

            // run query for page rules
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(query, connection);
                command.Parameters.Add("@user", MySqlDbType.VarChar).Value = data.TokenedUser;
                command.Parameters.Add("@page", MySqlDbType.VarChar).Value = data.Page;
                int rows = await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                return Response.Error("Unexpected Error");
            }

            // Query for authorized users
            const string secondQuery = @"
            DELETE FROM `Photography`.`authUsers`
            WHERE (page = @page AND user = @user);
            ";

            // run query for authorized users
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                // Run query
                await using var command = new MySqlCommand(secondQuery, connection);
                command.Parameters.Add("@user", MySqlDbType.VarChar).Value = data.TokenedUser;
                command.Parameters.Add("@page", MySqlDbType.VarChar).Value = data.Page;
                int rows = await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                return Response.Error("Unexpected Error");
            }


            await PageEdit.Delete($"u/{data.TokenedUser}/Pages/{data.Page}");

            return response.Respond();
        }

        // Create new page
        public static async Task<JsonElement> NewPage(Request.NewPage data)
        {
            // Test incoming data
            if (!Request.Test(data.TokenedUser, data.PageName))
            {
                return Response.Error("Invalid Arguments");
            }
            if (!await Users.AuthToken(data.TokenedUser, data.Token))
            {
                return Response.Error($"Invalid Token/User");
            }

            Response.NewPage response = new Response.NewPage($"New Page Created");

            // Create new page in sql
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await Users.GetSQLPassword()};";
            const string query = @" INSERT INTO `Photography`.`pageRules` (`user`, `page`) VALUES (@user, @page); ";

            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                // Run query
                await using var command = new MySqlCommand(query, connection);
                command.Parameters.Add("@user", MySqlDbType.VarChar).Value = data.TokenedUser;
                command.Parameters.Add("@page", MySqlDbType.VarChar).Value = data.PageName;
                int rows = await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                return Response.Error("Unexpected Error");
            }

            // Create the empty json and write file
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"pageAccess\": \"Private\", \"images\": []}"));
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = $"u/{data.TokenedUser}/Pages/{data.PageName}",
                InputStream = stream,
                ContentType = "text/plain"
            };

            await s3Client.PutObjectAsync(putRequest);
            
            return response.Respond();
        }

        // Copy page
        public static async Task<JsonElement> CopyPage(Request.CopyPage data)
        {
            // Test incoming data
            if (!Request.Test(data.TokenedUser, data.Page))
            {
                return Response.Error("Invalid Arguments");
            }
            if (!await Users.AuthToken(data.TokenedUser, data.Token))
            {
                return Response.Error($"Invalid Token/User");
            }

            Response.CopyPage response = new Response.CopyPage($"New Page Created");

            // Create new page query
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await Users.GetSQLPassword()};";
            const string query = @" INSERT INTO `Photography`.`pageRules` (`user`, `page`) VALUES (@user, @page); ";

            // run create new page query
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                // Run query
                await using var command = new MySqlCommand(query, connection);
                command.Parameters.Add("@user", MySqlDbType.VarChar).Value = data.TokenedUser;
                command.Parameters.Add("@page", MySqlDbType.VarChar).Value = data.Page + " (Copy)";
                int rows = await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                return Response.Error("Unexpected Error");
            }

            // Copy json file
            var copyRequest = new CopyObjectRequest
            {
                SourceBucket = bucketName,
                SourceKey = $"u/{data.TokenedUser}/Pages/{data.Page}",
                DestinationBucket = bucketName,
                DestinationKey = $"u/{data.TokenedUser}/Pages/{data.Page} (Copy)"
            };

            await s3Client.CopyObjectAsync(copyRequest);
            
            
            return response.Respond();
        }

        // Read s3 file
        private static async Task<String?> ReadFile(string FileKey)
        {
            try
            {
                // Send request
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = FileKey
                };

                // Return streamed Data
                using (var apiResponse = await s3Client.GetObjectAsync(request))
                using (var reader = new StreamReader(apiResponse.ResponseStream))
                {
                    string content = await reader.ReadToEndAsync();
                    return content;
                }
            }
            catch (AmazonS3Exception e)
            {
                return null;
            }
        }

        // Test to see if a user can look at a project file
        private static async Task<bool> TestPageReadability(string page, string pageOwner, string user)
        {
            // Create sql connection and query
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await Users.GetSQLPassword()};";

            // Test to see if the user or * (public) is in the allowed users and if allowed users can open the page
            const string query = @"
            SELECT a.id
            FROM authUsers a
            INNER JOIN pageRules p
                ON a.page = p.page
                AND a.user = p.user
            WHERE a.page = @page AND a.user = @pageOwner AND (a.authUser = @user or a.authUser = '*' ) AND p.authOpen = 1;
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

        // Read project file json
        public static async Task<JsonElement> ReadPage(Request.ReadPage data)
        {
            // Test incomming data
            if (!Request.Test(data.User, data.Page))
            {
                return Response.Error("Invalid Arguments");
            }
            string fileKey = $"u/{data.User}/Pages/{data.Page}";

            // Test credentials if logged in
            if (data.TokenedUser != null)
            {
                if (! await Users.AuthToken(data.TokenedUser, data.Token)) {
                    return Response.Error("Invalid Token");
                }
            }

            Response.ReadPage response = new Response.ReadPage($"Succsesfully read json contents of {data.Page}");

            try
            {
                bool canRead;
                // Allow reading always for project owner
                if (data.TokenedUser == data.User)
                {
                    canRead = true;
                } else {
                    // Test page readability for user
                    canRead = await TestPageReadability(data.Page, data.User, data.TokenedUser);
                }
                if (canRead)
                {
                    // Read project file
                    string? fileContents = await ReadFile(fileKey);
                    if (fileContents is null)
                    {
                        return Response.Error("Unexpected null Value");
                    }

                    // Convert project file to json and return
                    using JsonDocument doc = JsonDocument.Parse(fileContents);
                    JsonElement root = doc.RootElement;
                    response.SetJsonContents(root);
                    return response.Respond();
                } else
                {
                    return Response.Error("Permission DENIED");
                }
            }
            catch {
                return Response.Error("Unexpected Error");
            }
        }

        // Get a presigned url that allows for downloading content
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
                    if (! await Users.AuthToken(data.TokenedUser, data.Token)) {
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

            const int exp = 5; // Experation time 5 minuites 

            Response.GetPreSignedURL response = new Response.GetPreSignedURL($"Url for s3://{bucketName}/{fullPrefix} assigned expires in {exp} minuites\nthis is for uploading files only\nPUT to this link to write.");

            // Send request
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = fullPrefix,
                Verb = HttpVerb.GET, // GET allows uploading
                Expires = DateTime.UtcNow.AddMinutes(exp),
            };

            string url = s3Client.GetPreSignedURL(request);

            response.SetUrl(url);
            return response.Respond();
        }

                // Test to see if a user can look at a project file
        private static async Task<bool> TestPageDowloadablility(string page, string pageOwner, string user)
        {
            // Create sql connection and query
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await Users.GetSQLPassword()};";

            // Test to see if user or * (public) are in allowed users and if they can download
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
    }
}
