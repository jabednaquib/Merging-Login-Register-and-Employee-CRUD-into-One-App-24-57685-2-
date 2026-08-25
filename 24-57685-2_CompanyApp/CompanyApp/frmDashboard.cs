using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmployeeDetails
{
    public partial class frmDashboard : Form
    {
        public frmDashboard()
        {
            InitializeComponent();
        }

        private void visitWeb_Click(object sender, EventArgs e)
        {
            bmBrowser.Navigate("https://bloggingmetrics.com/");
        }

        // A logged-in user reaches Employee CRUD only through this button,
        // which only exists on frmDashboard, which in turn is only reachable
        // after a successful login.
        private void btnManageEmployees_Click(object sender, EventArgs e)
        {
            using (frmEmployee employeeForm = new frmEmployee())
            {
                employeeForm.ShowDialog(this);
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            Session.Clear();

            // A brand-new frmLogin is created instead of reusing the old
            // hidden one, so there is never a stale/orphaned login form
            // sitting behind the dashboard.
            frmLogin login = new frmLogin();
            login.Show();

            this.Close();
        }

        private void frmDashboard_Load(object sender, EventArgs e)
        {

        }
    }
}
