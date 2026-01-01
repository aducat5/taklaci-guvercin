using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaklaciGuvercin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Birds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    Rarity = table.Column<string>(type: "text", nullable: false),
                    PrimaryColor_TraitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrimaryColor_Allele1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrimaryColor_Allele2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrimaryColor_Type = table.Column<string>(type: "text", nullable: false),
                    SecondaryColor_TraitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SecondaryColor_Allele1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SecondaryColor_Allele2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SecondaryColor_Type = table.Column<string>(type: "text", nullable: false),
                    Pattern_TraitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Pattern_Allele1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Pattern_Allele2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Pattern_Type = table.Column<string>(type: "text", nullable: false),
                    TailType_TraitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TailType_Allele1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TailType_Allele2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TailType_Type = table.Column<string>(type: "text", nullable: false),
                    CrestType_TraitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CrestType_Allele1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CrestType_Allele2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CrestType_Type = table.Column<string>(type: "text", nullable: false),
                    ElementGene_TraitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ElementGene_Allele1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ElementGene_Allele2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ElementGene_Type = table.Column<string>(type: "text", nullable: false),
                    Element = table.Column<string>(type: "text", nullable: false),
                    HiddenTrait1_TraitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HiddenTrait1_Allele1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HiddenTrait1_Allele2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HiddenTrait1_Type = table.Column<string>(type: "text", nullable: false),
                    HiddenTrait2_TraitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HiddenTrait2_Allele1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HiddenTrait2_Allele2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HiddenTrait2_Type = table.Column<string>(type: "text", nullable: false),
                    MutationFactor = table.Column<float>(type: "real", nullable: false),
                    Leadership = table.Column<int>(type: "integer", nullable: false),
                    Loyalty = table.Column<int>(type: "integer", nullable: false),
                    Speed = table.Column<int>(type: "integer", nullable: false),
                    GeneticDominance = table.Column<int>(type: "integer", nullable: false),
                    MotherId = table.Column<Guid>(type: "uuid", nullable: true),
                    FatherId = table.Column<Guid>(type: "uuid", nullable: true),
                    Generation = table.Column<int>(type: "integer", nullable: false),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    MaxHealth = table.Column<int>(type: "integer", nullable: false),
                    Stamina = table.Column<int>(type: "integer", nullable: false),
                    MaxStamina = table.Column<int>(type: "integer", nullable: false),
                    LastFedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SickUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RestingUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Birds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Encounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatorSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatorPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    WinnerPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    LootedBirdIds = table.Column<string>(type: "text", nullable: false),
                    CoinsLooted = table.Column<int>(type: "integer", nullable: false),
                    InitiatorPower = table.Column<int>(type: "integer", nullable: false),
                    TargetPower = table.Column<int>(type: "integer", nullable: false),
                    RandomRoll = table.Column<int>(type: "integer", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InitiatorWasOnline = table.Column<bool>(type: "boolean", nullable: false),
                    TargetWasOnline = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encounters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BirdIds = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Altitude = table.Column<double>(type: "double precision", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EncountersCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Coins = table.Column<int>(type: "integer", nullable: false),
                    PremiumCurrency = table.Column<int>(type: "integer", nullable: false),
                    TotalBirdsOwned = table.Column<int>(type: "integer", nullable: false),
                    TotalEncountersWon = table.Column<int>(type: "integer", nullable: false),
                    TotalEncountersLost = table.Column<int>(type: "integer", nullable: false),
                    TotalBirdsLost = table.Column<int>(type: "integer", nullable: false),
                    TotalBirdsLooted = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false),
                    CoopCapacity = table.Column<int>(type: "integer", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Birds_OwnerId",
                table: "Birds",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Birds_State",
                table: "Birds",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_InitiatorPlayerId",
                table: "Encounters",
                column: "InitiatorPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_State",
                table: "Encounters",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_TargetPlayerId",
                table: "Encounters",
                column: "TargetPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSessions_IsActive",
                table: "FlightSessions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSessions_Latitude_Longitude",
                table: "FlightSessions",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_FlightSessions_PlayerId",
                table: "FlightSessions",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Email",
                table: "Players",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_Username",
                table: "Players",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Birds");

            migrationBuilder.DropTable(
                name: "Encounters");

            migrationBuilder.DropTable(
                name: "FlightSessions");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
