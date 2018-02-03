using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using ImageMagick;
using System;
using System.Linq;
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
		[Command(nameof(Copy), RunMode = RunMode.Async), ShortAlias(nameof(Copy))]
		public async Task Copy(Emote emote)
		{
			await Add(emote.Name, new Uri(emote.Url)).CAF();
		}
		//TODO: implement a queue for these commands, since they use high memory and download
		[Command(nameof(Add), RunMode = RunMode.Async), ShortAlias(nameof(Add))]
		public async Task Add(string name, Uri url)
		{
			var options = new ModerationReason(Context.User, null).CreateRequestOptions();
			var args = new ImageResizerArgs
			{
				MaxSize = 256000,
				ResizeTries = 5,
				AnimationDelay = 10,
				ColorFuzzingPercentage = new Percentage(30),
				FrameSkip = 3,
			};
			var resp = await url.UseImageStream(Context.Guild, args,
				async s => await Context.Guild.CreateEmoteAsync(name, new Image(s), default, options).CAF()).CAF();
			var text = resp == null
				? $"Successfully created the emote `{name}`."
				: $"Failed to create the emote `{name}`. Reason: " + resp;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, text);
		}
		[Command(nameof(Delete)), ShortAlias(nameof(Delete))]
		public async Task Delete(GuildEmote emote)
		{
			await Context.Guild.DeleteEmoteAsync(emote, new ModerationReason(Context.User, null).CreateRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted the emote `{emote.Name}`.");
		}
	}
}
