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
	private readonly FakeRemovablePunishmentDatabase _Db = new();
	private readonly PunishmentService _Punisher = new();
	private readonly TimedPunishmentService _Service;
	private readonly MutableTime _Time = new();

	public TimedPunishment_Tests()
	{
		_Service = Services.Value.GetRequiredService<TimedPunishmentService>();
	}

	[TestMethod]
	public async Task BansGetAdded_Test()
	{
		await AddBansAsync(5).ConfigureAwait(false);

		var retrieved = await _Db.GetOldPunishmentsAsync(DateTime.MaxValue.Ticks).ConfigureAwait(false);
		Assert.HasCount(5, retrieved);
	}

	[TestMethod]
	public async Task OldRemovablePunishmentsGetIgnored_Test()
	{
		await AddBansAsync(5).ConfigureAwait(false);

		_Time.UtcNow += TimeSpan.FromDays(10);
		{
			var retrieved = await _Db.GetOldPunishmentsAsync(_Time.UtcNow.Ticks).ConfigureAwait(false);
			Assert.HasCount(5, retrieved);
		}

		_Punisher.PunishmentRemoved += _ =>
		{
			Assert.Fail("Punishment was removed instead of ignored for being too old.");
			return Task.CompletedTask;
		};

		var tcs = new TaskCompletionSource<int>();
		_Db.PunishmentsModified += (added, punishments) =>
		{
			Assert.IsFalse(added);
			tcs.SetResult(punishments.Count());
		};
		_Service.Start();

		var value = await tcs.Task.ConfigureAwait(false);
		Assert.AreEqual(5, value);

		{
			var retrieved = await _Db.GetOldPunishmentsAsync(DateTime.MaxValue.Ticks).ConfigureAwait(false);
			Assert.IsEmpty(retrieved);
		}
	}

	[TestMethod]
	public async Task RemovablePunishmentsGetProcessed_Test()
	{
		await AddBansAsync(5).ConfigureAwait(false);

		_Time.UtcNow += TimeSpan.FromDays(3);
		{
			var retrieved = await _Db.GetOldPunishmentsAsync(_Time.UtcNow.Ticks).ConfigureAwait(false);
			Assert.HasCount(5, retrieved);
		}

		var punishmentRemovedCount = 0;
		_Punisher.PunishmentRemoved += _ =>
		{
			++punishmentRemovedCount;
			return Task.CompletedTask;
		};

		var tcs = new TaskCompletionSource<int>();
		_Db.PunishmentsModified += (added, punishments) =>
		{
			Assert.IsFalse(added);
			tcs.SetResult(punishments.Count());
		};
		_Service.Start();

		var value = await tcs.Task.ConfigureAwait(false);
		Assert.AreEqual(5, value);
		Assert.AreEqual(5, punishmentRemovedCount);

		{
			var retrieved = await _Db.GetOldPunishmentsAsync(DateTime.MaxValue.Ticks).ConfigureAwait(false);
			Assert.IsEmpty(retrieved);
		}
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<ITimedPunishmentDatabase>(_Db)
			.AddSingleton<IDiscordClient>(Context.Client)
			.AddSingleton<IPunishmentService>(_Punisher)
			.AddSingleton<ITime>(_Time)
			.AddSingleton<IConfig>(FakeConfig.Singleton)
			.AddSingleton<TimedPunishmentService>()
			.AddLogger<TimedPunishmentService>("TEMP");
	}

	private async Task AddBansAsync(ulong count)
	{
		for (ulong i = 1; i <= count; ++i)
		{
			await _Punisher.HandleAsync(new Ban(Context.Guild, i, true)
			{
				Time = TimeSpan.FromDays(1),
			}).ConfigureAwait(false);
		}
	}
}