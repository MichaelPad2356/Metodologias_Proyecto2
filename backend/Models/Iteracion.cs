namespace backend.Models
{
    public class Iteracion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Objetivo { get; set; } = string.Empty;
        public string FaseOpenUP { get; set; } = string.Empty; // Ej: Inicio, Elaboración, Construcción, Transición
        
        // Para HU-016: Velocidad y Capacidad
        public double CapacidadEquipoHoras { get; set; } // Horas disponibles del equipo
        public int PuntosCompletados { get; set; } // Para calcular velocidad histórica
        public int PuntosEstimados { get; set; } // Alcance planificado
    }
}