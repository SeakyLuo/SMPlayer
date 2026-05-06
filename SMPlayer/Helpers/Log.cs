using Microsoft.Services.Store.Engagement;
using SMPlayer.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer
{
    public class Log
    {
        private const string LogFileName = "SMPlayerLog";
        private static readonly Log log = new Log();
        private static StoreServicesCustomEventLogger logger;
        private static StorageFolder LogFolder;

        public static async Task Init()
        {
            LogFolder = await StorageHelper.CreateFolder("Logs");
            logger = StoreServicesCustomEventLogger.GetDefault();
        }

        public static async Task ClearLogFiles(int maxBackups = 5)
        {
            if (LogFolder == null) return;
            var files = await LogFolder.GetFilesAsync();
            await Helper.ClearBackup(files, LogFileName, maxBackups);
        }

        public static void Debug(string message, params object[] args)
        {
            log.debug(message, args);
        }

        public static void Info(string message, params object[] args)
        {
            log.info(message, args);
        }

        public static void Warn(string message, params object[] args)
        {
            log.warn(message, args);
        }

        public static void Error(string message, params object[] args)
        {
            log.error(message, args);
        }

        private void debug(string message, params object[] args)
        {
            PrintMessage(LogLevel.Debug, message, args);
        }

        private void info(string message, params object[] args)
        {
            string finalMessage = PrintMessage(LogLevel.Info, message, args);
            AppendText(LogFileName, finalMessage);
        }

        private void warn(string message, params object[] args)
        {
            string finalMessage = PrintMessage(LogLevel.Warn, message, args);
            AppendText(LogFileName, finalMessage);
        }

        private void error(string message, params object[] args)
        {
            string finalMessage = PrintMessage(LogLevel.Error, message, args);
            AppendText(LogFileName, finalMessage);
        }

        private string PrintMessage(LogLevel level, string message, params object[] args)
        {
            string finalMessage;
            try
            {
                finalMessage = string.Format($"{BuildMessageHeader(level)} {message}", args);
                System.Diagnostics.Debug.WriteLine(finalMessage);
                logger.Log(finalMessage);
                return finalMessage;
            }
            catch (Exception)
            {
                finalMessage = $"{BuildMessageHeader(level)} {message}";
                System.Diagnostics.Debug.WriteLine($"BuildMessage failed, message {finalMessage} args {args}");
                return finalMessage;
            }
        }

        private string BuildMessageHeader(LogLevel level)
        {
            try 
            {
                StackFrame[] frames = new StackTrace().GetFrames();
                StackFrame frame = frames[4];
                MethodBase method = frame.GetMethod();
                return string.Format($"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} [{level}] ({method.DeclaringType.Name})");
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private void AppendText(string filename, string message)
        {
            try
            {
                string filePath = Path.Combine(LogFolder.Path, $"{filename}_{DateTime.Now:yyyy-MM-dd}.log");
                File.AppendAllText(filePath, message + Environment.NewLine);
            }
            catch (Exception)
            {
                // 可能文件被占用
            }
        }
    }

    enum LogLevel {
        Debug, Info, Warn, Error
    }
}
