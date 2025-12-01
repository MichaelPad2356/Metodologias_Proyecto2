using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentStepId",
                table: "Artifacts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Artifacts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Artifacts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WorkflowId",
                table: "Artifacts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArtifactHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArtifactId = table.Column<int>(type: "INTEGER", nullable: false),
                    PreviousState = table.Column<string>(type: "TEXT", nullable: false),
                    NewState = table.Column<string>(type: "TEXT", nullable: false),
                    ChangedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Comments = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtifactHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtifactHistories_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "Artifacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkflowId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Workflows",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 1, "Ciclo de vida básico", "Flujo Estándar OpenUP" });

            migrationBuilder.InsertData(
                table: "WorkflowSteps",
                columns: new[] { "Id", "Name", "Order", "WorkflowId" },
                values: new object[,]
                {
                    { 1, "Borrador", 1, 1 },
                    { 2, "Revisión Técnica", 2, 1 },
                    { 3, "Aprobado", 3, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_CurrentStepId",
                table: "Artifacts",
                column: "CurrentStepId");

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_WorkflowId",
                table: "Artifacts",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactHistories_ArtifactId",
                table: "ArtifactHistories",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowId",
                table: "WorkflowSteps",
                column: "WorkflowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Artifacts_WorkflowSteps_CurrentStepId",
                table: "Artifacts",
                column: "CurrentStepId",
                principalTable: "WorkflowSteps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Artifacts_Workflows_WorkflowId",
                table: "Artifacts",
                column: "WorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artifacts_WorkflowSteps_CurrentStepId",
                table: "Artifacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Artifacts_Workflows_WorkflowId",
                table: "Artifacts");

            migrationBuilder.DropTable(
                name: "ArtifactHistories");

            migrationBuilder.DropTable(
                name: "WorkflowSteps");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.DropIndex(
                name: "IX_Artifacts_CurrentStepId",
                table: "Artifacts");

            migrationBuilder.DropIndex(
                name: "IX_Artifacts_WorkflowId",
                table: "Artifacts");

            migrationBuilder.DropColumn(
                name: "CurrentStepId",
                table: "Artifacts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Artifacts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Artifacts");

            migrationBuilder.DropColumn(
                name: "WorkflowId",
                table: "Artifacts");
        }
    }
}
