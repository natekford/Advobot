using FluentMigrator;

namespace Advobot.Settings.Database.Migrations;

[Migration(20200914021000)]
public sealed class AddGuildSettingsTable : Migration
{
	public override void Down()
		=> Delete.Table("GuildSetting");

	public override void Up()
	{
		Execute.Sql(@"
		CREATE TABLE IF NOT EXISTS GuildSetting
		(
			GuildId						TEXT NOT NULL,
			MuteRoleId					TEXT NOT NULL,
			Prefix						TEXT,
			Culture						TEXT,
			PRIMARY KEY(GuildId)
		);
		");
	}
}