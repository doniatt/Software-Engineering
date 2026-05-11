using System;
using System.Drawing;
using System.Windows.Forms;

namespace AcademicProjectSystem.Forms
{
    public partial class SplashForm : Form
    {
        private Label  lblTitle, lblSub, lblLoading;
        private ProgressBar progressBar;
        private Timer  timer;
        private int    progress = 0;

        public SplashForm()
        {
            // ── Window settings ──────────────────────────────────────────────
            FormBorderStyle = FormBorderStyle.None;
            StartPosition   = FormStartPosition.CenterScreen;
            Size            = new Size(520, 300);
            BackColor       = Color.FromArgb(18, 30, 54);
            DoubleBuffered  = true;

            // ── Title ────────────────────────────────────────────────────────
            lblTitle = new Label
            {
                Text      = "Academic Project System",
                Font      = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds    = new Rectangle(0, 60, 520, 50)
            };

            // ── Sub-title ────────────────────────────────────────────────────
            lblSub = new Label
            {
                Text      = "Project Tracking & Management Platform",
                Font      = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(150, 180, 220),
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds    = new Rectangle(0, 115, 520, 25)
            };

            // ── Progress bar ─────────────────────────────────────────────────
            progressBar = new ProgressBar
            {
                Bounds    = new Rectangle(60, 200, 400, 12),
                Maximum   = 100,
                Style     = ProgressBarStyle.Continuous,
                ForeColor = Color.FromArgb(0, 160, 230)
            };

            // ── Loading label ────────────────────────────────────────────────
            lblLoading = new Label
            {
                Text      = "Initialising…",
                Font      = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(120, 150, 190),
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds    = new Rectangle(0, 220, 520, 20)
            };

            Controls.AddRange(new Control[] { lblTitle, lblSub, progressBar, lblLoading });

            // ── Timer ─────────────────────────────────────────────────────────
            timer = new Timer { Interval = 30 };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void SplashForm_Load(object sender, EventArgs e)
        {

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            progress += 2;
            progressBar.Value = Math.Min(progress, 100);

            if (progress < 30)       lblLoading.Text = "Loading modules…";
            else if (progress < 60)  lblLoading.Text = "Connecting to database…";
            else if (progress < 90)  lblLoading.Text = "Preparing interface…";
            else                     lblLoading.Text = "Ready!";

            if (progress >= 100)
            {
                timer.Stop();
                Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Thin accent line at top
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 160, 230)),
                0, 0, Width, 4);
        }
    }
}
