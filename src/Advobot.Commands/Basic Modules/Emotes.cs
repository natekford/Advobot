using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using ImageMagick;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Commands.Emotes
{
	[Group(nameof(CreateEmote)), TopLevelShortAlias(typeof(CreateEmote))]
	[Summary("Adds an emote to the server. " +
		"Requires either an emote to copy, or the name and file to make an emote out of.")]
	[PermissionRequirement(new[] { GuildPermission.ManageEmojis }, null)]
	[DefaultEnabled(true)]
	[Queue(1)]
	public sealed class CreateEmote : NonSavingModuleBase
	{
		private static Dictionary<ulong, bool> _WorkingDictionary = new Dictionary<ulong, bool>();

		[Command(RunMode = RunMode.Async)]
		public async Task Command(Emote emote)
		{
			await Command(emote.Name, new Uri(emote.Url)).CAF();
		}
		//TODO: implement a queue for these commands, since they use high memory and download
		[Command(RunMode = RunMode.Async)]
		public async Task Command([VerifyStringLength(Target.Emote)] string name, Uri url, [Optional, Remainder] NamedArguments<EmoteResizerArgs> args)
		{
			EmoteResizerArgs obj;
			if (args == null)
			{
				obj = new EmoteResizerArgs();
			}
			else if (!args.TryCreateObject(new object[] { 5, new Percentage(30) }, out obj, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			if (_WorkingDictionary.TryGetValue(Context.Guild.Id, out var isWorking) && isWorking)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on creating an emote."));
				return;
			}

			_WorkingDictionary[Context.Guild.Id] = true;
			using (var resp = await ImageUtils.ResizeImageAsync(url, Context, obj))
			{
				if (resp.IsSuccess)
				{
					var emote = await Context.Guild.CreateEmoteAsync(name, new Image(resp.Stream), default, CreateRequestOptions()).CAF();
					await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully created the emote {emote}.");
					return;
				}
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"Failed to create the emote `{name}`. Reason: {resp.Error}.")).CAF();
			}
			_WorkingDictionary[Context.Guild.Id] = false;
		}
	}

	[Group(nameof(DeleteEmote)), TopLevelShortAlias(typeof(DeleteEmote))]
	[Summary("Deletes the supplied emote from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageEmojis }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteEmote : NonSavingModuleBase
	{
		[Command]
		public async Task Command(GuildEmote emote)
		{
			await Context.Guild.DeleteEmoteAsync(emote, CreateRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted the emote `{emote.Name}`.");
		}
	}

	[Group(nameof(ModifyEmoteName)), TopLevelShortAlias(typeof(ModifyEmoteName))]
	[Summary("Changes the name of the supplied emote.")]
	[PermissionRequirement(new[] { GuildPermission.ManageEmojis }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyEmoteName : NonSavingModuleBase
	{
		[Command]
		public async Task Command(GuildEmote emote, [VerifyStringLength(Target.Emote), Remainder] string newName)
		{
			await Context.Guild.ModifyEmoteAsync(emote, x => x.Name = newName, CreateRequestOptions()).CAF();
		}
	}

	[Group(nameof(DisplayEmotes)), TopLevelShortAlias(typeof(DisplayEmotes))]
	[Summary("Lists the emotes in the guild.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class DisplayEmotes : NonSavingModuleBase
	{
		[Command(nameof(Managed)), ShortAlias(nameof(Managed))]
		public async Task Managed()
		{
			await CommandRunner(Context.Guild.Emotes.Where(x => x.IsManaged).ToList()).CAF();
		}
		[Command(nameof(Local)), ShortAlias(nameof(Local))]
		public async Task Local()
		{
			await CommandRunner(Context.Guild.Emotes.Where(x => !x.IsManaged).ToList()).CAF();
		}

		private async Task CommandRunner(List<GuildEmote> emotes)
		{
			var embed = new EmbedWrapper
			{
				Title = "Emotes",
				Description = emotes.Any()
					? emotes.FormatNumberedList(x => $"<:{x.Name}:{x.Id}> `{x.Name}`")
					: $"This guild has no guild emotes.",
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}
}
