using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JustificantesUPP.Modelos
{
    public enum Genero
    {
        Femenino,
        Masculino,
        Otro
    }
    public class Alumno
    {
        [Key]
        public string Nc { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Grupo { get; set; }
       
        public Genero Genero { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }

    }
}
