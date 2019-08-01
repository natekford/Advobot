using System.Collections.Generic;
using System.Linq;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;

namespace Advobot.Tests.Utilities
{
	public static class FakeUtils
	{
		public delegate bool TryGetMentionDelegate(string input, out ulong id);

		public static IReadOnlyCollection<ulong> GetMentions(
			this string content,
			TryGetMentionDelegate mentionDelegate)
		{
			var ids = new List<ulong>();
			foreach (var part in content?.Split(' ') ?? Enumerable.Empty<string>())
			{
				if (mentionDelegate(part, out var id))
				{
					ids.Add(id);
				}
			}
			return ids;
		}
		public static FakeCommandContext CreateContext()
		{
			var client = new FakeClient();
			var guild = new FakeGuild();
			var channel = new FakeTextChannel(guild);
			var user = new FakeUser();
			var message = new FakeUserMessage(channel, user, "nothing");
			return new FakeCommandContext(client, message);
		}
	}
}
