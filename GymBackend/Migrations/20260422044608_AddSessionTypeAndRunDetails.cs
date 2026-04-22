using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GymBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionTypeAndRunDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionType",
                table: "WorkoutSessions",
                type: "text",
                nullable: false,
                defaultValue: "Strength");

            migrationBuilder.CreateTable(
                name: "RunDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkoutSessionId = table.Column<int>(type: "integer", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "numeric", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    AveragePaceSecondsPerKm = table.Column<int>(type: "integer", nullable: false),
                    RunType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RunDetails_WorkoutSessions_WorkoutSessionId",
                        column: x => x.WorkoutSessionId,
                        principalTable: "WorkoutSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RunDetails_WorkoutSessionId",
                table: "RunDetails",
                column: "WorkoutSessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RunDetails");

            migrationBuilder.DropColumn(
                name: "SessionType",
                table: "WorkoutSessions");
        }
    }
}
