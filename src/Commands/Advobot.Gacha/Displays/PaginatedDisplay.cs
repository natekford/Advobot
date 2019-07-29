using Advobot.Gacha.Database;
using Advobot.Gacha.MenuEmojis;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Gacha.Displays
{
	public abstract class PaginatedDisplay : Display
	{
		public int ItemCount { get; }
		public int ItemsPerPage { get; }
		public int PageCount { get; }
		public int PageIndex
		{
			get => _PageIndex;
			protected set => _PageIndex = value;
		}

		protected virtual TimeSpan Timeout { get; } = TimeSpan.FromSeconds(30);
		protected override EmojiMenu Menu { get; } = new EmojiMenu();

		private int _PageIndex;

		public PaginatedDisplay(
			BaseSocketClient client,
			GachaDatabase db,
			int itemCount,
			int itemsPerPage) : base(client, db)
		{
			ItemCount = itemCount;
			ItemsPerPage = itemsPerPage;
			PageCount = (ItemCount + ItemsPerPage - 1) / ItemsPerPage;
			_PageIndex = 0;

			if (PageCount > 1)
			{
				Menu.Add(new MovementEmoji(Constants.Left, -1));
				Menu.Add(new MovementEmoji(Constants.Right, 1));
			}
		}

		protected override async Task HandleReactionsAsync(
			IUserMessage message,
			SocketReaction reaction,
			IMenuEmote emoji)
		{
			if (emoji is MovementEmoji m && m.TryUpdatePage(ref _PageIndex, PageCount))
			{
				if (Message != null)
				{
					var embed = await GenerateEmbedAsync().CAF();
					await Message.ModifyAsync(x => x.Embed = embed).CAF();
				}
			}
		}
		protected override async Task KeepDisplayAliveAsync()
		{
			while (DateTime.UtcNow - LastInteractedWith > Timeout)
			{
				await Task.Delay(Timeout).CAF();
			}
		}
		protected EmbedFooterBuilder GeneratePaginationFooter()
		{
			return new EmbedFooterBuilder
			{
				Text = $"Page {PageIndex + 1} / {PageCount}",
			};
		}
		protected IEnumerable<T> GetPageValues<T>(IEnumerable<T> values)
			=> values.Skip(PageIndex * ItemsPerPage).Take(ItemsPerPage);
	}
}
