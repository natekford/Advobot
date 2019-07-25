using Advobot.Gacha.Database;
using Advobot.Gacha.MenuEmojis;
using Advobot.Gacha.Models;
using Advobot.Gacha.Utils;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Gacha.Displays
{
	/// <summary>
	/// Displays a character which has been rolled for claiming.
	/// </summary>
	public class RollDisplay : Display
	{
		protected override EmojiMenu Menu { get; } = new EmojiMenu
		{
			new ConfirmationEmoji(Constants.Heart, true),
		};

		private readonly Character _Character;
		private readonly IReadOnlyList<Wish> _Wishes;
		private readonly TaskCompletionSource<object?> _Claimed = new TaskCompletionSource<object?>();

		public RollDisplay(
			BaseSocketClient client,
			GachaDatabase db,
			Character character,
			IReadOnlyList<Wish> wishes) : base(client, db)
		{
			_Character = character;
			_Wishes = wishes;
		}

		protected override async Task HandleReactionsAsync(
			IUserMessage message,
			SocketReaction reaction,
			IMenuEmote emoji)
		{
			if (emoji is ConfirmationEmoji c && c.Value)
			{
				var user = await Database.GetUserAsync((IGuildUser)reaction.User.Value).CAF();
				//TODO: verify the user can claim

				_Claimed.SetResult(null);
				await Database.AddAndSaveAsync(new Marriage
				{
					User = user,
					Character = _Character,
				}).CAF();
			}
		}
		protected override Task<Embed> GenerateEmbedAsync()
			=> Task.FromResult(GenerateEmbed());
		protected override Task<string> GenerateTextAsync()
			=> Task.FromResult(GenerateText());
		protected override Task KeepDisplayAliveAsync()
		{
			var trigger = _Claimed.Task;
			var delay = Task.Delay(Constants.ClaimLength);
			return Task.WhenAny(trigger, delay);
		}
		private Embed GenerateEmbed()
		{
			return new EmbedBuilder
			{
				Title = _Character.Name,
				Description = _Character.Source.Name,
				ImageUrl = _Character.Images.First().Url,
				Color = _Wishes.Count > 0 ? Constants.Wished : Constants.Unclaimed,
			}.Build();
		}
		private string GenerateText()
		{
			if (_Wishes.Count == 0)
			{
				return "";
			}

			var orderedWishes = _Wishes.OrderBy(x => x.GetTimeCreated());
			var sb = new StringBuilder("Wished by ");
			foreach (var wish in _Wishes)
			{
				var mention = MentionUtils.MentionUser(wish.User.UserId) + " ";
				if (sb.Length + mention.Length > DiscordConfig.MaxMessageSize)
				{
					break;
				}
				sb.Append(mention);
			}
			return sb.ToString();
		}
	}
}
