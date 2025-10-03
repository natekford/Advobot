using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using Advobot.MyCommands.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.AutoMod.Database;
using Microsoft.Extensions.DependencyInjection;
using Advobot.Tests.Utilities;

namespace Advobot.Tests.Commands.MyCommands.Commands;

using MyCommands = Advobot.MyCommands.Commands.MyCommands;

[TestClass]
public sealed class MyCommands_Tests : Command_Tests
{
	private FakeGuildUser OtherUser { get; set; }
	private AutoModDatabase Db { get; set; }

	[TestMethod]
	public async Task SpanitchWithPermissions_Test()
	{
		var input = $"{nameof(MyCommands.Spanitch)} {OtherUser}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(3, OtherUser.RoleIds);
		Assert.IsEmpty(await Db.GetPersistentRolesAsync(Context.Guild.Id, OtherUser.Id).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task SpanitchWithMickeId_Test()
	{
		Context.User.Id = MyCommands.Spanitch.MIJE_ID;
		await Context.User.RemoveRolesAsync(Context.User.RoleIds).ConfigureAwait(false);
		Assert.HasCount(0, Context.User.RoleIds);
		await Context.User.AddRoleAsync(new FakeRole(Context.Guild)
		{
			Permissions = new(0),
			Position = 100
		}).ConfigureAwait(false);

		var input = $"{nameof(MyCommands.Spanitch)} {OtherUser}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(3, OtherUser.RoleIds);
		Assert.IsEmpty(await Db.GetPersistentRolesAsync(Context.Guild.Id, OtherUser.Id).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task SpanitchHard_Test()
	{
		var input = $"{nameof(MyCommands.Spanitch)} {nameof(MyCommands.Spanitch.Hard)} {OtherUser}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(3, OtherUser.RoleIds);
		Assert.HasCount(2, await Db.GetPersistentRolesAsync(Context.Guild.Id, OtherUser.Id).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task SpanitchHardNotInGuild_Test()
	{
		var id = 1234UL;
		var input = $"{nameof(MyCommands.Spanitch)} {nameof(MyCommands.Spanitch.Hard)} {id}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(2, await Db.GetPersistentRolesAsync(Context.Guild.Id, id).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task Unspanitch_Test()
	{
		var input = $"{nameof(MyCommands.Spanitch)} {nameof(MyCommands.Spanitch.Hard)} {OtherUser}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(3, OtherUser.RoleIds);
		Assert.HasCount(2, await Db.GetPersistentRolesAsync(Context.Guild.Id, OtherUser.Id).ConfigureAwait(false));

		var input2 = $"{nameof(MyCommands.Spanitch)} {nameof(MyCommands.Spanitch.Unspanitch)} {OtherUser}";

		var result2 = await ExecuteWithResultAsync(input2).ConfigureAwait(false);
		Assert.IsTrue(result2.InnerResult.IsSuccess);
		Assert.HasCount(1, OtherUser.RoleIds);
		Assert.IsEmpty(await Db.GetPersistentRolesAsync(Context.Guild.Id, OtherUser.Id).ConfigureAwait(false));
	}

	protected override async Task SetupAsync()
	{
		await base.SetupAsync().ConfigureAwait(false);

		Db = await Context.Services.GetDatabaseAsync<AutoModDatabase>().ConfigureAwait(false);

		Context.Guild.Id = MyCommands.Spanitch.SERV_ID;
		_ = new FakeRole(Context.Guild)
		{
			Id = MyCommands.Spanitch.MUTE_ID,
			Position = 0,
		};
		_ = new FakeRole(Context.Guild)
		{
			Id = MyCommands.Spanitch.SPAN_ID,
			Position = 0,
		};
		OtherUser = new FakeGuildUser(Context.Guild);
	}
}
