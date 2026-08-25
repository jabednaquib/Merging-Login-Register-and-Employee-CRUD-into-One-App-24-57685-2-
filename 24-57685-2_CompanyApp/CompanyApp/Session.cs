namespace EmployeeDetails
{
    /// <summary>
    /// Holds the currently logged-in user's identity for the lifetime of the
    /// application session. Populated by User.ValidateLogin() on successful
    /// login and cleared on logout.
    /// </summary>
    public static class Session
    {
        public static int UserID { get; set; }
        public static string Username { get; set; }

        public static void Clear()
        {
            UserID = 0;
            Username = null;
        }
    }
}
