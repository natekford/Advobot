namespace Advobot.TypeReaders;

/*
/// <summary>
/// Finds help entries with names or aliases similar to the passed in input.
/// </summary>
[TypeReaderTargetType(typeof(IReadOnlyList<IHelpModule>))]
public sealed class CloseHelpEntryTypeReader : TypeReader
{
	/// <inheritdoc />
	public override Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var helpEntries = services.GetRequiredService<IHelpService>();
		var matches = helpEntries.FindCloseHelpModules(input);
		return TypeReaderUtils.MultipleValidResults(matches, "help entries", input).AsTask();
	}
}*/