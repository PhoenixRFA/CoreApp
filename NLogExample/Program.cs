using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace NLogExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Set nlog.config and get logger
            Logger logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                //add debug log
                logger.Debug("Init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //catch exception
                logger.Error(ex, "Exceprion in Main");
            }
            finally
            {
                //dispose logger
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    //Remove dafault loggers and set minimum log level
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                //Add NLog dependencies
                .UseNLog();
    }
}
