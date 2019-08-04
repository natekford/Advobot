using System;
using System.Threading.Tasks;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Finds help entries with names or aliases similar to the passed in input.
	/// </summary>
	public sealed class CloseHelpEntryTypeReader : TypeReader
	{
		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var helpEntries = services.GetRequiredService<IHelpEntryService>();
			var matches = helpEntries.FindCloseHelpEntries(input);
			return TypeReaderUtils.MultipleValidResultsAsync(matches, "help entries", input);
		}
	}
}