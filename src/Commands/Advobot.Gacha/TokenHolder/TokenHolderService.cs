﻿using System.Collections.Concurrent;

using Advobot.Gacha.Utilities;

using Discord;

namespace Advobot.Gacha.ActionLimits
{
	public sealed class TokenHolderService : ITokenHolderService
	{
		private readonly ConcurrentDictionary<(ulong, ulong), CancellationTokenSource> _Tokens = new();

		public CancellationToken Get(IGuildUser user)
		{
			var newSource = new CancellationTokenSource();
			_Tokens.AddOrUpdate(user.ToKey(), newSource, (_, oldValue) =>
			{
				oldValue.Cancel();
				return newSource;
			});
			return newSource.Token;
		}
	}
}