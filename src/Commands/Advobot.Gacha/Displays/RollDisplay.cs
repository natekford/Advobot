using Advobot.Classes.Modules;
using Advobot.Gacha.Database;
using Advobot.Gacha.MenuEmojis;
using Advobot.Gacha.Models;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Gacha.Displays
{
	public class RollDisplay : Display
	{
		protected override EmojiMenu? Menu { get; } = new EmojiMenu
		{
			new ConfirmationEmoji(Constants.Heart, true),
		};

		private readonly Character _Character;
		private readonly IReadOnlyList<Wish> _Wishes;
		private readonly TaskCompletionSource<object?> _Claimed = new TaskCompletionSource<object?>();

		private RollDisplay(
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
				_Claimed.SetResult(null);
				var user = await Database.GetUserAsync((IGuildUser)reaction.User.Value).CAF();
				await Database.AddAndSaveAsync(new Marriage(user, _Character)).CAF();
			}
		}
		protected override Task<Embed> GenerateEmbedAsync()
			=> Task.FromResult(GenerateEmbed());
		protected override Task<string> GenerateTextAsync()
			=> Task.FromResult(GenerateText());
		protected override Task KeepMenuAliveAsync()
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

			var orderedWishes = _Wishes.OrderBy(x => x.TimeWished);
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

		public static async Task<RollDisplay> CreateAsync(
			GachaDatabase db,
			AdvobotCommandContext context)
		{
			var character = await db.GetRandomCharacterAsync(context.User).CAF();
			var wishes = await db.GetWishesAsync(context.Guild, character.CharacterId).CAF();
			return new RollDisplay(context.Client, db, character, wishes);
		}
		public static async Task RunAsync(
			GachaDatabase db,
			AdvobotCommandContext context)
		{
			var display = await CreateAsync(db, context).CAF();
			await display.SendAsync(context.Channel).CAF();
		}
	}
}
