using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Zeiterfassung.Migrations
{
    /// <inheritdoc />
    public partial class Test2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "43060658-5861-4b76-93bc-4fe4612472e6");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8c37d49f-7966-49cc-9f31-2241bf4c3ceb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "IsSampleUser", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "43060658-5861-4b76-93bc-4fe4612472e6", 0, "db5168ea-5e96-462d-a782-9b86fb531089", "ben.beispiel@email.com", true, true, false, null, "BEN.BEISPIEL@EMAIL.COM", "BEN.BEISPIEL@EMAIL.COM", "AQAAAAIAAYagAAAAEBBr9jIjwGoC1bQsgjEcMbLaoHCzViV9fwJa8pSdwcdxNcvi32fMQ2rHGQCqPrzU0w==", null, false, "46415573-fec0-404d-b5d4-3f0462da33ce", false, "ben.beispiel@email.com" },
                    { "8c37d49f-7966-49cc-9f31-2241bf4c3ceb", 0, "a0b6ab7a-654e-4cc7-8f78-0ee801ef1869", "anna.muster@email.com", true, true, false, null, "ANNA.MUSTER@EMAIL.COM", "ANNA.MUSTER@EMAIL.COM", "AQAAAAIAAYagAAAAENhaF/HF6/8UJmpj691OeXWSAl0sTlCj+mgsl1O5qAfY41yZf5dTyEig5r4a2JyICw==", null, false, "d608e522-93f1-4824-a36b-b5e834a1b899", false, "anna.muster@email.com" }
                });
        }
    }
}
