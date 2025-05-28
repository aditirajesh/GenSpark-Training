using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FirstAPI.Migrations
{
    /// <inheritdoc />
    public partial class second_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AppointmentId",
                table: "Appointments");

            migrationBuilder.RenameTable(
                name: "Appointments",
                newName: "Appointmnets");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointmnets",
                newName: "IX_Appointmnets_PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointmnets",
                newName: "IX_Appointmnets_DoctorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppointmentNumber",
                table: "Appointmnets",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FollwerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Followers",
                        column: x => x.FollwerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_FollwerId",
                table: "Users",
                column: "FollwerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppointmentNumber",
                table: "Appointmnets");

            migrationBuilder.RenameTable(
                name: "Appointmnets",
                newName: "Appointments");

            migrationBuilder.RenameIndex(
                name: "IX_Appointmnets_PatientId",
                table: "Appointments",
                newName: "IX_Appointments_PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointmnets_DoctorId",
                table: "Appointments",
                newName: "IX_Appointments_DoctorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppointmentId",
                table: "Appointments",
                column: "Id");
        }
    }
}
