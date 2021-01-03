using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DokanNet;
using DokanNet.Logging;
using EssentialsSql.Settings;
using Newtonsoft.Json;
using Serilog;
using SQLFS.Database;
using SQLFS.Drive;

namespace EssentialsSql
{
    public class Worker : BackgroundService
    {
        public Worker(ILogger<Worker> logger)
        {

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const string settingsFile = "settings.json";

            if (!File.Exists(settingsFile))
                throw new FileNotFoundException($"Settings file \"{settingsFile}\" was not found.");

            Log.Information("Reading settings...");
            var settingsContent = await File.ReadAllTextAsync(settingsFile, stoppingToken);
            var settings = JsonConvert.DeserializeObject<Settings.Settings>(settingsContent);

            var dbOptions = settings.Database;
            var fsOptions = settings.FileSystem;

            fsOptions.SecurityTemplate ??= Environment.CurrentDirectory;

            Log.Information("Initializing database...");
            var db = new FsDatabase<UserdataFile>(dbOptions, fsOptions.FileFactory, fsOptions.FileTemplate);

            Log.Information("Creating database tabled...");
            await db.CreateTables();

            var drive = new SqlFileSystem<UserdataFile>(fsOptions, db);
            const DokanOptions options = DokanOptions.NetworkDrive;

            Log.Information($"Mouting drive to {fsOptions.Mount}");
            drive.Mount(fsOptions.Mount, options, settings.FileSystem.Threads, logger: new DokanLogger());
        }

        private class DokanLogger : DokanNet.Logging.ILogger
        {
            public void Debug(string message, params object[] args)
            {
                Log.Debug(message, args);
            }

            public void Info(string message, params object[] args)
            {
                Log.Information(message, args);
            }

            public void Warn(string message, params object[] args)
            {
                Log.Warning(message, args);
            }

            public void Error(string message, params object[] args)
            {
                Log.Error(message, args);
            }

            public void Fatal(string message, params object[] args)
            {
                Log.Fatal(message, args);
            }
        }
    }
}
