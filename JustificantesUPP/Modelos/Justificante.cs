using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace JustificantesUPP.Modelos
{
    public class Justificante : INotifyPropertyChanged
    {
        DateOnly Fecha { get; set; }

        private string? _motivo;
        public string Motivo
        {
            get => _motivo;
            set { _motivo = value; OnPropertyChanged(); }
        }

        List<Alumno> Alumnos { get; set; }
        List<Profesor> Profesores { get; set; }
        
        DateOnly FechaInicio { get; set; }

        DateOnly FechaFinal { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Justificante()
        {
            Alumnos = new List<Alumno>();
            Profesores = new List<Profesor>();
            FechaInicio = DateOnly.FromDateTime(DateTime.Now);
            FechaFinal = DateOnly.FromDateTime(DateTime.Now);
            Motivo = string.Empty;  
        }

    }
}
