using Advobot.Classes.Modules;
using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Gacha.Utils;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Gacha.Displays
{
	public static class GachaRoll
	{
		public static async Task CreateGachaRollAsync(
			this GachaDatabase db,
			AdvobotCommandContext context)
		{
			var character = await db.GetRandomCharacterAsync(context.User).CAF();
			var wishes = await db.GetWishesAsync(context.Guild, character.CharacterId).CAF();

			var embed = MakeEmbed(character, wishes);
			var text = MakeText(wishes);

			var message = await context.Channel.SendMessageAsync(text, embed: embed);
			if (!await message.SafeAddReactionsAsync(Constants.Heart).CAF())
			{
				return;
			}

			var eventTrigger = new TaskCompletionSource<Marriage>();
			async Task Handler(Cacheable<IUserMessage, ulong> cached, ISocketMessageChannel channel, SocketReaction reaction)
			{
				if (reaction.Emote != Constants.Heart || cached.Id != message.Id)
				{
					return;
				}

				var user = await db.GetUserAsync((IGuildUser)reaction.User.Value).CAF();
				eventTrigger.SetResult(new Marriage(user, character));
			}

			context.Client.ReactionAdded += Handler;
			var trigger = eventTrigger.Task;
			var delay = Task.Delay(Constants.ClaimLength);
			var task = await Task.WhenAny(trigger, delay).CAF();
			context.Client.ReactionAdded -= Handler;

			await message.RemoveAllReactionsAsync().CAF();
			if (task == trigger)
			{
				var marriage = await trigger.CAF();
				await db.AddAndSaveAsync(marriage).CAF();
			}
		}
		private static Embed MakeEmbed(Character character, IReadOnlyList<Wish> wishes)
		{
			return new EmbedBuilder
			{
				Title = character.Name,
				Description = character.Source.Name,
				ImageUrl = character.Images.First().Url,
				Color = wishes.Count > 0 ? Constants.Wished : Constants.Unclaimed,
			}.Build();
		}
		private static string MakeText(IReadOnlyList<Wish> wishes)
		{
			if (wishes.Count == 0)
			{
				return "";
			}

			var orderedWishes = wishes.OrderBy(x => x.TimeWished);
			var sb = new StringBuilder("Wished by ");
			foreach (var wish in wishes)
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
