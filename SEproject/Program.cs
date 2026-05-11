using System;
using System.Windows.Forms;
using AcademicProjectSystem.Database;
using AcademicProjectSystem.Forms;

namespace AcademicProjectSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show splash, then login
            using (var splash = new SplashForm())
                splash.ShowDialog();

            // Initialise DB tables on first run
            try { DatabaseHelper.InitialiseDatabase(); }
            catch (Exception ex)
            {
                MessageBox.Show("Database initialisation failed:\n" + ex.Message,
                    "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}
