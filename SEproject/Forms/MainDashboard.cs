using System;
using System.Drawing;
using System.Windows.Forms;
using AcademicProjectSystem.Helpers;

namespace AcademicProjectSystem.Forms
{
    public partial class MainDashboard : Form
    {
        private Panel pnlSidebar, pnlTopBar, pnlContent;
        private Label lblAppTitle, lblUserInfo;
        private Button btnActive;

        public MainDashboard()
        {
            InitializeComponent();
            Text = "Academic Project System – Dashboard";
            Size = new Size(1200, 720);
            MinimumSize = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(240, 243, 248);

            BuildContent();
            BuildSidebar();
            BuildTopBar();

            LoadChildForm(new WelcomePanel());
        }

        private void BuildTopBar()
        {
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White
            };

            pnlTopBar.Paint += (s, e) =>
                e.Graphics.FillRectangle(Brushes.Gainsboro, 0, 54, pnlTopBar.Width, 1);

            lblAppTitle = new Label
            {
                Text = "APS",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 30, 54),
                AutoSize = true,
                Location = new Point(20, 14)
            };

            lblUserInfo = new Label
            {
                Text = $"{SessionManager.CurrentFullName} [{SessionManager.CurrentRole}]",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 80),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(0, 20)
            };

            pnlTopBar.Resize += (s, e) =>
            {
                lblUserInfo.Left = pnlTopBar.Width - lblUserInfo.Width - 20;
            };

            pnlTopBar.Controls.Add(lblAppTitle);
            pnlTopBar.Controls.Add(lblUserInfo);
            Controls.Add(pnlTopBar);
        }

        private void BuildSidebar()
        {
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 170,
                BackColor = Color.FromArgb(18, 30, 54),
                AutoScroll = true
            };

            int y = 20;

            AddSectionLabel("MAIN", ref y);
            AddNavButton("🏠  Dashboard", ref y, () => LoadChildForm(new WelcomePanel()));
            AddNavButton("👤  Users", ref y, () => LoadChildForm(new UserManagementForm()), "Admin");
            AddNavButton("🎓  Students", ref y, () => LoadChildForm(new StudentManagementForm()));
            AddNavButton("👥  Teams", ref y, () => LoadChildForm(new TeamManagementForm()));

            AddSectionLabel("PROJECTS", ref y);
            AddNavButton("📁  Projects", ref y, () => LoadChildForm(new ProjectManagementForm()));
            AddNavButton("🏷  Categories", ref y, () => LoadChildForm(new ProjectCategoryForm()));
            AddNavButton("✅  Tasks", ref y, () => LoadChildForm(new TaskManagementForm()));
            AddNavButton("📤  Submissions", ref y, () => LoadChildForm(new SubmissionForm()));
            AddNavButton("📊  Evaluations", ref y, () => LoadChildForm(new EvaluationForm()));

            AddSectionLabel("SYSTEM", ref y);
            AddNavButton("📈  Reports", ref y, () => LoadChildForm(new ReportingForm()));
            AddNavButton("🔒  Logout", ref y, Logout);

            Controls.Add(pnlSidebar);
        }

        private void AddSectionLabel(string text, ref int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 110, 150),
                Size = new Size(pnlSidebar.Width, 22),
                Location = new Point(0, y),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0)
            };

            pnlSidebar.Controls.Add(lbl);
            y += 22;
        }

        private void AddNavButton(string text, ref int y, Action onClick, string requiredRole = null)
        {
            if (requiredRole != null && SessionManager.CurrentRole != requiredRole)
            {
                y += 38;
                return;
            }

            var btn = new Button
            {
                Text = text,
                Size = new Size(pnlSidebar.Width, 38),
                Location = new Point(0, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(18, 30, 54),
                ForeColor = Color.FromArgb(190, 210, 235),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 50, 80);

            btn.Click += (s, e) =>
            {
                SetActiveButton(btn);
                onClick?.Invoke();
            };

            pnlSidebar.Controls.Add(btn);
            y += 38;
        }

        private void SetActiveButton(Button btn)
        {
            if (btnActive != null)
            {
                btnActive.BackColor = Color.FromArgb(18, 30, 54);
                btnActive.ForeColor = Color.FromArgb(190, 210, 235);
            }

            btn.BackColor = Color.FromArgb(0, 160, 230);
            btn.ForeColor = Color.White;
            btnActive = btn;
        }

        private void BuildContent()
        {
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 243, 248),
                Padding = new Padding(16)
            };

            Controls.Add(pnlContent);
        }

        public void LoadChildForm(UserControl child)
        {
            pnlContent.Controls.Clear();
            child.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(child);
            child.BringToFront();
        }

        private void Logout()
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SessionManager.Logout();
                Close();
            }
        }
    }
}