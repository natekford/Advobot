﻿using System.Collections.Generic;
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
		private readonly IGuildSettings _Settings;

		public CloseQuoteTypeReader_Tests()
		{
			_Settings = new GuildSettings();

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
			var quote = new Quote("dog", "kapow");
			_Settings.Quotes.Add(quote);

			var result = await ReadAsync(quote.Name).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(IEnumerable<Quote>));
		}
	}
}