using FluentMigrator;

namespace Advobot.Logging.Migrations;

[Migration(20200803192900)]
public sealed class AddLogChannelTable : Migration
{
	public override void Down()
		=> Delete.Table("LogChannel");

	public override void Up()
	{
		Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS LogChannel
			(
				GuildId						TEXT NOT NULL,
				ImageLogId					TEXT,
				ModLogId					TEXT,
				ServerLogId					TEXT,
				PRIMARY KEY(GuildId)
			);
			");
	}
}