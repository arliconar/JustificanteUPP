using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using JustificantesUPP.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            MimeMessage mensaje = justi.CrearCorreo();
            MailClass mail = new MailClass();
            await mail.EnviarAsync(mensaje);




        }

        private void PrepararJustificante()
        {
            justi.Alumnos?.Clear();
            justi.Profesores?.Clear();

            // Asegurarse de que las listas en el objeto 'justi' estén inicializadas
            if (justi.Alumnos == null) justi.Alumnos = new List<Alumno>();
            if (justi.Profesores == null) justi.Profesores = new List<Profesor>();

            // 2. Filtrar y agregar los Alumnos seleccionados
            var seleccionadosAlumnos = listaOriginalAlumnos.Where(a => a.IsSelected).ToList();
            justi.Alumnos.AddRange(seleccionadosAlumnos);

            // 3. Filtrar y agregar los Profesores seleccionados
            var seleccionadosProfesores = listaOriginalProfesores.Where(p => p.IsSelected).ToList();
            justi.Profesores.AddRange(seleccionadosProfesores);

            // 4. Capturar el texto del RichEditBox (ya que no soporta binding directo)
            TxtMotivo.Document.GetText(Microsoft.UI.Text.TextGetOptions.UseObjectText, out string contenido);
            justi.Motivo = contenido;
            // El motivo ya está enlazado por data binding, así que no es necesario asignarlo aquí
            DateTimeOffset? selectedDateInicio = DpFechaInicio.Date;

            if (selectedDateInicio.HasValue)
            {
                // 2. Convertimos el DateTimeOffset a DateOnly
                // Usamos .Date para obtener el DateTime y luego DateOnly.FromDateTime
                justi.FechaInicio = DateOnly.FromDateTime(selectedDateInicio.Value.DateTime);

       
            }
            DateTimeOffset? selectedDateFinal = DpFechaFinal.Date;

            if (selectedDateFinal.HasValue)
            {
                // 2. Convertimos el DateTimeOffset a DateOnly
                // Usamos .Date para obtener el DateTime y luego DateOnly.FromDateTime
                justi.FechaFinal = DateOnly.FromDateTime(selectedDateFinal.Value.DateTime);

            }
            else
            {
                justi.FechaFinal = justi.FechaInicio;
            }



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

