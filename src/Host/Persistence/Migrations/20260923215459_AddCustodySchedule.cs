using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoParenting.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustodySchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustodySchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnchorStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaiColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    MaeColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustodySchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustodySchedules_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustodySegments",
                columns: table => new
                {
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CustodyScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    DurationHours = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustodySegments", x => new { x.CustodyScheduleId, x.OrderIndex });
                    table.ForeignKey(
                        name: "FK_CustodySegments_CustodySchedules_CustodyScheduleId",
                        column: x => x.CustodyScheduleId,
                        principalTable: "CustodySchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustodySchedules_FamilyId",
                table: "CustodySchedules",
                column: "FamilyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustodySegments");

            migrationBuilder.DropTable(
                name: "CustodySchedules");
        }
    }
}
