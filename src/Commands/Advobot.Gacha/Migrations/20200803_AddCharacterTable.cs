using FluentMigrator;

namespace Advobot.Gacha.Migrations
{
	[Migration(20200803174700)]
	public sealed class AddCharacterTable : Migration
	{
		public override void Down()
			=> Delete.Table("Character");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS Character
			(
				CharacterId					INTEGER NOT NULL PRIMARY KEY,
				SourceId					INTEGER NOT NULL,
				Name						TEXT NOT NULL,
				GenderIcon					TEXT NOT NULL,
				Gender						INTEGER NOT NULL,
				RollType					INTEGER NOT NULL,
				FlavorText					TEXT,
				IsFakeCharacter				INTEGER NOT NULL,
				FOREIGN KEY(SourceId) REFERENCES Source(SourceId) ON DELETE CASCADE
			);
			CREATE INDEX IF NOT EXISTS Character_SourceId_Index ON Character
			(
				SourceId
			);
			CREATE INDEX IF NOT EXISTS Character_Name_Index ON Character
			(
				Name
			);
			CREATE INDEX IF NOT EXISTS Character_Gender_Index ON Character
			(
				Gender
			);
			");
		}
	}
}