using System;
using System.Threading.Tasks;

using Advobot.Invites.Database;
using Advobot.Invites.Models;
using Advobot.Invites.ReadOnlyModels;
using Advobot.Services.Time;
using Advobot.SQLite;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Invites
{
	public abstract class DatabaseTestsBase
	{
		public static readonly Random Rng = new Random();

		protected IServiceProvider Services { get; }
		protected ITime Time => Services.GetRequiredService<ITime>();

		protected DatabaseTestsBase()
		{
			Services = new ServiceCollection()
				.AddSingleton<InviteDatabase>()
				.AddSingleton<ITime, DefaultTime>()
				.AddSingleton<IInviteDatabaseStarter, FakeSQLiteDatabaseStarter>()
				.BuildServiceProvider();
		}

		protected static (FakeGuild Guild, IReadOnlyListedInvite Invite) CreateFakeInvite(FakeClient client, ITime time)
		{
			var guild = new FakeGuild(client);
			var channel = new FakeTextChannel(guild);
			var user = new FakeGuildUser(guild);
			var invite = new FakeInviteMetadata(channel, user);
			var listedInvite = new ListedInvite(invite, time.UtcNow);
			return (guild, listedInvite);
		}

		protected async Task<InviteDatabase> GetDatabaseAsync()
		{
			var starter = Services.GetRequiredService<IInviteDatabaseStarter>();
			await starter.EnsureCreatedAsync().CAF();
			starter.MigrateUp();

			return Services.GetRequiredService<InviteDatabase>();
		}
	}
}