using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using AcademicProjectSystem.Database;

namespace AcademicProjectSystem.Forms
{
    public partial class SubmissionForm : BaseCrudPanel
    {
        private ComboBox cmbTask, cmbStudent;
        private TextBox  txtNotes, txtFilePath;
        private Button   btnBrowse;

        public SubmissionForm() : base("Submissions")
        {
            AddLabel("Task",     16, 14); cmbTask    = AddComboBox(16,  32, 240);
            AddLabel("Student", 270, 14); cmbStudent = AddComboBox(270, 32, 220);

            AddLabel("File / Path", 16, 78); txtFilePath = AddTextBox(16, 96, 380);
            btnBrowse = new Button
            {
                Text      = "Browse…",
                Location  = new System.Drawing.Point(406, 95),
                Size      = new System.Drawing.Size(80, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(18, 30, 54),
                ForeColor = System.Drawing.Color.White,
                Cursor    = Cursors.Hand
            };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click += (s, e) =>
            {
                using var dlg = new OpenFileDialog { Filter = "All Files|*.*" };
                if (dlg.ShowDialog() == DialogResult.OK) txtFilePath.Text = dlg.FileName;
            };
            pnlForm.Controls.Add(btnBrowse);

            AddLabel("Notes", 16, 138); txtNotes = AddTextBox(16, 156, 500);
            txtNotes.Multiline = true; txtNotes.Height = 60;

            LoadCombos();
        }

        private void LoadCombos()
        {
            try
            {
                var tasks = DatabaseHelper.ExecuteQuery("SELECT TaskID, Title FROM Tasks");
                cmbTask.DataSource    = tasks;
                cmbTask.DisplayMember = "Title";
                cmbTask.ValueMember   = "TaskID";

                var studs = DatabaseHelper.ExecuteQuery(
                    "SELECT s.StudentID, u.FullName FROM Students s JOIN Users u ON s.UserID=u.UserID");
                cmbStudent.DataSource    = studs;
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
                    "SELECT sb.SubmissionID, tk.Title AS Task, u.FullName AS Student, " +
                    "sb.FilePath, sb.SubmittedAt FROM Submissions sb " +
                    "JOIN Tasks tk ON sb.TaskID=tk.TaskID " +
                    "JOIN Students s ON sb.StudentID=s.StudentID " +
                    "JOIN Users u ON s.UserID=u.UserID ORDER BY sb.SubmittedAt DESC");
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void SaveRecord()
        {
            int tid = cmbTask.SelectedValue    != null ? (int)cmbTask.SelectedValue    : 0;
            int sid = cmbStudent.SelectedValue != null ? (int)cmbStudent.SelectedValue : 0;
            if (tid == 0 || sid == 0) { Error("Select task and student."); return; }
            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO Submissions (TaskID,StudentID,FilePath,Notes) VALUES (@t,@s,@f,@n)",
                    new SqlParameter("@t", tid),
                    new SqlParameter("@s", sid),
                    new SqlParameter("@f", txtFilePath.Text.Trim()),
                    new SqlParameter("@n", txtNotes.Text.Trim()));
                Success("Submission recorded."); ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void DeleteRecord()
        {
            int id = SelectedID("SubmissionID");
            if (id == 0 || !Confirm("Delete submission?")) return;
            try
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Submissions WHERE SubmissionID=@id",
                    new SqlParameter("@id", id));
                ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void ClearFields()
        {
            txtFilePath.Clear(); txtNotes.Clear(); grid.CurrentCell = null;
        }
    }
}
