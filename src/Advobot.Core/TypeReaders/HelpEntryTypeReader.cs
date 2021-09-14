
using Advobot.Attributes;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to find a help entry with the supplied name.
	/// </summary>
	[TypeReaderTargetType(typeof(IModuleHelpEntry))]
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
			var help = services.GetRequiredService<IHelpEntryService>();
			var matches = help.GetHelpEntries().Where(x =>
			{
				var nameMatch = x.Name.CaseInsEquals(input);
				var aliasMatch = x.Aliases.CaseInsContains(input);
				return nameMatch || aliasMatch;
			}).ToArray();
			return TypeReaderUtils.SingleValidResult(matches, "help entries", input).AsTask();
		}
	}
}