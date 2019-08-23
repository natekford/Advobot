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
		public override Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			if (Emote.TryParse(input, out var tempEmote))
			{
				return TypeReaderUtils.FromSuccessAsync(tempEmote);
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