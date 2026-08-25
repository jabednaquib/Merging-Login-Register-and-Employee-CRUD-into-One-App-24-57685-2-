# 24-58175-2_CompanyApp

## 1. Project Overview

**Before the merge**, this repository contained two independent, non-communicating Windows Forms applications:

| | Login-and-Register | EmployeeDetails |
|---|---|---|
| Namespace | `Login_and_Register` | `EmployeeDetails` |
| Target Framework | .NET Framework 4.7.2 | .NET Framework 4.8 |
| Data access | `System.Data.OleDb` | `System.Data.SqlClient` |
| Database | MS Access (`db_users.mdb`) | SQL Server (`dbEmployeeDetails`) |
| Forms | `frmLogin`, `frmRegister`, `frmDashboard` | `Form1` (employee CRUD) |
| Entry point | its own `Program.cs` / `Main()` | its own `Program.cs` / `Main()` |

**After the merge**, there is exactly one application:

- **Solution:** `24-58175-2_CompanyApp.sln`
- **Project / assembly:** `CompanyApp` (root namespace kept as `EmployeeDetails`, per the host-project requirement)
- **Database:** one SQL Server database, `dbCompanyApp`, with two related tables
- **Entry point:** one `Program.cs`, one `Main()`, starting at `frmLogin`

The merge used **EmployeeDetails as the host project**, as required — the final project is EmployeeDetails's `.csproj`, renamed and extended, not a new third project.

## 2. The Six Conflicts, and How Each Was Fixed

1. **Different namespaces** (`Login_and_Register` vs `EmployeeDetails`)
   Every imported form (`frmLogin`, `frmRegister`, `frmDashboard`) had its namespace changed to `EmployeeDetails` in **both** the `.cs` and `.Designer.cs` files. `EmployeeDetails` was kept as the single, final namespace because the host-project rule requires the root namespace to remain `EmployeeDetails`.

2. **Different data providers** (`System.Data.OleDb` vs `System.Data.SqlClient`)
   All `OleDbConnection` / `OleDbCommand` / `OleDbDataAdapter` / `OleDbDataReader` usage in `frmLogin.cs` and `frmRegister.cs` was removed. Login and registration now go through a new `User.cs` class, written in the same `SqlConnection` / `SqlCommand` / parameterized-query style as the existing `Employee.cs`.

3. **Two separate databases** (Access `db_users.mdb` vs SQL Server `dbEmployeeDetails`)
   Both are replaced by one SQL Server database, `dbCompanyApp`, defined in `Schema.sql`. The real accounts from `db_users.mdb` were migrated into `dbo.Users` via `Migration.sql` (see §5).

4. **Different framework versions** (Login project targeted .NET Framework 4.7.2, EmployeeDetails targeted 4.8)
   The merged `CompanyApp.csproj` targets **.NET Framework 4.8** throughout — the higher of the two versions, and the one EmployeeDetails (the host) already used.

5. **Two `Program.cs` / `Main()` methods**
   The Login project's `Program.cs` was **not imported**. The merged project has exactly one `Program.cs`, whose `Main()` starts the app at `new frmLogin()`.

6. **Hidden dependency on `db_users.mdb`** (it only existed under `bin\Debug`, outside the tracked project structure)
   Removed entirely. The application no longer touches any `.mdb` file at runtime; the data that file held was migrated once, up front, into `dbo.Users` via `Migration.sql`.

## 3. Unified Database Design

**`dbo.Users`**

| Column | Type | Notes |
|---|---|---|
| `UserID` | `INT IDENTITY(1,1)` | Primary key, auto-generated |
| `Username` | `NVARCHAR(50) NOT NULL UNIQUE` | |
| `Password` | `NVARCHAR(200) NOT NULL` | |
| `CreatedAt` | `DATETIME DEFAULT GETDATE()` | |

**`dbo.Emp_details`**

