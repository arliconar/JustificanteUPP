using Microsoft.VisualBasic;
using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
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

        public List<Alumno> Alumnos { get; set; }
        public List<Profesor> Profesores { get; set; }
        Owner owner = new Owner
        {
            Nombre = "Dr. Angel Ricardo Licona Rodríguez",
            Correo = "arliconar@upp.edu.mx",
            Genero = Genero.Masculino,
        };
        public DateOnly FechaInicio { get; set; }

        public DateOnly FechaFinal { get; set; }
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

        private string getGenre(Alumno alumno)
        {
            return alumno.Genero switch
            {
                Genero.Femenino => " la alumna",
                Genero.Masculino => "l alumno",
                Genero.Otro => "el alumno",
                _ => "el alumno"
            };
        }
        private string getAlumnos()
        {
            if (Alumnos.Count == 1)
            {
                return getGenre(Alumnos[0]) + " " + Alumnos[0].Nombre;
            }
            else
            {
                string resultado = " los alumnos:";
                foreach (var alumno in Alumnos)
                {
                    resultado += alumno.Nombre + ", ";

                }
                return resultado;
            }

        }

    
        public string getProfesores()
        {
            if (Profesores.Count == 1)
            {
                if (Profesores[0].Genero == Genero.Femenino)
                {
                    return "Estimada profesora " + Profesores[0].Nombre;
                }
                else
                {
                    return "Estimado profesor " + Profesores[0].Nombre;
                }
            }
            else
            {
                string resultado = "Estimados profesores:";
                foreach (var profesor in Profesores)
                {
                    resultado +=Environment.NewLine+ profesor.Nombre ;
                }
                return resultado;
            }

        }
        
        private string getperido()
        {
            if (FechaInicio==FechaFinal)
            {
                return "día " + FechaInicio.ToString("dd/MM/yyyy");
            }
            else
            {
                return "periodo comprendido entre " + FechaInicio.ToString("dd/MM/yyyy") + " y " + FechaFinal.ToString("dd/MM/yyyy");
            }
        }
        public string getMotivo()
        {
            return $"{getProfesores()}, \n\n" +
              "Por medio de la presente, me permito hacer de su conocimiento la justificación de inasistencia de" + getAlumnos() + " correspondiente al periodo comprendido del " + getperido() + ".\n\n"+
              "Dicha ausencia se fundamenta en lo siguiente: " + Motivo + ".\n\n"+
              "Debido a la naturaleza de la situación, se solicita el apoyo de las instancias correspondientes para otorgar las facilidades académicas necesarias. Agradezco de antemano su atención y comprensión a la presente, quedando a su entera disposición para cualquier aclaración o información adicional.\n\n" +
              "Atentamente\n\n" + owner.Nombre+"\n\n" + owner.Correo;
        }
        public MimeKit.MimeMessage CrearCorreo()
        {
            var mensaje = new MimeKit.MimeMessage();
            mensaje.Subject = "Justificante de" + getAlumnos();
            mensaje.From.Add(new MailboxAddress(owner.Nombre, owner.Correo));
            mensaje.Body = new TextPart("plain")
            {
                Text = getMotivo()
            };
            foreach (var profesor in Profesores)
            {
                mensaje.To.Add(new MailboxAddress(profesor.Nombre, profesor.Correo));
            }
            foreach (var alumno in Alumnos)
            {
                mensaje.Cc.Add(new MailboxAddress(alumno.Nombre, alumno.Correo));
            }
            
            return mensaje;

        }
    }
}
