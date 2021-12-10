using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.TypeReaders;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.TypeReaders;

[TestClass]
public sealed class SelfRoleStateTypeReader_Tests : TypeReaderTestsBase
{
	private readonly FakeAutoModDatabase _Db = new();
	protected override TypeReader Instance { get; } = new SelfRoleStateTypeReader();

	[TestMethod]
	public async Task Valid_Test()
	{
		var roles = new List<IRole>();
		foreach (var name in new[] { "joe", "bob", "tom" })
		{
			roles.Add(await Context.Guild.CreateRoleAsync(name, null, null, false, null).CAF());
		}

		var selfRoles = roles.ConvertAll(x => new SelfRole
		{
			GuildId = x.Guild.Id,
			RoleId = x.Id,
			GroupId = 2,
		});
		await _Db.UpsertSelfRolesAsync(selfRoles).CAF();

		{
			var retrieved = await _Db.GetSelfRolesAsync(Context.Guild.Id).CAF();
			Assert.AreEqual(roles.Count, retrieved.Count);
		}

		{
			var result = await ReadAsync(roles[0].Name).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(SelfRoleState));

			var cast = (SelfRoleState)result.BestMatch;
			Assert.IsNotNull(cast.ConflictingRoles);
			Assert.AreEqual(selfRoles[0].RoleId, cast.Role.Id);
			Assert.AreEqual(selfRoles[0].GroupId, cast.Group);
			Assert.AreEqual(roles.Count - 1, cast.ConflictingRoles.Count);
		}

		await roles[^1].DeleteAsync().CAF();

		{
			var retrieved = await _Db.GetSelfRolesAsync(Context.Guild.Id).CAF();
			Assert.AreEqual(roles.Count, retrieved.Count);
		}

		{
			var result = await ReadAsync(roles[0].Name).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(SelfRoleState));

			var cast = (SelfRoleState)result.BestMatch;
			Assert.IsNotNull(cast.ConflictingRoles);
			Assert.AreEqual(selfRoles[0].RoleId, cast.Role.Id);
			Assert.AreEqual(selfRoles[0].GroupId, cast.Group);
			Assert.AreEqual(roles.Count - 2, cast.ConflictingRoles.Count);
		}

		{
			var retrieved = await _Db.GetSelfRolesAsync(Context.Guild.Id).CAF();
			Assert.AreEqual(roles.Count - 1, retrieved.Count);
		}
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<IAutoModDatabase>(_Db);
	}
}