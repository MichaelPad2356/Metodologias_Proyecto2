using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.Contracts;

public class CreateArtifactDto
{
    [Required]
    public ArtifactType Type { get; set; }

    [Required]
    public int ProjectPhaseId { get; set; }

    public bool IsMandatory { get; set; }

    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    public string? Content { get; set; }

    public IFormFile? File { get; set; }
}