using System;
using System.Threading.Tasks;
using Advobot.Gacha.Database;
using Advobot.Gacha.Interaction;
using Advobot.Modules;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha.Displays
{
	public abstract class Display
	{
		public IUserMessage? Message { get; protected set; }
		public DateTime LastInteractedWith { get; protected set; }
		public bool HasBeenSent { get; protected set; }

		public GachaDatabase Database { get; }
		public int Id { get; }

		protected IInteractionHandler InteractionHandler { get; }

		public Display(IServiceProvider services, int id)
		{
			Database = services.GetRequiredService<GachaDatabase>();

			var interactionFactory = services.GetRequiredService<IInteractionManager>();
			InteractionHandler = interactionFactory.CreateInteractionHandler(this);
			Id = id;
		}

		public virtual async Task<RuntimeResult> SendAsync(IMessageChannel channel)
		{
			if (HasBeenSent)
			{
				return AdvobotResult.Failure("Already sent from this instance.", CommandError.Exception);
			}

			try
			{
				var text = await GenerateTextAsync().CAF();
				var embed = await GenerateEmbedAsync().CAF();
				Message = await channel.SendMessageAsync(text, embed: embed);

				await InteractionHandler.StartAsync().CAF();
				await KeepDisplayAliveAsync().CAF();
				await InteractionHandler.StopAsync().CAF();
				return AdvobotResult.IgnoreSuccess;
			}
			catch (Exception e)
			{
				return AdvobotResult.Exception(e);
			}
		}
		public virtual Task InteractAsync(IInteractionContext context)
		{
			LastInteractedWith = DateTime.UtcNow;
			return HandleInteractionAsync(context);
		}
		protected abstract Task HandleInteractionAsync(IInteractionContext context);
		protected abstract Task KeepDisplayAliveAsync();
		protected abstract Task<Embed> GenerateEmbedAsync();
		protected abstract Task<string> GenerateTextAsync();
		protected EmbedFooterBuilder GenerateDefaultFooter()
		{
			return new EmbedFooterBuilder
			{
				Text = $"Id: {Id}",
			};
		}
	}
}
