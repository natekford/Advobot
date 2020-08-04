using FluentMigrator;

namespace Advobot.Gacha.Migrations
{
	[Migration(20200803175000)]
	public sealed class AddSourceTable : Migration
	{
		public override void Down()
			=> Delete.Table("Source");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS Source
			(
				SourceId					INTEGER NOT NULL PRIMARY KEY,
				Name						TEXT NOT NULL,
				ThumbnailUrl				TEXT
			);
			CREATE UNIQUE INDEX IF NOT EXISTS Source_Name_Index ON Source
			(
				Name
			);
			");
		}
	}
}