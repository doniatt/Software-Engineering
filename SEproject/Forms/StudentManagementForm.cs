using System;
using System.Data.SqlClient;
using AcademicProjectSystem.Database;

namespace AcademicProjectSystem.Forms
{
    public partial class StudentManagementForm : BaseCrudPanel
    {
        private System.Windows.Forms.TextBox  txtCode, txtDept, txtGPA;
        private System.Windows.Forms.ComboBox cmbUser, cmbYear;

        public StudentManagementForm() : base("Student Management")
        {
            AddLabel("Linked User Account", 16, 14);
            cmbUser = AddComboBox(16, 32, 240);
            LoadUserCombo();

            AddLabel("Student Code", 270, 14); txtCode = AddTextBox(270, 32, 130);
            AddLabel("Department",   414, 14); txtDept = AddTextBox(414, 32, 200);
            AddLabel("Year",          16, 78); cmbYear = AddComboBox(16, 96, 80);
            cmbYear.Items.AddRange(new object[] { 1, 2, 3, 4, 5 });
            cmbYear.SelectedIndex = 0;

            AddLabel("GPA", 110, 78); txtGPA = AddTextBox(110, 96, 80);

        }

        private void LoadUserCombo()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    "SELECT UserID, FullName FROM Users WHERE Role='Student' AND IsActive=1");
                cmbUser.DataSource    = dt;
                cmbUser.DisplayMember = "FullName";
                cmbUser.ValueMember   = "UserID";
            }
            catch { }
        }

        protected override void LoadData()
        {
            try
            {
                grid.DataSource = DatabaseHelper.ExecuteQuery(
                    "SELECT s.StudentID, u.FullName, s.StudentCode, s.Department, s.Year, s.GPA " +
                    "FROM Students s JOIN Users u ON s.UserID=u.UserID ORDER BY s.StudentID");
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void SaveRecord()
        {
            int id = SelectedID("StudentID");
            try
            {
                int uid = (int)(cmbUser.SelectedValue ?? 0);
                if (id == 0)
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Students (UserID,StudentCode,Department,Year,GPA) VALUES (@u,@c,@d,@y,@g)",
                        new SqlParameter("@u", uid),
                        new SqlParameter("@c", txtCode.Text.Trim()),
                        new SqlParameter("@d", txtDept.Text.Trim()),
                        new SqlParameter("@y", cmbYear.SelectedItem),
                        new SqlParameter("@g", decimal.TryParse(txtGPA.Text, out var gpa) ? gpa : 0));
                else
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Students SET StudentCode=@c,Department=@d,Year=@y,GPA=@g WHERE StudentID=@id",
                        new SqlParameter("@c",  txtCode.Text.Trim()),
                        new SqlParameter("@d",  txtDept.Text.Trim()),
                        new SqlParameter("@y",  cmbYear.SelectedItem),
                        new SqlParameter("@g",  decimal.TryParse(txtGPA.Text, out var g2) ? g2 : 0),
                        new SqlParameter("@id", id));

                Success("Student saved."); ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void DeleteRecord()
        {
            int id = SelectedID("StudentID");
            if (id == 0 || !Confirm("Delete student?")) return;
            try
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Students WHERE StudentID=@id",
                    new SqlParameter("@id", id));
                ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void ClearFields()
        {
            txtCode.Clear(); txtDept.Clear(); txtGPA.Clear();
            cmbYear.SelectedIndex = 0;
            grid.CurrentCell = null;
        }

        protected override void OnRowSelected()
        {
            if (grid.CurrentRow == null) return;
            var row = grid.CurrentRow;
            txtCode.Text = row.Cells["StudentCode"].Value?.ToString();
            txtDept.Text = row.Cells["Department"].Value?.ToString();
            txtGPA.Text  = row.Cells["GPA"].Value?.ToString();
            if (int.TryParse(row.Cells["Year"].Value?.ToString(), out int yr))
                cmbYear.SelectedItem = yr;
        }
    }
}
