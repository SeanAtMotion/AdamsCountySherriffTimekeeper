# Adams County Sheriff’s Office Timekeeping System

This document is a **step-by-step setup guide** for getting the Timekeeping System running on a **Windows PC** in your office. Follow the sections in order. You do not need to be a software developer, but you should be comfortable installing programs, opening files in a text editor, and using a command window.

**What “running locally” means:** The application will work in your web browser on this computer, with the database stored on this computer (or a SQL Server you already have). This setup is for **testing, training, or a small pilot**—not for production use on the public internet without extra security steps.

---

## 1. What This Application Does

The Adams County Sheriff’s Office Timekeeping System is a **web-based tool** for tracking when employees work.

**What employees can do**

- Sign in with a username and password  
- **Clock in** when they start work and **clock out** when they finish  
- Start and end **meal breaks** (if your office enables that)  
- See whether they are currently clocked in, on a break, or clocked out  
- View **their own** time entries and timesheets for a date range  
- Update **limited** profile information (such as phone and email), where your policies allow  
- Submit **correction requests** if a time entry needs an admin to fix it  

**What administrators can do**

- Everything employees can do, plus:  
- **Manage employees** (create accounts, activate or deactivate people, assign roles)  
- View **all** time entries and filter by employee, department, date, and status  
- **Approve or deny** correction requests  
- Run **reports** (hours summaries, overtime, missing punches, attendance) and **export to CSV**  
- View an **audit log** of important changes made by admins  

---

## 2. How the System Works (Simple Explanation)

Think of three parts working together:

| Part | Plain-English explanation |
|------|---------------------------|
| **Backend (server)** | This is the “brain” that runs on your computer. It listens for requests, checks passwords, saves data, and applies your office’s timekeeping rules. In technical terms it is an **ASP.NET Core** application. |
| **Frontend (website)** | This is what people **see in the web browser**—screens, buttons, and menus. It talks to the backend over the network. It is built with **React** (a common way to build web pages). |
| **Database** | This is where employee records, time entries, and login information are stored. Think of it like a **secure filing cabinet** that only the application opens: data is organized in tables (lists) such as employees and clock-in records. The software uses **Microsoft SQL Server** (or a small free edition called **LocalDB**—see prerequisites). |

When someone clicks “Clock in,” the **frontend** sends a message to the **backend**, the **backend** writes a row to the **database**, and the answer is sent back so the screen updates.

---

## 3. What You Need Before You Start (Prerequisites)

Install the items below on the **same Windows PC** where you will run the application for this guide. Use an account that is allowed to install software.

### 3.1 .NET 8 SDK

**What it is:** A free toolkit from Microsoft that lets this project run the backend server. **SDK** means “Software Development Kit,” but you only need it here to **run** the application—not to write code.

**Version required:** **.NET 8** (the project uses `net8.0`).

**How to install**

1. Open a web browser and go to: `https://dotnet.microsoft.com/download/dotnet/8.0`  
2. Download the **.NET 8 SDK** installer for Windows (x64).  
3. Run the installer and accept the defaults.  
4. **Restart** the computer if the installer asks you to.

**How to verify**

1. Press the **Windows key**, type **cmd**, and open **Command Prompt**.  
2. Type this and press Enter:

   ```text
   dotnet --version
   ```

**What you should see:** A line starting with **8.** (for example `8.0.416`).  
If you see an error that `dotnet` is not recognized, the SDK did not install correctly or you need a new Command Prompt window after installation.

---

### 3.2 Node.js (LTS)

**What it is:** A runtime that lets the frontend (the website) run on your machine during development. **LTS** means “Long Term Support”—a stable version recommended for most users.

**Version:** Use the current **LTS** release (for example **20.x** or **22.x**). The project does not require an exact patch number.

**How to install**

1. Go to `https://nodejs.org`  
2. Download the **LTS** Windows Installer (.msi).  
3. Run it and accept the defaults (including **npm**, the package manager).

**How to verify**

In Command Prompt:

```text
node --version
npm --version
```

**What you should see:** Version numbers (for example `v20.11.0` and `10.2.4`). No errors.

---

### 3.3 SQL Server (Express, Developer, or LocalDB)

**What it is:** Microsoft’s database engine. The application stores all timekeeping data here.

You can use any of these approaches:

| Option | Who it is for |
|--------|----------------|
| **SQL Server Express LocalDB** | Easiest for a single PC test. Often installed with **Visual Studio**. If you do not have Visual Studio, you can install the **SQL Server Express** installer and include **LocalDB**, or install **SQL Server Express** as a full local server. |
| **SQL Server Express** (full instance) | Good if you want a named server like `.\SQLEXPRESS` on your machine. You will edit the **connection string** later (see Section 6 and 7). |

