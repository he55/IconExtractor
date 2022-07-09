using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IconExtractor
{
    internal static class Program
    {
        [DllImport("User32.dll", SetLastError = false, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetProcessDPIAware();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if NETCOREAPP3_0_OR_GREATER
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
#else
            SetProcessDPIAware();
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
