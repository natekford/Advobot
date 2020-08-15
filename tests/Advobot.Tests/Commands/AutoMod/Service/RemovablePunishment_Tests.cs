using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod.Database;
using Advobot.AutoMod.Service;
using Advobot.Punishments;
using Advobot.Services.Time;
using Advobot.Tests.Fakes.Services.Time;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.Service
{
	[TestClass]
	public sealed class RemovablePunishment_Tests : TestsBase
	{
		private readonly FakeRemovablePunishmentDatabase _Db = new FakeRemovablePunishmentDatabase();
		private readonly Punisher _Punisher = new Punisher();
		private readonly RemovablePunishmentService _Service;
		private readonly MutableTime _Time = new MutableTime();

		public RemovablePunishment_Tests()
		{
			_Service = Services.GetRequiredService<RemovablePunishmentService>();
		}

		[TestMethod]
		public async Task BansGetAdded_Test()
		{
			await AddBansAsync(5).CAF();

			var retrieved = await _Db.GetOldPunishmentsAsync(DateTime.MaxValue.Ticks).CAF();
			Assert.AreEqual(5, retrieved.Count);
		}

		[TestMethod]
		public async Task OldRemovablePunishmentsGetIgnored_Test()
		{
			await AddBansAsync(5).CAF();

			_Time.UtcNow += TimeSpan.FromDays(10);
			{
				var retrieved = await _Db.GetOldPunishmentsAsync(_Time.UtcNow.Ticks).CAF();
				Assert.AreEqual(5, retrieved.Count);
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

			var value = await tcs.Task.CAF();
			Assert.AreEqual(5, value);

			{
				var retrieved = await _Db.GetOldPunishmentsAsync(DateTime.MaxValue.Ticks).CAF();
				Assert.AreEqual(0, retrieved.Count);
			}
		}

		[TestMethod]
		public async Task RemovablePunishmentsGetProcessed_Test()
		{
			await AddBansAsync(5).CAF();

			_Time.UtcNow += TimeSpan.FromDays(3);
			{
				var retrieved = await _Db.GetOldPunishmentsAsync(_Time.UtcNow.Ticks).CAF();
				Assert.AreEqual(5, retrieved.Count);
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

			var value = await tcs.Task.CAF();
			Assert.AreEqual(5, value);
			Assert.AreEqual(5, punishmentRemovedCount);

			{
				var retrieved = await _Db.GetOldPunishmentsAsync(DateTime.MaxValue.Ticks).CAF();
				Assert.AreEqual(0, retrieved.Count);
			}
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<IRemovablePunishmentDatabase>(_Db)
				.AddSingleton<IDiscordClient>(Context.Client)
				.AddSingleton<IPunisher>(_Punisher)
				.AddSingleton<ITime>(_Time)
				.AddSingleton<RemovablePunishmentService>();
		}

		private async Task AddBansAsync(ulong count)
		{
			for (ulong i = 1; i <= count; ++i)
			{
				await _Punisher.HandleAsync(new Ban(Context.Guild, i, true)
				{
					Time = TimeSpan.FromDays(1),
				}).CAF();
			}
		}
	}
}