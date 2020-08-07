using FluentMigrator;

namespace Advobot.AutoMod.Migrations
{
	[Migration(20200807004500)]
	public sealed class AddBannedNameTable : Migration
	{
		public override void Down()
			=> Delete.Table("BannedName");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS BannedName
			(
				GuildId					TEXT NOT NULL,
				Phrase					TEXT NOT NULL,
				IsContains				INTEGER NOT NULL,
				IsRegex					INTEGER NOT NULL,
				PunishmentType			TEXT NOT NULL,
				PRIMARY KEY(GuildId, Phrase)
			);
			CREATE INDEX IF NOT EXISTS BannedName_GuildId_Index ON BannedName
			(
				GuildId
			);
			");
		}
	}
}