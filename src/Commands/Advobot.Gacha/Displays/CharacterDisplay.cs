using Advobot.Classes.Modules;
using Advobot.Gacha.Database;
using Advobot.Gacha.MenuEmojis;
using Advobot.Gacha.Models;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Advobot.Gacha.Displays
{
	public class CharacterDisplay : Display
	{
		public int PageIndex => _PageIndex;

		protected override EmojiMenu? Menu { get; } = new EmojiMenu
		{
			new MovementEmoji(Constants.Right, 1),
			new MovementEmoji(Constants.DoubleRight, 2),
			new MovementEmoji(Constants.Left, -1),
			new MovementEmoji(Constants.DoubleLeft, -2),
			new ConfirmationEmoji(Constants.Confirm, true),
		};

		private readonly Character _Character;
		private readonly Marriage? _Marriage;
		private int _PageIndex;

		/// <summary>
		/// Creates an instance of <see cref="CharacterDisplay"/>.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="db"></param>
		/// <param name="character"></param>
		/// <param name="marriage"></param>
		private CharacterDisplay(
			BaseSocketClient client,
			GachaDatabase db,
			Character character,
			Marriage? marriage) : base(client, db)
		{
			_Character = character;
			_Marriage = marriage;
			_PageIndex = 0;

			if (marriage?.Image?.Url is string url)
			{
				for (var i = 0; i < _Character.Images.Count; ++i)
				{
					if (_Character.Images[i].Url == url)
					{
						_PageIndex = i;
						break;
					}
				}
			}
		}

		protected override Task HandleReactionsAsync(
			IUserMessage message,
			SocketReaction reaction,
			IMenuEmote emoji)
		{
			if (emoji is MovementEmoji m && m.TryUpdatePage(ref _PageIndex, _Character.Images.Count))
			{
				return Message?.ModifyAsync(x => x.Embed = GenerateEmbed()) ?? Task.CompletedTask;
			}
			else if (emoji is ConfirmationEmoji c && c.Value && _Marriage != null
				&& reaction.UserId == _Marriage.User.UserId)
			{
				return Database.UpdateAsync(_Marriage, x => x.Image, _Character.Images[_PageIndex]);
			}
			return Task.CompletedTask;
		}
		protected override Task<Embed> GenerateEmbedAsync()
			=> Task.FromResult(GenerateEmbed());
		protected override Task<string> GenerateTextAsync()
			=> Task.FromResult("");
		protected override async Task KeepMenuAliveAsync()
		{
			//TODO: see if this works as intended
			//Intention = keep menu alive unless last interacted with over 30 seconds ago
			var timeout = TimeSpan.FromSeconds(30);
			while (DateTime.UtcNow - LastInteractedWith > timeout)
			{
				await Task.Delay(timeout).CAF();
			}
		}
		private Embed GenerateEmbed()
		{
			var description = $"{_Character.Source} {_Character.GenderIcon}\n" +
				$"{_Character.RollType.ToString()}\n" +
				$"Claims: {_Character.Claims} (#-1)\n" +
				$"Likes: {_Character.Likes} (#-1)\n" +
				$"{_Character.FlavorText}";

			var embed = new EmbedBuilder
			{
				Title = _Character.Name,
				Description = description,
				ImageUrl = _Character.Images[_PageIndex].Url,
				Color = Constants.Unclaimed,
				Footer = new EmbedFooterBuilder
				{
					Text = $"{_PageIndex + 1}/{_Character.Images.Count}",
				},
			};
			if (_Marriage == null)
			{
				return embed.Build();
			}

			var owner = Client.GetUser(_Marriage.User.UserId);
			var ownerStr = owner?.ToString() ?? _Marriage.User.UserId.ToString();

			embed.Color = Constants.Claimed;
			embed.Footer.Text = $"Belongs to {ownerStr} -- {embed.Footer.Text}";
			embed.Footer.IconUrl = owner?.GetAvatarUrl();
			return embed.Build();
		}

		public static async Task<CharacterDisplay> CreateAsync(
			GachaDatabase db,
			AdvobotCommandContext context,
			Character character)
		{
			var marriage = await db.GetMarriageAsync(context.Guild, character.CharacterId).CAF();
			return new CharacterDisplay(context.Client, db, character, marriage);
		}
		public static async Task RunAsync(
			GachaDatabase db,
			AdvobotCommandContext context,
			Character character)
		{
			var display = await CreateAsync(db, context, character).CAF();
			await display.SendAsync(context.Channel).CAF();
		}
	}
}
