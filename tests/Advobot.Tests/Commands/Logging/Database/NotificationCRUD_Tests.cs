using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Logging.Database;

[TestClass]
public sealed class NotificationCRUD_Tests : Database_Tests<NotificationDatabase>
{
	private const ulong CHANNEL_ID = 73;
	private const string? CONTENT = "uh oh stinky";
	private const Notification EVENT = Notification.Goodbye;
	private const ulong GUILD_ID = ulong.MaxValue;

	[TestMethod]
	public async Task NotificationCRUD_Test()
	{
		{
			var retrieved = await Db.GetAsync(EVENT, GUILD_ID).ConfigureAwait(false);
			Assert.IsNull(retrieved);
		}

		await Db.UpsertNotificationChannelAsync(EVENT, GUILD_ID, CHANNEL_ID).ConfigureAwait(false);
		{
			var retrieved = await Db.GetAsync(EVENT, GUILD_ID).ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(CHANNEL_ID, retrieved!.ChannelId);
		}

		await Db.UpsertNotificationContentAsync(EVENT, GUILD_ID, CONTENT).ConfigureAwait(false);
		{
			var retrieved = await Db.GetAsync(EVENT, GUILD_ID).ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(CHANNEL_ID, retrieved!.ChannelId);
			Assert.AreEqual(CONTENT, retrieved.Content);
		}

		var embed = new CustomEmbed
		{
			AuthorIconUrl = "https://www.google.com",
			AuthorName = "steven",
			AuthorUrl = "https://www.youtube.com",
			Color = 1234,
			Description = "le monkey",
			Footer = "have foot",
			FooterIconUrl = "https://www.reddit.com",
			ImageUrl = "https://www.twitter.com",
			ThumbnailUrl = "https://www.discordapp.com",
			Title = "title is me",
			Url = "https://www.website.com",
		};
		await Db.UpsertNotificationEmbedAsync(EVENT, GUILD_ID, embed).ConfigureAwait(false);
		{
			var retrieved = await Db.GetAsync(EVENT, GUILD_ID).ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(CHANNEL_ID, retrieved!.ChannelId);
			Assert.AreEqual(CONTENT, retrieved.Content);
			Assert.AreEqual(embed.AuthorIconUrl, retrieved.AuthorIconUrl);
			Assert.AreEqual(embed.AuthorName, retrieved.AuthorName);
			Assert.AreEqual(embed.AuthorUrl, retrieved.AuthorUrl);
			Assert.AreEqual(embed.Color, retrieved.Color);
			Assert.AreEqual(embed.Description, retrieved.Description);
			Assert.AreEqual(embed.Footer, retrieved.Footer);
			Assert.AreEqual(embed.FooterIconUrl, retrieved.FooterIconUrl);
			Assert.AreEqual(embed.ImageUrl, retrieved.ImageUrl);
			Assert.AreEqual(embed.ThumbnailUrl, retrieved.ThumbnailUrl);
			Assert.AreEqual(embed.Title, retrieved.Title);
			Assert.AreEqual(embed.Url, retrieved.Url);
		}

		await Db.UpsertNotificationChannelAsync(EVENT, GUILD_ID, null).ConfigureAwait(false);
		{
			var retrieved = await Db.GetAsync(EVENT, GUILD_ID).ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(0UL, retrieved!.ChannelId);
			Assert.AreEqual(CONTENT, retrieved.Content);
			Assert.AreEqual(embed.AuthorIconUrl, retrieved.AuthorIconUrl);
			Assert.AreEqual(embed.AuthorName, retrieved.AuthorName);
			Assert.AreEqual(embed.AuthorUrl, retrieved.AuthorUrl);
			Assert.AreEqual(embed.Color, retrieved.Color);
			Assert.AreEqual(embed.Description, retrieved.Description);
			Assert.AreEqual(embed.Footer, retrieved.Footer);
			Assert.AreEqual(embed.FooterIconUrl, retrieved.FooterIconUrl);
			Assert.AreEqual(embed.ImageUrl, retrieved.ImageUrl);
			Assert.AreEqual(embed.ThumbnailUrl, retrieved.ThumbnailUrl);
			Assert.AreEqual(embed.Title, retrieved.Title);
			Assert.AreEqual(embed.Url, retrieved.Url);
		}
	}
}