using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Gacha.Database;
using Advobot.Gacha.MenuEmojis;
using Advobot.Gacha.Trading;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Gacha.Displays
{
	internal class ConfirmationDisplay
	{
	}

	public class GiveDisplay : PaginatedDisplay
	{
		private readonly IGuildUser _Giver;
		private readonly IGuildUser _Receiver;
		private readonly IReadOnlyList<ITrade> _Trades;

		public GiveDisplay(
			BaseSocketClient client,
			GachaDatabase db,
			int id,
			IGuildUser giver,
			IGuildUser receiver,
			IReadOnlyList<ITrade> trades)
			: base(client, db, id, trades.Count, Constants.CharactersPerPage)
		{
			_Giver = giver;
			_Receiver = receiver;
			_Trades = trades;

			Menu.Add(new Confirmation(Constants.Confirm, true));
			Menu.Add(new Confirmation(Constants.Deny, false));
		}

		protected override Task<Embed> GenerateEmbedAsync()
			=> Task.FromResult(GenerateEmbed());
		protected override Task<string> GenerateTextAsync()
			=> Task.FromResult(GenerateText());
		private Embed GenerateEmbed()
		{
			var values = GetPageValues(_Trades);
			var description = values.Select(x => x.Character.Name).Join("\n");

			return new EmbedBuilder
			{
				Description = description,
				Author = new EmbedAuthorBuilder
				{
					Name = $"{_Giver.Username} giving {_Receiver.Username}",
					IconUrl = _Giver.GetAvatarUrl(),
				},
				Footer = GeneratePaginationFooter(),
			}.Build();
		}
		private string GenerateText()
			=> $"{_Giver.Mention} giving {_Trades.Count} characters to {_Receiver.Mention}";
	}
}
