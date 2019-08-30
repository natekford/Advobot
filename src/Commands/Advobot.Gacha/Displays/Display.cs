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
		protected Display(IServiceProvider services, int id)
		{
			Database = services.GetRequiredService<GachaDatabase>();

			var interactionFactory = services.GetRequiredService<IInteractionManager>();
			InteractionHandler = interactionFactory.CreateInteractionHandler(this);
			Id = id;
		}

		public GachaDatabase Database { get; }
		public bool HasBeenSent { get; protected set; }
		public int Id { get; }
		public DateTime LastInteractedWith { get; protected set; }
		public IUserMessage? Message { get; protected set; }
		protected IInteractionHandler InteractionHandler { get; }

		public virtual Task InteractAsync(IInteractionContext context)
		{
			LastInteractedWith = DateTime.UtcNow;
			return HandleInteractionAsync(context);
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
				Message = await channel.SendMessageAsync(text, embed: embed).CAF();

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

		protected EmbedFooterBuilder GenerateDefaultFooter()
		{
			return new EmbedFooterBuilder
			{
				Text = $"Id: {Id}",
			};
		}

		protected abstract Task<Embed> GenerateEmbedAsync();

		protected abstract Task<string> GenerateTextAsync();

		protected abstract Task HandleInteractionAsync(IInteractionContext context);

		protected abstract Task KeepDisplayAliveAsync();
	}
}