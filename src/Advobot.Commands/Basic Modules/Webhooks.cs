using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Advobot.Commands.Webhooks
{
	[Group(nameof(DeleteWebhook)), TopLevelShortAlias(typeof(DeleteWebhook))]
	[Summary("Deletes a webhook from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageWebhooks }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteWebhook : NonSavingModuleBase
	{
		[Command]
		public async Task Command(IWebhook webhook)
		{
			await webhook.DeleteAsync(GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted the webhook `{webhook.Format()}`.").CAF();
		}
	}

	[Group(nameof(ModifyWebhookName)), TopLevelShortAlias(typeof(ModifyWebhookName))]
	[Summary("Changes the name of a webhook.")]
	[PermissionRequirement(new[] { GuildPermission.ManageWebhooks }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyWebhookName : NonSavingModuleBase
	{
		[Command]
		public async Task Command(IWebhook webhook, [Remainder, VerifyStringLength(Target.Name)] string name)
		{
			await webhook.ModifyAsync(x => x.Name = name, GetRequestOptions()).CAF();
			var resp = $"Successfully changed the name of `{webhook.Format()}` to `{name}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyWebhookChannel)), TopLevelShortAlias(typeof(ModifyWebhookChannel))]
	[Summary("Changes the channel of a webhook.")]
	[PermissionRequirement(new[] { GuildPermission.ManageWebhooks }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyWebhookChannel : NonSavingModuleBase
	{
		[Command]
		public async Task Command(IWebhook webhook, [VerifyObject(true, ObjectVerification.CanManageWebhooks)] ITextChannel channel)
		{
			await webhook.ModifyAsync(x => x.Channel = Optional.Create(channel), GetRequestOptions()).CAF();
			var resp = $"Successfully set the channel of `{webhook.Format()}` to `{channel.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyWebhookIcon)), TopLevelShortAlias(typeof(ModifyWebhookIcon))]
	[Summary("Changes the icon of a webhook.")]
	[PermissionRequirement(new[] { GuildPermission.ManageWebhooks }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyWebhookIcon : ImageCreationModuleBase<IconResizerArgs>
	{
		private static ConcurrentDictionary<ulong, IWebhook> _Webhooks = new ConcurrentDictionary<ulong, IWebhook>();

		[Command]
		public async Task Command(IWebhook webhook, Uri url)
		{
			if (GuildAlreadyProcessing)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on a webhook icon.")).CAF();
				return;
			}

			_Webhooks.AddOrUpdate(Context.Guild.Id, webhook, (k, v) => webhook);
			EnqueueArguments(new ImageCreationArguments<IconResizerArgs>
			{
				Uri = url,
				Name = null,
				Args = new IconResizerArgs(),
				Context = Context,
				Options = GetRequestOptions(),
			});
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Position in webhook icon creation queue: {QueueCount}.").CAF();
			if (CanStart)
			{
				StartProcessing();
			}
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(IWebhook webhook)
		{
			if (GuildAlreadyProcessing)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on a webhook icon.")).CAF();
				return;
			}

			await webhook.ModifyAsync(x => x.Image = new Image(), GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the webhook icon from {webhook.Format()}.").CAF();
		}

		protected override Task Create(ImageCreationArguments<IconResizerArgs> args)
		{
			return PrivateCreate(args);
		}
		private static async Task PrivateCreate(ImageCreationArguments<IconResizerArgs> args)
		{
			using (var resp = await ImageUtils.ResizeImageAsync(args.Uri, args.Context, args.Args).CAF())
			{
				if (!_Webhooks.TryGetValue(args.Context.Guild.Id, out var webhook))
				{
					await MessageUtils.SendErrorMessageAsync(args.Context, new Error("Unable to modify the webhook."));
					return;
				}
				if (resp.IsSuccess)
				{
					await webhook.ModifyAsync(x => x.Image = new Image(resp.Stream), args.Options).CAF();
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(args.Context, $"Successfully updated the webhook icon of `{webhook.Format()}`.").CAF();
					return;
				}
				await MessageUtils.SendErrorMessageAsync(args.Context, new Error($"Failed to update the webhook icon of `{webhook.Format()}`. Reason: {resp.Error}.")).CAF();
			}
		}
	}
}
