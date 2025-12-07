using Xunit;
using backend.Models;

namespace backend.Tests;

public class ProjectTests
{
    [Fact]
    public void Project_ShouldInitializeWithCreatedStatus()
    {
        // Arrange
        var project = new Project();

        // Act
        var status = project.Status;

        // Assert
        Assert.Equal(ProjectStatus.Created, status);
    }

    [Fact]
    public void Project_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var name = "Test Project";
        var code = "TP-001";

        // Act
        var project = new Project
        {
            Name = name,
            Code = code
        };

        // Assert
        Assert.Equal(name, project.Name);
        Assert.Equal(code, project.Code);
    }
}
