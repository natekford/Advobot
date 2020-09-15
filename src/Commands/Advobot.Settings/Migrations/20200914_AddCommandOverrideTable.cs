using FluentMigrator;

namespace Advobot.Settings.Migrations
{
	[Migration(20200914023000)]
	public sealed class AddCommandOverrideTable : Migration
	{
		public override void Down()
			=> Delete.Table("CommandOverride");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS CommandOverride
			(
				GuildId					TEXT NOT NULL,
				CommandId				TEXT NOT NULL,
				TargetId				TEXT NOT NULL,
				TargetType				INTEGER NOT NULL,
				Enabled					INTEGER NOT NULL,
				Priority				INTEGER NOT NULL,
				PRIMARY KEY(GuildId, TargetId)
			);
			CREATE INDEX IF NOT EXISTS CommandOverride_GuildId_Index ON CommandOverride
			(
				GuildId
			);
			");
		}
	}
}