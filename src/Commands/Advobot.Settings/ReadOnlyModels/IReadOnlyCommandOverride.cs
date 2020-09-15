using Advobot.SQLite.Relationships;

namespace Advobot.Settings.ReadOnlyModels
{
	public interface IReadOnlyCommandOverride : IGuildChild
	{
		string CommandId { get; }
		bool Enabled { get; }
		int Priority { get; }
		ulong TargetId { get; }
		CommandOverrideType TargetType { get; }
	}
}