**Minimum:** You need a SQL Server that accepts connections from this computer and supports the connection string you put in `appsettings.json`.

**Optional but recommended: SQL Server Management Studio (SSMS)**

**What it is:** A free Microsoft tool to **look at** databases, run simple queries, and confirm tables exist. It does not replace the steps below, but it helps you **see** that the database was created.

**Download:** `https://aka.ms/ssmsfullsetup`

---

### 3.4 A web browser

Use a current version of **Google Chrome**, **Microsoft Edge**, or **Mozilla Firefox**. **Chrome** is recommended for consistency with these instructions.

---

### 3.5 The `dotnet-ef` tool (for database setup)

**What it is:** A small add-on command that applies **database migrations** (creates and updates tables automatically from the project). It is not included in the base `dotnet` install.

**Install once** (in Command Prompt):

```text
dotnet tool install --global dotnet-ef --version 8.0.*
```

**How to verify:**

```text
dotnet ef
```

You should see help text mentioning Entity Framework, not “command not found.”

---

## 4. Downloading the Project

**If you received a ZIP file**

1. Save the ZIP to a simple location, for example **Desktop** or `C:\Projects`.  
2. Right-click the ZIP → **Extract All…**  
3. Choose a folder such as `C:\Projects\TimeKeeper` and finish extraction.  
4. After extraction you should have a **main folder** (often named `TimeKeeper` or similar) that contains at least:
   - A folder named **`backend`**
   - A folder named **`frontend`**
   - This **`README.md`** file at the top level (or inside the main folder)

**If you use Git**

If your IT staff uses Git, they may clone the repository into a folder such as `C:\Projects\TimeKeeper`. The contents should match the structure above.

**Important:** Note the **full path** to the main project folder. Example:

`C:\Projects\TimeKeeper`

You will use this path in commands below. Replace it with **your** actual path everywhere you see `C:\Projects\TimeKeeper`.

---

## 5. Project Folder Overview (Simple)

Below is what matters for setup:

| Location | What it is |
|----------|------------|
| `TimeKeeper\backend\src\Timekeeping.Api\` | **Backend**—the server code, configuration, and database project. |
| `TimeKeeper\backend\src\Timekeeping.Api\appsettings.json` | **Main configuration file** for local development (database connection, etc.). |
| `TimeKeeper\frontend\` | **Frontend**—the website source code. |
| `TimeKeeper\frontend\public\logo.png` | **County logo** shown on the login page and in the app header. If this file is missing, the layout may show a broken image—replace it with your official banner logo if needed. |
| `TimeKeeper\frontend\.env.development` | Tells the frontend to call `/api` during development (the dev server forwards that to the backend). |

You do **not** need to edit code files to complete this guide—only configuration where stated.

---

## 6. Step 1 – Set Up the Database (VERY IMPORTANT)

**What the database is doing:** The first time the application runs correctly, it will create **tables** (structured lists) inside your SQL database—such as employees, time entries, and login data.  

**What “migrations” are:** The project includes **migration** scripts (instructions) that describe which tables and columns to create. Running **`dotnet ef database update`** applies those instructions to your database so the structure matches what the application expects.

### 6.1 Use the default connection (LocalDB) if possible

The file `appsettings.json` is pre-configured for **SQL Server Express LocalDB** on Windows:

```text
Server=(localdb)\mssqllocaldb;Database=TimekeepingDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

This expects:

- LocalDB is installed  
- Windows authentication (your Windows login) is allowed  

**If you use a different SQL Server** (for example `.\SQLEXPRESS`), skip to Section 7 and **edit the connection string first**, then return here.

### 6.2 Open Command Prompt in the backend API folder

1. Press **Windows + R**, type `cmd`, press Enter.  
2. Go to the API folder (adjust the path if yours is different):

   ```text
   cd /d C:\Projects\TimeKeeper\backend\src\Timekeeping.Api
   ```

3. Confirm you see files like `Timekeeping.Api.csproj` and `appsettings.json` if you type `dir`.

### 6.3 Apply migrations (create/update tables)

Run:

```text
dotnet ef database update
```

**What success looks like**

- The command finishes with **“Done.”** or similar, **without** red error text about “cannot open database” or “login failed.”

**What this did**

- Created (or updated) a database named **`TimekeepingDb`** (when using the default connection string) and added the required tables.

### 6.4 Optional: confirm in SSMS

