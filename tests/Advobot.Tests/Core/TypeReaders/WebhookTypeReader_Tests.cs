﻿using System.Threading.Tasks;

using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders
{
	[TestClass]
	public sealed class WebhookTypeReader_Tests
		: TypeReader_TestsBase<WebhookTypeReader>
	{
		[TestMethod]
		public async Task Invalid_Test()
		{
			var result = await ReadAsync("asdf").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task ValidId_Test()
		{
			var wh = await Context.Channel.CreateWebhookAsync("testo").CAF();
			var result = await ReadAsync(wh.Id.ToString()).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(IWebhook));
		}

		[TestMethod]
		public async Task ValidName_Test()
		{
			var wh = await Context.Channel.CreateWebhookAsync("testo").CAF();
			var result = await ReadAsync(wh.Name).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(IWebhook));
		}
	}
}