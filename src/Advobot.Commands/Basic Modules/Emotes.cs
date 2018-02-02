using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using System;
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
		public async Task Add(string name, Uri url)
		{
			//TODO: remove emotes and gif emotes.
			var options = new ModerationReason(Context.User, null).CreateRequestOptions();
			var resp = await url.UseImageStream(256000, true, async s =>
			{
				await Context.Guild.CreateEmoteAsync(name, new Image(s)).CAF();
			}).CAF();
			var text = resp == null ? "Successfully created the emote." : "Failed to create the emote. Reason: " + resp;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, text);
		}
	}
}
