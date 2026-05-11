using System;
using System.Data;
using System.Data.SqlClient;

namespace AcademicProjectSystem.Database
{
    public static class DatabaseHelper
    {
        private static readonly string ConnectionString =
            @"Server=DONIA\SQLEXPRESS08;Database=AcademicProjectDB;Integrated Security=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Returns a DataTable from a SELECT query.</summary>
        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                conn.Open();
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        /// <summary>Returns a single scalar value.</summary>
        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  DATABASE INITIALISATION  –  run once on first launch
        // ─────────────────────────────────────────────────────────────────────
        public static void InitialiseDatabase()
        {
            string[] tables =
            {
                // Users
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                  CREATE TABLE Users (
                      UserID       INT IDENTITY PRIMARY KEY,
                      Username     NVARCHAR(50)  UNIQUE NOT NULL,
                      PasswordHash NVARCHAR(256) NOT NULL,
                      FullName     NVARCHAR(100) NOT NULL,
                      Email        NVARCHAR(100),
                      Role         NVARCHAR(20)  NOT NULL CHECK (Role IN ('Admin','Supervisor','Student')),
                      IsActive     BIT DEFAULT 1,
                      CreatedAt    DATETIME DEFAULT GETDATE()
                  );",

                // Admin is seeded AFTER the loop using a runtime-computed hash (see below)

                // Students
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Students' AND xtype='U')
                  CREATE TABLE Students (
                      StudentID    INT IDENTITY PRIMARY KEY,
                      UserID       INT REFERENCES Users(UserID),
                      StudentCode  NVARCHAR(20) UNIQUE NOT NULL,
                      Department   NVARCHAR(100),
                      Year         INT,
                      GPA          DECIMAL(3,2)
                  );",

                // Teams
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Teams' AND xtype='U')
                  CREATE TABLE Teams (
                      TeamID       INT IDENTITY PRIMARY KEY,
                      TeamName     NVARCHAR(100) NOT NULL,
                      SupervisorID INT REFERENCES Users(UserID),
                      CreatedAt    DATETIME DEFAULT GETDATE()
                  );",

                // TeamMembers
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TeamMembers' AND xtype='U')
                  CREATE TABLE TeamMembers (
                      ID        INT IDENTITY PRIMARY KEY,
                      TeamID    INT REFERENCES Teams(TeamID),
                      StudentID INT REFERENCES Students(StudentID)
                  );",

                // ProjectCategories
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProjectCategories' AND xtype='U')
                  CREATE TABLE ProjectCategories (
                      CategoryID   INT IDENTITY PRIMARY KEY,
                      CategoryName NVARCHAR(100) NOT NULL,
                      Description  NVARCHAR(500)
                  );",

                // Projects
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Projects' AND xtype='U')
                  CREATE TABLE Projects (
                      ProjectID    INT IDENTITY PRIMARY KEY,
                      Title        NVARCHAR(200) NOT NULL,
                      Description  NVARCHAR(MAX),
                      CategoryID   INT REFERENCES ProjectCategories(CategoryID),
                      SupervisorID INT REFERENCES Users(UserID),
                      TeamID       INT REFERENCES Teams(TeamID),
                      Status       NVARCHAR(20) DEFAULT 'Not Started'
                                   CHECK (Status IN ('Not Started','In Progress','Completed')),
                      Deadline     DATE,
                      CreatedAt    DATETIME DEFAULT GETDATE()
                  );",

                // Tasks
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Tasks' AND xtype='U')
                  CREATE TABLE Tasks (
                      TaskID       INT IDENTITY PRIMARY KEY,
                      ProjectID    INT REFERENCES Projects(ProjectID),
                      AssignedTo   INT REFERENCES Students(StudentID),
                      Title        NVARCHAR(200) NOT NULL,
                      Description  NVARCHAR(MAX),
                      DueDate      DATE,
                      Status       NVARCHAR(20) DEFAULT 'Pending'
                                   CHECK (Status IN ('Pending','In Progress','Done')),
                      CreatedAt    DATETIME DEFAULT GETDATE()
                  );",

                // Submissions
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Submissions' AND xtype='U')
                  CREATE TABLE Submissions (
                      SubmissionID INT IDENTITY PRIMARY KEY,
                      TaskID       INT REFERENCES Tasks(TaskID),
                      StudentID    INT REFERENCES Students(StudentID),
                      FilePath     NVARCHAR(500),
                      Notes        NVARCHAR(MAX),
                      SubmittedAt  DATETIME DEFAULT GETDATE()
                  );",

                // Evaluations
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Evaluations' AND xtype='U')
                  CREATE TABLE Evaluations (
                      EvaluationID   INT IDENTITY PRIMARY KEY,
                      SubmissionID   INT REFERENCES Submissions(SubmissionID),
                      SupervisorID   INT REFERENCES Users(UserID),
                      Grade          DECIMAL(5,2),
                      Feedback       NVARCHAR(MAX),
                      EvaluatedAt    DATETIME DEFAULT GETDATE()
                  );"
            };

            foreach (var sql in tables)
                ExecuteNonQuery(sql);

            // Seed admin using the same HashPassword() used at login — guarantees they match
            string adminHash = Helpers.SessionManager.HashPassword("admin123");

            // Insert if not exists
            ExecuteNonQuery(
                @"IF NOT EXISTS (SELECT 1 FROM Users WHERE Username='admin')
                  INSERT INTO Users (Username, PasswordHash, FullName, Email, Role, IsActive)
                  VALUES ('admin', @h, 'System Admin', 'admin@system.com', 'Admin', 1)",
                new SqlParameter("@h", adminHash));

            // Correct the hash if a broken row already exists from a previous launch
            ExecuteNonQuery(
                "UPDATE Users SET PasswordHash=@h WHERE Username='admin' AND PasswordHash <> @h",
                new SqlParameter("@h", adminHash));
        }
    }
}
