using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.CloseWords;
using Advobot.Classes.Modules;
using Advobot.Interfaces;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Finds help entries with names or aliases similar to the passed in input.
	/// </summary>
	public sealed class CloseHelpEntryTypeReader : TypeReader<AdvobotCommandContext>
	{
		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(AdvobotCommandContext context, string input, IServiceProvider services)
		{
			var helpEntries = services.GetRequiredService<IHelpEntryService>();
			var matches = new CloseHelpEntries(helpEntries).FindMatches(input);
			return matches.Length == 0
				? Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Unable to find an object matching `{input}`."))
				: Task.FromResult(TypeReaderResult.FromSuccess(matches.Select(x => x.Value)));
		}
	}
}