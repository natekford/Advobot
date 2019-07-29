using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

namespace Advobot.CommandMarking.Responses
{
	public sealed class Emotes : CommandResponses
	{
		private Emotes() { }

		public static AdvobotResult EnqueuedCreation(string name, int position)
			=> Success(Default.FormatInterpolated($"Successfully queued creating the emote {name} at position {position}."));
		public static AdvobotResult AddedRequiredRoles(IEmote emote, IEnumerable<IRole> roles)
			=> Success(Default.FormatInterpolated($"Successfully added {roles} as roles necessary to use {emote}."));
		public static AdvobotResult NoRequiredRoles(IEmote emote)
			=> Success(Default.FormatInterpolated($"There are no required roles for {emote}."));
		public static AdvobotResult RemoveRequiredRoles(IEmote emote, IEnumerable<IRole> roles)
			=> Success(Default.FormatInterpolated($"Successfully removed {roles} as roles necessary to use {emote}."));
		public static AdvobotResult DisplayMany(IEnumerable<IEmote> emotes, [CallerMemberName] string caller = "")
		{
			return Success(new EmbedWrapper
			{
				Title = Title.FormatInterpolated($"{caller} Emotes"),
				Description = BigBlock.FormatInterpolated($"{emotes.Join("\n", x => $"{x.Format()}")}"),
			});
		}
	}
}
