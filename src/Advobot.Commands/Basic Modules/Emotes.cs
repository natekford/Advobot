using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ImageMagick;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
	public sealed class CreateEmote : ImageCreationModuleBase<EmoteResizerArgs>
	{
		[Command]
		public async Task Command(Emote emote)
		{
			await Command(emote.Name, new Uri(emote.Url)).CAF();
		}
		[Command, Priority(1)]
		public async Task Command(
			[VerifyStringLength(Target.Emote)] string name,
			Uri url,
			[Optional, Remainder] NamedArguments<EmoteResizerArgs> args)
		{
			EmoteResizerArgs obj;
			if (GuildAlreadyProcessing)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on creating an emote.")).CAF();
				return;
			}
			else if (args == null)
			{
				obj = new EmoteResizerArgs();
			}
			else if (!args.TryCreateObject(new object[] { 5, new Percentage(30) }, out obj, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			EnqueueArguments(new ImageCreationArguments<EmoteResizerArgs>
			{
				Uri = url,
				Name = name,
				Args = obj,
				Context = Context,
				Options = GetRequestOptions(),
			});
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Position in emote creation queue: {QueueCount}.").CAF();
			if (CanStart)
			{
				StartProcessing();
			}
		}

		protected override Task Create(ImageCreationArguments<EmoteResizerArgs> args)
		{
			return PrivateCreate(args);
		}
		private static async Task PrivateCreate(ImageCreationArguments<EmoteResizerArgs> args)
		{
			using (var resp = await ImageUtils.ResizeImageAsync(args.Uri, args.Context, args.Args).CAF())
			{
				if (resp.IsSuccess)
				{
					var emote = await args.Context.Guild.CreateEmoteAsync(args.Name, new Image(resp.Stream), default, args.Options).CAF();
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(args.Context, $"Successfully created the emote {emote}.").CAF();
					return;
				}
				await MessageUtils.SendErrorMessageAsync(args.Context, new Error($"Failed to create the emote `{args.Name}`. Reason: {resp.Error}.")).CAF();
			}
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
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted the emote `{emote.Name}`.").CAF();
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

	[Group(nameof(ModifyEmoteRoles)), TopLevelShortAlias(typeof(ModifyEmoteRoles))]
	[Summary("Changes the roles which are ALL necessary to use an emote. " +
		"Your Discord client will need to be restarted after editing this in order to see the emote again, even if you give yourself the roles.")]
	[PermissionRequirement(new[] { GuildPermission.ManageEmojis }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyEmoteRoles : NonSavingModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(
			GuildEmote emote,
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsNotEveryone, ObjectVerification.IsNotManaged)] params SocketRole[] roles)
		{
			await Context.Guild.ModifyEmoteAsync(emote, x =>
			{
				if (x.Roles.IsSpecified)
				{
					x.Roles = Optional.Create(x.Roles.Value.Concat(roles).Distinct());
				}
				else
				{
					x.Roles = Optional.Create<IEnumerable<IRole>>(roles.Distinct());
				}
			}, GetRequestOptions()).CAF();
			var resp = $"Successfully added `{String.Join("`, `", roles.Select(x => x.Format()))}` as roles necessary to use `{emote}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(
			GuildEmote emote,
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsNotEveryone, ObjectVerification.IsNotManaged)] params SocketRole[] roles)
		{
			if (!emote.RoleIds.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"The emote {emote} does not have any restricting roles.")).CAF();
				return;
			}

			await Context.Guild.ModifyEmoteAsync(emote, x =>
			{
				x.Roles = x.Roles.IsSpecified
					? x.Roles = Optional.Create(x.Roles.Value.Where(r => !roles.Select(q => q.Id).Contains(r.Id)))
					: Optional<IEnumerable<IRole>>.Unspecified;
			}, GetRequestOptions()).CAF();
			var resp = $"Successfully removed `{String.Join("`, `", roles.Select(x => x.Format()))}` as roles necessary to use `{emote}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(RemoveAll)), ShortAlias(nameof(RemoveAll))]
		public async Task RemoveAll(GuildEmote emote)
		{
			if (!emote.RoleIds.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"The emote {emote} does not have any restricting roles.")).CAF();
				return;
			}

			await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional<IEnumerable<IRole>>.Unspecified, GetRequestOptions()).CAF();
			var resp = $"Successfully removed all roles necessary to use `{emote}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(DisplayEmotes)), TopLevelShortAlias(typeof(DisplayEmotes))]
	[Summary("Lists the emotes in the guild. If there are more than 20 emotes of a specified type, they will be uploaded in a file.")]
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
			await CommandRunner(Context.Guild.Emotes.Where(x => !x.IsManaged && !x.Animated).ToList()).CAF();
		}
		[Command(nameof(Animated)), ShortAlias(nameof(Animated))]
		public async Task Animated()
		{
			await CommandRunner(Context.Guild.Emotes.Where(x => x.Animated).ToList()).CAF();
		}

		private async Task CommandRunner(List<GuildEmote> emotes, [CallerMemberName] string caller = "")
		{
			var embed = new EmbedWrapper
			{
				Title = "Emotes",
				Description = emotes.Any()
					? emotes.FormatNumberedList(x => $"{x} `{x.Name}`")
					: $"This guild has no {caller.ToLower()} emotes.",
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}
}
