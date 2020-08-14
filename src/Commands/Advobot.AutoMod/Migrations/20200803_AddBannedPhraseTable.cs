using FluentMigrator;

namespace Advobot.AutoMod.Migrations
{
	[Migration(20200803173800)]
	public sealed class AddBannedPhraseTable : Migration
	{
		public override void Down()
			=> Delete.Table("BannedPhrase");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS BannedPhrase
			(
				GuildId					TEXT NOT NULL,
				Phrase					TEXT NOT NULL,
				IsContains				INTEGER NOT NULL,
				IsRegex					INTEGER NOT NULL,
				IsName					INTEGER NOT NULL,
				PunishmentType			INTEGER NOT NULL,
				PRIMARY KEY(GuildId, Phrase, IsName)
			);
			CREATE INDEX IF NOT EXISTS BannedPhrase_GuildId_Index ON BannedPhrase
			(
				GuildId
			);
			");
		}
	}
}