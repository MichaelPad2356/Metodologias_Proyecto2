using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Iteracion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Objetivo { get; set; } = string.Empty;
        public string FaseOpenUP { get; set; } = string.Empty;
        
        // Relación con Proyecto
        public int? ProjectId { get; set; }
        
        // Capacidad y velocidad
        public double CapacidadEquipoHoras { get; set; }
        public int PuntosCompletados { get; set; }
        public int PuntosEstimados { get; set; }
        
        // Almacenar tareas como JSON en la base de datos
        public string TareasJson { get; set; } = "[]";
        
        // Tareas de la iteración (no mapeado a BD, solo para uso en memoria)
        [NotMapped]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
    
    public class Tarea
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string FaseProyecto { get; set; } = string.Empty;
        public string Estado { get; set; } = "NoIniciada"; // NoIniciada, EnProgreso, Completada, Bloqueada
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string AsignadoA { get; set; } = string.Empty;
        public int PorcentajeCompletado { get; set; }
        public string Bloqueos { get; set; } = string.Empty;
    }
}