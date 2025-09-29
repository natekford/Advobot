using Advobot.Standard.Commands;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;

using Discord;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Tests.Commands.Standard;

[TestClass]
public sealed class Users_Tests : Command_Tests
{
	[TestMethod]
	public async Task Ban_Test()
	{
		var user = new FakeGuildUser(Context.Guild);

		var input = $"{nameof(Users.Ban)} {user}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsEmpty(Context.Guild.FakeUsers.Where(x => x.Id == user.Id));
		Assert.HasCount(1, Context.Guild.FakeBans);
	}

	[TestMethod]
	public async Task BanNotInGuild_Test()
	{
		var user = new FakeUser();
		Context.Client.FakeUsers.Add(user);

		var input = $"{nameof(Users.Ban)} {user}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(1, Context.Guild.FakeBans);
	}

	[TestMethod]
	public async Task BanUserDoesntExist_Test()
	{
		var input = $"{nameof(Users.Ban)} 1234";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.IsEmpty(Context.Guild.FakeBans);
	}

	[TestMethod]
	public async Task Deafen_Test()
	{
		var user = new FakeGuildUser(Context.Guild);
		Assert.IsFalse(user.IsDeafened);

		var input = $"{nameof(Users.Deafen)} {user}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsTrue(user.IsDeafened);
	}

	[TestMethod]
	public async Task Kick_Test()
	{
		var user = new FakeGuildUser(Context.Guild);
		var expected = Context.Guild.FakeUsers.Count - 1;

		var input = $"{nameof(Users.Kick)} {user}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.DoesNotContain(user, Context.Guild.FakeUsers);
		Assert.HasCount(expected, Context.Guild.FakeUsers);
	}

	[TestMethod]
	public async Task MoveUser_Test()
	{
		var user = new FakeGuildUser(Context.Guild);
		var inputChannel = new FakeVoiceChannel(Context.Guild);
		var outputChannel = new FakeVoiceChannel(Context.Guild);
		user.VoiceChannel = inputChannel;

		var input = $"{nameof(Users.MoveUser)} {user} {outputChannel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsEmpty(Context.Guild.FakeUsers.Where(x => x.VoiceChannel?.Id == inputChannel.Id));
		Assert.HasCount(1, Context.Guild.FakeUsers.Where(x => x.VoiceChannel?.Id == outputChannel.Id));
	}

	[TestMethod]
	public async Task MoveUsers_Test()
	{
		var user1 = new FakeGuildUser(Context.Guild);
		var user2 = new FakeGuildUser(Context.Guild);
		var inputChannel = new FakeVoiceChannel(Context.Guild);
		var outputChannel = new FakeVoiceChannel(Context.Guild);
		user1.VoiceChannel = inputChannel;
		user2.VoiceChannel = inputChannel;

		var input = $"{nameof(Users.MoveUsers)} {inputChannel} {outputChannel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsEmpty(Context.Guild.FakeUsers.Where(x => x.VoiceChannel?.Id == inputChannel.Id));
		Assert.HasCount(2, Context.Guild.FakeUsers.Where(x => x.VoiceChannel?.Id == outputChannel.Id));
	}
}