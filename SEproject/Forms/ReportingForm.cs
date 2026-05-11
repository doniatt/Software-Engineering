using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AcademicProjectSystem.Database;

namespace AcademicProjectSystem.Forms
{
    public partial class ReportingForm : UserControl
    {
        private DataGridView grid;
        private ComboBox     cmbReport;
        private Button       btnRun;
        private Label        lblTitle;

        public ReportingForm()
        {
            BackColor = Color.FromArgb(240, 243, 248);
            Dock      = DockStyle.Fill;

            lblTitle = new Label
            {
                Text      = "Reports & Analytics",
                Font      = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 30, 54),
                AutoSize  = true,
                Location  = new Point(0, 8)
            };

            var lblRep = new Label
            {
                Text     = "Report:",
                AutoSize = true,
                Font     = new Font("Segoe UI", 9),
                Location = new Point(0, 55)
            };

            cmbReport = new ComboBox
            {
                Location      = new Point(55, 50),
                Size          = new Size(320, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 9)
            };
            cmbReport.Items.AddRange(new[]
            {
                "Project Status Summary",
                "Student Submission Count",
                "Average Grade per Project",
                "Overdue Tasks",
                "Teams & Member Count"
            });
            cmbReport.SelectedIndex = 0;

            btnRun = new Button
            {
                Text      = "▶ Run",
                Location  = new Point(390, 48),
                Size      = new Size(90, 32),
                BackColor = Color.FromArgb(0, 160, 230),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnRun.FlatAppearance.BorderSize = 0;
            btnRun.Click += (s, e) => RunReport();

            grid = new DataGridView
            {
                Location            = new Point(0, 100),
                Anchor              = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Height              = 500,
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle         = BorderStyle.None,
                BackgroundColor     = Color.White,
                RowHeadersVisible   = false,
                Font                = new Font("Segoe UI", 9)
            };
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            Controls.AddRange(new Control[] { lblTitle, lblRep, cmbReport, btnRun, grid });
            Resize += (s, e) => grid.Width = Width - 20;

            RunReport();
        }

        private void RunReport()
        {
            string sql;
            switch (cmbReport.SelectedIndex)
            {
                case 0:
                    sql = "SELECT Status, COUNT(*) AS ProjectCount FROM Projects GROUP BY Status";
                    break;
                case 1:
                    sql = "SELECT u.FullName AS Student, COUNT(sb.SubmissionID) AS Submissions " +
                          "FROM Students s JOIN Users u ON s.UserID=u.UserID " +
                          "LEFT JOIN Submissions sb ON s.StudentID=sb.StudentID " +
                          "GROUP BY u.FullName ORDER BY Submissions DESC";
                    break;
                case 2:
                    sql = "SELECT p.Title AS Project, ISNULL(AVG(e.Grade),0) AS AvgGrade " +
                          "FROM Projects p " +
                          "LEFT JOIN Tasks t ON p.ProjectID=t.ProjectID " +
                          "LEFT JOIN Submissions sb ON t.TaskID=sb.TaskID " +
                          "LEFT JOIN Evaluations e ON sb.SubmissionID=e.SubmissionID " +
                          "GROUP BY p.Title ORDER BY AvgGrade DESC";
                    break;
                case 3:
                    sql = "SELECT t.Title AS Task, p.Title AS Project, " +
                          "t.DueDate, t.Status FROM Tasks t " +
                          "JOIN Projects p ON t.ProjectID=p.ProjectID " +
                          "WHERE t.DueDate < GETDATE() AND t.Status <> 'Done' " +
                          "ORDER BY t.DueDate";
                    break;
                case 4:
                    sql = "SELECT t.TeamName, COUNT(tm.StudentID) AS Members " +
                          "FROM Teams t LEFT JOIN TeamMembers tm ON t.TeamID=tm.TeamID " +
                          "GROUP BY t.TeamName ORDER BY Members DESC";
                    break;
                default:
                    return;
            }

            try
            {
                grid.DataSource = DatabaseHelper.ExecuteQuery(sql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
