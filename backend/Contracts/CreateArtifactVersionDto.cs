using System.ComponentModel.DataAnnotations;

namespace backend.Contracts;

public class CreateArtifactVersionDto
{
    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? RepositoryUrl { get; set; }

    public IFormFile? File { get; set; }
}
