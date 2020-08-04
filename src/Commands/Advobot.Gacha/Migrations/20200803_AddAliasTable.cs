using FluentMigrator;

namespace Advobot.Gacha.Migrations
{
	[Migration(20200803174600)]
	public sealed class AddAliasTable : Migration
	{
		public override void Down()
			=> Delete.Table("Alias");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS Alias
			(
				CharacterId					INTEGER NOT NULL,
				Name						TEXT NOT NULL,
				IsSpoiler					INTEGER NOT NULL,
				PRIMARY KEY(CharacterId, Name)
				FOREIGN KEY(CharacterId) REFERENCES Character(CharacterId) ON DELETE CASCADE
			);
			CREATE INDEX IF NOT EXISTS Alias_CharacterId_Index ON Alias
			(
				CharacterId
			);
			");
		}
	}
}