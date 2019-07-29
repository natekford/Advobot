using System;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Modules;
using Advobot.Services.GuildSettings.Settings;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to find a quote with the supplied name.
	/// </summary>
	[TypeReaderTargetType(typeof(Quote))]
	public sealed class QuoteTypeReader : TypeReader<AdvobotCommandContext>
	{
		/// <summary>
		/// Attempts to find a quote with the supplied input as a name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(AdvobotCommandContext context, string input, IServiceProvider services)
		{
			return context.GuildSettings.Quotes.TryGetSingle(x => x.Name.CaseInsEquals(input), out var quote)
				? Task.FromResult(TypeReaderResult.FromSuccess(quote))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Unable to find a quote matching `{input}`."));
		}
	}
}