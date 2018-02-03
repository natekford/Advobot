using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using ImageMagick;
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
			//TODO: implement a queue for these commands, since they use high memory and download
			var options = new ModerationReason(Context.User, null).CreateRequestOptions();
			var args = new ImageResizerArgs
			{
				MaxSize = 256000,
				ResizeTries = 5,
				AnimationDelay = 10,
				ColorFuzzingPercentage = new Percentage(30),
				FrameSkip = 3,
			};
			var resp = await url.UseImageStream(args, async s => await Context.Guild.CreateEmoteAsync(name, new Image(s)).CAF()).CAF();
			var text = resp == null ? "Successfully created the emote." : "Failed to create the emote. Reason: " + resp;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, text);
		}
	}
}
