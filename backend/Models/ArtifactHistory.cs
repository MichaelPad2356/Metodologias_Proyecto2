using System;

namespace backend.Models
{
    public class ArtifactHistory
    {
        public int Id { get; set; }
        public int ArtifactId { get; set; }
        public Artifact Artifact { get; set; } = null!;
        
        public string PreviousState { get; set; } = string.Empty;
        public string NewState { get; set; } = string.Empty;
        
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        
        public string Comments { get; set; } = string.Empty;
    }
}