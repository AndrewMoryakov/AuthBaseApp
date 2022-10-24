using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    public partial class CountryGuidId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Province_Country_CountryId1",
                table: "Province");

            migrationBuilder.DropIndex(
                name: "IX_Province_CountryId1",
                table: "Province");

            migrationBuilder.DropColumn(
                name: "CountryId1",
                table: "Province");

            migrationBuilder.AlterColumn<Guid>(
                name: "CountryId",
                table: "Province",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Province_CountryId",
                table: "Province",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Province_Country_CountryId",
                table: "Province",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Province_Country_CountryId",
                table: "Province");

            migrationBuilder.DropIndex(
                name: "IX_Province_CountryId",
                table: "Province");

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "Province",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId1",
                table: "Province",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Province_CountryId1",
                table: "Province",
                column: "CountryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Province_Country_CountryId1",
                table: "Province",
                column: "CountryId1",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
