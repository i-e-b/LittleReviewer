using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace LittleReviewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (! Debugger.IsAttached && !AppDomain.CurrentDomain.FriendlyName.EndsWith("vshost.exe"))
            {
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            }

            Application.Run(new MainForm());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException(e.ExceptionObject);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogException(e.Exception);
        }

        private static void LogException(object exceptionObject)
        {
            Directory.CreateDirectory("C:\\Temp");
            File.AppendAllText("C:\\Temp\\Reviewer.log", "\r\n\r\n" + DateTime.Now);
            File.AppendAllText("C:\\Temp\\Reviewer.log", "\r\n" + exceptionObject);
        }
    }
}
