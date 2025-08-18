using Advobot.Settings.Database;
using Advobot.Settings.Database.Models;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Settings.Database;

[TestClass]
public sealed class SettingsCRUD_Tests : Database_Tests<SettingsDatabase>
{
	[TestMethod]
	public async Task CommandOverridesCRUD_Test()
	{
		const string COMMAND_ID = "joe";

		var overrides = new[]
		{
			new CommandOverride(Context.Guild)
			{
				CommandId = COMMAND_ID,
				Enabled = true,
				Priority = 7,
			},
			new CommandOverride(Context.User)
			{
				CommandId = COMMAND_ID,
				Enabled = true,
				Priority = 7,
			},
			new CommandOverride(Context.Guild)
			{
				CommandId = COMMAND_ID + "ba",
				Enabled = true,
				Priority = 6,
			},
		};

		await Db.UpsertCommandOverridesAsync(overrides).ConfigureAwait(false);

		{
			var retrieved = await Db.GetCommandOverridesAsync(Context.Guild.Id).ConfigureAwait(false);
			var expected = overrides
				.OrderBy(x => x.CommandId)
				.ThenByDescending(x => x.Priority)
				.ThenBy(x => x.TargetType);
			CollectionAssert.AreEqual(expected.ToArray(), retrieved.ToArray());
		}

		{
			var retrieved = await Db.GetCommandOverridesAsync(Context.Guild.Id, COMMAND_ID).ConfigureAwait(false);
			var expected = overrides
				.Where(x => x.CommandId == COMMAND_ID)
				.OrderBy(x => x.CommandId)
				.ThenByDescending(x => x.Priority)
				.ThenBy(x => x.TargetType);
			CollectionAssert.AreEqual(expected.ToArray(), retrieved.ToArray());
		}

		for (var i = 0; i < overrides.Length; ++i)
		{
			overrides[i] = overrides[i] with
			{
				Enabled = false,
			};
		}
		await Db.UpsertCommandOverridesAsync(overrides).ConfigureAwait(false);

		{
			var retrieved = await Db.GetCommandOverridesAsync(Context.Guild.Id).ConfigureAwait(false);
			var expected = overrides
				.OrderBy(x => x.CommandId)
				.ThenByDescending(x => x.Priority)
				.ThenBy(x => x.TargetType);
			CollectionAssert.AreEqual(expected.ToArray(), retrieved.ToArray());
		}

		var toDelete = overrides.Where(x => x.CommandId != COMMAND_ID);
		await Db.DeleteCommandOverridesAsync(toDelete).ConfigureAwait(false);

		{
			var retrieved = await Db.GetCommandOverridesAsync(Context.Guild.Id).ConfigureAwait(false);
			var expected = overrides
				.Where(x => x.CommandId == COMMAND_ID)
				.OrderBy(x => x.CommandId)
				.ThenByDescending(x => x.Priority)
				.ThenBy(x => x.TargetType);
			CollectionAssert.AreEqual(expected.ToArray(), retrieved.ToArray());
		}
	}
}