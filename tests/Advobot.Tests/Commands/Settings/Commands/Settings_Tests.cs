using Advobot.AutoMod.Database;
using Advobot.Settings.Database;
using Advobot.Settings.Database.Models;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;
using Advobot.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YACCS.Commands.Linq;

namespace Advobot.Tests.Commands.Settings.Commands;

using Settings = Advobot.Settings.Commands.Settings;

[TestClass]
public sealed class Settings_Tests : Command_Tests
{
	public const string BAN_ID = "b798e679-3ca7-4af1-9544-585672ec9936";
	public const string KICK_ID = "1d86aa7d-da06-478c-861b-a62ca279523b";
	private SettingsDatabase Db { get; set; }

	[TestMethod]
	public async Task ClearAll_Test()
	{
		await Db.UpsertCommandOverridesAsync([new CommandOverride(Context.User) with
		{
			CommandId = BAN_ID
		}]).ConfigureAwait(false);

		var input = $"{nameof(Settings.ModifyCommands)} " +
			$"{nameof(Settings.ModifyCommands.Clear)} " +
			$"{Context.User}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsEmpty(await Db.GetCommandOverridesAsync(Context.Guild.Id).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ClearSelect_Test()
	{
		var @override = new CommandOverride(Context.User);
		await Db.UpsertCommandOverridesAsync(
		[
			@override with
			{
				CommandId = BAN_ID
			},
			@override with
			{
				CommandId = KICK_ID,
			}
		]).ConfigureAwait(false);

		var input = $"{nameof(Settings.ModifyCommands)} " +
			$"{nameof(Settings.ModifyCommands.Clear)} " +
			$"{Context.User} " +
			$"{GetBanCommand()}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(1, await Db.GetCommandOverridesAsync(Context.Guild.Id).ConfigureAwait(false));
	}

	protected override async Task SetupAsync()
	{
		await base.SetupAsync().ConfigureAwait(false);
		Db = await Services.GetDatabaseAsync<SettingsDatabase>().ConfigureAwait(false);
	}

	private string GetBanCommand()
		=> CommandService.Commands.ById(BAN_ID).First().Paths[0].Join(" ");
}