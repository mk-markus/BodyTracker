using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Logging
{
    /// <summary>
    /// Provides a lightweight, environment-aware logging mechanism that writes timestamped 
    /// log entries either to the debug console during development or to a local file in release mode.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Records a log entry with a standardized timestamp prefix. 
        /// Outputs to the debug window in DEBUG configurations or appends to 'app.log' in RELEASE configurations.
        /// </summary>
        /// <param name="message">The text message to log.</param>
        public static void Log(string message, string logFilename)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

#if DEBUG

            // Wird im Debug-Modus in die Debug-Konsole ausgegeben
            Debug.WriteLine(logEntry);

#else
            try
            {
                // Wird im Release-Modus in eine Log-Datei geschrieben
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", logFilename);
                string directoryPath = Path.GetDirectoryName(logFilePath);
                
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    // 3. Ordner erstellen, falls er nicht existiert
                    Directory.CreateDirectory(directoryPath);
                }


                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch { }
#endif
        }
    }
}