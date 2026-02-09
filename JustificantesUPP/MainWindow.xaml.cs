using Google.Apis.Gmail.v1;
using JustificantesUPP.Modelos;
using Microsoft.EntityFrameworkCore;
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
using Microsoft.UI.Text;
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
        private List<Alumno> listaOriginalAlumnos = new List<Alumno>();
        private List<Profesor> listaOriginalProfesores = new List<Profesor>();
        Justificante justi = new Justificante();
        public MainWindow()
        {
            InitializeComponent();
            CargarDatos();
            RootGrid.DataContext = justi;

        }
        private void BoldClick(object sender, RoutedEventArgs e)
        {
            // Cambia entre negrita y normal
            TxtMotivo.Document.Selection.CharacterFormat.Bold = FormatEffect.Toggle;
        }

        private void ItalicClick(object sender, RoutedEventArgs e)
        {
            // Cambia entre cursiva y normal
            TxtMotivo.Document.Selection.CharacterFormat.Italic = FormatEffect.Toggle;
        }

        private void UnderlineClick(object sender, RoutedEventArgs e)
        {
            // Para subrayado, comparamos si ya está activo
            if (TxtMotivo.Document.Selection.CharacterFormat.Underline == UnderlineType.Single)
            {
                TxtMotivo.Document.Selection.CharacterFormat.Underline = UnderlineType.None;
            }
            else
            {
                TxtMotivo.Document.Selection.CharacterFormat.Underline = UnderlineType.Single;
            }
        }
        private async void CargarDatos()
        {
            using var db = new UppDbContext();
            listaOriginalAlumnos = await db.Alumnos.ToListAsync();
            listaOriginalProfesores = await db.Profesores.ToListAsync();

            AlumnosGrid.ItemsSource = listaOriginalAlumnos;
            ProfesoresGrid.ItemsSource = listaOriginalProfesores;
        }

        private void TxtBusquedaAlumnos_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var texto = sender.Text.ToLower();
            var filtrados = listaOriginalAlumnos.Where(a =>
                a.Nombre.ToLower().Contains(texto) ||
                a.Nc.ToString().Contains(texto)
            ).ToList();

            AlumnosGrid.ItemsSource = filtrados;
        }

        private void TxtBusquedaProfesores_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var texto = sender.Text.ToLower();
            var filtrados = listaOriginalProfesores.Where(p =>
                p.Nombre.ToLower().Contains(texto) ||
                p.Correo.ToLower().Contains(texto)
            ).ToList();

            ProfesoresGrid.ItemsSource = filtrados;
        }
        private async void InicializarBaseDeDatos()
        {
            using var db = new UppDbContext();
            // Esto crea la DB y la tabla si no existen (útil para desarrollo rápido)
            await db.Database.EnsureCreatedAsync();
        }

        private async void BtnCargar_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                using var db = new UppDbContext();
                // Consultamos usando EF Core
                var listaAlumnos = await db.Alumnos.ToListAsync();

                // Asignamos al DataGrid
                AlumnosGrid.ItemsSource = listaAlumnos;
            }
            catch (System.Exception ex)
            {
            }
        }

        private async void BtnCargarprof_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                using var db = new UppDbContext();
                // Consultamos usando EF Core
                var listaProfesor = await db.Profesores.ToListAsync();

                // Asignamos al DataGrid
                ProfesoresGrid.ItemsSource = listaProfesor;
            }
            catch (System.Exception ex)
            {
            }
        }
        private async void BtnEnviarCorreo_Click(object sender, RoutedEventArgs e)
        {
            // 1. Recolectar datos y seleccionados
            PrepararJustificante();

        }

        private void PrepararJustificante()
        {
           
            // El motivo ya está enlazado por data binding, así que no es necesario asignarlo aquí
        }
        private void BtnGenerarWord_Click(object sender, RoutedEventArgs e)
        {
            // 1. Recolectar datos
            PrepararJustificante();

            // 2. Lógica para crear el Word
            // Aquí podrías usar librerías como DocX o OpenXML
            // GenerarDocumentoWord(MiJustificante);
        }

    }
}

