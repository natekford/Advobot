using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod.Context;
using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.Utils;
using Advobot.Punishments;
using Advobot.Services.Time;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.AutoMod.Service
{
	public sealed class MassBanRecentJoinsService
	{
		private readonly ConcurrentDictionary<ulong, ConcurrentStack<ulong>> _Stacks = new();

		public MassBanRecentJoinsService(BaseSocketClient client)
		{
			client.UserJoined += OnUserJoined;
		}

		public async Task Ban(IGuild guild, int amount)
		{
			if (!_Stacks.TryGetValue(guild.Id, out var stack))
			{
				return;
			}

			var ids = new HashSet<ulong>();
			lock (stack)
			{
				while (ids.Count < amount && stack.TryPop(out var id))
				{
					ids.Add(id);
				}
			}
		}

		private Task OnUserJoined(SocketGuildUser user)
		{
			var stack = _Stacks.GetOrAdd(user.Guild.Id, _ => new ConcurrentStack<ulong>());
			lock (stack)
			{
				// Don't bother adding the user to the stack if they're already in it
				// We can only check the last item though
				if (stack.IsEmpty || (stack.TryPeek(out var id) && id != user.Id))
				{
					stack.Push(user.Id);
				}
			}
			return Task.CompletedTask;
		}
	}
}