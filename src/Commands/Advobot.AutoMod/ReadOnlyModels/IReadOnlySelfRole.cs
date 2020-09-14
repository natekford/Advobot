namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlySelfRole
	{
		int GroupId { get; }
		ulong GuildId { get; }
		ulong RoleId { get; }
	}
}