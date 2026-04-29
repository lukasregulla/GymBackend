using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddedSessionTimeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "WorkoutSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ScheduledStartTime",
                table: "WorkoutSessions",
                type: "time without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                table: "WorkoutSessions");

            migrationBuilder.DropColumn(
                name: "ScheduledStartTime",
                table: "WorkoutSessions");
        }
    }
}
