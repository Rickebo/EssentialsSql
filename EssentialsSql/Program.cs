using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}
