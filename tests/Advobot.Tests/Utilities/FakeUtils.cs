using Advobot.Serilog;
using Advobot.SQLite;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Utilities;

public static class FakeUtils
{
	public static IServiceCollection AddFakeDatabase<TDb>(
		this IServiceCollection services)
		where TDb : class
	{
		return services
			.AddSingleton<TDb>()
			.AddSingleton<IConnectionString<TDb>>(new FakeSQLiteConnectionString(typeof(TDb)));
	}

	public static IServiceCollection AddSingletonWithFakeLogger<T>(
		this IServiceCollection services)
		where T : class
		=> services.AddSingletonWithLogger<T>(Guid.NewGuid().ToString());

	public static FakeCommandContext CreateContext()
	{
		var client = new FakeClient();
		var guild = new FakeGuild(client);
		var channel = new FakeTextChannel(guild)
		{
			Name = "General"
		};
		var user = new FakeGuildUser(guild);
		var message = new FakeUserMessage(channel, user, "nothing");
		return new(client, message);
	}

	public static async Task<TDb> GetDatabaseAsync<TDb>(
		this IServiceProvider services) where TDb : class
	{
		var starter = services.GetRequiredService<IConnectionString<TDb>>();
		await starter.EnsureCreatedAsync().ConfigureAwait(false);
		starter.MigrateUp();

		return services.GetRequiredService<TDb>();
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