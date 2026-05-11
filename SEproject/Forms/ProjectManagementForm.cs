using System;
using System.Data.SqlClient;
using AcademicProjectSystem.Database;

namespace AcademicProjectSystem.Forms
{
    public partial class ProjectManagementForm : BaseCrudPanel
    {
        private System.Windows.Forms.TextBox txtTitle, txtDesc;
        private System.Windows.Forms.ComboBox cmbCategory, cmbSupervisor, cmbTeam, cmbStatus;
        private System.Windows.Forms.DateTimePicker dtDeadline;

        public ProjectManagementForm() : base("Project Management")
        {
            // ?? Scrollable container docked to the bottom of the base panel ??????????
            var scrollPanel = new System.Windows.Forms.Panel
            {
                AutoScroll = true,
                Dock = System.Windows.Forms.DockStyle.Bottom,
                Height = 240,   // visible height; scrollbar appears if content is taller
                BorderStyle = System.Windows.Forms.BorderStyle.None
            };
            this.Controls.Add(scrollPanel);

            // ?? Fixed-size inner panel that holds all input controls ?????????????????
            var inner = new System.Windows.Forms.Panel
            {
                Location = new System.Drawing.Point(0, 0),
                Width = 800,   // wide enough for all columns
                Height = 220    // actual content height
            };
            scrollPanel.Controls.Add(inner);

            // ?? Local helpers that add controls to 'inner' ???????????????????????????
            System.Windows.Forms.Label MkLabel(string text, int x, int y)
            {
                var l = new System.Windows.Forms.Label
                { Text = text, Location = new System.Drawing.Point(x, y), AutoSize = true };
                inner.Controls.Add(l);
                return l;
            }

            System.Windows.Forms.TextBox MkTextBox(int x, int y, int w)
            {
                var t = new System.Windows.Forms.TextBox
                { Location = new System.Drawing.Point(x, y), Width = w };
                inner.Controls.Add(t);
                return t;
            }

            System.Windows.Forms.ComboBox MkCombo(int x, int y, int w)
            {
                var c = new System.Windows.Forms.ComboBox
                {
                    Location = new System.Drawing.Point(x, y),
                    Width = w,
                    DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
                };
                inner.Controls.Add(c);
                return c;
            }

            System.Windows.Forms.DateTimePicker MkDate(int x, int y, int w)
            {
                var d = new System.Windows.Forms.DateTimePicker
                { Location = new System.Drawing.Point(x, y), Width = w };
                inner.Controls.Add(d);
                return d;
            }

            // ?? Row 1 ????????????????????????????????????????????????????????????????
            MkLabel("Title", 16, 14); txtTitle = MkTextBox(16, 32, 300);
            MkLabel("Category", 330, 14); cmbCategory = MkCombo(330, 32, 200);
            MkLabel("Status", 544, 14); cmbStatus = MkCombo(544, 32, 140);
            cmbStatus.Items.AddRange(new[] { "Not Started", "In Progress", "Completed" });
            cmbStatus.SelectedIndex = 0;

            // ?? Row 2 ????????????????????????????????????????????????????????????????
            MkLabel("Description", 16, 78); txtDesc = MkTextBox(16, 96, 530);
            txtDesc.Multiline = true;
            txtDesc.Height = 50;

            // ?? Row 3 ????????????????????????????????????????????????????????????????
            MkLabel("Supervisor", 16, 160); cmbSupervisor = MkCombo(16, 178, 200);
            MkLabel("Team", 230, 160); cmbTeam = MkCombo(230, 178, 200);
            MkLabel("Deadline", 444, 160); dtDeadline = MkDate(444, 178, 160);

            LoadCombos();
        }



