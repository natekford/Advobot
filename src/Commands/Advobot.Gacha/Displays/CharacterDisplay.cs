using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Gacha.Interaction;
using Advobot.Gacha.Metadata;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;

namespace Advobot.Gacha.Displays
{
	/// <summary>
	/// Displays information and images about a character.
	/// </summary>
	public class CharacterDisplay : PaginatedDisplay
	{
		private readonly CharacterMetadata _Character;
		private readonly IReadOnlyClaim? _Claim;
		private readonly IDiscordClient _Client;
		private readonly IReadOnlyList<IReadOnlyImage> _Images;

		/// <summary>
		/// Creates an instance of <see cref="CharacterDisplay"/>.
		/// </summary>
		/// <param name="db"></param>
		/// <param name="time"></param>
		/// <param name="interaction"></param>
		/// <param name="client"></param>
		/// <param name="id"></param>
		/// <param name="character"></param>
		/// <param name="images"></param>
		/// <param name="claim"></param>
		public CharacterDisplay(
			IGachaDatabase db,
			ITime time,
			IInteractionManager interaction,
			IDiscordClient client,
			int id,
			CharacterMetadata character,
			IReadOnlyList<IReadOnlyImage> images,
			IReadOnlyClaim? claim)
			: base(db, time, interaction, id, images.Count, 1)
		{
			_Client = client;
			_Character = character;
			_Images = images;
			_Claim = claim;

			InteractionHandler.AddInteraction(InteractionType.Confirm);

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

		/// <inheritdoc />
		protected override async Task<Embed> GenerateEmbedAsync()
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
				Color = GachaConstants.Unclaimed,
				Footer = GeneratePaginationFooter(),
			};
			if (_Claim == null)
			{
				return embed.Build();
			}

			var owner = await _Client.GetUserAsync(_Claim.UserId).CAF();
			var ownerStr = owner?.ToString() ?? _Claim.UserId.ToString();

			embed.Color = GachaConstants.Claimed;
			embed.Footer.Text += $"| Belongs to {ownerStr}";
			embed.Footer.IconUrl = owner?.GetAvatarUrl();
			return embed.Build();
		}

		/// <inheritdoc />
		protected override Task<string> GenerateTextAsync()
			=> Task.FromResult("");

		/// <inheritdoc />
		protected override Task HandleInteractionAsync(IInteractionContext context)
		{
			if (context.Action is Confirmation c && c.Value && _Claim != null
				&& context.User.Id == _Claim.UserId)
			{
				var url = _Images[PageIndex].Url;
				return Database.UpdateClaimImageUrlAsync(_Claim, url);
			}
			return base.HandleInteractionAsync(context);
		}
	}
}