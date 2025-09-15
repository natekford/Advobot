using Advobot.Tests.TestBases;
using Advobot.TypeReaders;
using Advobot.Utilities;

using Discord;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class PermissionsTypeReader_Tests
	: TypeReader_Tests<PermissionsTypeReader<ChannelPermission>>
{
	protected override PermissionsTypeReader<ChannelPermission> Instance { get; } = new();

	[TestMethod]
	public async Task InvalidNumber_Test()
	{
		var result = await ReadAsync("123412341234123412341234132412341234123412341234").ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
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
		var result = await ReadAsync(perms.Select(x => x.ToString()).Join()).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<ChannelPermission>(result.Value);
	}

	[TestMethod]
	public async Task ValidNumber_Test()
	{
		var result = await ReadAsync("123456789").ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<ChannelPermission>(result.Value);
	}
}