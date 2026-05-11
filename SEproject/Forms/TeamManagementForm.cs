using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using AcademicProjectSystem.Database;

namespace AcademicProjectSystem.Forms
{
    public partial class TeamManagementForm : BaseCrudPanel
    {
        private TextBox  txtName;
        private ComboBox cmbSupervisor, cmbAddStudent;
        private ListBox  lstMembers;
        private Button   btnAddMember, btnRemoveMember;
        private int      currentTeamID = 0;

        public TeamManagementForm() : base("Team Management")
        {
            AddLabel("Team Name",   16, 14); txtName       = AddTextBox(16, 32, 200);
            AddLabel("Supervisor", 230, 14); cmbSupervisor = AddComboBox(230, 32, 220);
            LoadSupervisors();

            AddLabel("Members", 16, 80);
            lstMembers = new ListBox
            {
                Location  = new System.Drawing.Point(16, 98),
                Size      = new System.Drawing.Size(200, 120),
                Font      = new System.Drawing.Font("Segoe UI", 9)
            };
            pnlForm.Controls.Add(lstMembers);

            AddLabel("Add Student", 230, 80);
            cmbAddStudent = AddComboBox(230, 98, 200);
            LoadStudentCombo();

            btnAddMember = new Button
            {
                Text      = "Add →",
                Location  = new System.Drawing.Point(230, 132),
                Size      = new System.Drawing.Size(90, 28),
                BackColor = System.Drawing.Color.FromArgb(40, 167, 69),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnAddMember.FlatAppearance.BorderSize = 0;
            btnAddMember.Click += BtnAddMember_Click;
            pnlForm.Controls.Add(btnAddMember);

            btnRemoveMember = new Button
            {
                Text      = "Remove",
                Location  = new System.Drawing.Point(230, 168),
                Size      = new System.Drawing.Size(90, 28),
                BackColor = System.Drawing.Color.FromArgb(220, 53, 69),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnRemoveMember.FlatAppearance.BorderSize = 0;
            btnRemoveMember.Click += BtnRemoveMember_Click;
            pnlForm.Controls.Add(btnRemoveMember);

        }

        private void LoadSupervisors()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    "SELECT UserID, FullName FROM Users WHERE Role='Supervisor' AND IsActive=1");
                cmbSupervisor.DataSource    = dt;
                cmbSupervisor.DisplayMember = "FullName";
                cmbSupervisor.ValueMember   = "UserID";
            }
            catch { }
        }

        private void LoadStudentCombo()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    "SELECT s.StudentID, u.FullName FROM Students s JOIN Users u ON s.UserID=u.UserID");
                cmbAddStudent.DataSource    = dt;
                cmbAddStudent.DisplayMember = "FullName";
                cmbAddStudent.ValueMember   = "StudentID";
            }
            catch { }
        }

        private void LoadMembers(int teamID)
        {
            lstMembers.Items.Clear();
            try
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    "SELECT u.FullName FROM TeamMembers tm " +
                    "JOIN Students s ON tm.StudentID=s.StudentID " +
                    "JOIN Users u ON s.UserID=u.UserID WHERE tm.TeamID=@t",
                    new SqlParameter("@t", teamID));
                foreach (System.Data.DataRow r in dt.Rows)
                    lstMembers.Items.Add(r["FullName"]);
            }
            catch { }
        }

        protected override void LoadData()
        {
            try
            {
                grid.DataSource = DatabaseHelper.ExecuteQuery(
                    "SELECT t.TeamID, t.TeamName, u.FullName AS Supervisor, t.CreatedAt " +
                    "FROM Teams t LEFT JOIN Users u ON t.SupervisorID=u.UserID ORDER BY t.TeamID");
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void SaveRecord()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { Error("Team name required."); return; }
            int supID = (int)(cmbSupervisor.SelectedValue ?? 0);
            try
            {
                if (currentTeamID == 0)
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Teams (TeamName,SupervisorID) VALUES (@n,@s)",
                        new SqlParameter("@n", txtName.Text.Trim()),
                        new SqlParameter("@s", supID));
                }
                else
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Teams SET TeamName=@n,SupervisorID=@s WHERE TeamID=@id",
                        new SqlParameter("@n",  txtName.Text.Trim()),
                        new SqlParameter("@s",  supID),
                        new SqlParameter("@id", currentTeamID));
                }
                Success("Team saved."); ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void DeleteRecord()
        {
            if (currentTeamID == 0 || !Confirm("Delete team?")) return;
            try
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM TeamMembers WHERE TeamID=@id",
                    new SqlParameter("@id", currentTeamID));
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Teams WHERE TeamID=@id",
                    new SqlParameter("@id", currentTeamID));
                ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void ClearFields()
        {
            txtName.Clear(); currentTeamID = 0;
            lstMembers.Items.Clear();
            grid.ClearSelection();
        }

        protected override void OnRowSelected()
        {
            if (grid.CurrentRow == null) return;
            currentTeamID   = (int)grid.CurrentRow.Cells["TeamID"].Value;
            txtName.Text    = grid.CurrentRow.Cells["TeamName"].Value?.ToString();
            LoadMembers(currentTeamID);
        }

        private void BtnAddMember_Click(object sender, EventArgs e)
        {
            if (currentTeamID == 0) { Error("Save the team first."); return; }
            int sid = (int)(cmbAddStudent.SelectedValue ?? 0);
            if (sid == 0) return;
            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "IF NOT EXISTS (SELECT 1 FROM TeamMembers WHERE TeamID=@t AND StudentID=@s) " +
                    "INSERT INTO TeamMembers (TeamID,StudentID) VALUES (@t,@s)",
                    new SqlParameter("@t", currentTeamID),
                    new SqlParameter("@s", sid));
                LoadMembers(currentTeamID);
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        private void BtnRemoveMember_Click(object sender, EventArgs e)
        {
            if (lstMembers.SelectedIndex < 0 || currentTeamID == 0) return;
            string name = lstMembers.SelectedItem.ToString();
            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE tm FROM TeamMembers tm " +
                    "JOIN Students s ON tm.StudentID=s.StudentID " +
                    "JOIN Users u ON s.UserID=u.UserID " +
                    "WHERE tm.TeamID=@t AND u.FullName=@n",
                    new SqlParameter("@t", currentTeamID),
                    new SqlParameter("@n", name));
                LoadMembers(currentTeamID);
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
