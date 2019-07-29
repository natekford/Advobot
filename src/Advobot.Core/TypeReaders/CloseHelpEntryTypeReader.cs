using System;
using System.Threading.Tasks;
using Advobot.Modules;
using Advobot.Services.HelpEntries;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Finds help entries with names or aliases similar to the passed in input.
	/// </summary>
	public sealed class CloseHelpEntryTypeReader : TypeReader<IAdvobotCommandContext>
	{
		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(IAdvobotCommandContext context, string input, IServiceProvider services)
		{
			var helpEntries = services.GetRequiredService<IHelpEntryService>();
			var matches = helpEntries.FindCloseHelpEntries(input);
			return matches.Count == 0
				? Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Unable to find an object matching `{input}`."))
				: Task.FromResult(TypeReaderResult.FromSuccess(matches));
		}
	}
}