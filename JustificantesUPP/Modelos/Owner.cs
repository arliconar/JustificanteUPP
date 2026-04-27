using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace JustificantesUPP.Modelos
{
    public class Owner:Profesor
    {
       public string firmapath = "G:\\Mi unidad\\firma\\Firma email UPP\\Diapositiva2.png";

       public static Owner Load()
       {
           string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JustificantesUPP_Owner.json");
           if(System.IO.File.Exists(path))
           {
               string json = System.IO.File.ReadAllText(path);
               return JsonSerializer.Deserialize<Owner>(json);
           }
           return new Owner
           {
                Nombre = "Dummy Dumy",
                Correo = "dummy@upp.edu.mx",
                Genero = Genero.Masculino,
           };
       }
       
       public void Save()
       {
           string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JustificantesUPP_Owner.json");
           string json = JsonSerializer.Serialize(this);
           System.IO.File.WriteAllText(path, json);
       }
    }
}
