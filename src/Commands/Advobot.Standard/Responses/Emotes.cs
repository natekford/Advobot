using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Advobot.Classes;
using Advobot.Modules;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using static Advobot.Standard.Resources.Responses;

namespace Advobot.Standard.Responses
{
	public sealed class Emotes : AdvobotResult
	{
		private Emotes() : base(null, "")
		{
		}

		public static AdvobotResult AddedRequiredRoles(IEmote emote, IEnumerable<IRole> roles)
		{
			return Success(EmotesAddedRequiredRoles.Format(
				roles.Join(x => x.Format()).WithBlock(),
				emote.Format().WithBlock()
			));
		}

		public static AdvobotResult DisplayMany(
			IEnumerable<IEmote> emotes,
			[CallerMemberName] string caller = "")
		{
			var title = EmotesTitleDisplay.Format(
				caller.WithTitleCase()
			);
			var description = emotes
				.Join(x => x.Format(), Environment.NewLine)
				.WithBigBlock()
				.Value;
			return Success(new EmbedWrapper
			{
				Title = title,
				Description = description,
			});
		}

		public static AdvobotResult EnqueuedCreation(string name, int position)
		{
			return Success(EmotesEnqueuedCreation.Format(
				name.WithBlock(),
				position.ToString().WithBlock()
			));
		}

		public static AdvobotResult RemoveRequiredRoles(IEmote emote, IEnumerable<IRole> roles)
		{
			return Success(EmotesRemovedRequiredRoles.Format(
				roles.Join(x => x.Format()).WithBlock(),
				emote.Format().WithBlock()
			));
		}
	}
}