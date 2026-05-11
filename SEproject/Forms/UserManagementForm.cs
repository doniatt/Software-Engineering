using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using AcademicProjectSystem.Database;

namespace AcademicProjectSystem.Forms
{
    public partial class UserManagementForm : BaseCrudPanel
    {
        private TextBox  txtUsername, txtFullName, txtEmail, txtPassword;
        private ComboBox cmbRole;
        private CheckBox chkActive;

        public UserManagementForm() : base("User Management")
        {
            // ── Form fields ───────────────────────────────────────────────────
            AddLabel("Full Name",  16, 14); txtFullName = AddTextBox(16, 32, 200);
            AddLabel("Username",  230, 14); txtUsername = AddTextBox(230, 32, 160);
            AddLabel("Password",  404, 14); txtPassword = AddTextBox(404, 32, 160);
            AddLabel("Email",      16, 78); txtEmail    = AddTextBox(16, 96, 280);
            AddLabel("Role",      310, 78); cmbRole     = AddComboBox(310, 96, 140);
            cmbRole.Items.AddRange(new[] { "Admin", "Supervisor", "Student" });
            cmbRole.SelectedIndex = 2;

            chkActive = new CheckBox
            {
                Text     = "Active",
                Location = new System.Drawing.Point(464, 100),
                AutoSize = true,
                Checked  = true,
                Font     = new System.Drawing.Font("Segoe UI", 9)
            };
            pnlForm.Controls.Add(chkActive);

        }

        protected override void LoadData()
        {
            try
            {
                grid.DataSource = DatabaseHelper.ExecuteQuery(
                    "SELECT UserID, Username, FullName, Email, Role, IsActive, CreatedAt FROM Users ORDER BY UserID");
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void SaveRecord()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text))
            { Error("Username and Full Name are required."); return; }

            int id = SelectedID("UserID");
            try
            {
                if (id == 0) // INSERT
                {
                    if (string.IsNullOrWhiteSpace(txtPassword.Text))
                    { Error("Password is required for new users."); return; }

                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Users (Username,PasswordHash,FullName,Email,Role,IsActive) " +
                        "VALUES (@u,@p,@fn,@e,@r,@a)",
                        new SqlParameter("@u",  txtUsername.Text.Trim()),
                        new SqlParameter("@p",  Helpers.SessionManager.HashPassword(txtPassword.Text)),
                        new SqlParameter("@fn", txtFullName.Text.Trim()),
                        new SqlParameter("@e",  txtEmail.Text.Trim()),
                        new SqlParameter("@r",  cmbRole.SelectedItem.ToString()),
                        new SqlParameter("@a",  chkActive.Checked));
                    Success("User added.");
                }
                else // UPDATE
                {
                    string pwSql = string.IsNullOrWhiteSpace(txtPassword.Text)
                        ? "" : ", PasswordHash=@p";
                    var prms = new System.Collections.Generic.List<SqlParameter>
                    {
                        new SqlParameter("@fn", txtFullName.Text.Trim()),
                        new SqlParameter("@e",  txtEmail.Text.Trim()),
                        new SqlParameter("@r",  cmbRole.SelectedItem.ToString()),
                        new SqlParameter("@a",  chkActive.Checked),
                        new SqlParameter("@id", id)
                    };
                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                        prms.Add(new SqlParameter("@p", Helpers.SessionManager.HashPassword(txtPassword.Text)));

                    DatabaseHelper.ExecuteNonQuery(
                        $"UPDATE Users SET FullName=@fn, Email=@e, Role=@r, IsActive=@a{pwSql} WHERE UserID=@id",
                        prms.ToArray());
                    Success("User updated.");
                }
                ClearFields();
                LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void DeleteRecord()
        {
            int id = SelectedID("UserID");
            if (id == 0) { Error("Select a user first."); return; }
            if (!Confirm("Delete this user?")) return;
            try
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Users WHERE UserID=@id",
                    new SqlParameter("@id", id));
                ClearFields();
                LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void ClearFields()
        {
            txtUsername.Clear(); txtFullName.Clear();
            txtEmail.Clear();    txtPassword.Clear();
            cmbRole.SelectedIndex = 2;
            chkActive.Checked = true;
            grid.CurrentCell = null;
        }

        protected override void OnRowSelected()
        {
            if (grid.CurrentRow == null) return;
            var row = grid.CurrentRow;
            txtFullName.Text  = row.Cells["FullName"].Value?.ToString();
            txtUsername.Text  = row.Cells["Username"].Value?.ToString();
            txtEmail.Text     = row.Cells["Email"].Value?.ToString();
            cmbRole.Text      = row.Cells["Role"].Value?.ToString();
            chkActive.Checked = row.Cells["IsActive"].Value is bool b && b;
            txtPassword.Clear();
        }
    }
}
