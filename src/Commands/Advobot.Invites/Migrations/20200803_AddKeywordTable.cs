using FluentMigrator;

namespace Advobot.Invites.Migrations;

[Migration(20200803173400)]
public sealed class AddKeywordTable : Migration
{
	public override void Down()
		=> Delete.Table("Keyword");

	public override void Up()
	{
		Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS Keyword
			(
				GuildId						TEXT NOT NULL,
				Word						TEXT NOT NULL COLLATE NOCASE,
				PRIMARY KEY(GuildId, Word)
			);
			CREATE INDEX IF NOT EXISTS Keyword_GuildId ON Keyword
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS Keyword_Word ON Keyword
			(
				Word
			);
			");
	}
}