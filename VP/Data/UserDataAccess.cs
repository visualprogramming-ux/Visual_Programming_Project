using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Project.Data
{
    public class UserDataAccess
    {
        // Check if email exists
        public static bool EmailExists(string email)
        {
            string query = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);
                connection.Open();
                return (int)command.ExecuteScalar() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking email: {ex.Message}", ex);
            }
        }

        // Authenticate user (login)
        public static bool AuthenticateUser(string email, string passwordHash)
        {
            string query = @"
                SELECT COUNT(1) 
                FROM Users 
                WHERE Email = @Email 
                AND PasswordHash = @PasswordHash 
                AND IsActive = 1";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                connection.Open();
                return (int)command.ExecuteScalar() == 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error authenticating user: {ex.Message}", ex);
            }
        }

        // Create a new user account
        public static int CreateUser(string firstName, string lastName, string email, string passwordHash)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                connection.Open();
                
                // Check which columns exist
                string checkColumnsQuery = @"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Users' 
                    AND COLUMN_NAME IN ('FirstName', 'LastName', 'Username', 'Name')";
                
                using var checkCmd = new SqlCommand(checkColumnsQuery, connection);
                var existingColumns = new System.Collections.Generic.HashSet<string>();
                using var reader = checkCmd.ExecuteReader();
                while (reader.Read())
                {
                    existingColumns.Add(reader.GetString(0));
                }
                reader.Close();

                // Build query based on existing columns
                string insertQuery;
                SqlCommand command;
                
                if (existingColumns.Contains("FirstName") && existingColumns.Contains("LastName"))
                {
                    insertQuery = @"
                        INSERT INTO Users (FirstName, LastName, Email, PasswordHash, IsActive, CreatedAt, UpdatedAt)
                        OUTPUT INSERTED.UserId
                        VALUES (@FirstName, @LastName, @Email, @PasswordHash, 1, GETDATE(), GETDATE())";
                    command = new SqlCommand(insertQuery, connection);
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName ?? (object)DBNull.Value);
                }
                else if (existingColumns.Contains("Username"))
                {
                    string username = !string.IsNullOrEmpty(lastName) ? $"{firstName} {lastName}" : firstName;
                    insertQuery = @"
                        INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt, UpdatedAt)
                        OUTPUT INSERTED.UserId
                        VALUES (@Username, @Email, @PasswordHash, 1, GETDATE(), GETDATE())";
                    command = new SqlCommand(insertQuery, connection);
                    command.Parameters.AddWithValue("@Username", username);
                }
                else if (existingColumns.Contains("Name"))
                {
                    string fullName = !string.IsNullOrEmpty(lastName) ? $"{firstName} {lastName}" : firstName;
                    insertQuery = @"
                        INSERT INTO Users (Name, Email, PasswordHash, IsActive, CreatedAt, UpdatedAt)
                        OUTPUT INSERTED.UserId
                        VALUES (@Name, @Email, @PasswordHash, 1, GETDATE(), GETDATE())";
                    command = new SqlCommand(insertQuery, connection);
                    command.Parameters.AddWithValue("@Name", fullName);
                }
                else
                {
                    // Just use Email and PasswordHash if no name columns exist
                    insertQuery = @"
                        INSERT INTO Users (Email, PasswordHash, IsActive, CreatedAt, UpdatedAt)
                        OUTPUT INSERTED.UserId
                        VALUES (@Email, @PasswordHash, 1, GETDATE(), GETDATE())";
                    command = new SqlCommand(insertQuery, connection);
                }
                
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                
                return (int)command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating user: {ex.Message}", ex);
            }
        }

        // Hash password using SHA256
        public static string HashPassword(string password)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // Get user by email
        public static UserInfo? GetUserByEmail(string email)
        {
            string query = @"
                SELECT UserId, Email, IsActive
                FROM Users
                WHERE Email = @Email";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                connection.Open();
                
                // Check which name columns exist
                string checkColumnsQuery = @"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Users' 
                    AND COLUMN_NAME IN ('FirstName', 'LastName', 'Username', 'Name')";
                
                using var checkCmd = new SqlCommand(checkColumnsQuery, connection);
                var existingColumns = new System.Collections.Generic.HashSet<string>();
                using var checkReader = checkCmd.ExecuteReader();
                while (checkReader.Read())
                {
                    existingColumns.Add(checkReader.GetString(0));
                }
                checkReader.Close();

                // Build query based on existing columns
                if (existingColumns.Contains("FirstName") && existingColumns.Contains("LastName"))
                {
                    query = "SELECT UserId, FirstName, LastName, Email, IsActive FROM Users WHERE Email = @Email";
                }
                else if (existingColumns.Contains("Username"))
                {
                    query = "SELECT UserId, Username, Email, IsActive FROM Users WHERE Email = @Email";
                }
                else if (existingColumns.Contains("Name"))
                {
                    query = "SELECT UserId, Name, Email, IsActive FROM Users WHERE Email = @Email";
                }

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int userIdOrd = reader.GetOrdinal("UserId");
                    int emailOrd = reader.GetOrdinal("Email");
                    int isActiveOrd = reader.GetOrdinal("IsActive");

                    var userInfo = new UserInfo
                    {
                        UserId = reader.GetInt32(userIdOrd),
                        Email = reader.IsDBNull(emailOrd) ? "" : reader.GetString(emailOrd),
                        IsActive = !reader.IsDBNull(isActiveOrd) && reader.GetBoolean(isActiveOrd)
                    };

                    // Try to get name fields if they exist
                    if (existingColumns.Contains("FirstName"))
                    {
                        int firstNameOrd = reader.GetOrdinal("FirstName");
                        int lastNameOrd = reader.GetOrdinal("LastName");
                        userInfo.FirstName = reader.IsDBNull(firstNameOrd) ? "" : reader.GetString(firstNameOrd);
                        userInfo.LastName = reader.IsDBNull(lastNameOrd) ? "" : reader.GetString(lastNameOrd);
                    }
                    else if (existingColumns.Contains("Username"))
                    {
                        int usernameOrd = reader.GetOrdinal("Username");
                        string username = reader.IsDBNull(usernameOrd) ? "" : reader.GetString(usernameOrd);
                        var nameParts = username.Split(new[] { ' ' }, 2);
                        userInfo.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
                        userInfo.LastName = nameParts.Length > 1 ? nameParts[1] : "";
                    }

                    return userInfo;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting user: {ex.Message}", ex);
            }

            return null;
        }

        public class UserInfo
        {
            public int UserId { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }
    }
}

