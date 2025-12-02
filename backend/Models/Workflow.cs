using System.Collections.Generic;

namespace backend.Models
{
    public class Workflow
    {
        public int Id { get; set; }
        public string Name { get; set; } // Ej: "Flujo de Aprobación Estándar"
        public string Description { get; set; }
        
        // Relación con los pasos
        public List<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    }
}