using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;

namespace Advobot.Core.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find an <see cref="Emote"/>.
	/// </summary>
	public sealed class EmoteTypeReader : TypeReader
	{
		/// <summary>
		/// Checks for any emotes matching the input. Input is tested as an emote Id, then emote name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			IEmote emote = null;
			if (Emote.TryParse(input, out var tempEmote))
			{
				emote = tempEmote;
			}
			else if (ulong.TryParse(input, out var emoteId))
			{
				emote = context.Guild.Emotes.FirstOrDefault(x => x.Id == emoteId);
			}

			if (emote == null)
			{
				var emotes = context.Guild.Emotes.Where(x => x.Name.CaseInsEquals(input)).ToList();
				if (emotes.Count() == 1)
				{
					emote = emotes.First();
				}
				else if (emotes.Count() > 1)
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many emotes have the provided name."));
				}
			}

			return emote != null
				? Task.FromResult(TypeReaderResult.FromSuccess(emote))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching emote."));
		}
	}

	/// <summary>
	/// Attempts to find a <see cref="GuildEmote"/> on the guild.
	/// </summary>
	public sealed class GuildEmoteTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			IEmote emote = null;
			//Can still tryparse it, but want to make sure this emote is on the current guild, so just pass the id down
			if (Emote.TryParse(input, out var tempEmote))
			{
				input = tempEmote.Id.ToString();
			}
			if (ulong.TryParse(input, out var emoteId))
			{
				emote = context.Guild.Emotes.FirstOrDefault(x => x.Id == emoteId);
			}

			if (emote == null)
			{
				var emotes = context.Guild.Emotes.Where(x => x.Name.CaseInsEquals(input)).ToList();
				if (emotes.Count() == 1)
				{
					emote = emotes.First();
				}
				else if (emotes.Count() > 1)
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many emotes have the provided name."));
				}
			}

			return emote != null
				? Task.FromResult(TypeReaderResult.FromSuccess(emote))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching emote."));
		}
	}
}