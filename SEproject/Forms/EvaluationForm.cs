using System;
using System.Data.SqlClient;
using AcademicProjectSystem.Database;
using AcademicProjectSystem.Helpers;

namespace AcademicProjectSystem.Forms
{
    public partial class EvaluationForm : BaseCrudPanel
    {
        private System.Windows.Forms.ComboBox cmbSubmission;
        private System.Windows.Forms.TextBox  txtGrade, txtFeedback;

        public EvaluationForm() : base("Evaluation & Grading")
        {
            AddLabel("Submission", 16, 14); cmbSubmission = AddComboBox(16, 32, 380);
            LoadSubmissions();

            AddLabel("Grade (0-100)", 410, 14); txtGrade    = AddTextBox(410, 32, 100);
            AddLabel("Feedback",       16, 78); txtFeedback = AddTextBox(16,  96, 600);
            txtFeedback.Multiline = true; txtFeedback.Height = 80;

        }

        private void LoadSubmissions()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    "SELECT sb.SubmissionID, " +
                    "u.FullName + ' – ' + tk.Title AS Label " +
                    "FROM Submissions sb " +
                    "JOIN Students s ON sb.StudentID=s.StudentID " +
                    "JOIN Users u ON s.UserID=u.UserID " +
                    "JOIN Tasks tk ON sb.TaskID=tk.TaskID");
                cmbSubmission.DataSource    = dt;
                cmbSubmission.DisplayMember = "Label";
                cmbSubmission.ValueMember   = "SubmissionID";
            }
            catch { }
        }

        protected override void LoadData()
        {
            try
            {
                grid.DataSource = DatabaseHelper.ExecuteQuery(
                    "SELECT e.EvaluationID, u2.FullName AS Student, tk.Title AS Task, " +
                    "e.Grade, e.Feedback, e.EvaluatedAt FROM Evaluations e " +
                    "JOIN Submissions sb ON e.SubmissionID=sb.SubmissionID " +
                    "JOIN Students s ON sb.StudentID=s.StudentID " +
                    "JOIN Users u2 ON s.UserID=u2.UserID " +
                    "JOIN Tasks tk ON sb.TaskID=tk.TaskID " +
                    "ORDER BY e.EvaluatedAt DESC");
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void SaveRecord()
        {
            int subID = cmbSubmission.SelectedValue != null ? (int)cmbSubmission.SelectedValue : 0;
            if (subID == 0) { Error("Select a submission."); return; }
            if (!decimal.TryParse(txtGrade.Text, out decimal grade) || grade < 0 || grade > 100)
            { Error("Enter a valid grade (0-100)."); return; }

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO Evaluations (SubmissionID,SupervisorID,Grade,Feedback) " +
                    "VALUES (@sub,@sup,@g,@f)",
                    new SqlParameter("@sub", subID),
                    new SqlParameter("@sup", SessionManager.CurrentUserID),
                    new SqlParameter("@g",   grade),
                    new SqlParameter("@f",   txtFeedback.Text.Trim()));
                Success("Evaluation saved."); ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void DeleteRecord()
        {
            int id = SelectedID("EvaluationID");
            if (id == 0 || !Confirm("Delete evaluation?")) return;
            try
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Evaluations WHERE EvaluationID=@id",
                    new SqlParameter("@id", id));
                ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void ClearFields()
        {
            txtGrade.Clear(); txtFeedback.Clear(); grid.ClearSelection();
        }

        protected override void OnRowSelected()
        {
            if (grid.CurrentRow == null) return;
            txtGrade.Text    = grid.CurrentRow.Cells["Grade"].Value?.ToString();
            txtFeedback.Text = grid.CurrentRow.Cells["Feedback"].Value?.ToString();
        }
    }
}
