using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Gacha.Database;
using Advobot.Gacha.MenuEmojis;
using Advobot.Gacha.Metadata;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;
using Discord;
using Discord.WebSocket;

namespace Advobot.Gacha.Displays
{
	/// <summary>
	/// Displays information and images about a character.
	/// </summary>
	public class CharacterDisplay : PaginatedDisplay
	{
		private readonly CharacterMetadata _Character;
		private readonly IReadOnlyList<IReadOnlyImage> _Images;
		private readonly IReadOnlyClaim? _Claim;

		/// <summary>
		/// Creates an instance of <see cref="CharacterDisplay"/>.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="db"></param>
		/// <param name="character"></param>
		/// <param name="images"></param>
		/// <param name="claim"></param>
		public CharacterDisplay(
			BaseSocketClient client,
			GachaDatabase db,
			int id,
			CharacterMetadata character,
			IReadOnlyList<IReadOnlyImage> images,
			IReadOnlyClaim? claim)
			: base(client, db, id, images.Count, 1)
		{
			_Character = character;
			_Images = images;
			_Claim = claim;

			Menu.Add(new Confirmation(Constants.Confirm, true));

			if (claim?.ImageUrl is string url)
			{
				for (var i = 0; i < _Images.Count; ++i)
				{
					if (_Images[i].Url == url)
					{
						PageIndex = i;
						break;
					}
				}
			}
		}

		protected override Task HandleActionAsync(ActionContext context)
		{
			if (context.Action is Confirmation c && c.Value && _Claim != null
				&& context.User.Id == _Claim.GetUserId())
			{
				var url = _Images[PageIndex].Url;
				return Database.UpdateClaimImageUrlAsync(_Claim, url);
			}
			return base.HandleActionAsync(context);
		}
		protected override Task<Embed> GenerateEmbedAsync()
			=> Task.FromResult(GenerateEmbed());
		protected override Task<string> GenerateTextAsync()
			=> Task.FromResult("");
		private Embed GenerateEmbed()
		{
			var description = $"{_Character.Source.Name} {_Character.Data.GenderIcon}\n" +
				$"{_Character.Data.RollType}\n" +
				$"{_Character.Claims}\n" +
				$"{_Character.Likes}\n" +
				$"{_Character.Wishes}\n" +
				$"{_Character.Data.FlavorText}";

			var embed = new EmbedBuilder
			{
				Title = _Character.Data.Name,
				Description = description,
				ImageUrl = _Images[PageIndex].Url,
				Color = Constants.Unclaimed,
				Footer = GeneratePaginationFooter(),
			};
			if (_Claim == null)
			{
				return embed.Build();
			}

			var owner = Client.GetUser(_Claim.GetUserId());
			var ownerStr = owner?.ToString() ?? _Claim.UserId.ToString();

			embed.Color = Constants.Claimed;
			embed.Footer.Text += $"| Belongs to {ownerStr}";
			embed.Footer.IconUrl = owner?.GetAvatarUrl();
			return embed.Build();
		}
	}
}
