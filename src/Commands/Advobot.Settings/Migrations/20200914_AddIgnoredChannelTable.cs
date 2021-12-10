using FluentMigrator;

namespace Advobot.Settings.Migrations;

[Migration(20200914022000)]
public sealed class AddIgnoredChannelTable : Migration
{
	public override void Down()
		=> Delete.Table("IgnoredChannel");

	public override void Up()
	{
		Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS IgnoredChannel
			(
				GuildId					TEXT NOT NULL,
				ChannelId				TEXT NOT NULL,
				PRIMARY KEY(ChannelId)
			);
			CREATE INDEX IF NOT EXISTS IgnoredChannel_GuildId_Index ON IgnoredChannel
			(
				GuildId
			);
			");
	}
}