using FluentMigrator;

namespace Advobot.Logging.Migrations;

[Migration(20200803192800)]
public sealed class AddLogActionTable : Migration
{
	public override void Down()
		=> Delete.Table("LogAction");

	public override void Up()
	{
		Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS LogAction
			(
				GuildId						TEXT NOT NULL,
				Action						TEXT NOT NULL,
				PRIMARY KEY(GuildId, Action)
			);
			CREATE INDEX IF NOT EXISTS LogAction_GuildId_Index ON LogAction
			(
				GuildId
			);
			");
	}
}