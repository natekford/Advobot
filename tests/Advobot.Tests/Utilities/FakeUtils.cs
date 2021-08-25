
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;

namespace Advobot.Tests.Utilities
{
	public static class FakeUtils
	{
		public static FakeCommandContext CreateContext()
		{
			var client = new FakeClient();
			var guild = new FakeGuild(client);
			var channel = new FakeTextChannel(guild);
			var user = new FakeGuildUser(guild);
			var message = new FakeUserMessage(channel, user, "nothing");
			return new(client, message);
		}

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

		public delegate bool TryGetMentionDelegate(string input, out ulong id);
	}
}