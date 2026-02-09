using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text;

namespace JustificantesUPP.Modelos
{
    public class Profesor
    {
        [Key]
        public int Id { get; set; }

        public string Nombre { get; set; }  

        public string Correo { get; set; }  

        public Genero Genero { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
