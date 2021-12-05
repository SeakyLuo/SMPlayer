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
        private static StorageFolder LogFolder;

        public static async Task Init()
        {
            LogFolder = await FileHelper.CreateFolder("Logs");
        }

        public static async Task ClearLogFiles(int maxBackups = 5)
        {
            if (LogFolder == null) return;
            var files = await LogFolder.GetFilesAsync();
            await Helper.ClearBackup(files, LogFileName, maxBackups);
        }

        public static void Debug(string message, params object[] args)
        {
            log.Debug_(message, args);
        }

        public static void Info(string message, params object[] args)
        {
            log.Info_(message, args);
        }

        public static void Warn(string message, params object[] args)
        {
            log.Warn_(message, args);
        }

        public static void Error(string message, params object[] args)
        {
            log.Error_(message, args);
        }

        public void Debug_(string message, params object[] args)
        {
            PrintMessage(LogLevel.Debug, message, args);
        }

        public void Info_(string message, params object[] args)
        {
            string finalMessage = PrintMessage(LogLevel.Info, message, args);
            AppendText(LogFileName, finalMessage);
        }

        public void Warn_(string message, params object[] args)
        {
            string finalMessage = PrintMessage(LogLevel.Warn, message, args);
            AppendText(LogFileName, finalMessage);
        }

        public void Error_(string message, params object[] args)
        {
            string finalMessage = PrintMessage(LogLevel.Error, message, args);
            AppendText(LogFileName, finalMessage);
        }

        private string PrintMessage(LogLevel level, string message, params object[] args)
        {
            string finalMessage = string.Format($"{BuildMessageHeader(level)} {message}", args);
            System.Diagnostics.Debug.WriteLine(finalMessage);
            return finalMessage;
        }

        private string BuildMessageHeader(LogLevel level)
        {
            StackFrame[] frames = new StackTrace().GetFrames();
            StackFrame frame = frames[4];
            MethodBase method = frame.GetMethod();
            string className = method.DeclaringType.FullName;
            return string.Format($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}][{className}][{method.Name}][{level}]");
        }

        private void AppendText(string filename, string message)
        {
            string filePath = Path.Combine(LogFolder.Path, $"{filename}_{DateTime.UtcNow:yyyy-MM-dd}.log");
            File.AppendAllText(filePath, message + Environment.NewLine);
        }
    }

    enum LogLevel {
        Debug, Info, Warn, Error
    }
}
