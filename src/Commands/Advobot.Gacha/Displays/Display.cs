using System;
using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Gacha.Interaction;
using Advobot.Modules;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Gacha.Displays
{
	public abstract class Display
	{
		public bool HasBeenSent { get; protected set; }
		public int Id { get; }
		public DateTimeOffset LastInteractedWith { get; protected set; }
		public IUserMessage? Message { get; protected set; }
		protected IGachaDatabase Database { get; }
		protected IInteractionHandler InteractionHandler { get; }
		protected ITime Time { get; }

		protected Display(
			IGachaDatabase db,
			ITime time,
			IInteractionManager interaction,
			int id)
		{
			Database = db;
			Time = time;
			InteractionHandler = interaction.CreateInteractionHandler(this);
			Id = id;
		}

		public virtual Task InteractAsync(IInteractionContext context)
		{
			LastInteractedWith = Time.UtcNow;
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
				Message = await channel.SendMessageAsync(text, embed: embed, allowedMentions: new AllowedMentions()).CAF();

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