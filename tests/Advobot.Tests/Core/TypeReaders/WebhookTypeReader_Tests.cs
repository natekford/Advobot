using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class WebhookTypeReader_Tests : TypeReaderTestsBase
{
	protected override TypeReader Instance { get; } = new WebhookTypeReader();

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