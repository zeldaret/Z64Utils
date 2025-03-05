using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Z64;
using Z64.Forms;

namespace Z64
{
    static class Program
    {
        public static readonly string Version =
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "UnknownVersion";

        [System.Runtime.InteropServices.DllImport("nvapi64.dll", EntryPoint = "fake")]
        static extern int LoadNvApi64();

        [System.Runtime.InteropServices.DllImport("nvapi.dll", EntryPoint = "fake")]
        static extern int LoadNvApi32();

        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            try
            {
                if (Environment.Is64BitProcess)
                    LoadNvApi64();
                else
                    LoadNvApi32();
            }
            catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
