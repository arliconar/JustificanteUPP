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
        private Owner _ownerData = Owner.Load();
        public Owner OwnerData
        {
            get => _ownerData;
            set { _ownerData = value; OnPropertyChanged(); }
        }
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
                return "periodo comprendido entre " + FechaInicio.ToString("dd/MM/yyyy") + " al " + FechaFinal.ToString("dd/MM/yyyy");
            }
        }

        public string getAsunto()
        {
            string asunto = "Justificante de";
                if (Alumnos.Count == 1)
                {
                    asunto += getGenre(Alumnos[0]) + " " + Alumnos[0].Nombre;
                }
                else
                {
                    asunto += " alumnos";
            }
                if (FechaInicio != FechaFinal)
                {
                    asunto += " del periodo " + FechaInicio.ToString("dd/MM/yyyy") + " al " + FechaFinal.ToString("dd/MM/yyyy");
                }
                else
                {
                    asunto += " del día " + FechaInicio.ToString("dd/MM/yyyy");
                }
            return asunto;
        }
        public string getMotivo()
        {
            return $"{getProfesores()}, \n\n" +
              "Por medio de la presente, me permito hacer de su conocimiento la justificación de inasistencia de" + getAlumnos() + " correspondiente del " + getperido() + ".\n\n"+
              "Dicha ausencia se fundamenta en lo siguiente: " + Motivo + ".\n\n"+
              "Debido a la naturaleza de la situación, se solicita el apoyo de las instancias correspondientes para otorgar las facilidades académicas necesarias. Agradezco de antemano su atención y comprensión a la presente, quedando a su entera disposición para cualquier aclaración o información adicional.\n\n" +
              "Atentamente\n\n" + OwnerData.Nombre+"\n\n" + OwnerData.Correo;
        }
        public string getMotivoHtml()
        {
            // Convertimos los saltos de línea normales a etiquetas <br/> para HTML
            string htmlMotivo = Motivo.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
            string profesoresHtml = getProfesores().Replace(Environment.NewLine, "<br/>");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            font-size: 16px;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 20px;
            background-color: #f9f9f9;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 8px;
            border: 1px solid #e0e0e0;
        }}
        p {{
            margin-bottom: 15px;
        }}
        .footer {{
            margin-top: 30px;
            color: #666666;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <p>{profesoresHtml},</p>
        <p>Por medio de la presente, me permito hacer de su conocimiento la justificación de inasistencia de <strong>{getAlumnos()}</strong> correspondiente del {getperido()}.</p>
        <p>Dicha ausencia se fundamenta en lo siguiente:</p>
        <p style='background-color: #f1f1f1; padding: 15px; border-left: 4px solid #005A9E; border-radius: 4px;'>
            <em>{htmlMotivo}</em>
        </p>
        <p>Debido a la naturaleza de la situación, se solicita el apoyo de las instancias correspondientes para otorgar las facilidades académicas necesarias. Agradezco de antemano su atención y comprensión a la presente, quedando a mi entera disposición para cualquier aclaración o información adicional.</p>
        <div class='footer'>
            <p>Atentamente,<br/><br/>
            <strong>{OwnerData.Nombre}</strong><br/>
            <a href='mailto:{OwnerData.Correo}' style='color: #005A9E; text-decoration: none;'>{OwnerData.Correo}</a></p>
        </div>
    </div>
</body>
</html>";
        }

        public MimeKit.MimeMessage CrearCorreo()
        {
            var mensaje = new MimeKit.MimeMessage();
            mensaje.Subject = getAsunto();
            mensaje.From.Add(new MailboxAddress(OwnerData.Nombre, OwnerData.Correo));
            
            // Usamos formato HTML en lugar de texto plano
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = getMotivoHtml();
            
            // También adjuntamos la versión de texto plano como respaldo (buena práctica)
            bodyBuilder.TextBody = getMotivo();

            mensaje.Body = bodyBuilder.ToMessageBody();
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