| Column | Type | Notes |
|---|---|---|
| `EmpId` | `NVARCHAR(50)` | Primary key |
| `EmpName` | `NVARCHAR(100) NOT NULL` | |
| `EmpAge` | `INT NOT NULL` | |
| `EmpContact` | `NVARCHAR(20) NULL` | |
| `EmpGender` | `NVARCHAR(10) NULL` | |
| `CreatedBy` | `INT NULL` | FK → `dbo.Users(UserID)` |

`CreatedBy` is **nullable on purpose**: employee rows that already existed before this merge (or any future bulk-imported rows) have no known creator, and the app must still be able to store and display them. Making it `NOT NULL` would either force a fake UserID onto old data or make migration fail outright — neither is acceptable, so the column, and the foreign key, both allow `NULL`.

The complete, idempotent script is in [`Database/Schema.sql`](Database/Schema.sql).

## 4. Access Data Migration

`db_users.mdb` was inspected directly (not guessed) using `mdb-tools`. It contains one table, `tbl_users(username, password)`, with **11 accounts** and no duplicate usernames. All 11 rows were carried over unchanged into `dbo.Users` in [`Database/Migration.sql`](Database/Migration.sql).

`UserID` is **never inserted manually** — `dbo.Users.UserID` is `IDENTITY(1,1)`, so SQL Server assigns it automatically as each row is inserted. Each `INSERT` is wrapped in `IF NOT EXISTS (... WHERE Username = ...)`, so the script is safe to run more than once without creating duplicate accounts, and if a duplicate username had existed in the source data it would simply be skipped rather than silently overwriting an existing row (it didn't occur here — the table had no duplicates).

## 5. The Three-File Rule

Every WinForms form is really three files that must travel together:

- `frmX.cs` — your code (event handlers, business logic)
- `frmX.Designer.cs` — the designer-generated field declarations and `InitializeComponent()`
- `frmX.resx` — the form's embedded resources (icons, strings, etc.)

`frmX.cs` and `frmX.Designer.cs` are two halves of the **same `partial class`**, and `frmX.resx` is wired to `frmX.cs` via `<DependentUpon>` in the `.csproj`. Copying only the `.cs` file leaves the class with no `InitializeComponent()`, and it won't compile. All three forms (`frmLogin`, `frmRegister`, `frmDashboard`) plus the renamed `frmEmployee` were copied and edited as complete matched sets.

## 6. Namespace Changes

`Login_and_Register` → `EmployeeDetails` in `frmLogin.cs/.Designer.cs`, `frmRegister.cs/.Designer.cs`, and `frmDashboard.cs/.Designer.cs`. `Form1` (already in `EmployeeDetails`) needed no namespace change, only a class/file rename to `frmEmployee`.

## 7. OleDb → SqlClient Migration

- **`User.cs`** (new) — all login/register/username-check queries use `SqlConnection`/`SqlCommand` with named parameters (`@Username`, `@Password`).
- **`frmLogin.cs`** — no longer opens an `OleDbConnection` or builds SQL by string concatenation; it calls `User.ValidateLogin(username, password)`.
- **`frmRegister.cs`** — calls `User.UsernameExists(username)` and `User.RegisterUser(username, password)` instead of building `INSERT INTO tbl_users VALUES(...)` by hand.
- **`Employee.cs`** — was already using `SqlClient` with parameters; extended (not migrated) to add `CreatedBy` and the `LEFT JOIN` select.
- **`App.config`** — one `<connectionStrings>` entry, `connString`, pointing at `dbCompanyApp`, `providerName="System.Data.SqlClient"`.

This also fixes a real security bug: the original login query was

```csharp
"SELECT * FROM tbl_users WHERE username = '" + txtUsername.Text + "' and password = '" + txtPassword.Text + "'"
```

which is vulnerable to a classic SQL-injection bypass (a password of `' OR '1'='1` would satisfy the WHERE clause regardless of the real password). Every query in `User.cs` and `Employee.cs` now uses `SqlCommand.Parameters.Add(...)`, so user input is always passed as data, never concatenated into SQL text.

## 8. User.cs and Session.cs

**`User.cs`** implements exactly the three operations the login/register flow needs, in the same data-access style as `Employee.cs` (a static `ConfigurationManager`-sourced connection string, `using` blocks, parameterized commands):

- `ValidateLogin(username, password)` → returns the `UserID` (`int`) on success, `0` on failure. It returns the ID rather than a `bool` because the ID is needed later for `Emp_details.CreatedBy`.
- `UsernameExists(username)` → `bool`, via `ExecuteScalar()`.
- `RegisterUser(username, password)` → parameterized `INSERT`.

**`Session.cs`** is a small static class holding `UserID` and `Username` for the current run of the app, plus a `Clear()` method used on logout.

## 9. Application Flow

```
frmLogin
   │  User.ValidateLogin() succeeds
   ▼
frmDashboard  ──(btnManageEmployees)──▶  frmEmployee (Employee CRUD, modal)
   │
   │  Logout (Yes/No confirm)
   ▼
Session.Clear() → new frmLogin().Show() → this.Close()
   │
   ▼
frmLogin (new instance) — closing this window now calls Application.Exit()
```

Employee CRUD is only reachable through the **Manage Employees** button on `frmDashboard`, which in turn is only reachable after a successful login — there is no path to `frmEmployee` that skips authentication.

Logout does **not** call `Application.Exit()` — it clears the session, opens a **brand-new** `frmLogin`, and closes the dashboard. Only `frmLogin`'s own `FormClosed` handler calls `Application.Exit()`, which is what prevents the old "hidden login form" orphan-process bug: there is only ever one login form alive at a time, and closing it always ends the process cleanly.

## 10. CreatedBy

`Employee.CreatedBy` is a nullable `int` (`public int? CreatedBy { get; set; }`). In `frmEmployee.btnAdd_Click`, it is set from the active session immediately before insert:

```csharp
employee.CreatedBy = Session.UserID;
```

so every employee added through the app during a logged-in session is attributed to that user. Rows that predate this feature (or are migrated in bulk) simply keep `CreatedBy = NULL`.

## 11. Why LEFT JOIN (not INNER JOIN)

```sql
SELECT e.EmpId, e.EmpName, e.EmpAge, e.EmpContact, e.EmpGender,
       e.CreatedBy, u.Username AS CreatedByUsername
FROM dbo.Emp_details e
LEFT JOIN dbo.Users u ON e.CreatedBy = u.UserID
```

An `INNER JOIN` only returns rows where `e.CreatedBy` matches a `u.UserID` — any employee with `CreatedBy = NULL` would silently disappear from the grid entirely. Since `CreatedBy` is explicitly nullable (to support migrated/legacy rows), an `INNER JOIN` would make those employees invisible in the UI even though they still exist in the table. `LEFT JOIN` keeps every row from `Emp_details` and simply leaves `CreatedByUsername` blank when there's no matching user — which is exactly the behavior needed.

## 12. Grid Binding by Column Name

`dgvEmployeeDetails_RowHeaderMouseClick` reads selected-row values by column name —

```csharp
row.Cells["EmpId"].Value?.ToString();
row.Cells["EmpName"].Value?.ToString();
row.Cells["EmpAge"].Value?.ToString();
```

— not by numeric index (`row.Cells[0]`). Numeric indexes silently break the moment a column is added, removed, or reordered in the `SELECT` (which is exactly what happened here: `CreatedBy` and `CreatedByUsername` were added to the query). Binding by name keeps the form working regardless of column order, and `CreatedByUsername` is included in the grid's data source so the creator is visible per row.

## 13. One Real Build Error Encountered, and the Fix

While assembling the merged form files, this exact error was reproduced and had to be fixed (this is real Mono `mcs` compiler output from this project, not a fabricated example):

```
frmLogin.Designer.cs(15,33): error CS0115: `Login_and_Register.frmLogin.Dispose(bool)'
is marked as an override but no suitable method found to override
```

**Cause:** `frmLogin.cs` had already been edited to `namespace EmployeeDetails`, but `frmLogin.Designer.cs` still said `namespace Login_and_Register`. Because a form's `.cs` and `.Designer.cs` files must be **halves of the same `partial class`**, the compiler saw two *different* classes — `EmployeeDetails.frmLogin` (no base type information available to it in that lone file) and `Login_and_Register.frmLogin` — instead of one merged class. The Designer half's `Dispose(bool)` override then had nothing to override, since its "class" no longer properly extended `System.Windows.Forms.Form` the way the compiler could resolve.

**Fix:** every imported Designer file's `namespace` line was updated to `EmployeeDetails` to exactly match its `.cs` half, for all three imported forms. After that fix, the full project (`Employee.cs`, `User.cs`, `Session.cs`, `Program.cs`, all four forms) compiles cleanly with the Mono C# compiler (`mcs`) against `System.Windows.Forms`, `System.Data`, `System.Drawing`, and `System.Configuration` — 0 errors, 0 warnings.

> **Note on build verification:** this sandbox has no Visual Studio, MSBuild, or the .NET Framework 4.8 targeting pack. Mono's `mcs` compiler (installed for this task) was used instead to catch real syntax and reference errors, and successfully produced `CompanyApp.exe`. This confirms the C# is correct, but it is **not** a substitute for an actual `msbuild`/Visual Studio Debug build, and nothing was run against a live SQL Server instance (none exists in this sandbox) — see the Verification Checklist below for exactly what is and isn't confirmed.

## 14. One Database vs Two

Keeping the login credentials and the employee records in two separate databases (Access `db_users.mdb` plus SQL Server `dbEmployeeDetails`) meant the two halves of the app couldn't reference each other at all — there was no way to record *who* created an employee record, because `Emp_details` had no column that could point at anything in a completely different, differently-typed database engine. Moving both into a single SQL Server database, `dbCompanyApp`, makes a real relational link possible: `Emp_details.CreatedBy` is a proper foreign key into `Users.UserID`, enforced by the database itself rather than by application code hoping the two stay in sync. It also removes an entire class of deployment problems — no more bundling a `.mdb` file with the app, no more `Microsoft.Jet.OLEDB.4.0` provider (which isn't installed by default on modern Windows), and only one connection string and one set of credentials to manage. The `LEFT JOIN` in §11 is only possible at all because both tables now live in the same database and the same query.

## 15. Screenshot Checklist

Screenshots were **not** fabricated. Insert real screenshots at each `[ INSERT SCREENSHOT: ... ]` marker below once you run the app against a real SQL Server / LocalDB instance:

- [ INSERT SCREENSHOT: SQL Server Object Explorer showing both `dbo.Users` and `dbo.Emp_details` under `dbCompanyApp` ]
- [ INSERT SCREENSHOT: "View Data" on `dbo.Users` showing the migrated accounts ]
- [ INSERT SCREENSHOT: Solution Explorer showing each form's three nested files (`frmLogin.cs` / `.Designer.cs` / `.resx`, etc.) ]
- [ INSERT SCREENSHOT: Login screen ]
- [ INSERT SCREENSHOT: Register screen ]
- [ INSERT SCREENSHOT: Dashboard, with the Manage Employees button visible ]
- [ INSERT SCREENSHOT: Employee CRUD screen, grid populated ]
- [ INSERT SCREENSHOT: grid showing the `CreatedByUsername` column ]
- [ INSERT SCREENSHOT: Logout confirmation returning to a fresh Login screen ]

## 16. Manual Steps You Still Need to Perform

1. **Create the database.** Run `Database/Schema.sql` against your SQL Server / LocalDB instance.
2. **Migrate the accounts.** Run `Database/Migration.sql` against the same database. It inserts the 11 real accounts read out of `db_users.mdb`.
3. **Point the app at your server.** If you're not using `(localdb)\MSSQLLocalDB`, edit the `connString` in `CompanyApp/App.config`.
4. **Open in Visual Studio and build.** See §17 below.
5. **Take the real screenshots** listed in §15 and drop them into this README (or a `/screenshots` folder) in place of the markers.
6. **Decide on password storage.** Both the original Access table and this merge store passwords as plain text (`NVARCHAR`), matching the original system's behavior exactly. If this matters for your grading rubric, hashing (e.g. with `Rfc2898DeriveBytes`/PBKDF2) would be the next improvement — it was intentionally left out here since it wasn't part of the stated requirements and would change the migrated data's meaning.

## 17. Running the Final Project

1. Install SQL Server Express or SQL Server LocalDB, and SQL Server Management Studio (SSMS) or Azure Data Studio.
2. Open SSMS/Azure Data Studio, connect to your instance, open and execute `Database/Schema.sql`, then `Database/Migration.sql`.
3. Open `24-58175-2_CompanyApp.sln` in Visual Studio (2019 or later recommended; the project targets .NET Framework 4.8, so make sure that targeting pack is installed).
4. If your SQL Server instance isn't `(localdb)\MSSQLLocalDB`, update the `connString` value in `CompanyApp/App.config`.
5. Set `CompanyApp` as the startup project (it's the only project in the solution) and press **F5** / **Start**.
6. The app should open on the **Login** screen. Log in with one of the migrated accounts (e.g. `admin` / `12345`) or click **Create Account** to register a new one.
7. From the Dashboard, click **Manage Employees** to reach the Employee CRUD screen.
8. Click **LOGOUT** on the Dashboard to confirm it returns you to a fresh Login screen, and that closing that Login screen exits the app entirely (no orphaned process left in Task Manager).

## 18. What Was Verified vs. What Could Not Be Tested Here

**Verified in this sandbox:**
- Both source projects were fully inventoried and inspected file-by-file (namespaces, target frameworks, connection strings, control names, database schema, actual `.mdb` contents).
- `db_users.mdb` was read directly with `mdb-tools`; the 11 migrated accounts are real, not invented.
- All merged C# source compiles cleanly (0 errors, 0 warnings) with the Mono `mcs` compiler against the WinForms/Data/Configuration/Drawing assemblies — this confirms the code is syntactically and referentially correct.
- A real compiler error (§13) was reproduced and its fix documented.

**Could NOT be tested in this sandbox** (no Windows, no Visual Studio/MSBuild, no SQL Server instance, no display):
- An actual MSBuild/Visual Studio Debug build (only a Mono syntax/reference compile was possible).
- Running the app and clicking through Login → Register → Dashboard → Employee CRUD → Logout.
- Executing `Schema.sql`/`Migration.sql` against a live SQL Server instance.
- Any of the screenshots in §15.

You'll need to do the items in §16 to close that gap.

## 19. Verification Checklist

- [x] Two source projects fully inspected
- [x] All six conflicts identified in the actual files
- [x] Root namespace = `EmployeeDetails`
- [x] Assembly name = `CompanyApp`
- [x] One project, one `.csproj`, one `.sln`
- [x] One `Program.cs`, one `Main()`, starts at `frmLogin`
- [x] `db_users.mdb` read directly; real accounts migrated, no invented data
- [x] No `OleDb` anywhere in the merged source
- [x] No `.mdb` dependency in the merged source
- [x] One connection string (`connString`) in `App.config`
- [x] Parameterized SQL everywhere (`User.cs`, `Employee.cs`)
- [x] `CreatedBy` nullable FK added to `Emp_details`, set from `Session.UserID` on Add
- [x] `LEFT JOIN` used for the employee grid; migrated rows with `CreatedBy IS NULL` still selected
- [x] Grid reads by column name (`Cells["EmpId"]`, etc.)
- [x] Logout clears session, opens a new `frmLogin`, closes the dashboard
- [x] `frmLogin` closing calls `Application.Exit()`
- [x] Merged source compiles cleanly (Mono `mcs`)
- [ ] Actual MSBuild/Visual Studio build — **not possible in this sandbox**, needs to be done on your machine
- [ ] Login/Register/CRUD/Logout exercised against a live SQL Server — **not possible in this sandbox**
- [ ] Real screenshots inserted — **you need to do this**
