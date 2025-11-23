using System.Security.Cryptography;
using System.Globalization;
using Amazon; // For RegionEndpoint
using Amazon.S3;
using System.Text.Json;
using MySql.Data.MySqlClient;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;


namespace PhotographyAPI
{
    public class Users
    {
        private static readonly string bucketName = "photographydata";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast2; // Change as needed
        private readonly IAmazonS3 s3Client = new AmazonS3Client(bucketRegion);



        // Get the sql password for aws seceret manager
        public static async Task<string> GetSQLPassword()
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

        // Generates user tokens
        public static string GenerateSecureToken(int byteLength = 32)
        {
            // 32 bytes = 256 bits of entropy (very strong)
            byte[] randomBytes = new byte[byteLength];
            RandomNumberGenerator.Fill(randomBytes);

            // Convert to a URL-safe Base64 string
            string token = Convert.ToBase64String(randomBytes)
                .Replace("\'", "-")  // URL safe
                .Replace("\"", "_")
                .TrimEnd('=');
            token = Uri.EscapeDataString(token);

            return token;
        }

        // Create a user
        public static async Task<JsonElement> CreateUser(Request.CreateUser data)
        {
            // Verify data is correct
            if (!Request.Test(data.Username, data.Email, data.Password))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.CreateUser response = new Response.CreateUser("User Created");
            // Generate user creation query
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await GetSQLPassword()};";
            const string query = @"
            INSERT INTO `Photography`.`users` 
            (`userName`, `email`, `hashedPswd`)
            VALUES (@username, @email, @pswdHash)
            ";

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(data.Password, workFactor: 12);

            try {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                // Run query
                try
                {
                    await using var command = new MySqlCommand(query, connection);
                    command.Parameters.Add("@username", MySqlDbType.VarChar).Value = data.Username;
                    command.Parameters.Add("@email", MySqlDbType.VarChar).Value = data.Email;
                    command.Parameters.Add("@pswdHash", MySqlDbType.VarChar).Value = hashedPassword;
                    int rows = await command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    return Response.Error("Could Not Create User");
                }

                // Create Folders
                bool folderCreation = await PageEdit.CreateFolder($"u/{data.Username}") && await PageEdit.CreateFolder($"LowRez/{data.Username}");
                folderCreation = folderCreation && await PageEdit.CreateFolder($"u/{data.Username}/Pics") && await PageEdit.CreateFolder($"u/{data.Username}/Pages") && await PageEdit.CreateFolder($"LowRez/{data.Username}/Pics");


                if (folderCreation)
                {
                    return response.Respond();
                }
                else
                {
                    return Response.Error("Could Not Create User");
                } 
            } catch (Exception e) {
                return Response.Error("User not Created");
            }
        }

