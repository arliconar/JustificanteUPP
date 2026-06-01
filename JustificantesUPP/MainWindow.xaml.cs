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

namespace JustificantesUPP
{
    public sealed partial class MainWindow : Window
    {
        MailClass mail = new MailClass();
        private List<Alumno> listaOriginalAlumnos = new List<Alumno>();
        private List<Profesor> listaOriginalProfesores = new List<Profesor>();
        Justificante justi = new Justificante();
        
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "icon.ico"));
            }
            catch (Exception)
            {
                // Ignorar si el icono no se puede cargar
            }
            CargarDatos();
            RootGrid.DataContext = justi;
        }

        private void BoldClick(object sender, RoutedEventArgs e)
        {
            TxtMotivo.Document.Selection.CharacterFormat.Bold = FormatEffect.Toggle;
        }

        private void ItalicClick(object sender, RoutedEventArgs e)
        {
            TxtMotivo.Document.Selection.CharacterFormat.Italic = FormatEffect.Toggle;
        }

        private void UnderlineClick(object sender, RoutedEventArgs e)
        {
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
            await db.Database.EnsureCreatedAsync();
        }

        private async void BtnCargar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var db = new UppDbContext();
                var listaAlumnos = await db.Alumnos.ToListAsync();
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
                var listaProfesor = await db.Profesores.ToListAsync();
                ProfesoresGrid.ItemsSource = listaProfesor;
            }
            catch (System.Exception ex)
            {
            }
        }
        private async void BtnEnviarCorreo_Click(object sender, RoutedEventArgs e)
        {
            PrepararJustificante();
            MimeMessage mensaje = justi.CrearCorreo();
            MailClass mail = new MailClass();
            await mail.EnviarAsync(mensaje);
        }

        private void PrepararJustificante()
        {
            justi.Alumnos?.Clear();
            justi.Profesores?.Clear();

            if (justi.Alumnos == null) justi.Alumnos = new List<Alumno>();
            if (justi.Profesores == null) justi.Profesores = new List<Profesor>();

            var seleccionadosAlumnos = listaOriginalAlumnos.Where(a => a.IsSelected).ToList();
            justi.Alumnos.AddRange(seleccionadosAlumnos);

            var seleccionadosProfesores = listaOriginalProfesores.Where(p => p.IsSelected).ToList();
            justi.Profesores.AddRange(seleccionadosProfesores);

            TxtMotivo.Document.GetText(Microsoft.UI.Text.TextGetOptions.UseObjectText, out string contenido);
            justi.Motivo = contenido;
            
            DateTimeOffset? selectedDateInicio = DpFechaInicio.Date;

            if (selectedDateInicio.HasValue)
            {
                justi.FechaInicio = DateOnly.FromDateTime(selectedDateInicio.Value.DateTime);
            }
            
            DateTimeOffset? selectedDateFinal = DpFechaFinal.Date;

            if (selectedDateFinal.HasValue)
            {
                justi.FechaFinal = DateOnly.FromDateTime(selectedDateFinal.Value.DateTime);
            }
            else
            {
                justi.FechaFinal = justi.FechaInicio;
            }
        }

        private void BtnGenerarWord_Click(object sender, RoutedEventArgs e)
        {
            PrepararJustificante();
        }

        private async void MenuAgregarAlumno_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Agregar Alumno",
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot
            };

            var panel = new StackPanel { Spacing = 10, MinWidth = 300 };
            var txtNc = new TextBox { Header = "NC" };
            var txtNombre = new TextBox { Header = "Nombre" };
            var txtCorreo = new TextBox { Header = "Correo" };
            var txtGrupo = new TextBox { Header = "Grupo" };
            var cmbGenero = new ComboBox { Header = "Género" };
            cmbGenero.Items.Add("Femenino");
            cmbGenero.Items.Add("Masculino");
            cmbGenero.Items.Add("Otro");
            cmbGenero.SelectedIndex = 0;

            panel.Children.Add(txtNc);
            panel.Children.Add(txtNombre);
            panel.Children.Add(txtCorreo);
            panel.Children.Add(txtGrupo);
            panel.Children.Add(cmbGenero);
            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (string.IsNullOrWhiteSpace(txtNc.Text)) return;
                using var db = new UppDbContext();
                if (await db.Alumnos.AnyAsync(a => a.Nc == txtNc.Text)) return;
                var nuevoAlumno = new Alumno
                {
                    Nc = txtNc.Text,
                    Nombre = txtNombre.Text ?? "",
                    Correo = txtCorreo.Text ?? "",
                    Grupo = txtGrupo.Text ?? "",
                    Genero = (Genero)cmbGenero.SelectedIndex
                };
                db.Alumnos.Add(nuevoAlumno);
                await db.SaveChangesAsync();
                CargarDatos();
            }
        }

        private async void MenuEditarAlumno_Click(object sender, RoutedEventArgs e)
        {
            if (AlumnosGrid.SelectedItem is not Alumno alumnoSeleccionado) return;

            var dialog = new ContentDialog
            {
                Title = "Editar Alumno",
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot
            };

            var panel = new StackPanel { Spacing = 10, MinWidth = 300 };
            var txtNc = new TextBox { Header = "NC", Text = alumnoSeleccionado.Nc, IsEnabled = false };
            var txtNombre = new TextBox { Header = "Nombre", Text = alumnoSeleccionado.Nombre };
            var txtCorreo = new TextBox { Header = "Correo", Text = alumnoSeleccionado.Correo };
            var txtGrupo = new TextBox { Header = "Grupo", Text = alumnoSeleccionado.Grupo };
            var cmbGenero = new ComboBox { Header = "Género" };
            cmbGenero.Items.Add("Femenino");
            cmbGenero.Items.Add("Masculino");
            cmbGenero.Items.Add("Otro");
            cmbGenero.SelectedIndex = (int)alumnoSeleccionado.Genero;

            panel.Children.Add(txtNc);
            panel.Children.Add(txtNombre);
            panel.Children.Add(txtCorreo);
            panel.Children.Add(txtGrupo);
            panel.Children.Add(cmbGenero);
            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                using var db = new UppDbContext();
                var alumno = await db.Alumnos.FindAsync(txtNc.Text);
                if (alumno != null)
                {
                    alumno.Nombre = txtNombre.Text ?? "";
                    alumno.Correo = txtCorreo.Text ?? "";
                    alumno.Grupo = txtGrupo.Text ?? "";
                    alumno.Genero = (Genero)cmbGenero.SelectedIndex;
                    await db.SaveChangesAsync();
                    CargarDatos();
                }
            }
        }

        private async void MenuEditarGrupoAlumnos_Click(object sender, RoutedEventArgs e)
        {
            var alumnos = AlumnosGrid.ItemsSource as IEnumerable<Alumno>;
            var seleccionados = alumnos?.Where(a => a.IsSelected).ToList() ?? new List<Alumno>();

            if (seleccionados.Count == 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "Aviso",
                    Content = "Debes seleccionar al menos un alumno para editar su grupo.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            var editDialog = new ContentDialog
            {
                Title = "Editar Grupo Masivo",
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot
            };

            var panel = new StackPanel { Spacing = 10, MinWidth = 300 };
            var txtGrupo = new TextBox { Header = "Nuevo Grupo", PlaceholderText = "Escribe el grupo para los seleccionados" };
            panel.Children.Add(txtGrupo);
            editDialog.Content = panel;

            var result = await editDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                using var db = new UppDbContext();
                foreach (var al in seleccionados)
                {
                    var alumno = await db.Alumnos.FindAsync(al.Nc);
                    if (alumno != null)
                    {
                        alumno.Grupo = txtGrupo.Text ?? "";
                    }
                }
                await db.SaveChangesAsync();
                CargarDatos();
            }
        }

        private async void MenuEliminarAlumno_Click(object sender, RoutedEventArgs e)
        {
            var alumnos = AlumnosGrid.ItemsSource as IEnumerable<Alumno>;
            var seleccionados = alumnos?.Where(a => a.IsSelected).ToList() ?? new List<Alumno>();

            if (seleccionados.Count == 0)
            {
                if (AlumnosGrid.SelectedItem is Alumno alumnoSeleccionado)
                {
                    seleccionados.Add(alumnoSeleccionado);
                }
                else return;
            }

            string mensaje = seleccionados.Count > 1 
                ? "¿Estás seguro de que deseas eliminar a " + seleccionados.Count + " alumnos seleccionados?"
                : "¿Estás seguro de que deseas eliminar al alumno " + seleccionados[0].Nombre + "?";

            var confirmDialog = new ContentDialog
            {
                Title = "Confirmar Eliminación",
                Content = mensaje,
                PrimaryButtonText = "Eliminar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                using var db = new UppDbContext();
                foreach(var al in seleccionados)
                {
                    var alumno = await db.Alumnos.FindAsync(al.Nc);
                    if (alumno != null)
                    {
                        db.Alumnos.Remove(alumno);
                    }
                }
                await db.SaveChangesAsync();
                CargarDatos();
            }
        }

        private async void MenuAgregarProfesor_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Agregar Profesor",
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot
            };

            var panel = new StackPanel { Spacing = 10, MinWidth = 300 };
            var txtNombre = new TextBox { Header = "Nombre" };
            var txtCorreo = new TextBox { Header = "Correo" };
            var cmbGenero = new ComboBox { Header = "Género" };
            cmbGenero.Items.Add("Femenino");
            cmbGenero.Items.Add("Masculino");
            cmbGenero.Items.Add("Otro");
            cmbGenero.SelectedIndex = 0;

            panel.Children.Add(txtNombre);
            panel.Children.Add(txtCorreo);
            panel.Children.Add(cmbGenero);
            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                using var db = new UppDbContext();
                var nuevoProfesor = new Profesor
                {
                    Nombre = txtNombre.Text ?? "",
                    Correo = txtCorreo.Text ?? "",
                    Genero = (Genero)cmbGenero.SelectedIndex
                };
                db.Profesores.Add(nuevoProfesor);
                await db.SaveChangesAsync();
                CargarDatos();
            }
        }

        private async void MenuEditarProfesor_Click(object sender, RoutedEventArgs e)
        {
            if (ProfesoresGrid.SelectedItem is not Profesor profesorSeleccionado) return;

            var dialog = new ContentDialog
            {
                Title = "Editar Profesor",
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot
            };

            var panel = new StackPanel { Spacing = 10, MinWidth = 300 };
            var txtId = new TextBox { Header = "ID", Text = profesorSeleccionado.Id.ToString(), IsEnabled = false, Visibility = Visibility.Collapsed };
            var txtNombre = new TextBox { Header = "Nombre", Text = profesorSeleccionado.Nombre };
            var txtCorreo = new TextBox { Header = "Correo", Text = profesorSeleccionado.Correo };
            var cmbGenero = new ComboBox { Header = "Género" };
            cmbGenero.Items.Add("Femenino");
            cmbGenero.Items.Add("Masculino");
            cmbGenero.Items.Add("Otro");
            cmbGenero.SelectedIndex = (int)profesorSeleccionado.Genero;

            panel.Children.Add(txtId);
            panel.Children.Add(txtNombre);
            panel.Children.Add(txtCorreo);
            panel.Children.Add(cmbGenero);
            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                using var db = new UppDbContext();
                var profesor = await db.Profesores.FindAsync(profesorSeleccionado.Id);
                if (profesor != null)
                {
                    profesor.Nombre = txtNombre.Text ?? "";
                    profesor.Correo = txtCorreo.Text ?? "";
                    profesor.Genero = (Genero)cmbGenero.SelectedIndex;
                    await db.SaveChangesAsync();
                    CargarDatos();
                }
            }
        }

        private async void MenuEliminarProfesor_Click(object sender, RoutedEventArgs e)
        {
            var profesores = ProfesoresGrid.ItemsSource as IEnumerable<Profesor>;
            var seleccionados = profesores?.Where(p => p.IsSelected).ToList() ?? new List<Profesor>();

            if (seleccionados.Count == 0)
            {
                if (ProfesoresGrid.SelectedItem is Profesor profesorSeleccionado)
                {
                    seleccionados.Add(profesorSeleccionado);
                }
                else return;
            }

            string mensaje = seleccionados.Count > 1 
                ? "¿Estás seguro de que deseas eliminar a " + seleccionados.Count + " profesores seleccionados?"
                : "¿Estás seguro de que deseas eliminar al profesor " + seleccionados[0].Nombre + "?";

            var confirmDialog = new ContentDialog
            {
                Title = "Confirmar Eliminación",
                Content = mensaje,
                PrimaryButtonText = "Eliminar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                using var db = new UppDbContext();
                foreach(var prof in seleccionados)
                {
                    var profesor = await db.Profesores.FindAsync(prof.Id);
                    if (profesor != null)
                    {
                        db.Profesores.Remove(profesor);
                    }
                }
                await db.SaveChangesAsync();
                CargarDatos();
            }
        }

        private async void BtnImportarAlumnos_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".csv");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var lineas = await System.IO.File.ReadAllLinesAsync(file.Path);
                using var db = new UppDbContext();
                foreach (var linea in lineas.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    var partes = linea.Split(',');
                    if (partes.Length >= 4)
                    {
                        if (await db.Alumnos.AnyAsync(a => a.Nc == partes[0])) continue;
                        Enum.TryParse(partes[3], true, out Genero g);
                        string grupo = partes.Length >= 5 ? partes[4] : "";
                        db.Alumnos.Add(new Alumno
                        {
                            Nc = partes[0],
                            Nombre = partes[1],
                            Correo = partes[2],
                            Grupo = grupo,
                            Genero = g
                        });
                    }
                }
                await db.SaveChangesAsync();
                CargarDatos();
            }
        }

        private async void BtnExportarAlumnos_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("CSV File", new List<string>() { ".csv" });
            picker.SuggestedFileName = "Alumnos_Seleccionados.csv";

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var alumnosGrid = AlumnosGrid.ItemsSource as IEnumerable<Alumno>;
            var seleccionados = alumnosGrid?.Where(a => a.IsSelected).ToList() ?? new List<Alumno>();
            
            if (seleccionados.Count == 0 && AlumnosGrid.SelectedItem is Alumno al) 
            {
                seleccionados.Add(al);
            }

            if (seleccionados.Count == 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "Aviso",
                    Content = "No has seleccionado ningún alumno para exportar.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                var lineas = new List<string> { "NC,Nombre,Correo,Genero,Grupo" };
                lineas.AddRange(seleccionados.Select(a => $"{a.Nc},{a.Nombre},{a.Correo},{a.Genero},{a.Grupo}"));
                await System.IO.File.WriteAllLinesAsync(file.Path, lineas);
            }
        }

        private async void BtnImportarProfesores_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".csv");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var lineas = await System.IO.File.ReadAllLinesAsync(file.Path);
                using var db = new UppDbContext();
                foreach (var linea in lineas.Skip(1)) 
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    var partes = linea.Split(',');
                    if (partes.Length >= 3)
                    {
                        Enum.TryParse(partes[2], true, out Genero g);
                        db.Profesores.Add(new Profesor
                        {
                            Nombre = partes[0],
                            Correo = partes[1],
                            Genero = g
                        });
                    }
                }
                await db.SaveChangesAsync();
                CargarDatos();
            }
        }

        private async void BtnExportarProfesores_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("CSV File", new List<string>() { ".csv" });
            picker.SuggestedFileName = "Profesores_Seleccionados.csv";

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var profesoresGrid = ProfesoresGrid.ItemsSource as IEnumerable<Profesor>;
            var seleccionados = profesoresGrid?.Where(p => p.IsSelected).ToList() ?? new List<Profesor>();

            if (seleccionados.Count == 0 && ProfesoresGrid.SelectedItem is Profesor prof) 
            {
                seleccionados.Add(prof);
            }

            if (seleccionados.Count == 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "Aviso",
                    Content = "No has seleccionado ningún profesor para exportar.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                var lineas = new List<string> { "Nombre,Correo,Genero" };
                lineas.AddRange(seleccionados.Select(p => $"{p.Nombre},{p.Correo},{p.Genero}"));
                await System.IO.File.WriteAllLinesAsync(file.Path, lineas);
            }
        }

        private async void BtnEditarOwner_Click(object sender, RoutedEventArgs e)
        {
            var owner = justi.OwnerData;

            var dialog = new ContentDialog
            {
                Title = "Editar Información del Remitente",
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot
            };

            var panel = new StackPanel { Spacing = 10, MinWidth = 300 };
            var txtNombre = new TextBox { Header = "Nombre", Text = owner.Nombre };
            var txtCorreo = new TextBox { Header = "Correo", Text = owner.Correo };
            var cmbGenero = new ComboBox { Header = "Género" };
            cmbGenero.Items.Add("Femenino");
            cmbGenero.Items.Add("Masculino");
            cmbGenero.Items.Add("Otro");
            cmbGenero.SelectedIndex = (int)owner.Genero;
            var txtFirma = new TextBox { Header = "Ruta de la Firma", Text = owner.firmapath };

            panel.Children.Add(txtNombre);
            panel.Children.Add(txtCorreo);
            panel.Children.Add(cmbGenero);
            panel.Children.Add(txtFirma);
            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                owner.Nombre = txtNombre.Text ?? "";
                owner.Correo = txtCorreo.Text ?? "";
                owner.Genero = (Genero)cmbGenero.SelectedIndex;
                owner.firmapath = txtFirma.Text ?? "";
                owner.Save();
                justi.OwnerData = owner;
            }
        }

        private void BtnSeleccionarTodosAlumnos_Click(object sender, RoutedEventArgs e)
        {
            if (AlumnosGrid.ItemsSource is not IEnumerable<Alumno> alumnos) return;
            var list = alumnos.ToList();
            foreach (var al in list) al.IsSelected = true;
            AlumnosGrid.ItemsSource = null;
            AlumnosGrid.ItemsSource = list;
        }

        private void BtnDeseleccionarTodosAlumnos_Click(object sender, RoutedEventArgs e)
        {
            if (AlumnosGrid.ItemsSource is not IEnumerable<Alumno> alumnos) return;
            var list = alumnos.ToList();
            foreach (var al in list) al.IsSelected = false;
            AlumnosGrid.ItemsSource = null;
            AlumnosGrid.ItemsSource = list;
        }

        private void BtnSeleccionarTodosProfesores_Click(object sender, RoutedEventArgs e)
        {
            if (ProfesoresGrid.ItemsSource is not IEnumerable<Profesor> profesores) return;
            var list = profesores.ToList();
            foreach (var prof in list) prof.IsSelected = true;
            ProfesoresGrid.ItemsSource = null;
            ProfesoresGrid.ItemsSource = list;
        }

        private void BtnDeseleccionarTodosProfesores_Click(object sender, RoutedEventArgs e)
        {
            if (ProfesoresGrid.ItemsSource is not IEnumerable<Profesor> profesores) return;
            var list = profesores.ToList();
            foreach (var prof in list) prof.IsSelected = false;
            ProfesoresGrid.ItemsSource = null;
            ProfesoresGrid.ItemsSource = list;
        }
    }
}
