using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using AcademicProjectSystem.Database;
using AcademicProjectSystem.Helpers;

namespace AcademicProjectSystem.Forms
{
    public partial class LoginForm : Form
    {
        private Panel  pnlLeft, pnlRight;
        private Label  lblAppName, lblWelcome, lblUserLabel, lblPassLabel, lblError;
        private TextBox  txtUsername, txtPassword;
        private ComboBox cmbRole;
        private Button   btnLogin;
        private CheckBox chkShowPass;

        public LoginForm()
        {
            InitializeComponent();
            Text            = "Academic Project System – Login";
            Size            = new Size(820, 500);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            BackColor       = Color.White;
            BuildUI();
        }

        private void BuildUI()
        {
            // ── Left branding panel ───────────────────────────────────────────
            pnlLeft = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 340,
                BackColor = Color.FromArgb(18, 30, 54)
            };

            lblAppName = new Label
            {
                Text      = "Academic\nProject\nSystem",
                Font      = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds    = new Rectangle(0, 140, 340, 160)
            };

            var lblVersion = new Label
            {
                Text      = "v1.0  |  University Edition",
                Font      = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(100, 130, 170),
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds    = new Rectangle(0, 400, 340, 25)
            };

            pnlLeft.Controls.AddRange(new Control[] { lblAppName, lblVersion });
            pnlLeft.Paint += (s, e) =>
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 160, 230)),
                    336, 0, 4, pnlLeft.Height);

            // ── Right login panel ─────────────────────────────────────────────
            pnlRight = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White,
                Padding   = new Padding(50, 60, 50, 0)
            };

            lblWelcome = new Label
            {
                Text      = "Welcome Back",
                Font      = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 30, 54),
                AutoSize  = true,
                Location  = new Point(50, 60)
            };

            var lblSub = new Label
            {
                Text      = "Sign in to your account",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize  = true,
                Location  = new Point(50, 95)
            };

            // Role
            var lblRole = MakeLabel("Role", 50, 135);
            cmbRole = new ComboBox
            {
                Location      = new Point(50, 155),
                Size          = new Size(360, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 10)
            };
            cmbRole.Items.AddRange(new[] { "Admin", "Supervisor", "Student" });
            cmbRole.SelectedIndex = 2;

            // Username
            lblUserLabel = MakeLabel("Username", 50, 195);
            txtUsername = new TextBox
            {
                Location    = new Point(50, 215),
                Size        = new Size(360, 30),
                Font        = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Password
            lblPassLabel = MakeLabel("Password", 50, 255);
            txtPassword = new TextBox
            {
                Location     = new Point(50, 275),
                Size         = new Size(360, 30),
                Font         = new Font("Segoe UI", 10),
                PasswordChar = '●',
                BorderStyle  = BorderStyle.FixedSingle
            };

            chkShowPass = new CheckBox
            {
                Text      = "Show password",
                Location  = new Point(50, 312),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };
            chkShowPass.CheckedChanged += (s, e) =>
                txtPassword.PasswordChar = chkShowPass.Checked ? '\0' : '●';

            // Error label
            lblError = new Label
            {
                Text      = "",
                ForeColor = Color.Crimson,
                Font      = new Font("Segoe UI", 8),
                AutoSize  = false,
                Size      = new Size(360, 20),
                Location  = new Point(50, 338)
            };

            // Login button
            btnLogin = new Button
            {
                Text      = "SIGN IN",
                Location  = new Point(50, 365),
                Size      = new Size(360, 42),
                BackColor = Color.FromArgb(18, 30, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            AcceptButton = btnLogin;

            pnlRight.Controls.AddRange(new Control[]
            {
                lblWelcome, lblSub,
                lblRole, cmbRole,
                lblUserLabel, txtUsername,
                lblPassLabel, txtPassword,
                chkShowPass, lblError, btnLogin
            });

            // Add left first, then right (DockStyle.Fill fills remaining space)
            Controls.Add(pnlRight);
            Controls.Add(pnlLeft);
        }

        private Label MakeLabel(string text, int x, int y) => new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Color.FromArgb(80, 80, 80),
            AutoSize  = true,
            Location  = new Point(x, y)
        };

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string role     = cmbRole.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Please enter username and password.";
                return;
            }

            try
            {
                string hash = SessionManager.HashPassword(password);
                var dt = DatabaseHelper.ExecuteQuery(
                    "SELECT UserID, FullName, Role FROM Users " +
                    "WHERE Username=@u AND PasswordHash=@p AND Role=@r AND IsActive=1",
                    new SqlParameter("@u", username),
                    new SqlParameter("@p", hash),
                    new SqlParameter("@r", role));

                if (dt.Rows.Count == 0)
                {
                    lblError.Text = "Invalid credentials or role. Please try again.";
                    return;
                }

                var row = dt.Rows[0];
                SessionManager.Login(
                    (int)row["UserID"],
                    username,
                    row["FullName"].ToString(),
                    row["Role"].ToString());

                Hide();
                new MainDashboard().ShowDialog();
                Show();
                SessionManager.Logout();
                txtPassword.Clear();
                txtUsername.Clear();
            }
            catch (Exception ex)
            {
                lblError.Text = "Database error: " + ex.Message;
            }
        }
    }
}
