﻿using FluentMigrator;

namespace Advobot.Levels.Migrations;

[Migration(20200803171600)]
public sealed class AddIgnoredChannelTable : Migration
{
	public override void Down()
		=> Delete.Table("IgnoredChannel");

	public override void Up()
	{
		Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS IgnoredChannel
			(
				GuildId						TEXT NOT NULL,
				ChannelId					TEXT NOT NULL,
				PRIMARY KEY(GuildId, ChannelId)
			);
			CREATE INDEX IF NOT EXISTS IgnoredChannel_GuildId_Index ON IgnoredChannel
			(
				GuildId
			);
			");
	}
}