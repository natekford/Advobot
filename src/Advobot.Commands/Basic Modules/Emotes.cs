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
	[Group(nameof(Create)), TopLevelShortAlias(typeof(CreateEmote))]
	[Summary("Adds an emote to the server. " +
		"Requires either an emote to copy, or the name and file to make an emote out of.")]
	[PermissionRequirement(new[] { GuildPermission.ManageEmojis }, null)]
	[DefaultEnabled(true)]
	[Queue(1)]
	public sealed class CreateEmote : NonSavingModuleBase
	{
		private static ConcurrentQueue<EmoteCreationArguments> _Args = new ConcurrentQueue<EmoteCreationArguments>();
		private static ConcurrentDictionary<ulong, object> _CurrentlyMakingEmoteOnGuild = new ConcurrentDictionary<ulong, object>();
		private static SemaphoreSlim _SemaphoreSlim = new SemaphoreSlim(1);

		[Command(RunMode = RunMode.Async)]
		public async Task Command(Emote emote)
		{
			await Command(emote.Name, new Uri(emote.Url)).CAF();
		}
		[Priority(1), Command(RunMode = RunMode.Async)]
		public async Task Command(
			[VerifyStringLength(Target.Emote)] string name, 
			Uri url, 
			[Optional, Remainder] NamedArguments<EmoteResizerArgs> args)
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
			if (_Args.Any(x => x.Context.Guild.Id == Context.Guild.Id) || _CurrentlyMakingEmoteOnGuild.Any(x => x.Key == Context.Guild.Id))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on creating an emote."));
				return;
			}

			_Args.Enqueue(new EmoteCreationArguments
			{
				Uri = url,
				Name = name,
				Args = obj,
				Context = Context,
				Options = GetRequestOptions(),
			});
			//Start the emote creating, but keep it synchronous so as not to use a ton of memory/download
			if (_SemaphoreSlim.CurrentCount > 0)
			{
				//Store it as a variable to get rid of the warning and allow it to run on its own
				var t = Task.Run(async () =>
				{
					//Lock since only one thread should be processing this at once (maybe bump count up later)
					await _SemaphoreSlim.WaitAsync().CAF();
					while (_Args.TryDequeue(out var d))
					{
						_CurrentlyMakingEmoteOnGuild.TryAdd(d.Context.Guild.Id, new object());
						await Create(d).CAF();
						_CurrentlyMakingEmoteOnGuild.TryRemove(d.Context.Guild.Id, out var removed);
					}
					_SemaphoreSlim.Release();
				}).CAF();
			}
			else
			{
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Position in emote creation queue: {_Args.Count}.").CAF();
			}
		}

		private static async Task Create(EmoteCreationArguments args)
		{
			using (var resp = await ImageUtils.ResizeImageAsync(args.Uri, args.Context, args.Args))
			{
				if (resp.IsSuccess)
				{
					var emote = await args.Context.Guild.CreateEmoteAsync(args.Name, new Image(resp.Stream), default, args.Options).CAF();
					await MessageUtils.SendMessageAsync(args.Context.Channel, $"Successfully created the emote {emote}.");
					return;
				}
				await MessageUtils.SendErrorMessageAsync(args.Context, new Error($"Failed to create the emote `{args.Name}`. Reason: {resp.Error}.")).CAF();
			}
		}

		private struct EmoteCreationArguments
		{
			public Uri Uri;
			public string Name;
			public EmoteResizerArgs Args;
			public AdvobotSocketCommandContext Context;
			public RequestOptions Options;
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
			await Context.Guild.DeleteEmoteAsync(emote, GetRequestOptions()).CAF();
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
			await Context.Guild.ModifyEmoteAsync(emote, x => x.Name = newName, GetRequestOptions()).CAF();
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