1. Open **SQL Server Management Studio**.  
2. Connect to `(localdb)\mssqllocaldb` (or your server name).  
3. Open **Databases** → **TimekeepingDb** → **Tables**.  
4. You should see tables with names such as **Employees**, **TimeEntries**, **AspNetUsers**, and others.  

If you see those, the database step succeeded.

---

## 7. Step 2 – Configure the Application

### 7.1 Main file: `appsettings.json`

**Path:**

`C:\Projects\TimeKeeper\backend\src\Timekeeping.Api\appsettings.json`

Open it with **Notepad** or another text editor (not Word, unless you save as plain text).

### 7.2 Connection string (`ConnectionStrings` → `DefaultConnection`)

**Purpose:** Tells the backend **which SQL Server** to use and **which database name** to open.

**Default (LocalDB):** You can leave this as-is if Step 6 worked.

**Example if you use SQL Server Express on `.\SQLEXPRESS`:**

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=TimekeepingDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

**Rules:**

- Use **double backslashes** `\\` inside the JSON string for a server name like `.\SQLEXPRESS`.  
- If SQL Server uses **SQL authentication** (username/password), your IT staff must supply the full connection string—do not guess passwords.  
- Save the file after editing.

### 7.3 CORS (`Cors` → `Origins`)

**Purpose:** For security, the backend only accepts browser requests from **listed website addresses**. For local development, the frontend runs at **`http://localhost:5173`**.

**Default in the project:**

```json
"Cors": {
  "Origins": [ "http://localhost:5173" ]
}
```

**Leave this as-is** for the setup in this README. If you change the frontend port later, you must add that address here too.

### 7.4 Other settings (usually leave as-is for local use)

| Setting | Plain meaning |
|---------|----------------|
| `Timekeeping:OfficeTimeZoneId` | Business time zone (default **America/New_York**). |
| `Timekeeping:OvertimeWeeklyThresholdHours` | After how many hours per week overtime is counted (default **40**). |
| `Timekeeping:EnableMealBreaks` | Whether break start/end buttons are used (**true** / **false**). |
| `AllowedHosts` | For local testing, **`*`** is acceptable. Tighten for production. |

### 7.5 Environment

For local testing, use **Development**. The commands in Section 8 use the **`http`** profile, which sets `ASPNETCORE_ENVIRONMENT` to **Development** automatically.

You do **not** need to set `Production` on your PC for this guide.

---

## 8. Step 3 – Start the Backend (Server)

Do this in **Command Prompt** (a **new** window is fine).

### 8.1 Go to the backend API folder

```text
cd /d C:\Projects\TimeKeeper\backend\src\Timekeeping.Api
```

### 8.2 Restore packages (downloads dependencies)

```text
dotnet restore
```

**What it does:** Ensures all required libraries are downloaded.  
**Success:** Ends without errors; may say “restore completed.”

### 8.3 Build the project (compile)

```text
dotnet build
```

**What it does:** Compiles the server code to check for errors.  
**Success:** Ends with **“Build succeeded.”**

### 8.4 Run the server

```text
dotnet run --launch-profile http
```

**What it does:** Starts the backend. It will keep running until you close the window or press **Ctrl+C**.

**Success:** You should see log lines in the window, and near the bottom something indicating the app is listening, for example that it is running on **`http://localhost:5050`**.

**Important URL for this project**

- **API base:** `http://localhost:5050`  
- **Swagger (API test page, development only):** open a browser to  
  **`http://localhost:5050/swagger`**  

Swagger is a **technical helper page** that lists API endpoints. You do not need it to use the normal website, but it proves the backend is alive.

**Leave this Command Prompt window open** while you use the application.

### 8.5 Alternative: Visual Studio (one-click debug)

If your IT staff uses **Visual Studio 2022** instead of two Command Prompt windows, they can start **both** the API and the website together.

**Important:** **`Timekeeping.sln` is not something you “run” or pick as the startup item.** You **open** the solution file once (like opening a Word document). What Visual Studio **runs** is the **project** named **`Timekeeping.Api`**. The green **Start** button’s dropdown must show **`Timekeeping.Api`** (or **https** / **http** under that project), not the `.sln` file.

**One-time setup**

1. Install **Visual Studio 2022** with the **ASP.NET and web development** workload (and ensure **Node.js** is installed as in **Section 3.2**—the build uses `npm run dev`).  
2. In **Command Prompt**, run **`npm install`** once in the **`frontend`** folder (same as **Section 9.2**) so dependencies exist.

**Every time you run**

