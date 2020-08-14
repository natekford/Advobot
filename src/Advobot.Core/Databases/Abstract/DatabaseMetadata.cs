namespace Advobot.Databases.Abstract
{
	internal sealed class DatabaseMetadata : IDatabaseEntry
	{
		public string ProgramVersion { get; set; } = Constants.BOT_VERSION;
		public int SchemaVersion { get; set; } = 3;

		//IDatabaseEntry
		object IDatabaseEntry.Id { get => ProgramVersion; set => ProgramVersion = (string)value; }
	}
}