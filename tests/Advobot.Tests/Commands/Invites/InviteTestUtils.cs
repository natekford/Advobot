using Advobot.Invites.Models;
using Advobot.Invites.ReadOnlyModels;
using Advobot.Services.Time;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;

namespace Advobot.Tests.Commands.Invites
{
	public static class InviteTestUtils
	{
		public static (FakeGuild Guild, IReadOnlyListedInvite Invite) CreateFakeInvite(
			this FakeClient client,
			ITime time)
		{
			var guild = new FakeGuild(client);
			var channel = new FakeTextChannel(guild);
			var user = new FakeGuildUser(guild);
			var invite = new FakeInviteMetadata(channel, user);
			var listedInvite = new ListedInvite(invite, time.UtcNow);
			return (guild, listedInvite);
		}
	}
}