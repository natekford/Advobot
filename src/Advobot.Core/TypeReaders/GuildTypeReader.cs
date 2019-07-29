using Advobot.Attributes;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to find an <see cref="IGuild"/>.
	/// </summary>
	[TypeReaderTargetType(typeof(IGuild))]
	public sealed class GuildTypeReader : TypeReader
	{
		/// <summary>
		/// Checks for any guilds matching the input.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (ulong.TryParse(input, out var id))
			{
				var guild = await context.Client.GetGuildAsync(id).CAF();
				if (guild != null)
				{
					return TypeReaderResult.FromSuccess(guild);
				}
			}

			var guilds = await context.Client.GetGuildsAsync().CAF();
			var matchingGuilds = guilds.Where(x => x.Name.CaseInsEquals(input)).ToArray();
			if (matchingGuilds.Length == 1)
			{
				return TypeReaderResult.FromSuccess(matchingGuilds[0]);
			}
			if (matchingGuilds.Length > 1)
			{
				return TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many guilds have the provided name.");
			}
			return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Guild not found.");
		}
	}
}
