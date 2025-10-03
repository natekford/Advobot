using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Logging.Resetters;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Tests.Commands.Logging.Commands;

using Logging = Advobot.Logging.Commands.Logging;

[TestClass]
public sealed class Logging_Tests : Command_Tests
{
	private LoggingDatabase Db { get; set; }

	[TestMethod]
	public async Task ModifyActionsAll_Disable_Test()
	{
		var input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyActions)} " +
			$"{nameof(Logging.ModifyActions.All)} " +
			$"{false}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsEmpty(await Db.GetLogActionsAsync(Context.Guild.Id).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ModifyActionsAll_Enable_Test()
	{
		var input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyActions)} " +
			$"{nameof(Logging.ModifyActions.All)} " +
			$"{true}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(Enum.GetValues<LogAction>().Length, await Db.GetLogActionsAsync(Context.Guild.Id).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ModifyActionsDefault_Test()
	{
		const string input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyActions)} " +
			$"{nameof(Logging.ModifyActions.Default)} ";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(LogActionsResetter.Default.Count, await Db.GetLogActionsAsync(Context.Guild.Id).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ModifyActionsSelect_Disable_Test()
	{
		var input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyActions)} " +
			$"{true} " +
			$"{LogAction.UserLeft} {LogAction.UserJoined}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(2, await Db.GetLogActionsAsync(Context.Guild.Id).ConfigureAwait(false));

		var input2 = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyActions)} " +
			$"{false} " +
			$"{LogAction.UserLeft} {LogAction.UserJoined}";

		var result2 = await ExecuteWithResultAsync(input2).ConfigureAwait(false);
		Assert.IsTrue(result2.InnerResult.IsSuccess);
		Assert.IsEmpty(await Db.GetLogActionsAsync(Context.Guild.Id).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ModifyActionsSelect_Enable_Test()
	{
		var input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyActions)} " +
			$"{true} " +
			$"{LogAction.UserLeft} {LogAction.UserJoined}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(2, await Db.GetLogActionsAsync(Context.Guild.Id).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ModifyIgnoredChannels_Test()
	{
		var input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyIgnoredChannels)} " +
			$"{nameof(Logging.ModifyIgnoredChannels.Add)} " +
			$"{Context.Channel} {OtherTextChannel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.HasCount(2, await Db.GetIgnoredChannelsAsync(Context.Guild.Id).ConfigureAwait(false));

		var input2 = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyIgnoredChannels)} " +
			$"{nameof(Logging.ModifyIgnoredChannels.Remove)} " +
			$"{Context.Channel} {OtherTextChannel}";

		var result2 = await ExecuteWithResultAsync(input2).ConfigureAwait(false);
		Assert.IsTrue(result2.InnerResult.IsSuccess);
		Assert.IsEmpty(await Db.GetIgnoredChannelsAsync(Context.Guild.Id).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ModifyImageLog_Remove_Test()
	{
		var input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyImageLog)} " +
			$"{Context.Channel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(Context.Channel.Id, (await Db.GetLogChannelsAsync(Context.Guild.Id).ConfigureAwait(false)).ImageLogId);

		const string input2 = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyImageLog)} " +
			$"{nameof(Logging.ModifyImageLog.Remove)} ";

		var result2 = await ExecuteWithResultAsync(input2).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(0UL, (await Db.GetLogChannelsAsync(Context.Guild.Id).ConfigureAwait(false)).ImageLogId);
	}

	[TestMethod]
	public async Task ModifyImageLog_SetAlreadySet_Test()
	{
		var input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyImageLog)} " +
			$"{Context.Channel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(Context.Channel.Id, (await Db.GetLogChannelsAsync(Context.Guild.Id).ConfigureAwait(false)).ImageLogId);

		var result2 = await NoExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsFalse(result2.InnerResult.IsSuccess);
	}

	[TestMethod]
	public async Task ModifyModLog_Remove_Test()
	{
		var input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyModLog)} " +
			$"{Context.Channel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(Context.Channel.Id, (await Db.GetLogChannelsAsync(Context.Guild.Id).ConfigureAwait(false)).ModLogId);

		const string input2 = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyModLog)} " +
			$"{nameof(Logging.ModifyModLog.Remove)} ";

		var result2 = await ExecuteWithResultAsync(input2).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(0UL, (await Db.GetLogChannelsAsync(Context.Guild.Id).ConfigureAwait(false)).ModLogId);
	}

	[TestMethod]
	public async Task ModifyModLog_SetAlreadySet_Test()
	{
		var input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyModLog)} " +
			$"{Context.Channel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(Context.Channel.Id, (await Db.GetLogChannelsAsync(Context.Guild.Id).ConfigureAwait(false)).ModLogId);

		var result2 = await NoExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsFalse(result2.InnerResult.IsSuccess);
	}

	[TestMethod]
	public async Task ModifyServerLog_Remove_Test()
	{
		var input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyServerLog)} " +
			$"{Context.Channel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(Context.Channel.Id, (await Db.GetLogChannelsAsync(Context.Guild.Id).ConfigureAwait(false)).ServerLogId);

		const string input2 = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyServerLog)} " +
			$"{nameof(Logging.ModifyServerLog.Remove)} ";

		var result2 = await ExecuteWithResultAsync(input2).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(0UL, (await Db.GetLogChannelsAsync(Context.Guild.Id).ConfigureAwait(false)).ServerLogId);
	}

	[TestMethod]
	public async Task ModifyServerLog_SetAlreadySet_Test()
	{
		var input = $"{nameof(Logging)} " +
			$"{nameof(Logging.ModifyServerLog)} " +
			$"{Context.Channel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(Context.Channel.Id, (await Db.GetLogChannelsAsync(Context.Guild.Id).ConfigureAwait(false)).ServerLogId);

		var result2 = await NoExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsFalse(result2.InnerResult.IsSuccess);
	}

	protected override async Task SetupAsync()
	{
		await base.SetupAsync().ConfigureAwait(false);

		Db = await Context.Services.GetDatabaseAsync<LoggingDatabase>().ConfigureAwait(false);
	}
}