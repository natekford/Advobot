using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.CloseWords;
using Advobot.Classes.Modules;
using Advobot.Services.GuildSettings.Settings;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Finds quotes with names similar to the passed in input.
	/// </summary>
	public sealed class CloseQuoteTypeReader : TypeReader<AdvobotCommandContext>
	{
		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(AdvobotCommandContext context, string input, IServiceProvider services)
		{
			var matches = new CloseWords<Quote>(context.GuildSettings.Quotes).FindMatches(input);
			return matches.Count == 0
				? Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Unable to find an object matching `{input}`."))
				: Task.FromResult(TypeReaderResult.FromSuccess(matches.Select(x => x.Value)));
		}
	}
}