        private void LoadCombos()
        {
            try
            {
                var cats = DatabaseHelper.ExecuteQuery("SELECT CategoryID, CategoryName FROM ProjectCategories");
                cmbCategory.DataSource = cats;
                cmbCategory.DisplayMember = "CategoryName";
                cmbCategory.ValueMember = "CategoryID";

                var sups = DatabaseHelper.ExecuteQuery(
                    "SELECT UserID, FullName FROM Users WHERE Role='Supervisor' AND IsActive=1");
                cmbSupervisor.DataSource = sups;
                cmbSupervisor.DisplayMember = "FullName";
                cmbSupervisor.ValueMember = "UserID";

                var teams = DatabaseHelper.ExecuteQuery("SELECT TeamID, TeamName FROM Teams");
                cmbTeam.DataSource = teams;
                cmbTeam.DisplayMember = "TeamName";
                cmbTeam.ValueMember = "TeamID";
            }
            catch { }
        }

        protected override void LoadData()
        {
            try
            {
                grid.DataSource = DatabaseHelper.ExecuteQuery(
                    "SELECT p.ProjectID, p.Title, c.CategoryName, u.FullName AS Supervisor, " +
                    "t.TeamName, p.Status, p.Deadline " +
                    "FROM Projects p " +
                    "LEFT JOIN ProjectCategories c ON p.CategoryID=c.CategoryID " +
                    "LEFT JOIN Users u ON p.SupervisorID=u.UserID " +
                    "LEFT JOIN Teams t ON p.TeamID=t.TeamID " +
                    "ORDER BY p.ProjectID");
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void SaveRecord()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) { Error("Title required."); return; }
            int id = SelectedID("ProjectID");

            int catID = cmbCategory.SelectedValue != null ? (int)cmbCategory.SelectedValue : 0;
            int supID = cmbSupervisor.SelectedValue != null ? (int)cmbSupervisor.SelectedValue : 0;
            int tmID = cmbTeam.SelectedValue != null ? (int)cmbTeam.SelectedValue : 0;

            try
            {
                if (id == 0)
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Projects (Title,Description,CategoryID,SupervisorID,TeamID,Status,Deadline) " +
                        "VALUES (@ti,@d,@c,@s,@tm,@st,@dl)",
                        new SqlParameter("@ti", txtTitle.Text.Trim()),
                        new SqlParameter("@d", txtDesc.Text.Trim()),
                        new SqlParameter("@c", catID),
                        new SqlParameter("@s", supID),
                        new SqlParameter("@tm", tmID),
                        new SqlParameter("@st", cmbStatus.SelectedItem.ToString()),
                        new SqlParameter("@dl", dtDeadline.Value.Date));
                else
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Projects SET Title=@ti,Description=@d,CategoryID=@c," +
                        "SupervisorID=@s,TeamID=@tm,Status=@st,Deadline=@dl WHERE ProjectID=@id",
                        new SqlParameter("@ti", txtTitle.Text.Trim()),
                        new SqlParameter("@d", txtDesc.Text.Trim()),
                        new SqlParameter("@c", catID),
                        new SqlParameter("@s", supID),
                        new SqlParameter("@tm", tmID),
                        new SqlParameter("@st", cmbStatus.SelectedItem.ToString()),
                        new SqlParameter("@dl", dtDeadline.Value.Date),
                        new SqlParameter("@id", id));

                Success("Project saved."); ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void DeleteRecord()
        {
            int id = SelectedID("ProjectID");
            if (id == 0 || !Confirm("Delete this project?")) return;
            try
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Projects WHERE ProjectID=@id",
                    new SqlParameter("@id", id));
                ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void ClearFields()
        {
            txtTitle.Clear(); txtDesc.Clear();
            cmbStatus.SelectedIndex = 0;
            dtDeadline.Value = DateTime.Today;
            grid.CurrentCell = null;
        }

        protected override void OnRowSelected()
        {
            if (grid.CurrentRow == null) return;
            txtTitle.Text = grid.CurrentRow.Cells["Title"].Value?.ToString();
            cmbStatus.Text = grid.CurrentRow.Cells["Status"].Value?.ToString();
            if (grid.CurrentRow.Cells["Deadline"].Value is DateTime dl)
                dtDeadline.Value = dl;
        }
    }
}