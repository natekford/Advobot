using Advobot.Classes.Results;
using Advobot.Gacha.Database;
using Advobot.Gacha.MenuEmojis;
using Advobot.Gacha.Utils;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Advobot.Gacha.Displays
{
	public abstract class Display
	{
		protected IUserMessage? Message { get; set; }
		protected DateTime LastInteractedWith { get; set; }
		protected bool HasBeenSent { get; set; }

		protected BaseSocketClient Client { get; }
		protected GachaDatabase Database { get; }
		protected abstract EmojiMenu? Menu { get; }
		protected abstract bool ShouldUpdateLastInteractedWith { get; }

		public Display(BaseSocketClient client, GachaDatabase db)
		{
			Client = client;
			Database = db;
		}

		public virtual async Task<IResult> SendAsync(IMessageChannel channel)
		{
			if (HasBeenSent)
			{
				return AdvobotResult.Failure("Already sent from this instance.", CommandError.Exception);
			}

			var text = await GenerateTextAsync().CAF();
			var embed = await GenerateEmbedAsync().CAF();
			var message = await channel.SendMessageAsync(text, embed: embed);
			if (Menu != null && !await message.SafeAddReactionsAsync(Menu.Values).CAF())
			{
				return AdvobotResult.Failure("Unable to add reactions.", CommandError.Exception);
			}

			Message = message;
			Client.ReactionAdded += HandleReactionsAsync;
			if (ShouldUpdateLastInteractedWith)
			{
				Client.ReactionAdded += UpdateLastInteractedWith;
			}
			await KeepMenuAlive().CAF();
			Client.ReactionAdded -= HandleReactionsAsync;
			if (ShouldUpdateLastInteractedWith)
			{
				Client.ReactionAdded -= UpdateLastInteractedWith;
			}
			return AdvobotResult.Ignore;
		}
		protected abstract Task HandleReactionsAsync(
			Cacheable<IUserMessage, ulong> cached,
			ISocketMessageChannel channel,
			SocketReaction reaction);
		protected abstract Task KeepMenuAlive();
		protected abstract Task<Embed> GenerateEmbedAsync();
		protected abstract Task<string> GenerateTextAsync();
		protected bool TryGetMenuEmoji(Cacheable<IUserMessage, ulong> cached, SocketReaction reaction, out Emoji? emoji)
		{
			emoji = null;
			return cached.Id == Message?.Id && (Menu == null || Menu.TryGet(reaction.Emote, out emoji));
		}
		private Task UpdateLastInteractedWith(
			Cacheable<IUserMessage, ulong> cached,
			ISocketMessageChannel channel,
			SocketReaction reaction)
		{
			if (TryGetMenuEmoji(cached, reaction, out _))
			{
				LastInteractedWith = DateTime.UtcNow;
			}
			return Task.CompletedTask;
		}
	}
}
