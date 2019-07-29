using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to find a <see cref="GuildEmote"/> on the guild.
	/// </summary>
	[TypeReaderTargetType(typeof(GuildEmote))]
	public sealed class GuildEmoteTypeReader : TypeReader
	{
		/// <summary>
		/// Checks for any guild emotes matching the input. Input is tested as an emote id, then emote name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var emotes = context.Guild.Emotes;
			if (Emote.TryParse(input, out var tempEmote))
			{
				var guildEmote = emotes.FirstOrDefault(x => x.Id == tempEmote.Id);
				if (guildEmote != null)
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(guildEmote));
				}
			}
			if (ulong.TryParse(input, out var id))
			{
				var guildEmote = emotes.FirstOrDefault(x => x.Id == id);
				if (guildEmote != null)
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(guildEmote));
				}
			}

			var matchingEmotes = emotes.Where(x => x.Name.CaseInsEquals(input)).ToArray();
			if (matchingEmotes.Length == 1)
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(matchingEmotes[0]));
			}
			if (matchingEmotes.Length > 1)
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many emotes have the provided name."));
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Emote not found."));
		}
	}
}