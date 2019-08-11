using System;
using System.Threading.Tasks;
using Advobot.Gacha.Database;
using Advobot.Gacha.MenuEmojis;
using Advobot.Modules;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Gacha.Displays
{
	public abstract class Display
	{
		protected IUserMessage? Message { get; set; }
		protected DateTime LastInteractedWith { get; set; }
		protected bool HasBeenSent { get; set; }

		protected BaseSocketClient Client { get; }
		protected GachaDatabase Database { get; }
		protected int Id { get; }
		protected abstract InteractiveMenu Menu { get; }

		public Display(BaseSocketClient client, GachaDatabase db, int id)
		{
			Client = client;
			Database = db;
			Id = id;
		}

		public virtual async Task<IResult> SendAsync(IMessageChannel channel)
		{
			if (HasBeenSent)
			{
				return AdvobotResult.Failure("Already sent from this instance.", CommandError.Exception);
			}

			var text = await GenerateTextAsync().CAF();
			var embed = await GenerateEmbedAsync().CAF();
			Message = await channel.SendMessageAsync(text, embed: embed);

			Task Handle(SocketMessage message)
			{
				if (!(message is IUserMessage msg)
					|| !TryGetMenuAction(msg, out var action)
					|| action == null)
				{
					return Task.CompletedTask;
				}

				LastInteractedWith = DateTime.UtcNow;
				return HandleActionAsync(new ActionContext(msg, action));
			}

			Client.MessageReceived += Handle;
			await KeepDisplayAliveAsync().CAF();
			Client.MessageReceived -= Handle;
			await DisposeMenuAsync().CAF();
			return AdvobotResult.IgnoreSuccess;
		}
		protected abstract Task HandleActionAsync(ActionContext context);
		protected abstract Task KeepDisplayAliveAsync();
		protected virtual Task DisposeMenuAsync()
			=> Task.CompletedTask;
		protected abstract Task<Embed> GenerateEmbedAsync();
		protected abstract Task<string> GenerateTextAsync();
		protected EmbedFooterBuilder GenerateDefaultFooter()
		{
			return new EmbedFooterBuilder
			{
				Text = $"Id: {Id}",
			};
		}
		protected bool TryGetMenuAction(IUserMessage message, out IMenuAction? action)
		{
			action = null;
			var argPos = -1;
			return Menu != null
				&& message.HasStringPrefix(Id.ToString(), ref argPos)
				&& Menu.TryGet(message.Content.Substring(argPos), out action);
		}

		protected sealed class ActionContext
		{
			public IUserMessage Message { get; }
			public IGuildUser User { get; }
			public ITextChannel Channel { get; }
			public IGuild Guild { get; }
			public IMenuAction Action { get; }

			public ActionContext(IMessage message, IMenuAction action)
			{
				Message = (IUserMessage)message;
				User = (IGuildUser)message.Author;
				Channel = (ITextChannel)message.Channel;
				Guild = Channel.Guild;
				Action = action;
			}
		}
	}
}
