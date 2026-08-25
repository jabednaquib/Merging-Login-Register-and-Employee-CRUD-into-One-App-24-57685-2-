using System;
using System.Windows.Forms;

namespace EmployeeDetails
{
    public partial class frmEmployee : Form
    {
        Employee employee = new Employee();

        public frmEmployee()
        {
            InitializeComponent();

            LoadEmployeeData();
        }


        // LOAD DATA
        private void LoadEmployeeData()
        {
            try
            {
                dgvEmployeeDetails.DataSource = employee.GetEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // ADD BUTTON
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                employee.EmpId = txtEmpId.Text.Trim();
                employee.EmpName = txtEmpName.Text.Trim();
                employee.Age = txtAge.Text.Trim();
                employee.ContactNo = txtContactNo.Text.Trim();
                employee.Gender = cboGender.SelectedItem.ToString();

                // Every newly-added employee is attributed to whoever is
                // currently logged in.
                employee.CreatedBy = Session.UserID;

                bool success = employee.InsertEmployee(employee);

                if (success)
                {
                    MessageBox.Show(
                        "Employee has been added successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    LoadEmployeeData();
                    ClearControls();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Insert Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // UPDATE BUTTON
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                employee.EmpId = txtEmpId.Text.Trim();
                employee.EmpName = txtEmpName.Text.Trim();
                employee.Age = txtAge.Text.Trim();
                employee.ContactNo = txtContactNo.Text.Trim();
                employee.Gender = cboGender.SelectedItem.ToString();

                bool success = employee.UpdateEmployee(employee);

                if (success)
                {
                    MessageBox.Show(
                        "Employee has been updated successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    LoadEmployeeData();
                    ClearControls();
                }
                else
                {
                    MessageBox.Show(
                        "Employee ID was not found.",
                        "Update",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Update Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // DELETE BUTTON
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmpId.Text))
            {
                MessageBox.Show(
                    "Please enter or select an Employee ID.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this employee?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                employee.EmpId = txtEmpId.Text.Trim();

                bool success = employee.DeleteEmployee(employee);

                if (success)
                {
                    MessageBox.Show(
                        "Employee has been deleted successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    LoadEmployeeData();
                    ClearControls();
                }
                else
                {
                    MessageBox.Show(
                        "Employee ID was not found.",
                        "Delete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Delete Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // CLEAR BUTTON
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearControls();
        }


        // CLEAR ALL TEXTBOXES
        private void ClearControls()
        {
            txtEmpId.Clear();
            txtEmpName.Clear();
            txtAge.Clear();
            txtContactNo.Clear();

            cboGender.SelectedIndex = -1;
            cboGender.Text = "";
        }


        // VALIDATION
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtEmpId.Text))
            {
                MessageBox.Show(
                    "Please enter Employee ID.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtEmpId.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmpName.Text))
            {
                MessageBox.Show(
                    "Please enter Employee Name.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtEmpName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAge.Text))
            {
                MessageBox.Show(
                    "Please enter Age.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtAge.Focus();
                return false;
            }

            int age;

            if (!int.TryParse(txtAge.Text, out age))
            {
                MessageBox.Show(
                    "Age must be a valid number.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtAge.Focus();
                return false;
            }

            if (age < 18 || age > 100)
            {
                MessageBox.Show(
                    "Age must be between 18 and 100.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtAge.Focus();
                return false;
            }

            if (cboGender.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Please select Gender.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                cboGender.Focus();
                return false;
            }

            return true;
        }


        // SELECT ROW
        private void dgvEmployeeDetails_RowHeaderMouseClick(
            object sender,
            DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row =
                dgvEmployeeDetails.Rows[e.RowIndex];

            txtEmpId.Text =
                row.Cells["EmpId"].Value?.ToString();

            txtEmpName.Text =
                row.Cells["EmpName"].Value?.ToString();

            txtAge.Text =
                row.Cells["EmpAge"].Value?.ToString();

            txtContactNo.Text =
                row.Cells["EmpContact"].Value?.ToString();

            cboGender.Text =
                row.Cells["EmpGender"].Value?.ToString();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
        }


        private void cboGender_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
        }


        private void dgvEmployeeDetails_CellContentClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
        }


        private void txtEmpId_TextChanged(
            object sender,
            EventArgs e)
        {
        }
    }
}
