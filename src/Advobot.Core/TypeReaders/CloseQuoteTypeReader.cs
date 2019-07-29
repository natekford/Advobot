using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.CloseWords;
using Advobot.Modules;
using Advobot.Services.GuildSettings.Settings;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Finds quotes with names similar to the passed in input.
	/// </summary>
	public sealed class CloseQuoteTypeReader : TypeReader<IAdvobotCommandContext>
	{
		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(IAdvobotCommandContext context, string input, IServiceProvider services)
		{
			var matches = new CloseWords<Quote>(context.Settings.Quotes).FindMatches(input);
			return matches.Count == 0
				? Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Unable to find an object matching `{input}`."))
				: Task.FromResult(TypeReaderResult.FromSuccess(matches.Select(x => x.Value)));
		}
	}
}