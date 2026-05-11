using System;
using System.Data.SqlClient;
using AcademicProjectSystem.Database;

namespace AcademicProjectSystem.Forms
{
    public partial class ProjectCategoryForm : BaseCrudPanel
    {
        private System.Windows.Forms.TextBox txtName, txtDesc;

        public ProjectCategoryForm() : base("Project Categories")
        {
            AddLabel("Category Name", 16,  14); txtName = AddTextBox(16,  32, 250);
            AddLabel("Description",   16,  78); txtDesc = AddTextBox(16,  96, 500);
            txtDesc.Multiline = true; txtDesc.Height = 60;
            pnlForm.Controls[pnlForm.Controls.Count - 1].Height = 60; // keep ref same
        }

        protected override void LoadData()
        {
            try
            {
                grid.DataSource = DatabaseHelper.ExecuteQuery(
                    "SELECT CategoryID, CategoryName, Description FROM ProjectCategories ORDER BY CategoryID");
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void SaveRecord()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { Error("Category name required."); return; }
            int id = SelectedID("CategoryID");
            try
            {
                if (id == 0)
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO ProjectCategories (CategoryName,Description) VALUES (@n,@d)",
                        new SqlParameter("@n", txtName.Text.Trim()),
                        new SqlParameter("@d", txtDesc.Text.Trim()));
                else
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE ProjectCategories SET CategoryName=@n,Description=@d WHERE CategoryID=@id",
                        new SqlParameter("@n",  txtName.Text.Trim()),
                        new SqlParameter("@d",  txtDesc.Text.Trim()),
                        new SqlParameter("@id", id));
                Success("Category saved."); ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void DeleteRecord()
        {
            int id = SelectedID("CategoryID");
            if (id == 0 || !Confirm("Delete category?")) return;
            try
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM ProjectCategories WHERE CategoryID=@id",
                    new SqlParameter("@id", id));
                ClearFields(); LoadData();
            }
            catch (Exception ex) { Error(ex.Message); }
        }

        protected override void ClearFields()
        {
            txtName.Clear(); txtDesc.Clear(); grid.CurrentCell = null;
        }

        protected override void OnRowSelected()
        {
            if (grid.CurrentRow == null) return;
            txtName.Text = grid.CurrentRow.Cells["CategoryName"].Value?.ToString();
            txtDesc.Text = grid.CurrentRow.Cells["Description"].Value?.ToString();
        }
    }
}
