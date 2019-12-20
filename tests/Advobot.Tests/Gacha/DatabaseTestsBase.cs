using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.GachaTests.Utilities;
using Advobot.Services.Time;
using Advobot.Tests.Fakes.Database;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Gacha
{
	public abstract class DatabaseTestsBase
	{
		public static readonly Random Rng = new Random();

		protected IServiceProvider Services { get; }

		protected DatabaseTestsBase()
		{
			Services = new ServiceCollection()
				.AddSingleton<IGachaDatabase, GachaDatabase>()
				.AddSingleton<ITime, DefaultTime>()
				.AddSingleton<IGachaDatabaseStarter, FakeGachaDatabaseStarter>()
				.BuildServiceProvider();
		}

		protected async Task<(List<IReadOnlySource>, List<IReadOnlyCharacter>)> AddSourcesAndCharacters(
			IGachaDatabase db,
			int sourceCount,
			int charactersPerSource)
		{
			var sources = new List<IReadOnlySource>();
			var characters = new List<IReadOnlyCharacter>();
			for (var i = 0; i < sourceCount; ++i)
			{
				var source = GachaTestUtils.GenerateFakeSource();
				sources.Add(source);

				for (var j = 0; j < charactersPerSource; ++j)
				{
					characters.Add(GachaTestUtils.GenerateFakeCharacter(source));
				}
			}
			var addedSources = await db.AddSourcesAsync(sources).CAF();
			Assert.AreEqual(sourceCount, addedSources);
			var addedCharacters = await db.AddCharactersAsync(characters).CAF();
			Assert.AreEqual(sourceCount * charactersPerSource, addedCharacters);
			return (sources, characters);
		}

		protected async Task<IGachaDatabase> GetDatabaseAsync()
		{
			var db = Services.GetRequiredService<IGachaDatabase>();
			await db.CreateDatabaseAsync().CAF();
			return db;
		}

		private sealed class FakeGachaDatabaseStarter : FakeSQLiteDatabaseStarter, IGachaDatabaseStarter
		{
			public override string GetDbFileName() => "Gacha.db";
		}
	}
}