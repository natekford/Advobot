using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find an <see cref="Emote"/>.
	/// </summary>
	[TypeReaderTargetType(typeof(Emote))]
	public sealed class EmoteTypeReader : TypeReader
	{
		/// <summary>
		/// Checks for any emotes matching the input. Input is tested as an emote id, then emote name.
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
				return Task.FromResult(TypeReaderResult.FromSuccess(tempEmote));
			}
			if (ulong.TryParse(input, out var id))
			{
				var emote = emotes.FirstOrDefault(x => x.Id == id);
				if (emote != null)
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(emote));
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