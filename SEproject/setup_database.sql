
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AcademicProjectDB')
    CREATE DATABASE AcademicProjectDB;
GO

USE AcademicProjectDB;
GO

INSERT INTO Users (Username, PasswordHash, FullName, Email, Role, IsActive)
VALUES ('supervisor2', HASHBYTES('SHA2_256', 'Pass@123'), 'Dr. Amr Ibrahim', 'Amr@must.edu.eg', 'Supervisor', 1);

PRINT 'Database AcademicProjectDB is ready.';
GO
