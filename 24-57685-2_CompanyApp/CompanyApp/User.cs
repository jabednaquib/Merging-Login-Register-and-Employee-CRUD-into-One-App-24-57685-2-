using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace EmployeeDetails
{
    /// <summary>
    /// Data-access class for dbo.Users. Mirrors the data-access style used in
    /// Employee.cs (ConfigurationManager for the connection string, using
    /// blocks for connections/commands, fully parameterized SQL).
    /// </summary>
    class User
    {
        private static readonly string myConn =
            ConfigurationManager.ConnectionStrings["connString"].ConnectionString;

        private const string ValidateLoginQuery =
            "SELECT UserID FROM dbo.Users WHERE Username = @Username AND Password = @Password";

        private const string UsernameExistsQuery =
            "SELECT COUNT(1) FROM dbo.Users WHERE Username = @Username";

        private const string RegisterUserQuery =
            "INSERT INTO dbo.Users (Username, Password) VALUES (@Username, @Password)";

        // VALIDATE LOGIN
        // Returns the UserID when the username/password match a row in
        // dbo.Users, or 0 when the login fails. Using named parameters
        // instead of concatenated SQL prevents SQL injection (e.g. a
        // password of ' OR '1'='1 no longer bypasses the check).
        public int ValidateLogin(string username, string password)
        {
            int userId = 0;

            using (SqlConnection con = new SqlConnection(myConn))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(ValidateLoginQuery, con))
                {
                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 200).Value = password;

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        userId = Convert.ToInt32(result);
                    }
                }
            }

            return userId;
        }

        // USERNAME EXISTS
        public bool UsernameExists(string username)
        {
            using (SqlConnection con = new SqlConnection(myConn))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(UsernameExistsQuery, con))
                {
                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;

                    int count = (int)cmd.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        // REGISTER USER
        public bool RegisterUser(string username, string password)
        {
            int rows;

            using (SqlConnection con = new SqlConnection(myConn))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(RegisterUserQuery, con))
                {
                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                    cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 200).Value = password;

                    rows = cmd.ExecuteNonQuery();
                }
            }

            return rows > 0;
        }
    }
}
