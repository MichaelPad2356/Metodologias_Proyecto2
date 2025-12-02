using System.Collections.Generic;

namespace backend.Contracts
{
    public class WorkflowDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<WorkflowStepDto> Steps { get; set; }
    }

    public class WorkflowStepDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }

    public class UpdateArtifactStatusDto
    {
        public int NewStepId { get; set; }
        public string Comments { get; set; }
        public string ChangedBy { get; set; } // En un sistema real, esto viene del token de usuario
    }
}