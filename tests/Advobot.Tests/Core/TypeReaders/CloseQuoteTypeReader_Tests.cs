using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class CloseQuoteTypeReader_Tests
		: TypeReader_TestsBase<CloseQuoteTypeReader>
	{
		private const string NAME = "dog";

		private readonly IGuildSettings _Settings;

		public CloseQuoteTypeReader_Tests()
		{
			_Settings = new GuildSettings();
			_Settings.Quotes.Add(new Quote(NAME, "kapow"));

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task Invalid_Test()
		{
			var result = await ReadAsync("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await ReadAsync(NAME).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(IEnumerable<Quote>));
		}
	}
}