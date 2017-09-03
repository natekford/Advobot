using Advobot.Enums;
using Advobot.Structs;
using Discord;
using Discord.Commands;
using System;
using System.Linq;

namespace Advobot.Actions
{
	public static class EmoteActions
	{
		public static ReturnedObject<Emote> GetEmote(ICommandContext context, bool usage, string input)
		{
			Emote emote = null;
			if (!String.IsNullOrWhiteSpace(input))
			{
				if (Emote.TryParse(input, out emote))
				{
					return new ReturnedObject<Emote>(emote, FailureReason.NotFailure);
				}
				else if (ulong.TryParse(input, out ulong emoteID))
				{
					emote = context.Guild.Emotes.FirstOrDefault(x => x.Id == emoteID);
				}
				else
				{
					var emotes = context.Guild.Emotes.Where(x => x.Name.CaseInsEquals(input));
					if (emotes.Count() == 1)
					{
						emote = emotes.First();
					}
					else if (emotes.Count() > 1)
					{
						return new ReturnedObject<Emote>(emote, FailureReason.TooMany);
					}
				}
			}

			if (emote == null && usage)
			{
				var emoteMentions = context.Message.Tags.Where(x => x.Type == TagType.Emoji);
				if (emoteMentions.Count() == 1)
				{
					emote = emoteMentions.First().Value as Emote;
				}
				else if (emoteMentions.Count() > 1)
				{
					return new ReturnedObject<Emote>(emote, FailureReason.TooMany);
				}
			}

			return new ReturnedObject<Emote>(emote, FailureReason.NotFailure);
		}
	}
}