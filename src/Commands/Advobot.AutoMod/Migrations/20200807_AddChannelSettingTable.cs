using FluentMigrator;

namespace Advobot.AutoMod.Migrations;

[Migration(20200807004600)]
public sealed class AddChannelSettingTable : Migration
{
	public override void Down()
		=> Delete.Table("ChannelSetting");

	public override void Up()
	{
		Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS ChannelSetting
			(
				GuildId					TEXT NOT NULL,
				ChannelId				TEXT NOT NULL,
				IsImageOnly				INTEGER NOT NULL,
				PRIMARY KEY(ChannelId)
			);
			CREATE INDEX IF NOT EXISTS ChannelSetting_GuildId_Index ON ChannelSetting
			(
				GuildId
			);
			");
	}
}