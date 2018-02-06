using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.NamedArguments;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using ImageMagick;
using System;
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
		[Command(nameof(Copy), RunMode = RunMode.Async), ShortAlias(nameof(Copy))]
		public async Task Copy(Emote emote)
		{
			await Add(emote.Name, new Uri(emote.Url)).CAF();
		}
		//TODO: implement a queue for these commands, since they use high memory and download
		[Command(nameof(Add), RunMode = RunMode.Async), ShortAlias(nameof(Add))]
		public async Task Add(string name, Uri url, [Optional, Remainder] NamedArguments<EmoteResizerArgs> args)
		{
			EmoteResizerArgs obj;
			if (args == null)
			{
				obj = EmoteResizerArgs.Default;
			}
			else if (!args.TryCreateObject(new object[] { 5, new Percentage(30) }, out obj, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			var resp = await url.UseImageStreamAsync(Context, obj, async (f, s) =>
			{
				var options = new ModerationReason(Context.User, null).CreateRequestOptions();
				await Context.Guild.CreateEmoteAsync(name, new Image(s), default, options).CAF();
				s.Seek(0, SeekOrigin.Begin);
				await Context.Channel.SendFileAsync(s, $"{name}.{f}", $"Successfully created the emote `{name}`.", false, options).CAF();
			}).CAF();
			if (resp != null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"Failed to create the emote `{name}`. Reason: {resp}.")).CAF();
			}
		}
		[Command(nameof(Delete)), ShortAlias(nameof(Delete))]
		public async Task Delete(GuildEmote emote)
		{
			await Context.Guild.DeleteEmoteAsync(emote, new ModerationReason(Context.User, null).CreateRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted the emote `{emote.Name}`.");
		}
	}
}
