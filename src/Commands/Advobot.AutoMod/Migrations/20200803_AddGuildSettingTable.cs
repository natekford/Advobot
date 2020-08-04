using FluentMigrator;

namespace Advobot.AutoMod.Migrations
{
	[Migration(20200803173900)]
	public sealed class AddGuildSettingTable : Migration
	{
		public override void Down()
			=> Delete.Table("GuildSetting");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS GuildSetting
			(
				GuildId					TEXT NOT NULL,
				Ticks					INTEGER NOT NULL,
				IgnoreAdmins			INTEGER NOT NULL,
				IgnoreHigherHierarchy	INTEGET NOT NULL,
				PRIMARY KEY(GuildId)
			);
			");
		}
	}
}