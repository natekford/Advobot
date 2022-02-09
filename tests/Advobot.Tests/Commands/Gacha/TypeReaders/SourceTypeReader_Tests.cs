using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Gacha.TypeReaders;
using Advobot.Gacha.Utilities;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Gacha.TypeReaders;

[TestClass]
public sealed class SourceTypeReader_Tests : TypeReader_Tests<SourceTypeReader>
{
	private readonly FakeGachaDatabase _Db = new();
	protected override SourceTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task InvalidMultipleMatches_Test()
	{
		var sources = new[]
		{
				GenerateStaticSource("Gamers!"),
				GenerateStaticSource("Gamers!"),
			};
		await _Db.AddSourcesAsync(sources).CAF();

		var result = await ReadAsync(sources[0].Name).CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task Valid_Test()
	{
		var sources = new[]
		{
				GenerateStaticSource("Gamers!"),
				GenerateStaticSource("not Gamers!"),
			};
		await _Db.AddSourcesAsync(sources).CAF();

		var result = await ReadAsync(sources[0].Name).CAF();
		Assert.IsTrue(result.IsSuccess);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<IGachaDatabase>(_Db);
	}

	private Source GenerateStaticSource(string name)
	{
		return new()
		{
			SourceId = TimeUtils.UtcNowTicks,
			Name = name,
		};
	}
}