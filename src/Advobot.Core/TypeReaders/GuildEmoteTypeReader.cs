using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Utilities;

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
		public override Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			if (Emote.TryParse(input, out var temp))
			{
				var emote = context.Guild.Emotes.FirstOrDefault(x => x.Id == temp.Id);
				if (emote != null)
				{
					return TypeReaderUtils.FromSuccessAsync(emote);
				}
			}
			if (ulong.TryParse(input, out var id))
			{
				var emote = context.Guild.Emotes.FirstOrDefault(x => x.Id == id);
				if (emote != null)
				{
					return TypeReaderUtils.FromSuccessAsync(emote);
				}
			}

			var matches = context.Guild.Emotes.Where(x => x.Name.CaseInsEquals(input)).ToArray();
			return TypeReaderUtils.SingleValidResultAsync(matches, "emotes", input);
		}
	}
}