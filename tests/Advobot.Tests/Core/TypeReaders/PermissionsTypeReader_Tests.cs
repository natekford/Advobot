using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class PermissionsTypeReader_Tests : TypeReaderTestsBase
{
	protected override TypeReader Instance { get; } = new PermissionsTypeReader<ChannelPermission>();

	[TestMethod]
	public async Task InvalidNumber_Test()
	{
		var result = await ReadAsync("123412341234123412341234132412341234123412341234").CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task ValidNames_Test()
	{
		var perms = new[]
		{
				ChannelPermission.AddReactions,
				ChannelPermission.AttachFiles,
				ChannelPermission.CreateInstantInvite,
				ChannelPermission.EmbedLinks,
				ChannelPermission.ManageWebhooks
			};
		var result = await ReadAsync(perms.Join(x => x.ToString())).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(ChannelPermission));
	}

	[TestMethod]
	public async Task ValidNumber_Test()
	{
		var result = await ReadAsync("123456789").CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(ChannelPermission));
	}
}