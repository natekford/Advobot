using Advobot.Standard.Commands;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using Discord;

namespace Advobot.Tests.Commands.Standard.Commands;

[TestClass]
public sealed class Roles_Tests : Command_Tests
{
	[TestMethod]
	public async Task ClearRolePermsAll_Test()
	{
		var role = new FakeRole(Context.Guild)
		{
			Permissions = new(123456789),
			Position = 1,
		};

		var input = $"{nameof(Roles.ClearRolePerms)} {role}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(0UL, role.Permissions.RawValue);
	}

	[TestMethod]
	public async Task ClearRolePermsAllowed_Test()
	{
		var role = new FakeRole(Context.Guild)
		{
			Permissions = new((ulong)(0UL
				| GuildPermission.ChangeNickname
				| GuildPermission.AddReactions)),
			Position = 1,
		};
		var role2 = new FakeRole(Context.Guild)
		{
			Permissions = new((ulong)(0UL
				| GuildPermission.ChangeNickname
				| GuildPermission.ManageRoles)),
			Position = 100,
		};
		await Context.User.RemoveRoleAsync(AdminRole).ConfigureAwait(false);
		await Context.User.AddRoleAsync(role2).ConfigureAwait(false);

		var input = $"{nameof(Roles.ClearRolePerms)} {role}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual((ulong)GuildPermission.AddReactions, role.Permissions.RawValue);
	}

	[TestMethod]
	public async Task CopyRolePermsAll_Test()
	{
		var role = new FakeRole(Context.Guild)
		{
			Permissions = new(0UL),
			Position = 1,
		};

		var input = $"{nameof(Roles.CopyRolePerms)} {AdminRole} {role}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsTrue(role.Permissions.Administrator);
	}

	[TestMethod]
	public async Task CopyRolePermsAllowed_Test()
	{
		var inputRole = new FakeRole(Context.Guild)
		{
			Permissions = new((ulong)(0UL | GuildPermission.AttachFiles | GuildPermission.BanMembers)),
			Position = 1,
		};
		var outputRole = new FakeRole(Context.Guild)
		{
			Permissions = new((ulong)(0UL | GuildPermission.ChangeNickname)),
			Position = 1,
		};
		await Context.User.RemoveRoleAsync(AdminRole).ConfigureAwait(false);
		await Context.User.AddRoleAsync(new FakeRole(Context.Guild)
		{
			Permissions = new((ulong)(0UL | GuildPermission.ManageRoles | GuildPermission.AttachFiles)),
			Position = 100,
		}).ConfigureAwait(false);

		var input = $"{nameof(Roles.CopyRolePerms)} {inputRole} {outputRole}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual((ulong)(0UL
			| GuildPermission.AttachFiles
			| GuildPermission.ChangeNickname
		), outputRole.Permissions.RawValue);
	}

	[TestMethod]
	public async Task ModifyRolePosition_Test()
	{
		var expected = new FakeRole(Context.Guild)
		{
			Name = "asdf",
			Permissions = new((ulong)(0UL | GuildPermission.AttachFiles)),
			Color = Color.Blue,
			IsHoisted = true,
			IsMentionable = true,
			Position = 1,
		};

		var input = $"{nameof(Roles.SoftDeleteRole)} {expected}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);

		var actual = Context.Guild.FakeRoles.Single(x => x.Name == expected.Name);
		Assert.AreNotEqual(expected.Id, actual.Id);
		Assert.AreEqual(expected.Name, actual.Name);
		Assert.AreEqual(expected.Permissions, actual.Permissions);
		Assert.AreEqual(expected.Color, actual.Color);
		Assert.AreEqual(expected.IsHoisted, actual.IsHoisted);
		Assert.AreEqual(expected.IsMentionable, actual.IsMentionable);
		Assert.AreEqual(expected.Position, actual.Position);
	}
}