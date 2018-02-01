using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.Emotes
{
	[Group(nameof(ModifyEmotes)), TopLevelShortAlias(typeof(ModifyEmotes))]
	[Summary("Adds or removes an emote. " +
		"Adding requires an image url.")]
	[PermissionRequirement(new[] { GuildPermission.ManageEmojis }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyEmotes : NonSavingModuleBase
    {
		[Command(nameof(Add), RunMode = RunMode.Async), ShortAlias(nameof(Add))]
		public async Task Add(string name, [Optional, Remainder] string url)
		{
			//TODO: custom typereader for image urls
			//TODO: also get it so it can get the url from an attachment without giving 403 error because too slow
			//TODO: remove emotes and gif emotes.
			if (!ImageUtils.TryGetUri(Context.Message, url, out var imageUrl, out var error))
			{
				if (error != null)
				{
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				}
				else
				{
					await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image()).CAF();
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the bot's icon.").CAF();
				}
				return;
			}

			var options = new ModerationReason(Context.User, null).CreateRequestOptions();
			var resp = await imageUrl.UseImageStream(256000, true, async s =>
			{
				await Context.Guild.CreateEmoteAsync(name, new Image(s)).CAF();
			}).CAF();
			var text = resp == null ? "Successfully updated the bot icon" : "Failed to update the bot icon. Reason: " + resp;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, text);
		}
	}
}
