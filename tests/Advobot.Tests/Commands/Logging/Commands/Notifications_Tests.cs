using Advobot.Logging;
using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Logging.Utilities;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

namespace Advobot.Tests.Commands.Logging.Commands;

using Notifications = Advobot.Logging.Commands.Notifications;

[TestClass]
public sealed class Notifications_Tests : Command_Tests
{
	private NotificationDatabase Db { get; set; }

	[TestMethod]
	public async Task Channel_Test()
	{
		var input = $"{nameof(Notifications)} " +
			$"{nameof(Notifications.ModifyGoodbyeMessage)} " +
			$"{nameof(NotificationModuleBase.Channel)} " +
			$"{Context.Channel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		var notif = await Db.GetAsync(Notification.Goodbye, Context.Guild.Id).ConfigureAwait(false);
		Assert.IsNotNull(notif);
		Assert.AreEqual(Context.Channel.Id, notif.ChannelId);
	}

	[TestMethod]
	public async Task Content_Test()
	{
		const string content = "asdf joe mama";
		const string input = $"{nameof(Notifications)} " +
			$"{nameof(Notifications.ModifyGoodbyeMessage)} " +
			$"{nameof(NotificationModuleBase.Content)} " +
			$"{content} ";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		var notif = await Db.GetAsync(Notification.Goodbye, Context.Guild.Id).ConfigureAwait(false);
		Assert.IsNotNull(notif);
		Assert.AreEqual(content, notif.Content);
	}

	[TestMethod]
	public async Task Default_Test()
	{
		await Db.UpsertNotificationChannelAsync(Notification.Goodbye, Context.Guild.Id, Context.Channel.Id).ConfigureAwait(false);
		await Db.UpsertNotificationContentAsync(Notification.Goodbye, Context.Guild.Id, "asdf joe mama").ConfigureAwait(false);
		await Db.UpsertNotificationEmbedAsync(Notification.Goodbye, Context.Guild.Id, new()
		{
			Description = "asdf joe mama",
		}).ConfigureAwait(false);
		var notif = await Db.GetAsync(Notification.Goodbye, Context.Guild.Id).ConfigureAwait(false);
		Assert.IsNotNull(notif);
		Assert.IsNotNull(notif.Content);
		Assert.IsFalse(notif.EmbedEmpty());
		Assert.AreEqual(Context.Channel.Id, notif.ChannelId);

		const string input = $"{nameof(Notifications)} " +
			$"{nameof(Notifications.ModifyGoodbyeMessage)} " +
			$"{nameof(NotificationModuleBase.Default)} ";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		var notif2 = await Db.GetAsync(Notification.Goodbye, Context.Guild.Id).ConfigureAwait(false);
		Assert.IsNotNull(notif2);
		Assert.IsNull(notif2.Content);
		Assert.IsTrue(notif2.EmbedEmpty());
		Assert.AreEqual(0UL, notif2.ChannelId);
	}

	[TestMethod]
	public async Task Disable_Test()
	{
		await Db.UpsertNotificationChannelAsync(Notification.Goodbye, Context.Guild.Id, Context.Channel.Id).ConfigureAwait(false);

		const string input = $"{nameof(Notifications)} " +
			$"{nameof(Notifications.ModifyGoodbyeMessage)} " +
			$"{nameof(NotificationModuleBase.Disable)} ";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		var notif = await Db.GetAsync(Notification.Goodbye, Context.Guild.Id).ConfigureAwait(false);
		Assert.IsNotNull(notif);
		Assert.AreEqual(0UL, notif.ChannelId);
	}

	[TestMethod]
	public async Task Embed_Test()
	{
		const string description = "\"asdf joe mama\"";
		const string input = $"{nameof(Notifications)} " +
			$"{nameof(Notifications.ModifyGoodbyeMessage)} " +
			$"{nameof(NotificationModuleBase.Embed)} " +
			$"description {description}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		var notif = await Db.GetAsync(Notification.Goodbye, Context.Guild.Id).ConfigureAwait(false);
		Assert.IsNotNull(notif);
		Assert.AreEqual(description.Trim('\"'), notif.Description);
	}

	protected override async Task SetupAsync()
	{
		await base.SetupAsync().ConfigureAwait(false);

		Db = await Context.Services.GetDatabaseAsync<NotificationDatabase>().ConfigureAwait(false);
	}
}