using BodyTracker.Models;
using BodyTracker.Models.Languages;
using BodyTracker.Models.WorkoutLog;
using BodyTracker.Resources.Errors;
using BodyTracker.Resources.Languages;
using BodyTracker.Services;
using BodyTracker.Services.Localization;
using BodyTracker.Services.WorkoutLog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Web;
using System.Windows;

namespace BodyTracker.Services.App
{
    /// <summary>
    /// Provides utility methods for restarting the current application instance.
    /// </summary>
    public static class RestartApplication
    {
        /// <summary>
        /// Restarts the application by launching a new process instance using the current executable path 
        /// and shutting down the currently running application.
        /// </summary>
        /// <exception cref="Exception">Thrown when the application fails to restart successfully.</exception>
        public static void Restart()
        {
            var executablePath = Environment.ProcessPath;

            if (string.IsNullOrEmpty(executablePath))
                return;

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                UseShellExecute = true
            };

            try
            {
                Process.Start(startInfo);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                throw new Exception($"{ErrorMessage.RestartFailed}: {ex}", ex);
            }
        }
    }
}
