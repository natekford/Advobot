using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Gacha.Interaction;
using Advobot.Gacha.Metadata;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;
using AdvorangesUtils;
using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.Displays
{
	/// <summary>
	/// Displays information and images about a character.
	/// </summary>
	public class CharacterDisplay : PaginatedDisplay
	{
		private readonly IDiscordClient _Client;
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
			IServiceProvider services,
			int id,
			CharacterMetadata character,
			IReadOnlyList<IReadOnlyImage> images,
			IReadOnlyClaim? claim)
			: base(services, id, images.Count, 1)
		{
			_Client = services.GetRequiredService<IDiscordClient>();
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
		protected override Task HandleInteractionAsync(IInteractionContext context)
		{
			if (context.Action is Confirmation c && c.Value && _Claim != null
				&& context.User.Id == _Claim.GetUserId())
			{
				var url = _Images[PageIndex].Url;
				return Database.UpdateClaimImageUrlAsync(_Claim, url);
			}
			return base.HandleInteractionAsync(context);
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
				Color = Constants.Unclaimed,
				Footer = GeneratePaginationFooter(),
			};
			if (_Claim == null)
			{
				return embed.Build();
			}

			var owner = await _Client.GetUserAsync(_Claim.GetUserId()).CAF();
			var ownerStr = owner?.ToString() ?? _Claim.UserId.ToString();

			embed.Color = Constants.Claimed;
			embed.Footer.Text += $"| Belongs to {ownerStr}";
			embed.Footer.IconUrl = owner?.GetAvatarUrl();
			return embed.Build();
		}
		/// <inheritdoc />
		protected override Task<string> GenerateTextAsync()
			=> Task.FromResult("");
	}
}
