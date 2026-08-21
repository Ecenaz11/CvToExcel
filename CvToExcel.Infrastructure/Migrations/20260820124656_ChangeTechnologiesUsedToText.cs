using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvToExcel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTechnologiesUsedToText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "WorkExperiences");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Educations");

            migrationBuilder.AlterColumn<string>(
                name: "TechnologiesUsed",
                table: "Projects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WorkExperiences",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TechnologiesUsed",
                table: "Projects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Educations",
                type: "text",
                nullable: true);
        }
    }
}
