using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using static Advobot.Standard.Resources.Responses;

namespace Advobot.Standard.Responses
{
	public sealed class Emotes : CommandResponses
	{
		private Emotes() { }

		public static AdvobotResult EnqueuedCreation(string name, int position)
			=> Success(Default.FormatInterpolated($"Successfully queued creating the emote {name} at position {position}."));
		public static AdvobotResult AddedRequiredRoles(IEmote emote, IEnumerable<IRole> roles)
			=> Success(Default.FormatInterpolated($"Successfully added {roles} as roles required to use {emote}."));
		public static AdvobotResult RemoveRequiredRoles(IEmote emote, IEnumerable<IRole> roles)
			=> Success(Default.FormatInterpolated($"Successfully removed {roles} as roles required to use {emote}."));
		public static AdvobotResult DisplayMany(
			IEnumerable<IEmote> emotes,
			[CallerMemberName] string caller = "")
		{
			var text = emotes.Join("\n", x => $"{x.Format()}");
			return Success(new EmbedWrapper
			{
				Title = Title.Format(EmotesTitleDisplay, caller),
				Description = BigBlock.FormatInterpolated($"{text}"),
			});
		}
	}
}
