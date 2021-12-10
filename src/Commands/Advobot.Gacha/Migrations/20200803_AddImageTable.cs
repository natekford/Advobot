using FluentMigrator;

namespace Advobot.Gacha.Migrations;

[Migration(20200803174900)]
public sealed class AddImageTable : Migration
{
	public override void Down()
		=> Delete.Table("Image");

	public override void Up()
	{
		Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS Image
			(
				CharacterId					INTEGER NOT NULL,
				Url							TEXT NOT NULL,
				PRIMARY KEY(CharacterId, Url),
				FOREIGN KEY(CharacterId) REFERENCES Character(CharacterId) ON DELETE CASCADE
			);
			CREATE INDEX IF NOT EXISTS Image_CharacterId_Index ON Image
			(
				CharacterId
			);
			");
	}
}