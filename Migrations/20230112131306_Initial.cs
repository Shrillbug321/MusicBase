using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicBase.Migrations
{
	public partial class Initial : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Musics",
				columns: table => new
				{
					MusicId = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
					PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
					Length = table.Column<DateTime>(type: "datetime2", nullable: false),
					Publisher = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Genre = table.Column<int>(type: "int", nullable: false),
					Cover = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
					Track = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Musics", x => x.MusicId);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Musics");
		}
	}
}
