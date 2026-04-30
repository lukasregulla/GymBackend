using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBackend.Migrations
{
    /// <inheritdoc />
    public partial class CalenderSubscrptionToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CalendarSubscriptionToken",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalendarSubscriptionToken",
                table: "Users");
        }
    }
}
