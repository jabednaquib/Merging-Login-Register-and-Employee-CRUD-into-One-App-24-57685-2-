/* ==========================================================================
   Migration.sql
   24-58175-2_CompanyApp - Access -> SQL Server user migration

   Source: db_users.mdb (Login-and-Register/bin/Debug/db_users.mdb)
   Table:  tbl_users
   Columns: username, password

   Read directly from the actual uploaded .mdb file with `mdb-export`
   (mdbtools). No usernames/passwords were invented. The table contained
   exactly 11 rows and no duplicate usernames, so every row below is a
   straight, unmodified copy of what was in Access.

   IMPORTANT:
     - UserID is never inserted manually; dbo.Users.UserID is IDENTITY(1,1)
       and SQL Server generates it automatically on each INSERT.
     - Run Schema.sql before this script.
     - Safe to re-run: each INSERT is guarded by a NOT EXISTS check so
       running this script twice will not create duplicate accounts.
   ========================================================================== */

USE [dbCompanyApp];
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'admin')
    INSERT INTO dbo.Users (Username, Password) VALUES ('admin', '12345');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'sayan')
    INSERT INTO dbo.Users (Username, Password) VALUES ('sayan', 'Sayan@1234');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'sayanchamp')
    INSERT INTO dbo.Users (Username, Password) VALUES ('sayanchamp', '12345');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'sachin')
    INSERT INTO dbo.Users (Username, Password) VALUES ('sachin', '123');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'sayan_admin')
    INSERT INTO dbo.Users (Username, Password) VALUES ('sayan_admin', '12345');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'iftee')
    INSERT INTO dbo.Users (Username, Password) VALUES ('iftee', '123');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'ifte')
    INSERT INTO dbo.Users (Username, Password) VALUES ('ifte', '123');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'minhaj')
    INSERT INTO dbo.Users (Username, Password) VALUES ('minhaj', 'munzarin');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = '24-58175')
    INSERT INTO dbo.Users (Username, Password) VALUES ('24-58175', '2222222');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'jabed2')
    INSERT INTO dbo.Users (Username, Password) VALUES ('jabed2', 'nokibqq');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = '24-58175-22')
    INSERT INTO dbo.Users (Username, Password) VALUES ('24-58175-22', '466588');

GO

-- Verify: 11 rows should now exist (or already existed) in dbo.Users
SELECT * FROM dbo.Users ORDER BY UserID;
GO
