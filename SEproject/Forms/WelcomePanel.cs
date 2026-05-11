using System;
using System.Drawing;
using System.Windows.Forms;
using AcademicProjectSystem.Database;
using AcademicProjectSystem.Helpers;

namespace AcademicProjectSystem.Forms
{
    public partial class WelcomePanel : UserControl
    {
        public WelcomePanel()
        {
            BackColor = Color.FromArgb(240, 243, 248);
            Dock      = DockStyle.Fill;
            BuildUI();
        }

        private void BuildUI()
        {
            // ── Header ────────────────────────────────────────────────────────
            var lblGreet = new Label
            {
                Text      = $"Good day, {SessionManager.CurrentFullName}!",
                Font      = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 30, 54),
                AutoSize  = true,
                Location  = new Point(0, 10)
            };

            var lblSub = new Label
            {
                Text      = "Here's a quick overview of the system.",
                Font      = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize  = true,
                Location  = new Point(0, 48)
            };

            Controls.AddRange(new Control[] { lblGreet, lblSub });

            // ── Stat cards ────────────────────────────────────────────────────
            int[] counts = GetCounts();
            string[] titles = { "Students", "Projects", "Teams", "Tasks" };
            string[] icons  = { "🎓", "📁", "👥", "✅" };
            Color[]  colors = {
                Color.FromArgb(0,  160, 230),
                Color.FromArgb(40, 167,  69),
                Color.FromArgb(253,126,  20),
                Color.FromArgb(111, 66, 193)
            };

            int x = 0;
            for (int i = 0; i < 4; i++)
            {
                var card = MakeCard(icons[i], titles[i], counts[i].ToString(), colors[i]);
                card.Location = new Point(x, 90);
                Controls.Add(card);
                x += card.Width + 16;
            }

            // ── Recent projects ───────────────────────────────────────────────
            var lblRecent = new Label
            {
                Text      = "Recent Projects",
                Font      = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 30, 54),
                AutoSize  = true,
                Location  = new Point(0, 230)
            };
            Controls.Add(lblRecent);

            var grid = new DataGridView
            {
                Location          = new Point(0, 260),
                Anchor            = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Size              = new Size(760, 280),
                ReadOnly          = true,
                AllowUserToAddRows= false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle       = BorderStyle.None,
                BackgroundColor   = Color.White,
                RowHeadersVisible = false,
                Font              = new Font("Segoe UI", 9),
                SelectionMode     = DataGridViewSelectionMode.FullRowSelect
            };
            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9, FontStyle.Bold);
            Controls.Add(grid);
            Resize += (s, e) => { grid.Width = Width - 10; grid.Height = Height - 270; };

            try
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    "SELECT TOP 10 p.Title, u.FullName AS Supervisor, " +
                    "p.Status, p.Deadline FROM Projects p " +
                    "LEFT JOIN Users u ON p.SupervisorID=u.UserID " +
                    "ORDER BY p.CreatedAt DESC");
                grid.DataSource = dt;
            }
            catch { /* DB not yet connected */ }
        }

        private Panel MakeCard(string icon, string title, string value, Color accent)
        {
            var p = new Panel
            {
                Size      = new Size(170, 110),
                BackColor = Color.White
            };
            p.Paint += (s, e) =>
                e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 5, p.Height);

            var lblIcon = new Label { Text = icon,  Font = new Font("Segoe UI", 22),
                AutoSize = true, Location = new Point(20, 16) };
            var lblVal  = new Label { Text = value, Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(18,30,54), AutoSize = true, Location = new Point(70, 15) };
            var lblTit  = new Label { Text = title, Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray, AutoSize = true, Location = new Point(70, 60) };

            p.Controls.AddRange(new Control[] { lblIcon, lblVal, lblTit });
            return p;
        }

        private int[] GetCounts()
        {
            int[] r = new int[4];
            try
            {
                r[0] = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Students") ?? 0);
                r[1] = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Projects") ?? 0);
                r[2] = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Teams")    ?? 0);
                r[3] = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Tasks")    ?? 0);
            }
            catch { }
            return r;
        }
    }
}
