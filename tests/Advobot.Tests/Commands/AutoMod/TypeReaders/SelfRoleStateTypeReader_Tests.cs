using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;
using Advobot.AutoMod.TypeReaders;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using Discord;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.AutoMod.TypeReaders;

[TestClass]
public sealed class SelfRoleStateTypeReader_Tests
	: TypeReader_Tests<SelfRoleStateTypeReader>
{
	protected override SelfRoleStateTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task Valid_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);

		var roles = new List<IRole>();
		foreach (var name in new[] { "joe", "bob", "tom" })
		{
			roles.Add(await Context.Guild.CreateRoleAsync(name, null, null, false, null).ConfigureAwait(false));
		}

		var selfRoles = roles.ConvertAll(x => new SelfRole
		{
			GuildId = x.Guild.Id,
			RoleId = x.Id,
			GroupId = 2,
		});
		await db.UpsertSelfRolesAsync(selfRoles).ConfigureAwait(false);

		{
			var retrieved = await db.GetSelfRolesAsync(Context.Guild.Id).ConfigureAwait(false);
			Assert.AreEqual(roles.Count, retrieved.Count);
		}

		{
			var result = await ReadAsync(roles[0].Name).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(SelfRoleState));

			var cast = (SelfRoleState)result.BestMatch;
			Assert.IsNotNull(cast.ConflictingRoles);
			Assert.AreEqual(selfRoles[0].RoleId, cast.Role.Id);
			Assert.AreEqual(selfRoles[0].GroupId, cast.Group);
			Assert.AreEqual(roles.Count - 1, cast.ConflictingRoles.Count);
		}

		await roles[^1].DeleteAsync().ConfigureAwait(false);

		{
			var retrieved = await db.GetSelfRolesAsync(Context.Guild.Id).ConfigureAwait(false);
			Assert.AreEqual(roles.Count, retrieved.Count);
		}

		{
			var result = await ReadAsync(roles[0].Name).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(SelfRoleState));

			var cast = (SelfRoleState)result.BestMatch;
			Assert.IsNotNull(cast.ConflictingRoles);
			Assert.AreEqual(selfRoles[0].RoleId, cast.Role.Id);
			Assert.AreEqual(selfRoles[0].GroupId, cast.Group);
			Assert.AreEqual(roles.Count - 2, cast.ConflictingRoles.Count);
		}

		{
			var retrieved = await db.GetSelfRolesAsync(Context.Guild.Id).ConfigureAwait(false);
			Assert.AreEqual(roles.Count - 1, retrieved.Count);
		}
	}

	protected override Task SetupAsync()
		=> GetDatabaseAsync();

	private Task<AutoModDatabase> GetDatabaseAsync()
		=> Services.GetDatabaseAsync<AutoModDatabase>();
}