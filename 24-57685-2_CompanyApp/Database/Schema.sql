/* ==========================================================================
   Schema.sql
   24-58175-2_CompanyApp - Unified database

   Creates ONE SQL Server database (dbCompanyApp) with the two tables the
   merged application depends on:

     dbo.Users        - accounts migrated from the old Access db_users.mdb
                         plus any newly registered accounts
     dbo.Emp_details   - employee records (unchanged data, plus a new
                         nullable CreatedBy column tracing who added each row)

   Safe to run repeatedly on a clean SQL Server / LocalDB instance: every
   CREATE is guarded so re-running this script will not error out or drop
   existing data.
   ========================================================================== */

IF DB_ID(N'dbCompanyApp') IS NULL
BEGIN
    CREATE DATABASE [dbCompanyApp];
END;
GO

USE [dbCompanyApp];
GO

-- ---------------------------------------------------------------------------
-- dbo.Users
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserID    INT IDENTITY(1,1) PRIMARY KEY,
        Username  NVARCHAR(50)  NOT NULL UNIQUE,
        Password  NVARCHAR(200) NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE()
    );
END;
GO

-- ---------------------------------------------------------------------------
-- dbo.Emp_details
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Emp_details', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Emp_details
    (
        EmpId      NVARCHAR(50)  NOT NULL PRIMARY KEY,
        EmpName    NVARCHAR(100) NOT NULL,
        EmpAge     INT           NOT NULL,
        EmpContact NVARCHAR(20)  NULL,
        EmpGender  NVARCHAR(10)  NULL,

        -- Nullable: migrated/legacy employee rows may not have a known
        -- creator, and must still be insertable/visible.
        CreatedBy  INT NULL,

        CONSTRAINT FK_Emp_CreatedBy
            FOREIGN KEY (CreatedBy)
            REFERENCES dbo.Users(UserID)
    );
END;
GO

-- If Emp_details already existed from an older run without CreatedBy,
-- add the column/FK now instead of erroring out.
IF COL_LENGTH('dbo.Emp_details', 'CreatedBy') IS NULL
BEGIN
    ALTER TABLE dbo.Emp_details ADD CreatedBy INT NULL;

    ALTER TABLE dbo.Emp_details
        ADD CONSTRAINT FK_Emp_CreatedBy
        FOREIGN KEY (CreatedBy)
        REFERENCES dbo.Users(UserID);
END;
GO
