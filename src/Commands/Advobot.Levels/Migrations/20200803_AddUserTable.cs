using FluentMigrator;

namespace Advobot.Levels.Migrations;

[Migration(20200803171500)]
public sealed class AddUserTable : Migration
{
	public override void Down()
		=> Delete.Table("User");

	public override void Up()
	{
		Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS User
			(
				GuildId                     TEXT NOT NULL,
				ChannelId                   TEXT NOT NULL,
				UserId                      TEXT NOT NULL,
				Experience                  INTEGER NOT NULL,
				MessageCount                INTEGER NOT NULL,
				PRIMARY KEY(GuildId, ChannelId, UserId)
			);
			CREATE INDEX IF NOT EXISTS User_GuildId_ChannelId_Index ON User
			(
				GuildId,
				ChannelId
			);
			CREATE INDEX IF NOT EXISTS User_GuildId_Index ON User
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS User_ChannelId_Index ON User
			(
				ChannelId
			);
			");
	}
}