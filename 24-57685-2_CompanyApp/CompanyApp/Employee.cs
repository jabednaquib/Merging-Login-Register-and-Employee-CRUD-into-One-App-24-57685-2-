using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace EmployeeDetails
{
    class Employee
    {
        private static readonly string myConn =
            ConfigurationManager.ConnectionStrings["connString"].ConnectionString;

        public string EmpId { get; set; }
        public string EmpName { get; set; }
        public string Age { get; set; }
        public string ContactNo { get; set; }
        public string Gender { get; set; }

        // Nullable: migrated employees may not have a known creator.
        public int? CreatedBy { get; set; }

        // SELECT uses a LEFT JOIN (not INNER JOIN) against dbo.Users so that
        // employees whose CreatedBy is NULL (e.g. rows migrated from the old
        // system) still appear in the grid, with a blank creator column.
        private const string SelectQuery =
            "SELECT e.EmpId, e.EmpName, e.EmpAge, e.EmpContact, e.EmpGender, " +
            "e.CreatedBy, u.Username AS CreatedByUsername " +
            "FROM dbo.Emp_details e " +
            "LEFT JOIN dbo.Users u ON e.CreatedBy = u.UserID " +
            "ORDER BY e.EmpId";

        private const string InsertQuery =
            "INSERT INTO dbo.Emp_details " +
            "(EmpId, EmpName, EmpAge, EmpContact, EmpGender, CreatedBy) " +
            "VALUES (@EmpId, @EmpName, @EmpAge, @EmpContact, @EmpGender, @CreatedBy)";

        private const string UpdateQuery =
            "UPDATE dbo.Emp_details SET " +
            "EmpName = @EmpName, " +
            "EmpAge = @EmpAge, " +
            "EmpContact = @EmpContact, " +
            "EmpGender = @EmpGender " +
            "WHERE EmpId = @EmpId";

        private const string DeleteQuery =
            "DELETE FROM dbo.Emp_details WHERE EmpId = @EmpId";


        // GET ALL EMPLOYEES (including creator username via LEFT JOIN)
        public DataTable GetEmployees()
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection con = new SqlConnection(myConn))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(SelectQuery, con))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Could not load employee data.\n\n" +
                    "Please check your SQL Server connection.\n\n" +
                    ex.Message);
            }

            return dataTable;
        }


        // INSERT EMPLOYEE
        public bool InsertEmployee(Employee employee)
        {
            int rows;

            using (SqlConnection con = new SqlConnection(myConn))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(InsertQuery, con))
                {
                    cmd.Parameters.Add("@EmpId", SqlDbType.NVarChar, 50)
                        .Value = employee.EmpId;

                    cmd.Parameters.Add("@EmpName", SqlDbType.NVarChar, 100)
                        .Value = employee.EmpName;

                    cmd.Parameters.Add("@EmpAge", SqlDbType.Int)
                        .Value = int.Parse(employee.Age);

                    cmd.Parameters.Add("@EmpContact", SqlDbType.NVarChar, 20)
                        .Value = string.IsNullOrWhiteSpace(employee.ContactNo)
                            ? (object)DBNull.Value
                            : employee.ContactNo;

                    cmd.Parameters.Add("@EmpGender", SqlDbType.NVarChar, 10)
                        .Value = employee.Gender;

                    cmd.Parameters.Add("@CreatedBy", SqlDbType.Int)
                        .Value = employee.CreatedBy.HasValue
                            ? (object)employee.CreatedBy.Value
                            : DBNull.Value;

                    rows = cmd.ExecuteNonQuery();
                }
            }

            return rows > 0;
        }


        // UPDATE EMPLOYEE
        public bool UpdateEmployee(Employee employee)
        {
            int rows;

            using (SqlConnection con = new SqlConnection(myConn))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(UpdateQuery, con))
                {
                    cmd.Parameters.Add("@EmpName", SqlDbType.NVarChar, 100)
                        .Value = employee.EmpName;

                    cmd.Parameters.Add("@EmpAge", SqlDbType.Int)
                        .Value = int.Parse(employee.Age);

                    cmd.Parameters.Add("@EmpContact", SqlDbType.NVarChar, 20)
                        .Value = string.IsNullOrWhiteSpace(employee.ContactNo)
                            ? (object)DBNull.Value
                            : employee.ContactNo;

                    cmd.Parameters.Add("@EmpGender", SqlDbType.NVarChar, 10)
                        .Value = employee.Gender;

                    cmd.Parameters.Add("@EmpId", SqlDbType.NVarChar, 50)
                        .Value = employee.EmpId;

                    rows = cmd.ExecuteNonQuery();
                }
            }

            return rows > 0;
        }


        // DELETE EMPLOYEE
        public bool DeleteEmployee(Employee employee)
        {
            int rows;

            using (SqlConnection con = new SqlConnection(myConn))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(DeleteQuery, con))
                {
                    cmd.Parameters.Add("@EmpId", SqlDbType.NVarChar, 50)
                        .Value = employee.EmpId;

                    rows = cmd.ExecuteNonQuery();
                }
            }

            return rows > 0;
        }
    }
}
