namespace backend.Models
{
    public class WorkflowStep
    {
        public int Id { get; set; }
        public string Name { get; set; } // Ej: "En Revisión QA"
        public int Order { get; set; } // 1, 2, 3... para saber el orden
        
        public int WorkflowId { get; set; }
        public Workflow Workflow { get; set; }
    }
}