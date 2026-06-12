using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiTaskApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentJobRetries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "AgentJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "AgentJobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "AgentJobs",
                type: "integer",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptAt",
                table: "AgentJobs",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "AgentJobs");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "AgentJobs");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "AgentJobs");

            migrationBuilder.DropColumn(
                name: "NextAttemptAt",
                table: "AgentJobs");
        }
    }
}
