using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using backend.Controllers;
using backend.Data;
using backend.Models;
using backend.Contracts;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace backend.Tests
{
    public class ArtifactsControllerTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public ArtifactsControllerTests()
        {
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;
        }

        private ApplicationDbContext CreateContext()
        {
            var context = new ApplicationDbContext(_options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task ValidateProjectClosure_ShouldReturnFalse_WhenMandatoryArtifactsAreMissing()
        {
            // Arrange
            using var context = CreateContext();
            var projectId = 1;
            var phaseId = 1;

            context.Projects.Add(new Project { Id = projectId, Name = "Test Project", Code = "TP1" });
            context.ProjectPhases.Add(new ProjectPhase { Id = phaseId, ProjectId = projectId, Name = "Fase de Transición", Order = 4 });
            await context.SaveChangesAsync();

            var controller = new ArtifactsController(context, _mockEnvironment.Object);

            // Act
            var result = await controller.ValidateProjectClosure(projectId);

            // Assert
            var actionResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var response = Assert.IsType<ClosureValidationResponse>(actionResult.Value);
            
            Assert.False(response.CanClose);
            Assert.NotEmpty(response.MissingArtifacts);
        }

        [Fact]
        public async Task ValidateProjectClosure_ShouldReturnFalse_WhenArtifactsAreNotApproved()
        {
            // Arrange
            using var context = CreateContext();
            var projectId = 2;
            var phaseId = 2;

            context.Projects.Add(new Project { Id = projectId, Name = "Test Project 2", Code = "TP2" });
            context.ProjectPhases.Add(new ProjectPhase { Id = phaseId, ProjectId = projectId, Name = "Fase de Transición", Order = 4 });
            
            // Add mandatory artifacts but with Pending status
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.UserManual, Status = ArtifactStatus.Pending, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.TechnicalManual, Status = ArtifactStatus.Approved, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.DeploymentPlan, Status = ArtifactStatus.Approved, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.ClosureDoc, Status = ArtifactStatus.Approved, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.FinalBuild, Status = ArtifactStatus.Approved, IsMandatory = true });

            await context.SaveChangesAsync();

            var controller = new ArtifactsController(context, _mockEnvironment.Object);

            // Act
            var result = await controller.ValidateProjectClosure(projectId);

            // Assert
            var actionResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var response = Assert.IsType<ClosureValidationResponse>(actionResult.Value);
            
            Assert.False(response.CanClose);
            Assert.Contains(response.PendingApproval, a => a.Type == ArtifactType.UserManual);
        }

        [Fact]
        public async Task ValidateProjectClosure_ShouldReturnFalse_WhenOptionalArtifactIsNotApproved()
        {
            // Arrange
            using var context = CreateContext();
            var projectId = 3;
            var phaseId = 3;

            context.Projects.Add(new Project { Id = projectId, Name = "Test Project 3", Code = "TP3" });
            context.ProjectPhases.Add(new ProjectPhase { Id = phaseId, ProjectId = projectId, Name = "Fase de Transición", Order = 4 });
            
            // Add all mandatory artifacts as Approved
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.UserManual, Status = ArtifactStatus.Approved, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.TechnicalManual, Status = ArtifactStatus.Approved, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.DeploymentPlan, Status = ArtifactStatus.Approved, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.ClosureDoc, Status = ArtifactStatus.Approved, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.FinalBuild, Status = ArtifactStatus.Approved, IsMandatory = true });

            // Add an OPTIONAL artifact that is NOT approved (Pending)
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.BetaTestReport, Status = ArtifactStatus.Pending, IsMandatory = false });

            await context.SaveChangesAsync();

            var controller = new ArtifactsController(context, _mockEnvironment.Object);

            // Act
            var result = await controller.ValidateProjectClosure(projectId);

            // Assert
            var actionResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var response = Assert.IsType<ClosureValidationResponse>(actionResult.Value);
            
            // This asserts the fix: CanClose should be false because an optional artifact is pending
            Assert.False(response.CanClose);
            Assert.Contains(response.PendingApproval, a => a.Type == ArtifactType.BetaTestReport);
        }

        [Fact]
        public async Task ValidateProjectClosure_ShouldReturnTrue_WhenAllArtifactsAreApproved()
        {
            // Arrange
            using var context = CreateContext();
            var projectId = 4;
            var phaseId = 4;

            context.Projects.Add(new Project { Id = projectId, Name = "Test Project 4", Code = "TP4" });
            context.ProjectPhases.Add(new ProjectPhase { Id = phaseId, ProjectId = projectId, Name = "Fase de Transición", Order = 4 });
            
            // Add all mandatory artifacts as Approved
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.UserManual, Status = ArtifactStatus.Approved, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.TechnicalManual, Status = ArtifactStatus.Approved, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.DeploymentPlan, Status = ArtifactStatus.Approved, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.ClosureDoc, Status = ArtifactStatus.Approved, IsMandatory = true });
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.FinalBuild, Status = ArtifactStatus.Approved, IsMandatory = true });

            // Add an OPTIONAL artifact that IS approved
            context.Artifacts.Add(new Artifact { ProjectPhaseId = phaseId, Type = ArtifactType.BetaTestReport, Status = ArtifactStatus.Approved, IsMandatory = false });

            await context.SaveChangesAsync();

            var controller = new ArtifactsController(context, _mockEnvironment.Object);

            // Act
            var result = await controller.ValidateProjectClosure(projectId);

            // Assert
            var actionResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var response = Assert.IsType<ClosureValidationResponse>(actionResult.Value);
            
            Assert.True(response.CanClose);
            Assert.Empty(response.MissingArtifacts);
            Assert.Empty(response.PendingApproval);
        }
    }
}
