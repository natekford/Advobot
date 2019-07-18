using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Advobot.Services.HelpEntries;
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
			var matches = helpEntries.FindCloseHelpEntries(input);
			return matches.Count == 0
				? Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Unable to find an object matching `{input}`."))
				: Task.FromResult(TypeReaderResult.FromSuccess(matches));
		}
	}
}