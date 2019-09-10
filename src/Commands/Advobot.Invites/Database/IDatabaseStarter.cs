namespace Advobot.Invites.Database
{
	public interface IDatabaseStarter
	{
		string GetConnectionString();

		bool IsDatabaseCreated();
	}
}