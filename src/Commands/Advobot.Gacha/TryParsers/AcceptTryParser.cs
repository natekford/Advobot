﻿using System.Threading.Tasks;

using Advobot.Interactivity.TryParsers;

using AdvorangesUtils;

using Discord;

namespace Advobot.Gacha.TryParsers
{
	public sealed class AcceptTryParser : IMessageTryParser<bool>
	{
		public ValueTask<Optional<bool>> TryParseAsync(IMessage message)
		{
			if (message.Content.CaseInsEquals("y"))
			{
				return new ValueTask<Optional<bool>>(true);
			}
			else if (message.Content.CaseInsEquals("n"))
			{
				return new ValueTask<Optional<bool>>(false);
			}
			return new ValueTask<Optional<bool>>(Optional<bool>.Unspecified);
		}
	}
}