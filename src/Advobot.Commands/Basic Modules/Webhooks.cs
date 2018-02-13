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
	public sealed class ModifyWebhookIcon : NonSavingModuleBase
	{
		private static ImageResizer<IconResizerArguments> _Resizer = new ImageResizer<IconResizerArguments>(4, "webhook icon", async (c, s, f, n, o) =>
		{
			if (!(await c.Guild.GetWebhookAsync(Convert.ToUInt64(n)).CAF() is IWebhook webhook))
			{
				return new Error("Unable to find the webhook to update.");
			}
			await webhook.ModifyAsync(x => x.Image = new Image(s), o).CAF();
			return null;
		});

		[Command]
		public async Task Command(IWebhook webhook, Uri url)
		{
			if (_Resizer.IsGuildAlreadyProcessing(Context.Guild))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on a webhook icon.")).CAF();
				return;
			}

			_Resizer.EnqueueArguments(Context, new IconResizerArguments(), url, GetRequestOptions(), webhook.Id.ToString());
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Position in webhook icon creation queue: {_Resizer.QueueCount}.").CAF();
			if (_Resizer.CanStart)
			{
				_Resizer.StartProcessing();
			}
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(IWebhook webhook)
		{
			if (_Resizer.IsGuildAlreadyProcessing(Context.Guild))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on a webhook icon.")).CAF();
				return;
			}

			await webhook.ModifyAsync(x => x.Image = new Image(), GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the webhook icon from {webhook.Format()}.").CAF();
		}
	}
}
