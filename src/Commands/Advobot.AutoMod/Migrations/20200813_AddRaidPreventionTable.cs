using FluentMigrator;

namespace Advobot.AutoMod.Migrations;

[Migration(20200813223100)]
public sealed class AddRaidPreventionTable : Migration
{
	public override void Down()
		=> Delete.Table("RaidPrevention");

	public override void Up()
	{
		Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS RaidPrevention
			(
				GuildId					TEXT NOT NULL,
				PunishmentType			TEXT NOT NULL,
				Instances				INTEGER NOT NULL,
				LengthTicks				INTEGER,
				RoleId					TEXT,
				Enabled					INTEGER NOT NULL,
				IntervalTicks			INTEGER NOT NULL,
				Size					INTEGER NOT NULL,
				RaidType				INTEGER NOT NULL,
				PRIMARY KEY(GuildId, RaidType)
			);
			CREATE INDEX IF NOT EXISTS RaidPrevention_GuildId_Index ON RaidPrevention
			(
				GuildId
			);
			");
	}
}