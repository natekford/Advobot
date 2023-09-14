using Advobot.Classes.CloseWords;

namespace Advobot.Services.HelpEntries;

/// <summary>
/// Implementation of <see cref="CloseWords{T}"/> which searches through help entries.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="CloseHelpEntries"/>.
/// </remarks>
/// <param name="source"></param>
internal sealed class CloseHelpEntries(IReadOnlyList<IModuleHelpEntry> source) : CloseWords<IModuleHelpEntry>(source, x => x.Name)
{

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