using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Gacha.Interaction;

using AdvorangesUtils;

using Discord;

namespace Advobot.Gacha.Displays
{
	public abstract class PaginatedDisplay : Display
	{
		private int _PageIndex;

		protected PaginatedDisplay(
			IServiceProvider services,
			int id,
			int itemCount,
			int itemsPerPage) : base(services, id)
		{
			ItemCount = itemCount;
			ItemsPerPage = itemsPerPage;
			PageCount = (ItemCount + ItemsPerPage - 1) / ItemsPerPage;
			_PageIndex = 0;

			if (PageCount > 1)
			{
				InteractionHandler.AddInteraction(InteractionType.Left);
				InteractionHandler.AddInteraction(InteractionType.Right);
			}
		}

		public int ItemCount { get; }
		public int ItemsPerPage { get; }
		public int PageCount { get; }

		public int PageIndex
		{
			get => _PageIndex;
			protected set => _PageIndex = value;
		}

		protected virtual TimeSpan Timeout { get; } = TimeSpan.FromSeconds(30);

		protected EmbedFooterBuilder GeneratePaginationFooter()
		{
			var footer = GenerateDefaultFooter();
			footer.Text += $"| Page {PageIndex + 1} / {PageCount}";
			return footer;
		}

		protected IEnumerable<T> GetPageValues<T>(IEnumerable<T> values)
			=> values.Skip(PageIndex * ItemsPerPage).Take(ItemsPerPage);

		protected override async Task HandleInteractionAsync(IInteractionContext context)
		{
			if (context.Action is Movement m && m.TryUpdatePage(ref _PageIndex, PageCount))
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
	}
}