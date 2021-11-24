using Advobot.Gacha.Database;
using Advobot.Gacha.Interaction;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;

using InteractionType = Advobot.Gacha.Interaction.InteractionType;

namespace Advobot.Gacha.Displays
{
	public abstract class PaginatedDisplay : Display
	{
		private int _PageIndex;
		public int ItemCount { get; }
		public int ItemsPerPage { get; }
		public int PageCount { get; }

		public int PageIndex
		{
			get => _PageIndex;
			protected set => _PageIndex = value;
		}

		protected virtual TimeSpan Timeout { get; } = TimeSpan.FromSeconds(30);

		protected PaginatedDisplay(
			IGachaDatabase db,
			ITime time,
			IInteractionManager interaction,
			int id,
			int itemCount,
			int itemsPerPage)
			: base(db, time, interaction, id)
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
			while (Time.UtcNow - LastInteractedWith > Timeout)
			{
				await Task.Delay(Timeout).CAF();
			}
		}
	}
}