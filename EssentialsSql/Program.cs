using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace EssentialsSql
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var prefix = "-wd=";
            var wdArg = args.FirstOrDefault(arg => arg.StartsWith(prefix, StringComparison.InvariantCulture));

            if (wdArg != null)
            {
                args = args.Where(arg => arg != wdArg).ToArray();
                Environment.CurrentDirectory = wdArg.Substring(prefix.Length);
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("settings.json", false, true);
                    config.AddCommandLine(args);
                })
                .ConfigureLogging(logBuilder =>
                {
                    var config = new ConfigurationBuilder()
                            .AddJsonFile("settings.json")
                            .Build();

                    var logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(config)
                            .CreateLogger();

                    Log.Logger = logger;
                    logBuilder.AddSerilog(logger, dispose: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}
