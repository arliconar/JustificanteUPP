using Google.Apis.Gmail.v1;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace JustificantesUPP
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        MailClass mail = new MailClass();

        public MainWindow()
        {
            InitializeComponent();
        }
        private async void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {

          

            try
            {
                // Llamamos a tu clase de envío
                await mail.EnviarAsync();

                StatusText.Text = "¡Correo enviado con éxito!";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
            }
            finally
            {
                BtnEnviar.IsEnabled = true;
                Progreso.IsActive = false;
            }
        }
    }
}