        public static async Task<bool> AuthToken(string user, string token)
        {
            // Generate Query to add token
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

        // Add authorized user
        public static async Task<JsonElement> AddAuthUser(Request.AddAuthUser data)
        {
            // Verify data is correct
            if (!Request.Test(data.User, data.Page))
            {
                return Response.Error("Invalid Arguments");
            }
            if (!await AuthToken(data.TokenedUser, data.Token))
            {
                return Response.Error($"Invalid Token/User");
            }

            Response.AddAuthUser response = new Response.AddAuthUser("Added new authorized user");
            // Generate query to add user to list
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await GetSQLPassword()};";
            const string query = @"
            INSERT INTO `Photography`.`authUsers`
            (`page`, `user`, `authUser`)
            VALUES (@page, @pageOwner, @newAuthUser);
            ";


            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                // Run query
                try
                {
                    await using var command = new MySqlCommand(query, connection);
                    command.Parameters.Add("@page", MySqlDbType.VarChar).Value = data.Page;
                    command.Parameters.Add("@pageOwner", MySqlDbType.VarChar).Value = data.TokenedUser;
                    command.Parameters.Add("@newAuthUser", MySqlDbType.VarChar).Value = data.User;
                    int rows = await command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    response.SetAuthUpdation(false);
                    return response.Respond();
                }
                response.SetAuthUpdation(true);
                return response.Respond();

            }
            catch (Exception e)
            {
                return Response.Error("Unexpected Error");
            }
        }

        // Remove authorized user
        public static async Task<JsonElement> RemoveAuthUser(Request.RemoveAuthUser data)
        {
            // Verify data is correct
            if (!Request.Test(data.User, data.Page))
            {
                return Response.Error("Invalid Arguments");
            }
            if (!await AuthToken(data.TokenedUser, data.Token))
            {
                return Response.Error($"Invalid Token/User");
            }

            Response.RemoveAuthUser response = new Response.RemoveAuthUser("Removed authorized user");
            // Generate query to remove username from list
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await GetSQLPassword()};";
            string query;
                query = @"
                DELETE FROM `Photography`.`authUsers`
                WHERE (page = @page AND user = @pageOwner AND authUser = @authUser);
                ";


            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                // Run query
                try
                {
                    await using var command = new MySqlCommand(query, connection);
                    command.Parameters.Add("@page", MySqlDbType.VarChar).Value = data.Page;
                    command.Parameters.Add("@pageOwner", MySqlDbType.VarChar).Value = data.TokenedUser;
                    if (data.User != "*")
                    {
                        command.Parameters.Add("@authUser", MySqlDbType.VarChar).Value = data.User;
                    }
                    int rows = await command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    response.SetAuthUpdation(false);
                    return response.Respond();
                }
                response.SetAuthUpdation(true);
                return response.Respond();

            }
            catch (Exception e)
            {
                return Response.Error("Unexpected Error");
            }
        }
        
        // Generate a list of authorized users        
        public static async Task<JsonElement> ListAuthUsers(Request.ListAuthUsers data)
        {
            // Verify data is correct
            if (!Request.Test(data.Page))
            {
                return Response.Error("Invalid Arguments");
            }
            if (!await AuthToken(data.TokenedUser, data.Token))
            {
                return Response.Error($"Invalid Token/User");
            }
            Response.ListAuthUsers response = new Response.ListAuthUsers("Retreved list of authorized users");

            // Generate SQL Info
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await GetSQLPassword()};";
            const string query = @"
                SELECT authUser FROM Photography.authUsers
                WHERE page = @page AND user = @pageOwner;
            ";
            try {
                // Connect to sql
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                Console.WriteLine($"Page: {data.Page}");
                Console.WriteLine($"TokenedUser: {data.TokenedUser}");

                // Run query
                await using var command = new MySqlCommand(query, connection);
                command.Parameters.Add("@page", MySqlDbType.VarChar).Value = data.Page;
                command.Parameters.Add("@pageOwner", MySqlDbType.VarChar).Value = data.TokenedUser;
                using var reader = await command.ExecuteReaderAsync();

                // Read all rows
                var authenticatedUsers = new List<string>();
                while (await reader.ReadAsync())
                {
                    string? user = reader["authUser"].ToString();
                    Console.WriteLine($"user: {user}");
                    if (user != null)
                    {
                        authenticatedUsers.Add(user);
                    }
                }
                var safeAuthenticatedUsers = authenticatedUsers?.ToArray() ?? Array.Empty<string>();
                response.SetUsers(safeAuthenticatedUsers);

                return response.Respond(); 
            } catch {
                return Response.Error("Unexpected Error");
            }

        }
        // Change page authorization settings
        public static async Task<JsonElement> ChangePageAuth(Request.ChangePageAuth data)
        {
            // Verify data is correct
            if (!Request.Test(data.Page))
            {
                return Response.Error("Invalid Arguments");
            }
            if (!await AuthToken(data.TokenedUser, data.Token))
            {
                return Response.Error($"Invalid Token/User");
            }

            Response.ChangePageAuth response = new Response.ChangePageAuth("Changed page authorization settings");
            // Generate SQL Info
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await GetSQLPassword()};";
            const string query = @"
            UPDATE `Photography`.`pageRules`
            SET `authOpen` = @open, `authDownload` = @download
            WHERE (user = @user AND page = @page);
            ";


            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                // Run query
                try
                {
                    await using var command = new MySqlCommand(query, connection);
                    command.Parameters.Add("@page", MySqlDbType.VarChar).Value = data.Page;
                    command.Parameters.Add("@user", MySqlDbType.VarChar).Value = data.TokenedUser;
                    command.Parameters.Add("@open", MySqlDbType.VarChar).Value = data.Open ? 1 : 0;
                    command.Parameters.Add("@download", MySqlDbType.VarChar).Value = data.Download ? 1 : 0;
                    int rows = await command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    return Response.Error("Unexpected Error");
                }
                return response.Respond();

            }
            catch (Exception e)
            {
                return Response.Error("Unexpected Error");
            }
        }
        // Test if user exists
        public static async Task<JsonElement> DoesUserExist(Request.DoesUserExist data)
        {
            // Verify data is correct
            if (!Request.Test(data.User))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.DoesUserExist response = new Response.DoesUserExist("Tested user existance!");

            // Generate SQL Info
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await GetSQLPassword()};";
            const string query = @"
                SELECT userName
                FROM Photography.users
                WHERE userName = @user OR email = @user;
            ";

            try
            {
                // Connect to sql
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                // Run query
                await using var command = new MySqlCommand(query, connection);
                command.Parameters.Add("@user", MySqlDbType.VarChar).Value = data.User;
                using var reader = await command.ExecuteReaderAsync();

                // Read all rows
                while (await reader.ReadAsync())
                {
                    // If theres anything to read user exists
                    string username = reader["username"].ToString() ?? "";
                    response.SetUserExistance(true);
                    return response.Respond();
                }
                // Nothing return means no user
                response.SetUserExistance(false);
                return response.Respond();
            }
            catch (Exception ex)
            {
                return Response.Error(ex.Message);
            }
        }

        // Generate token for user
        public static async Task<JsonElement> GenerateToken(Request.GenerateToken data)
        {
            // Verify Data is correct
            if (!Request.Test(data.User, data.Password))
            {
                return Response.Error("Invalid Arguments");
            }
            Response.GenerateToken response = new Response.GenerateToken("Token Generated!");

            // Generate SQL info
            string ConnectionString = $"server=micah.is-a-techie.com;Database=Photography;User ID=root;Password={await GetSQLPassword()};";
            const string query = @"
                SELECT id, hashedPswd
                FROM Photography.users
                WHERE userName = @user;
            ";

            try
            {
                // Connect to server
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();

                // Run the query getting user
                await using var command = new MySqlCommand(query, connection);
                command.Parameters.Add("@user", MySqlDbType.VarChar).Value = data.User;

                // Ready data
                string id = null;
                string hashedPswd = null;

                // Read data
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        id = reader["id"].ToString();
                        hashedPswd = reader["hashedPswd"].ToString();
                    }
                }

                // Check if user exists
                if (id == null)
                    return Response.Error("User does not exist");

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(data.Password, hashedPswd))
                    return Response.Error("Incorrect Password");


                // Generate Token
                string token = GenerateSecureToken();
                response.SetToken(token);

                // Generate expiration time
                DateTime oneHourLater = DateTime.Now.AddHours(5);
                string formattedTime = oneHourLater.ToString("yyyy-MM-dd HH:mm:ss");

                // Update SQL (parameterized to avoid SQL injection)
                string update = "UPDATE `Photography`.`users` SET `token` = @token, `tokenExpiration` = @expiration WHERE `id` = @id;";
                await using var updateCommand = new MySqlCommand(update, connection);
                updateCommand.Parameters.Add("@token", MySqlDbType.VarChar).Value = token;
                updateCommand.Parameters.Add("@expiration", MySqlDbType.DateTime).Value = oneHourLater;
                updateCommand.Parameters.Add("@id", MySqlDbType.Int32).Value = int.Parse(id);

                int rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                return response.Respond();

            }
            catch (Exception ex)
            {
                return Response.Error(ex.Message);
            }
        }



    }
}
