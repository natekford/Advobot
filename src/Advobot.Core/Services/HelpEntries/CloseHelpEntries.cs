using Advobot.Classes.CloseWords;

namespace Advobot.Services.HelpEntries;

/// <summary>
/// Implementation of <see cref="CloseWords{T}"/> which searches through help entries.
/// </summary>
internal sealed class CloseHelpEntries : CloseWords<IModuleHelpEntry>
{
	/// <summary>
	/// Creates an instance of <see cref="CloseHelpEntries"/>.
	/// </summary>
	/// <param name="source"></param>
	public CloseHelpEntries(IReadOnlyList<IModuleHelpEntry> source)
		: base(source, x => x.Name) { }

	/// <inheritdoc />
	protected override CloseWord<IModuleHelpEntry> FindCloseness(string search, IModuleHelpEntry obj)
	{
		var closest = obj.Name;
		var distance = FindCloseness(obj.Name, search);
		foreach (var alias in obj.Aliases)
		{
			var aliasDistance = FindCloseness(alias, search);
			if (aliasDistance < distance)
			{
				closest = alias;
				distance = aliasDistance;
			}
		}
		return new(closest, search, distance, obj);
	}
}