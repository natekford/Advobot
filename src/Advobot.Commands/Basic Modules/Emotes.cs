using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.ImageResizing;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Emotes
{
	[Category(typeof(CreateEmote)), Group(nameof(CreateEmote)), TopLevelShortAlias(typeof(CreateEmote))]
	[Summary("Adds an emote to the server. " +
		"Requires either an emote to copy, or the name and file to make an emote out of.")]
	[UserPermissionRequirement(GuildPermission.ManageEmojis)]
	[DefaultEnabled(true)]
	[RateLimit(RateLimitAttribute.TimeUnit.Minutes, 1)]
	public sealed class CreateEmote : ImageResizerModule
	{
		[Command]
		public async Task Command(Emote emote)
			=> await Command(emote.Name, new Uri(emote.Url)).CAF();
		[Command, Priority(1)]
		public async Task Command(
			[ValidateEmoteName] string name,
			Uri url,
			[Optional, Remainder] UserProvidedImageArgs args)
			=> await ProcessAsync(new EmoteCreationArgs(Context, url, args, name)).CAF();
	}

	[Category(typeof(DeleteEmote)), Group(nameof(DeleteEmote)), TopLevelShortAlias(typeof(DeleteEmote))]
	[Summary("Deletes the supplied emote from the guild.")]
	[UserPermissionRequirement(GuildPermission.ManageEmojis)]
	[DefaultEnabled(true)]
	public sealed class DeleteEmote : AdvobotModuleBase
	{
		[Command]
		public async Task Command(GuildEmote emote)
		{
			await Context.Guild.DeleteEmoteAsync(emote, GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully deleted the emote `{emote.Name}`.").CAF();
		}
	}

	[Category(typeof(ModifyEmoteName)), Group(nameof(ModifyEmoteName)), TopLevelShortAlias(typeof(ModifyEmoteName))]
	[Summary("Changes the name of the supplied emote.")]
	[UserPermissionRequirement(GuildPermission.ManageEmojis)]
	[DefaultEnabled(true)]
	public sealed class ModifyEmoteName : AdvobotModuleBase
	{
		[Command]
		public async Task Command(GuildEmote emote, [Remainder, ValidateEmoteName] string name)
		{
			await Context.Guild.ModifyEmoteAsync(emote, x => x.Name = name, GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully changed the emote name to `{name}`.").CAF();
		}
	}

	[Category(typeof(ModifyEmoteRoles)), Group(nameof(ModifyEmoteRoles)), TopLevelShortAlias(typeof(ModifyEmoteRoles))]
	[Summary("Changes the roles which are ALL necessary to use an emote. " +
		"Your Discord client will need to be restarted after editing this in order to see the emote again, even if you give yourself the roles.")]
	[UserPermissionRequirement(GuildPermission.ManageEmojis)]
	[DefaultEnabled(true)]
	public sealed class ModifyEmoteRoles : AdvobotModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(GuildEmote emote, [NotEveryoneOrManaged] params SocketRole[] roles)
		{
			await Context.Guild.ModifyEmoteAsync(emote, x =>
			{
				var currentRoles = x.Roles.GetValueOrDefault() ?? Enumerable.Empty<IRole>();
				var concat = currentRoles.Concat(roles).Distinct();
				x.Roles = Optional.Create(concat);
			}, GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully added `{roles.Join("`, `", x => x.Format())}` as roles necessary to use `{emote}`.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(GuildEmote emote, [NotEveryoneOrManaged] params SocketRole[] roles)
		{
			if (!emote.RoleIds.Any())
			{
				await ReplyErrorAsync(new Error($"The emote `{emote}` does not have any restricting roles.")).CAF();
				return;
			}

			await Context.Guild.ModifyEmoteAsync(emote, x =>
			{
				if (!x.Roles.IsSpecified)
				{
					return;
				}

				var ids = roles.Select(r => r.Id);
				x.Roles = Optional.Create(x.Roles.Value.Where(r => !ids.Contains(r.Id)));
			}, GenerateRequestOptions()).CAF();
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

			await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional.Create<IEnumerable<IRole>>(null), GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully removed all roles necessary to use `{emote}`.").CAF();
		}
	}

	[Category(typeof(DisplayEmotes)), Group(nameof(DisplayEmotes)), TopLevelShortAlias(typeof(DisplayEmotes))]
	[Summary("Lists the emotes in the guild. If there are more than 20 emotes of a specified type, they will be uploaded in a file.")]
	[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class DisplayEmotes : AdvobotModuleBase
	{
		[Command(nameof(Managed)), ShortAlias(nameof(Managed))]
		public async Task Managed()
			=> await CommandRunner(x => x.IsManaged).CAF();
		[Command(nameof(Local)), ShortAlias(nameof(Local))]
		public async Task Local()
			=> await CommandRunner(x => !x.IsManaged && !x.Animated).CAF();
		[Command(nameof(Animated)), ShortAlias(nameof(Animated))]
		public async Task Animated()
			=> await CommandRunner(x => x.Animated).CAF();

		private async Task CommandRunner(Func<GuildEmote, bool> predicate, [CallerMemberName] string caller = null)
			=> await ReplyIfAny(Context.Guild.Emotes.Where(predicate), caller + "Emotes", x => $"{x} `{x.Name}`").CAF();
	}
}
