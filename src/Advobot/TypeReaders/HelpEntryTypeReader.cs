using Advobot.Services.Help;
using Advobot.Utilities;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.TypeReaders;

/// <summary>
/// Attempts to find a help entry with the supplied name.
/// </summary>
[TypeReaderTargetType(typeof(IHelpModule))]
public sealed class HelpEntryTypeReader : TypeReader
{
	/// <summary>
	/// Attempts to find a help entry with the supplied input as a name.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="input"></param>
	/// <param name="services"></param>
	/// <returns></returns>
	public override Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var help = services.GetRequiredService<IHelpService>();
		var matches = help.GetHelpModules().Where(x =>
		{
			var nameMatch = x.Name.CaseInsEquals(input);
			var aliasMatch = x.Aliases.CaseInsContains(input);
			return nameMatch || aliasMatch;
		}).ToArray();
		return TypeReaderUtils.SingleValidResult(matches, "help entries", input).AsTask();
	}
}