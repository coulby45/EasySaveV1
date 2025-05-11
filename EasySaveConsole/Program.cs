using System;
using EasySaveConsole;
using EasySaveConsole.Controllers;
using EasySaveLogging.Logger;
using EasySaveConsole.config;

namespace EasySaveV1.EasySave.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // Init Logger
            var logDir = Config.GetLogDirectory();
            var logger = Logger.GetInstance();
            var logFile = Path.Combine(
                logDir,
                DateTime.Today.ToString("yyyy-MM-dd") + ".json"
            );
            logger.SetLogFilePath(logFile);

            // Lancer le Controller
            var controller = new BackupController();
            controller.Start(args);
        }
    }
}
