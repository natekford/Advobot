using FluentMigrator;

namespace Advobot.AutoMod.Database.Migrations;

[Migration(20200813205300)]
public sealed class AddTimedPunishmentTable : Migration
{
	public override void Down()
		=> Delete.Table("TimedPunishment");

	public override void Up()
	{
		Execute.Sql(@"
		CREATE TABLE IF NOT EXISTS TimedPunishment
		(
			GuildId					TEXT NOT NULL,
			UserId					TEXT NOT NULL,
			PunishmentType			TEXT NOT NULL,
			EndTimeTicks			INTEGER NOT NULL,
			RoleId					TEXT,
			PRIMARY KEY(GuildId, UserId, PunishmentType)
		);
		CREATE INDEX IF NOT EXISTS TimedPunishment_GuildId_Index ON TimedPunishment
		(
			GuildId
		);
		CREATE INDEX IF NOT EXISTS TimedPunishment_GuildId_PunishmentType_Index ON TimedPunishment
		(
			GuildId,
			PunishmentType
		);
		");
	}
}