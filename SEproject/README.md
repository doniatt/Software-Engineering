# Academic Project System — C# Windows Forms

A complete academic project tracking and management platform built with C# (.NET 4.8), Windows Forms, and SQL Server.

---

## Project Structure

```
AcademicProjectSystem/
│
├── Program.cs                      Entry point (Splash → Login → Dashboard)
├── AcademicProjectSystem.csproj    Project file (.NET 4.8 WinForms)
├── setup_database.sql              Run once in SSMS to create the DB
│
├── Database/
│   └── DatabaseHelper.cs           Central DB connection, ExecuteQuery/NonQuery/Scalar
│                                   + InitialiseDatabase() — auto-creates all tables
│
├── Helpers/
│   └── SessionManager.cs           Login session state + SHA-1 password hashing
│
└── Forms/
    ├── SplashForm.cs               Startup loading screen (animated progress bar)
    ├── LoginForm.cs                Role-based login (Admin / Supervisor / Student)
    ├── MainDashboard.cs            Sidebar nav + child-form hosting panel
    ├── WelcomePanel.cs             Dashboard home with stat cards + recent projects
    ├── BaseCrudPanel.cs            Shared CRUD layout (grid + form + buttons)
    │
    ├── UserManagementForm.cs       Add/Edit/Delete users — Admin only
    ├── StudentManagementForm.cs    Manage students, link to user accounts
    ├── TeamManagementForm.cs       Create teams, assign students as members
    │
    ├── ProjectCategoryForm.cs      Academic project categories/types
    ├── ProjectManagementForm.cs    Full project CRUD (title, supervisor, team, status, deadline)
    │
    ├── TaskManagementForm.cs       Assign tasks to students per project
    ├── SubmissionForm.cs           Upload/record file submissions per task
    ├── EvaluationForm.cs           Supervisor grading & feedback on submissions
    │
    └── ReportingForm.cs            5 built-in reports (status, grades, overdue, etc.)
```

---

## Database Schema

| Table              | Purpose                                          |
|--------------------|--------------------------------------------------|
| Users              | All system accounts (Admin, Supervisor, Student) |
| Students           | Student profile linked to a User account         |
| Teams              | Student groups supervised by a Supervisor        |
| TeamMembers        | Many-to-many: Students ↔ Teams                  |
| ProjectCategories  | Classification of project types                  |
| Projects           | Core academic projects                           |
| Tasks              | Individual tasks within a project                |
| Submissions        | Student file/report submissions per task         |
| Evaluations        | Supervisor grades and feedback per submission    |

---

## Quick Start

### 1. Prerequisites
- Visual Studio 2019+ (or VS Code with C# extension)
- .NET Framework 4.8
- SQL Server or SQL Server Express

### 2. Database
Open **SQL Server Management Studio** and run `setup_database.sql` to create the `AcademicProjectDB` database.

The application auto-creates all tables on first launch via `DatabaseHelper.InitialiseDatabase()`.

### 3. Connection String
Edit `Database/DatabaseHelper.cs`, line 10:
```csharp
private static readonly string ConnectionString =
    "Server=.\\SQLEXPRESS;Database=AcademicProjectDB;Integrated Security=True;";
```
Change `.\SQLEXPRESS` to your SQL Server instance name.

### 4. Build & Run
```
dotnet restore
dotnet build
dotnet run
```
Or open in Visual Studio and press **F5**.

### 5. Default Login
| Field    | Value        |
|----------|--------------|
| Username | `admin`      |
| Password | `admin123`   |
| Role     | `Admin`      |

> Change the admin password immediately after first login via **User Management**.

---

## Role Permissions

| Feature            | Admin | Supervisor | Student |
|--------------------|-------|------------|---------|
| User Management    | ✅    | ❌         | ❌      |
| Students / Teams   | ✅    | ✅         | ❌      |
| Projects / Tasks   | ✅    | ✅         | ✅ (view)|
| Submissions        | ✅    | ✅         | ✅      |
| Evaluations        | ✅    | ✅         | ❌      |
| Reports            | ✅    | ✅         | ❌      |

---

## Pipeline Coverage

| Phase | Feature                                      | File                         |
|-------|----------------------------------------------|------------------------------|
| 1.1   | Login Form (Student/Supervisor/Admin)         | LoginForm.cs                 |
| 1.2   | Main Dashboard Form                           | MainDashboard.cs             |
| 1.3   | App Branding                                  | .csproj + SplashForm.cs      |
| 1.4   | Dashboard Layout (Sidebar, Nav, Panels)       | MainDashboard.cs             |
| 1.5   | Welcome / Splash Screen                       | SplashForm.cs                |
| 2.1   | SQL Server DB & Tables                        | DatabaseHelper.cs            |
| 2.2   | Connection String                             | DatabaseHelper.cs            |
| 2.3   | User Management (CRUD + DataGridView)         | UserManagementForm.cs        |
| 2.4   | User Module (Register Students/Supervisors)   | UserManagementForm.cs        |
| 2.5   | Login ↔ DB Authentication                    | LoginForm.cs + SessionManager|
| 3.1   | Student Management                            | StudentManagementForm.cs     |
| 3.2   | Team Management                               | TeamManagementForm.cs        |
| 3.3   | Student CRUD                                  | StudentManagementForm.cs     |
| 3.4   | Project Category Form                         | ProjectCategoryForm.cs       |
| 3.5   | Assign Students to Projects & Teams           | TeamManagementForm.cs        |
| 3.6   | Student-Project DB Relationships              | DatabaseHelper.cs (schema)   |
| 4.1   | Project Form Design                           | ProjectManagementForm.cs     |
| 4.2   | Project Module (Add/Edit)                     | ProjectManagementForm.cs     |
| 4.3   | Project CRUD                                  | ProjectManagementForm.cs     |
| 4.4   | Load Projects into Dashboard                  | WelcomePanel.cs              |
| 4.5   | Insert Project Data                           | ProjectManagementForm.cs     |
| 4.6   | Track Project Status                          | ProjectManagementForm.cs     |
| 5.1   | Load Child Forms in Dashboard                 | MainDashboard.LoadChildForm()|
| 5.2   | Task Management                               | TaskManagementForm.cs        |
| 5.3   | Submission Form                               | SubmissionForm.cs            |
| 5.4   | Evaluation / Grading Module                   | EvaluationForm.cs            |
| 5.5   | Edit/Update/Delete User Data                  | UserManagementForm.cs        |
| 5.6   | Logout & Session Control                      | SessionManager.cs + Dashboard|
| 5.7   | Reporting & Progress Tracking                 | ReportingForm.cs             |
