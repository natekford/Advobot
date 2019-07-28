using Advobot.Gacha.Database;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.GachaTests.Utilities;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.GachaTests
{
	public abstract class DatabaseTestsBase
	{
		public readonly static Random Rng = new Random();

		protected IServiceProvider Provider { get; }

		public DatabaseTestsBase()
		{
			Provider = new ServiceCollection()
				.AddSingleton<GachaDatabase>()
				.AddSingleton<IDatabaseStarter, SQLiteTestDatabaseFactory>()
				.BuildServiceProvider();
		}

		protected async Task<GachaDatabase> GetDatabaseAsync()
		{
			var db = Provider.GetRequiredService<GachaDatabase>();
#pragma warning disable IDE0059 // Value assigned to symbol is never used
			var tables = await db.CreateDatabaseAsync().CAF();
#pragma warning restore IDE0059 // Value assigned to symbol is never used
			return db;
		}
		protected async Task<(List<IReadOnlySource>, List<IReadOnlyCharacter>)> AddSourcesAndCharacters(
			GachaDatabase db,
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
	}
}
