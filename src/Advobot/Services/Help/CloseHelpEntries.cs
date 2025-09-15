namespace Advobot.Services.Help;

/*
/// <summary>
/// Implementation of <see cref="CloseWords{T}"/> which searches through help entries.
/// </summary>
/// <param name="source"></param>
internal sealed class CloseHelpEntries(IEnumerable<IHelpModule> source)
	: CloseWords<IHelpModule>(source, x => x.Name)
{
	/// <inheritdoc />
	protected override CloseWord<IHelpModule> FindCloseness(string search, IHelpModule obj)
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
}*/