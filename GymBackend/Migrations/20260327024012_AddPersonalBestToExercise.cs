using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalBestToExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeffaultSets",
                table: "TemplateExercises",
                newName: "DefaultSets");

            migrationBuilder.RenameColumn(
                name: "DeffaultReps",
                table: "TemplateExercises",
                newName: "DefaultReps");

            migrationBuilder.AddColumn<int>(
                name: "PersonalBestSetId",
                table: "Exercises",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_PersonalBestSetId",
                table: "Exercises",
                column: "PersonalBestSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Sets_PersonalBestSetId",
                table: "Exercises",
                column: "PersonalBestSetId",
                principalTable: "Sets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Sets_PersonalBestSetId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_PersonalBestSetId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "PersonalBestSetId",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "DefaultSets",
                table: "TemplateExercises",
                newName: "DeffaultSets");

            migrationBuilder.RenameColumn(
                name: "DefaultReps",
                table: "TemplateExercises",
                newName: "DeffaultReps");
        }
    }
}
