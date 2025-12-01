using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.Contracts;

public class CreateArtifactDto
{
    public int ProjectPhaseId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public ArtifactType Type { get; set; }
    
    public bool IsMandatory { get; set; }
    
    public int? WorkflowId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;
    
    public string? Content { get; set; }
    
    public IFormFile? File { get; set; }
}