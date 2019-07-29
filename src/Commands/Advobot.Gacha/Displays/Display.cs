using Advobot.Gacha.Database;
using Advobot.Gacha.MenuEmojis;
using Advobot.Gacha.Utilities;
using Advobot.Modules;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Cached = Discord.Cacheable<Discord.IUserMessage, ulong>;

namespace Advobot.Gacha.Displays
{
	public abstract class Display
	{
		protected IUserMessage? Message { get; set; }
		protected DateTime LastInteractedWith { get; set; }
		protected bool HasBeenSent { get; set; }

		protected BaseSocketClient Client { get; }
		protected GachaDatabase Database { get; }
		protected abstract EmojiMenu Menu { get; }

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
			if (Menu?.Count > 0 && !await message.SafeAddReactionsAsync(Menu.Values).CAF())
			{
				return AdvobotResult.Failure("Unable to add reactions.", CommandError.Exception);
			}

			Task Handle(Cached cached, ISocketMessageChannel _, SocketReaction reaction)
			{
				if (!TryGetMenuEmote(cached, reaction, out var emoji) || emoji == null)
				{
					return Task.CompletedTask;
				}

				LastInteractedWith = DateTime.UtcNow;
				return HandleReactionsAsync(cached.Value, reaction, emoji);
			}

			Message = message;
			Client.ReactionAdded += Handle;
			Client.ReactionRemoved += Handle;
			await KeepDisplayAliveAsync().CAF();
			Client.ReactionAdded -= Handle;
			Client.ReactionRemoved -= Handle;
			await DisposeMenuAsync().CAF();
			return AdvobotResult.IgnoreFailure;
		}
		protected abstract Task HandleReactionsAsync(
			IUserMessage message,
			SocketReaction reaction,
			IMenuEmote emoji);
		protected abstract Task KeepDisplayAliveAsync();
		protected virtual Task DisposeMenuAsync()
			=> Message?.RemoveAllReactionsAsync() ?? Task.CompletedTask;
		protected abstract Task<Embed> GenerateEmbedAsync();
		protected abstract Task<string> GenerateTextAsync();
		protected bool TryGetMenuEmote(Cached cached, SocketReaction reaction, out IMenuEmote? emoji)
		{
			emoji = null;
			return cached.Id == Message?.Id && (Menu == null || Menu.TryGet(reaction.Emote, out emoji));
		}
	}
}
