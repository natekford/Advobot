using FluentMigrator;

namespace Advobot.MyCommands.Migrations;

[Migration(20200914023000)]
public sealed class AddDetectLanguageConfigTable : Migration
{
	public override void Down()
		=> Delete.Table("DetectLanguageConfig");

	public override void Up()
	{
		Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS DetectLanguageConfig
			(
				APIKey					TEXT,
				ConfidenceLimit			FLOAT NOT NULL,
				CooldownStartTicks		INTEGER
			);
			");
	}
}