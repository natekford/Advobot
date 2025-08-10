using Advobot.AutoMod.Database;
using Advobot.AutoMod.Service;
using Advobot.Punishments;
using Advobot.Serilog;
using Advobot.Services.Time;
using Advobot.Tests.Fakes.Services;
using Advobot.Tests.Fakes.Services.Time;
using Advobot.Tests.TestBases;

using Discord;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.AutoMod.Service;

[TestClass]
public sealed class TimedPunishment_Tests : TestsBase
{
	private readonly FakeTimedPunishmentDatabase _Db = new();
	private readonly MutableTime _Time = new();
	private readonly TimedPunishmentService _TimedPunishmentService;

	public TimedPunishment_Tests()
	{
		_TimedPunishmentService = Services.Value.GetRequiredService<TimedPunishmentService>();
	}

	[TestMethod]
	public async Task BansGetAdded_Test()
	{
		await AddBansAsync(5).ConfigureAwait(false);

		var retrieved = await _Db.GetExpiredPunishmentsAsync(DateTime.MaxValue.Ticks).ConfigureAwait(false);
		Assert.HasCount(5, retrieved);
	}

	[TestMethod]
	public async Task OldTimedPunishmentsGetIgnored_Test()
	{
		await AddBansAsync(5).ConfigureAwait(false);

		_Time.UtcNow += TimeSpan.FromDays(10);
		{
			var retrieved = await _Db.GetExpiredPunishmentsAsync(_Time.UtcNow.Ticks).ConfigureAwait(false);
			Assert.HasCount(5, retrieved);
		}

		var tcs = new TaskCompletionSource<int>();
		_Db.PunishmentsModified += (added, punishments) =>
		{
			Assert.IsFalse(added);
			tcs.SetResult(punishments.Count());
		};
		_TimedPunishmentService.Start();

		var value = await tcs.Task.ConfigureAwait(false);
		Assert.AreEqual(5, value);
		Assert.IsEmpty(_Db.Punishments);
	}

	[TestMethod]
	public async Task TimedPunishmentsGetProcessed_Test()
	{
		await AddBansAsync(5).ConfigureAwait(false);

		_Time.UtcNow += TimeSpan.FromDays(3);
		{
			var retrieved = await _Db.GetExpiredPunishmentsAsync(_Time.UtcNow.Ticks).ConfigureAwait(false);
			Assert.HasCount(5, retrieved);
		}

		var count = 0;
		var tcs = new TaskCompletionSource<int>();
		_Db.PunishmentsModified += (added, _) =>
		{
			Assert.IsFalse(added);
			++count;

			if (_Db.Punishments.IsEmpty)
			{
				tcs.SetResult(count);
			}
		};
		_TimedPunishmentService.Start();

		var value = await tcs.Task.ConfigureAwait(false);
		Assert.AreEqual(5, value);
		Assert.IsEmpty(_Db.Punishments);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<ITimedPunishmentDatabase>(_Db)
			.AddSingleton<IDiscordClient>(Context.Client)
			.AddSingleton<TimedPunishmentService>()
			.AddSingleton<ITimeService>(_Time)
			.AddSingleton<IConfig>(FakeConfig.Singleton)
			.AddLogger<TimedPunishmentService>("TEMP");
	}

	private async Task AddBansAsync(ulong count)
	{
		for (ulong i = 1; i <= count; ++i)
		{
			await _TimedPunishmentService.HandleAsync(new Ban(Context.Guild, i, true)
			{
				Time = TimeSpan.FromDays(1),
			}).ConfigureAwait(false);
		}
	}
}