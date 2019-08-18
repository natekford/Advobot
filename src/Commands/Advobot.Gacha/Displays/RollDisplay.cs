﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Gacha.Counters;
using Advobot.Gacha.Interaction;
using Advobot.Gacha.Models;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

namespace Advobot.Gacha.Displays
{
	/// <summary>
	/// Displays a character which has been rolled for claiming.
	/// </summary>
	public class RollDisplay : Display
	{
		private readonly ICounter<ulong> _ClaimChecker;
		private readonly IReadOnlyCharacter _Character;
		private readonly IReadOnlySource _Source;
		private readonly IReadOnlyList<IReadOnlyWish> _Wishes;
		private readonly IReadOnlyList<IReadOnlyImage> _Images;
		private readonly TaskCompletionSource<object?> _Claimed = new TaskCompletionSource<object?>();

		public RollDisplay(
			IServiceProvider services,
			int id,
			ICounter<ulong> claimChecker,
			IReadOnlyCharacter character,
			IReadOnlySource source,
			IReadOnlyList<IReadOnlyWish> wishes,
			IReadOnlyList<IReadOnlyImage> images)
			: base(services, id)
		{
			_ClaimChecker = claimChecker;
			_Character = character;
			_Source = source;
			_Wishes = wishes;
			_Images = images;

			InteractionHandler.AddInteraction(InteractionType.Claim);
		}

		protected override async Task HandleInteractionAsync(IInteractionContext context)
		{
			if (context.Action is Confirmation c && c.Value
				&& !_Claimed.Task.IsCompleted
				&& _ClaimChecker.CanDo(context.User.Id))
			{
				_Claimed.SetResult(null);
				_ClaimChecker.HasBeenDone(context.User.Id);

				var user = await Database.GetUserAsync(context.Guild.Id, context.User.Id).CAF();
				var claim = new Claim(user, _Character);
				await Database.AddClaimAsync(claim).CAF();
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
				Description = _Source.Name,
				ImageUrl = _Images.First().Url,
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
				var mention = MentionUtils.MentionUser(wish.GetUserId()) + " ";
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