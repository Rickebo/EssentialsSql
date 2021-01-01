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
using SQLFS.Database;
using SQLFS.Drive;

namespace EssentialsSql
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const string settingsFile = "settings.json";

            if (!File.Exists(settingsFile))
                throw new FileNotFoundException($"Settings file \"{settingsFile}\" was not found.");

            _logger.LogInformation("Reading settings...");
            var settingsContent = await File.ReadAllTextAsync(settingsFile, stoppingToken);
            var settings = JsonConvert.DeserializeObject<Settings.Settings>(settingsContent);

            var dbOptions = settings.Database;
            var fsOptions = settings.FileSystem;

            _logger.LogInformation("Initializing database...");
            var db = new FsDatabase<UserdataFile>(dbOptions, fsOptions.FileFactory, fsOptions.FileTemplate);

            _logger.LogInformation("Creating database tabled...");
            await db.CreateTables();

            var drive = new SqlFileSystem<UserdataFile>(fsOptions, db);
            const DokanOptions options = DokanOptions.NetworkDrive;

            var dokanLogger = settings.LogToConsole
                    ? (DokanNet.Logging.ILogger) new ConsoleLogger()
                    : (DokanNet.Logging.ILogger) new NullLogger();

            _logger.LogInformation($"Mouting drive to {fsOptions.Mount}");
            drive.Mount(fsOptions.Mount, options, settings.FileSystem.Threads, logger: dokanLogger);
        }
    }
}
