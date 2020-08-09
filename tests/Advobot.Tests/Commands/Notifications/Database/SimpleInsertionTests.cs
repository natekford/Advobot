using System.Threading.Tasks;

using Advobot.Logging;
using Advobot.Logging.Database;
using Advobot.Logging.Models;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Notifications.Database
{
	[TestClass]
	public sealed class SimpleInsertionTests
		: Database_TestsBase<NotificationDatabase, FakeSQLiteConnectionString>
	{
		private const ulong CHANNEL_ID = 73;
		private const string? CONTENT = "uh oh stinky";
		private const Notification EVENT = Notification.Goodbye;
		private const ulong GUILD_ID = ulong.MaxValue;

		[TestMethod]
		public async Task NotificationInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			{
				var retrieved = await db.GetAsync(EVENT, GUILD_ID).CAF();
				Assert.IsNull(retrieved);
			}

			await db.UpsertNotificationChannelAsync(EVENT, GUILD_ID, CHANNEL_ID).CAF();
			{
				var retrieved = await db.GetAsync(EVENT, GUILD_ID).CAF()!;
				if (retrieved is null)
				{
					Assert.IsNotNull(retrieved);
					return;
				}
				Assert.AreEqual(CHANNEL_ID, retrieved.ChannelId);
			}

			await db.UpsertNotificationContentAsync(EVENT, GUILD_ID, CONTENT).CAF();
			{
				var retrieved = await db.GetAsync(EVENT, GUILD_ID).CAF()!;
				if (retrieved is null)
				{
					Assert.IsNotNull(retrieved);
					return;
				}
				Assert.AreEqual(CHANNEL_ID, retrieved.ChannelId);
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
			await db.UpsertNotificationEmbedAsync(EVENT, GUILD_ID, embed).CAF();
			{
				var retrieved = await db.GetAsync(EVENT, GUILD_ID).CAF();
				if (retrieved is null)
				{
					Assert.IsNotNull(retrieved);
					return;
				}
				Assert.AreEqual(CHANNEL_ID, retrieved.ChannelId);
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

			await db.UpsertNotificationChannelAsync(EVENT, GUILD_ID, null).CAF();
			{
				var retrieved = await db.GetAsync(EVENT, GUILD_ID).CAF();
				if (retrieved is null)
				{
					Assert.IsNotNull(retrieved);
					return;
				}
				Assert.AreEqual(0UL, retrieved.ChannelId);
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
}