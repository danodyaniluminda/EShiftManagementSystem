using System;
using System.IO;
using System.Data.SQLite;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using EShiftManagementSystem.DAL;

namespace EShiftManagementSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                CreateDatabaseManually();

                var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eshift.db");
                var connectionString = $"Data Source={dbPath}";

                var optionsBuilder = new DbContextOptionsBuilder<EShiftDbContext>();
                optionsBuilder.UseSqlite(connectionString);

                var context = new EShiftDbContext(optionsBuilder.Options);
                var dataManager = new DataManager(context);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new LoginForm(dataManager, context));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CreateDatabaseManually()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eshift.db");

            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                var createAdminsTable = @"
            CREATE TABLE IF NOT EXISTS Admins (
                AdminId INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL,
                FirstName TEXT,
                LastName TEXT,
                Email TEXT,
                Phone TEXT,
                Role TEXT,
                CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
                IsActive INTEGER NOT NULL DEFAULT 1
            );";

                var createCustomersTable = @"
            CREATE TABLE IF NOT EXISTS Customers (
                CustomerId INTEGER PRIMARY KEY AUTOINCREMENT,
                FirstName TEXT,
                LastName TEXT,
                Email TEXT,
                Phone TEXT,
                Address TEXT,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL,
                RegistrationDate TEXT NOT NULL DEFAULT (datetime('now'))
            );";

                var createTransportUnitsTable = @"
            CREATE TABLE IF NOT EXISTS TransportUnits (
                TransportUnitId INTEGER PRIMARY KEY AUTOINCREMENT,
                UnitType TEXT,
                LicensePlate TEXT,
                MaxWeight REAL,
                MaxVolume REAL,
                DriverName TEXT,
                DriverPhone TEXT,
                DriverName TEXT,
                IsAvailable INTEGER NOT NULL DEFAULT 1,
                CreatedDate TEXT NOT NULL DEFAULT (datetime('now'))
            );";

                var createJobsTable = @"
            CREATE TABLE IF NOT EXISTS Jobs (
                JobId INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId INTEGER,
                StartLocation TEXT,
                Destination TEXT,
                Description TEXT,
                RequestDate TEXT NOT NULL DEFAULT (datetime('now')),
                ScheduleDate TEXT,
                CompletionDate  TEXT,
                Status TEXT DEFAULT 'Pending',
                Cost REAL,
                CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
                FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
            );";

                var createLoadsTable = @"
            CREATE TABLE IF NOT EXISTS Loads (
                LoadId INTEGER PRIMARY KEY AUTOINCREMENT,
                JobId INTEGER,
                TransportUnitId INTEGER,
                Description TEXT,
                Weight REAL,
                Volume REAL,
                Category TEXT,
                Status TEXT DEFAULT 'Pending',
                CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
                FOREIGN KEY (JobId) REFERENCES Jobs(JobId),
                FOREIGN KEY (TransportUnitId) REFERENCES TransportUnits(TransportUnitId)
            );";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = createAdminsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createCustomersTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createTransportUnitsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createJobsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createLoadsTable;
                    command.ExecuteNonQuery();

                }
            }
        }

    }
}
