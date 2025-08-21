using Advobot.AutoMod.Database;
using Advobot.AutoMod.Service;
using Advobot.Punishments;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.AutoMod.Service;

[TestClass]
public sealed class TimedPunishment_Tests : TestsBase
{
	[TestMethod]
	public async Task BansGetAdded_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);
		var service = Services.GetRequiredService<TimedPunishmentService>();
		await AddBansAsync(service, 5).ConfigureAwait(false);

		var retrieved = await db.GetExpiredPunishmentsAsync(DateTime.MaxValue.Ticks).ConfigureAwait(false);
		Assert.HasCount(5, retrieved);
		Assert.HasCount(5, Context.Guild.FakeBans);
	}

	[TestMethod]
	public async Task TimedPunishmentsGetProcessed_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);
		var service = Services.GetRequiredService<TimedPunishmentService>();
		await AddBansAsync(service, 5).ConfigureAwait(false);

		Time.UtcNow += TimeSpan.FromDays(3);
		{
			var retrieved = await db.GetExpiredPunishmentsAsync(Time.UtcNow.Ticks).ConfigureAwait(false);
			Assert.HasCount(5, retrieved);
			Assert.HasCount(5, Context.Guild.FakeBans);
		}

		await service.StartAsync().ConfigureAwait(false);
		while (Context.Guild.FakeBans.Count > 0)
		{
			await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);
		}

		{
			var retrieved = await db.GetExpiredPunishmentsAsync(Time.UtcNow.Ticks).ConfigureAwait(false);
			Assert.IsEmpty(retrieved);
			Assert.IsEmpty(Context.Guild.FakeBans);
		}
	}

	protected override void ModifyServices(IServiceCollection services)
		=> services.AddSingletonWithFakeLogger<TimedPunishmentService>();

	protected override Task SetupAsync()
		=> GetDatabaseAsync();

	private async Task AddBansAsync(TimedPunishmentService service, ulong count)
	{
		for (ulong i = 1; i <= count; ++i)
		{
			await service.PunishAsync(new Ban(Context.Guild, i, true)
			{
				Duration = TimeSpan.FromDays(1),
			}).ConfigureAwait(false);
		}
	}

	private Task<TimedPunishmentDatabase> GetDatabaseAsync()
		=> Services.GetDatabaseAsync<TimedPunishmentDatabase>();
}