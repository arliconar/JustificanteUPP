using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace JustificantesUPP
{
    public partial class App : Application
    {
        private Window? _window;

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

        private void MostrarErrorGlobal(string mensaje)
        {
            Log(mensaje);
            MessageBoxW(IntPtr.Zero, mensaje, "Error fatal en JustificantesUPP", 0x10); // 0x10 is MB_ICONERROR
        }

        private void Log(string message)
        {
            try
            {
                string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JustificantesUPP_Log.txt");
                System.IO.File.AppendAllText(path, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch { }
        }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                MostrarErrorGlobal($"UnhandledException (AppDomain): {e.ExceptionObject}");
            };

            this.UnhandledException += App_UnhandledException;
            
            try 
            {
                Log("App initializing...");
                InitializeComponent();
                SQLitePCL.Batteries.Init();
                Log("App initialization completed.");
            } 
            catch (Exception ex) 
            {
                MostrarErrorGlobal($"Error in constructor: {ex}");
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            MostrarErrorGlobal($"Unhandled Exception (XAML): {e.Exception}");
            e.Handled = true; 
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Log("OnLaunched called.");
            try 
            {
                _window = new MainWindow();
                _window.Activate();
                Log("MainWindow activated.");
            } 
            catch (Exception ex) 
            {
                MostrarErrorGlobal($"Error in OnLaunched: {ex}");
            }
        }
    }
}
