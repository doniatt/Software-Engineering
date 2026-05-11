using System;
using System.Data.SqlClient;
using AcademicProjectSystem.Database;

namespace AcademicProjectSystem.Forms
{
    public partial class TaskManagementForm : BaseCrudPanel
    {
        private System.Windows.Forms.TextBox    txtTitle, txtDesc;
        private System.Windows.Forms.ComboBox   cmbProject, cmbStudent, cmbStatus;
        private System.Windows.Forms.DateTimePicker dtDue;

        public TaskManagementForm() : base("Task Management")
        {
            AddLabel("Title",    16,  14); txtTitle  = AddTextBox(16,  32, 280);
            AddLabel("Project", 310,  14); cmbProject= AddComboBox(310, 32, 220);
            AddLabel("Status",  544,  14); cmbStatus = AddComboBox(544, 32, 140);
            cmbStatus.Items.AddRange(new[] { "Pending", "In Progress", "Done" });
            cmbStatus.SelectedIndex = 0;

            AddLabel("Description",  16, 78); txtDesc = AddTextBox(16, 96, 500);
            txtDesc.Multiline = true; txtDesc.Height = 50;

            AddLabel("Assign To",    16, 160); cmbStudent= AddComboBox(16, 178, 220);
            AddLabel("Due Date",    250, 160); dtDue     = AddDatePicker(250, 178, 160);

            LoadCombos();
        }

        private void LoadCombos()
        {
            try
            {
                var proj = DatabaseHelper.ExecuteQuery("SELECT ProjectID, Title FROM Projects");
                cmbProject.DataSource    = proj;
                cmbProject.DisplayMember = "Title";
                cmbProject.ValueMember   = "ProjectID";

                var stud = DatabaseHelper.ExecuteQuery(
                    "SELECT s.StudentID, u.FullName FROM Students s JOIN Users u ON s.UserID=u.UserID");
                cmbStudent.DataSource    = stud;
                cmbStudent.DisplayMember = "FullName";
                cmbStudent.ValueMember   = "StudentID";
            }
            catch { }
        }

        protected override void LoadData()
        {
            try
            {
                grid.DataSource = DatabaseHelper.ExecuteQuery(
                    "SELECT t.TaskID, t.Title, p.Title AS Project, u.FullName AS AssignedTo, " +
                    "t.Status, t.DueDate FROM Tasks t " +
                    "LEFT JOIN Projects p ON t.ProjectID=p.ProjectID " +
                    "LEFT JOIN Students s ON t.AssignedTo=s.StudentID " +
                    "LEFT JOIN Users u ON s.UserID=u.UserID ORDER BY t.TaskID");
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void SaveRecord()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) { Error("Title required."); return; }
            int id   = SelectedID("TaskID");
            int pid  = cmbProject.SelectedValue != null ? (int)cmbProject.SelectedValue : 0;
            int sid  = cmbStudent.SelectedValue != null ? (int)cmbStudent.SelectedValue : 0;
            try
            {
                if (id == 0)
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Tasks (ProjectID,AssignedTo,Title,Description,DueDate,Status) " +
                        "VALUES (@p,@s,@ti,@d,@du,@st)",
                        new SqlParameter("@p",  pid),
                        new SqlParameter("@s",  sid),
                        new SqlParameter("@ti", txtTitle.Text.Trim()),
                        new SqlParameter("@d",  txtDesc.Text.Trim()),
                        new SqlParameter("@du", dtDue.Value.Date),
                        new SqlParameter("@st", cmbStatus.SelectedItem));
                else
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Tasks SET ProjectID=@p,AssignedTo=@s,Title=@ti,Description=@d," +
                        "DueDate=@du,Status=@st WHERE TaskID=@id",
                        new SqlParameter("@p",  pid),
                        new SqlParameter("@s",  sid),
                        new SqlParameter("@ti", txtTitle.Text.Trim()),
                        new SqlParameter("@d",  txtDesc.Text.Trim()),
                        new SqlParameter("@du", dtDue.Value.Date),
                        new SqlParameter("@st", cmbStatus.SelectedItem),
                        new SqlParameter("@id", id));
                Success("Task saved."); ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void DeleteRecord()
        {
            int id = SelectedID("TaskID");
            if (id == 0 || !Confirm("Delete task?")) return;
            try
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Tasks WHERE TaskID=@id",
                    new SqlParameter("@id", id));
                ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void ClearFields()
        {
            txtTitle.Clear(); txtDesc.Clear();
            cmbStatus.SelectedIndex = 0; dtDue.Value = DateTime.Today;
            grid.CurrentCell = null;
        }

        protected override void OnRowSelected()
        {
            if (grid.CurrentRow == null) return;
            txtTitle.Text  = grid.CurrentRow.Cells["Title"].Value?.ToString();
            cmbStatus.Text = grid.CurrentRow.Cells["Status"].Value?.ToString();
            if (grid.CurrentRow.Cells["DueDate"].Value is DateTime dd)
                dtDue.Value = dd;
        }
    }
}