1. In Visual Studio: **File** → **Open** → **Project/Solution…** → browse to **`Timekeeping.sln`** and open it. (Do **not** rely on **Open Folder** for this step unless you know how to pick a startup project in folder view—opening the `.sln` is simpler.)  
2. Wait for **Solution Explorer** to show **`Timekeeping.Api`** (a C# project under the solution). Right-click **`Timekeeping.Api`** → **Set as Startup Project**. The project name should appear **bold**.  
3. In the toolbar next to the green **Start** button, open the dropdown and choose the launch profile **`http`** for **`Timekeeping.Api`** (API listens on **`http://localhost:5050`**).  
4. Press **F5** (Start Debugging). Visual Studio starts the API and launches the **Vite** dev server for the React site; your browser should open **`http://localhost:5173`**.  
5. **Swagger** (optional technical page) remains at **`http://localhost:5050/swagger`**.

**If Start is grayed out or the dropdown never shows `Timekeeping.Api`:** you only opened a **folder**, not the solution—use **File** → **Open** → **Project/Solution** and select **`Timekeeping.sln`** again.

If **`npm run dev` fails** from Visual Studio, confirm **Node.js** is on the system **PATH** and that **`npm install`** was run in **`frontend`**.

---

## 9. Step 4 – Start the Frontend (Website)

Open a **second** Command Prompt window (keep the backend running in the first).

### 9.1 Go to the frontend folder

```text
cd /d C:\Projects\TimeKeeper\frontend
```

### 9.2 Install dependencies (first time only, or after updates)

```text
npm install
```

**What it does:** Downloads the libraries the website needs (this may take a few minutes).  
**Success:** Ends without `npm ERR!` errors.

### 9.3 Start the development website

```text
npm run dev
```

**What it does:** Starts a small local web server for the user interface.

**Success:** You should see text that includes a local address. This project is configured to use port **5173**.

**The address to remember**

**`http://localhost:5173`**

> **Note:** Some tutorials use port 3000. **This project uses 5173** (Vite default as set in `vite.config.ts`). Always use **5173** unless your IT staff changed it.

**Leave this window open** while you use the site.

---

## 10. Step 5 – Open the Application

1. Open **Chrome** (or Edge).  
2. In the address bar, type exactly:  
   **`http://localhost:5173`**  
3. Press Enter.

**What you should see**

- A **login page** with the **Adams County**–style branding and logo (if `frontend\public\logo.png` is present).  
- Fields for **username** and **password** and a **Sign in** button.

**If the page does not load**

- Confirm **both** Command Prompt windows are still running (`dotnet run` and `npm run dev`).  
- Confirm the address is **`http://localhost:5173`** (not https, unless your IT changed it).  
- Try **`http://127.0.0.1:5173`**.  
- Read **Section 14 – Troubleshooting**.

---

## 11. First Login (Admin Account)

The first time the database was created **and empty**, the application **automatically created** sample users (see **DataSeeder** in the project). If your database already had data, these accounts might not exist—your IT staff must create an admin user.

### Default administrator (if seeded)

| Field | Value |
|--------|--------|
| **Username** | `admin` |
| **Password** | `ChangeMe!123` |

**Security requirement:**  
**Change this password immediately** after first login in a real environment. These defaults are for **local testing and training only** and must not be used on a live network without strong passwords and your county’s security policies.

**Optional sample employee accounts (same password if seeded)**

| Username | Role |
|----------|------|
| `jdoe` | Employee |
| `jsmith` | Employee |

Password for both (if present): **`ChangeMe!123`**

**What to do first as admin**

- Sign in as **`admin`**.  
- Explore **Administration** (or **Admin**) in the menu: employees, time entries, reports.  
- Create or adjust employee accounts according to your office policy.  

---

## 12. Basic Usage (Simple Walkthrough)

### Employees

1. Sign in with an **employee** account.  
2. On the **Dashboard**, use **Clock in** when starting work.  
3. Use **Clock out** when finished (end any break first if prompted).  
4. Open **My Timesheet** to pick dates and see entries and totals.  
5. Open **My Profile** to view office-held fields; edit **phone/email** only if your process allows.  

### Administrators

1. Sign in as **admin**.  
2. **Employees:** create users, set department/division/badge/supervisor fields, activate or deactivate accounts, assign roles.  
3. **Time entries:** search and edit entries as needed; corrections may require audit entries.  
4. **Reports:** choose date ranges and run hours, overtime, missing punches, or attendance; export CSV where offered.  
5. **Corrections:** approve or deny employee requests.  

---

## 13. Quick Test Checklist

Use this to confirm the system works end-to-end on your PC.

- [ ] Backend starts with `dotnet run --launch-profile http` and **`http://localhost:5050/swagger`** opens.  
- [ ] Frontend starts with `npm run dev` and **`http://localhost:5173`** opens.  
- [ ] Login page shows with logo/branding.  
- [ ] Can log in as **`admin`** with **`ChangeMe!123`** (if seeded).  
- [ ] Can open **Dashboard** and use **Clock in** / **Clock out** as a test employee.  
- [ ] **My Timesheet** shows the test entry.  
- [ ] Admin can open **Employee management** and see users.  
- [ ] Admin can open **Reports** and see data load without errors.  

---

## 14. Troubleshooting (VERY IMPORTANT)

### “Cannot connect to database” / errors when running `dotnet ef database update`

**What it usually means:** SQL Server is not running, LocalDB is not installed, or the connection string is wrong.

**What to try**

1. Confirm **SQL Server** or **LocalDB** is installed.  
2. Re-read **Section 7** and verify the **connection string** matches your server name.  
3. For LocalDB, try connecting with **SSMS** to `(localdb)\mssqllocaldb`.  
4. Ask IT whether Windows Authentication is allowed for that database.  

---

### “Port already in use” (5050 or 5173)

**What it means:** Another program is using the same port.

**What to try**

1. Close other copies of this application.  
2. Close other dev servers if any.  
3. **Change the port** only if you know how: for the backend, edit `launchSettings.json` (applicationUrl); for the frontend, edit `vite.config.ts` (`server.port`). If you change ports, update **`Cors:Origins`** in `appsettings.json` to match the new frontend URL.  

---

### `npm install` fails

**What to try**

1. Install the latest **LTS Node.js** from `https://nodejs.org`.  
2. Run Command Prompt **as Administrator** and try again.  
3. If you are behind a corporate proxy, ask IT for npm proxy settings.  
4. Delete the folder `frontend\node_modules` (if it exists) and run `npm install` again.  

---

### Frontend cannot reach backend / login never succeeds / “Network Error”

**What it usually means:** The backend is not running, or the browser is blocked from talking to it.

**What to try**

1. Confirm **`dotnet run --launch-profile http`** is running and shows **port 5050**.  
2. Confirm **`frontend\.env.development`** contains `VITE_API_BASE=/api` (this project ships with that).  
3. Use **`http://localhost:5173`** (not a different port) unless you changed it.  
4. Temporarily disable **strict** VPN or firewall rules for localhost (ask IT).  
5. Confirm **`Cors`** in `appsettings.json` includes **`http://localhost:5173`**.  

---

### Login not working

**What to try**

1. Confirm the database was migrated (**Section 6**).  
2. Use **`admin`** / **`ChangeMe!123`** only if accounts were seeded (empty database on first run).  
3. Check that the account is **active** (admins can reactivate in **Employee management**).  
4. Caps Lock off; username is **case-sensitive** as typed in the system.  

---

### App loads but shows a blank page

**What to try**

1. Press **F12** → **Console** tab in the browser and note any red errors; share with IT.  
2. Confirm **`npm run dev`** is still running.  
3. Try a **hard refresh**: Ctrl+F5.  
4. Clear site data for `localhost` only if IT approves.  

---

### “Production” error mentioning LocalDB

**What it means:** Someone set the environment to **Production** while still using a LocalDB connection string. This project **blocks** that on purpose.

**What to try:** Run locally with **`dotnet run --launch-profile http`** (Development), or ask IT to supply a production SQL connection string for production hosting—not LocalDB.

---

## 15. Security Notes (Simple but Important)

- **Change default passwords** (`ChangeMe!123`) before any real use.  
- **Limit who has admin access**; admins can see and change sensitive data.  
- **Protect database credentials**; do not email connection strings openly.  
- **Local HTTP** (`http://localhost`) is for **trusted office networks and testing** only.  
- For **internet-facing** or **remote** access, use **HTTPS**, strong passwords, and your county’s IT security standards (this README does not replace a security assessment).  

---

## 16. What Happens Next (Optional Next Steps)

When you are ready to move beyond a single PC:

- **Hosting:** The same application can be deployed to **IIS** on a Windows Server or to **Microsoft Azure App Service**, with a production SQL database and HTTPS. Technical staff should follow separate **deployment** documentation and your county policies.  
- **Backups:** Plan regular **backups of the SQL database** so time records are never lost.  
- **Future enhancements:** Examples include PTO tracking, payroll exports, scheduling, and hardware badge readers—those are not required for the current version but may be added later.  

---

**Document version:** Written to match the **TimeKeeper** project structure (backend: `Timekeeping.Api`, frontend: Vite on port **5173**, API on port **5050**, solution file **`Timekeeping.sln`**, default seed user **`admin`** / **`ChangeMe!123`**). If your IT team changes ports, paths, or removes seed users, they should update this document to match.
