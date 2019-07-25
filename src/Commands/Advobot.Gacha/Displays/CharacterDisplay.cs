using Advobot.Gacha.Database;
using Advobot.Gacha.MenuEmojis;
using Advobot.Gacha.Metadata;
using Advobot.Gacha.Models;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Advobot.Gacha.Displays
{
	/// <summary>
	/// Displays information and images about a character.
	/// </summary>
	public class CharacterDisplay : PaginatedDisplay
	{
		private readonly CharacterMetadata _Character;
		private readonly Claim? _Marriage;

		/// <summary>
		/// Creates an instance of <see cref="CharacterDisplay"/>.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="db"></param>
		/// <param name="character"></param>
		/// <param name="marriage"></param>
		public CharacterDisplay(
			BaseSocketClient client,
			GachaDatabase db,
			CharacterMetadata character,
			Claim? marriage) : base(client, db, character.Data.Images.Count, 1)
		{
			_Character = character;
			_Marriage = marriage;

			Menu.Add(new ConfirmationEmoji(Constants.Confirm, true));

			if (marriage?.ImageUrl is string url)
			{
				for (var i = 0; i < _Character.Data.Images.Count; ++i)
				{
					if (_Character.Data.Images[i].Url == url)
					{
						PageIndex = i;
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
			if (emoji is ConfirmationEmoji c && c.Value && _Marriage != null
				&& reaction.UserId.ToString() == _Marriage.User.UserId)
			{
				var url = _Character.Data.Images[PageIndex].Url;
				return Database.UpdateClaimImageUrlAsync(_Marriage, url);
			}
			return base.HandleReactionsAsync(message, reaction, emoji);
		}
		protected override Task<Embed> GenerateEmbedAsync()
			=> Task.FromResult(GenerateEmbed());
		protected override Task<string> GenerateTextAsync()
			=> Task.FromResult("");
		private Embed GenerateEmbed()
		{
			var description = $"{_Character.Data.Source.Name} {_Character.Data.GenderIcon}\n" +
				$"{_Character.Data.RollType}\n" +
				$"{_Character.Claims}\n" +
				$"{_Character.Likes}\n" +
				$"{_Character.Wishes}\n" +
				$"{_Character.Data.FlavorText}";

			var embed = new EmbedBuilder
			{
				Title = _Character.Data.Name,
				Description = description,
				ImageUrl = _Character.Data.Images[PageIndex].Url,
				Color = Constants.Unclaimed,
				Footer = GeneratePaginationFooter(),
			};
			if (_Marriage == null)
			{
				return embed.Build();
			}

			var owner = Client.GetUser(ulong.Parse(_Marriage.User.UserId));
			var ownerStr = owner?.ToString() ?? _Marriage.User.UserId.ToString();

			embed.Color = Constants.Claimed;
			embed.Footer.Text = $"Belongs to {ownerStr} -- {embed.Footer.Text}";
			embed.Footer.IconUrl = owner?.GetAvatarUrl();
			return embed.Build();
		}
	}
}
