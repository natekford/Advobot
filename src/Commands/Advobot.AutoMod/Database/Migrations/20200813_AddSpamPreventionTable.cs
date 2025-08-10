using FluentMigrator;

namespace Advobot.AutoMod.Database.Migrations;

[Migration(20200813223200)]
public sealed class AddSpamPreventionTable : Migration
{
	public override void Down()
		=> Delete.Table("SpamPrevention");

	public override void Up()
	{
		Execute.Sql(@"
		CREATE TABLE IF NOT EXISTS SpamPrevention
		(
			GuildId					TEXT NOT NULL,
			PunishmentType			TEXT NOT NULL,
			Instances				INTEGER NOT NULL,
			LengthTicks				INTEGER,
			RoleId					TEXT,
			Enabled					INTEGER NOT NULL,
			IntervalTicks			INTEGER NOT NULL,
			Size					INTEGER NOT NULL,
			SpamType				INTEGER NOT NULL,
			PRIMARY KEY(GuildId, SpamType)
		);
		CREATE INDEX IF NOT EXISTS SpamPrevention_GuildId_Index ON SpamPrevention
		(
			GuildId
		);
		");
	}
}