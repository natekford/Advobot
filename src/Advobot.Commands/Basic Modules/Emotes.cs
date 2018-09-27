using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.ImageResizing;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ImageMagick;

namespace Advobot.Commands.Emotes
{
	[Category(typeof(CreateEmote)), Group(nameof(CreateEmote)), TopLevelShortAlias(typeof(CreateEmote))]
	[Summary("Adds an emote to the server. " +
		"Requires either an emote to copy, or the name and file to make an emote out of.")]
	[PermissionRequirement(new[] { GuildPermission.ManageEmojis }, null)]
	[DefaultEnabled(true)]
	[RateLimit(1)]
	public sealed class CreateEmote : AdvobotModuleBase
	{
		private static EmoteResizer _Resizer = new EmoteResizer(4);

		[Command]
		public async Task Command(Emote emote)
			=> await Command(emote.Name, new Uri(emote.Url)).CAF();
		[Command, Priority(1)]
		public async Task Command(
			[ValidateString(Target.Emote)] string name,
			Uri url,
			[Optional, Remainder] EmoteResizerArguments args)
		{
			if (_Resizer.IsGuildAlreadyProcessing(Context.Guild))
			{
				await ReplyErrorAsync(new Error("Currently already working on creating an emote.")).CAF();
				return;
			}

			_Resizer.EnqueueArguments(Context, args, url, GetRequestOptions(), name);
			if (_Resizer.CanStart)
			{
				_Resizer.StartProcessing();
			}
			await ReplyTimedAsync($"Position in emote creation queue: {_Resizer.QueueCount}.").CAF();
		}
	}

	[Category(typeof(DeleteEmote)), Group(nameof(DeleteEmote)), TopLevelShortAlias(typeof(DeleteEmote))]
	[Summary("Deletes the supplied emote from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageEmojis }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteEmote : AdvobotModuleBase
	{
		[Command]
		public async Task Command(GuildEmote emote)
		{
			await Context.Guild.DeleteEmoteAsync(emote, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully deleted the emote `{emote.Name}`.").CAF();
		}
	}

	[Category(typeof(ModifyEmoteName)), Group(nameof(ModifyEmoteName)), TopLevelShortAlias(typeof(ModifyEmoteName))]
	[Summary("Changes the name of the supplied emote.")]
	[PermissionRequirement(new[] { GuildPermission.ManageEmojis }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyEmoteName : AdvobotModuleBase
	{
		[Command]
		public async Task Command(GuildEmote emote, [ValidateString(Target.Emote), Remainder] string name)
		{
			await Context.Guild.ModifyEmoteAsync(emote, x => x.Name = name, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully changed the emote name to `{name}`.").CAF();
		}
	}

	[Category(typeof(ModifyEmoteRoles)), Group(nameof(ModifyEmoteRoles)), TopLevelShortAlias(typeof(ModifyEmoteRoles))]
	[Summary("Changes the roles which are ALL necessary to use an emote. " +
		"Your Discord client will need to be restarted after editing this in order to see the emote again, even if you give yourself the roles.")]
	[PermissionRequirement(new[] { GuildPermission.ManageEmojis }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyEmoteRoles : AdvobotModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(
			GuildEmote emote,
			[ValidateObject(Verif.CanBeEdited, Verif.IsNotEveryone, Verif.IsNotManaged)] params SocketRole[] roles)
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
			await ReplyTimedAsync($"Successfully added `{roles.Join("`, `", x => x.Format())}` as roles necessary to use `{emote}`.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(
			GuildEmote emote,
			[ValidateObject(Verif.CanBeEdited, Verif.IsNotEveryone, Verif.IsNotManaged)] params SocketRole[] roles)
		{
			if (!emote.RoleIds.Any())
			{
				await ReplyErrorAsync(new Error($"The emote `{emote}` does not have any restricting roles.")).CAF();
				return;
			}

			await Context.Guild.ModifyEmoteAsync(emote, x =>
			{
				if (x.Roles.IsSpecified)
				{
					x.Roles = Optional.Create(x.Roles.Value.Where(r => !roles.Select(q => q.Id).Contains(r.Id)));
				}
			}, GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully removed `{roles.Join("`, `", x => x.Format())}` as roles necessary to use `{emote}`.").CAF();
		}
		[Command(nameof(RemoveAll)), ShortAlias(nameof(RemoveAll))]
		public async Task RemoveAll(GuildEmote emote)
		{
			if (!emote.RoleIds.Any())
			{
				await ReplyErrorAsync(new Error($"The emote `{emote}` does not have any restricting roles.")).CAF();
				return;
			}

			await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional.Create<IEnumerable<IRole>>(null), GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully removed all roles necessary to use `{emote}`.").CAF();
		}
	}

	[Category(typeof(DisplayEmotes)), Group(nameof(DisplayEmotes)), TopLevelShortAlias(typeof(DisplayEmotes))]
	[Summary("Lists the emotes in the guild. If there are more than 20 emotes of a specified type, they will be uploaded in a file.")]
	[PermissionRequirement(new[] { PermissionRequirementAttribute.GenericPerms }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayEmotes : AdvobotModuleBase
	{
		[Command(nameof(Managed)), ShortAlias(nameof(Managed))]
		public async Task Managed()
			=> await CommandRunner(Context.Guild.Emotes.Where(x => x.IsManaged)).CAF();
		[Command(nameof(Local)), ShortAlias(nameof(Local))]
		public async Task Local()
			=> await CommandRunner(Context.Guild.Emotes.Where(x => !x.IsManaged && !x.Animated)).CAF();
		[Command(nameof(Animated)), ShortAlias(nameof(Animated))]
		public async Task Animated()
			=> await CommandRunner(Context.Guild.Emotes.Where(x => x.Animated)).CAF();

		private async Task CommandRunner(IEnumerable<GuildEmote> emotes, [CallerMemberName] string caller = "")
		{
			if (!emotes.Any())
			{
				await ReplyErrorAsync(new Error($"This guild has no {caller.ToLower()} emotes.")).CAF();
				return;
			}
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = "Emotes",
				Description = emotes.FormatNumberedList(x => $"{x} `{x.Name}`"),
			}).CAF();
		}
	}
}
