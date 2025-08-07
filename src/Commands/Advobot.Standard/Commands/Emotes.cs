using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Emotes;
using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.ParameterPreconditions.Strings;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands;

[Category(nameof(Emotes))]
public sealed class Emotes : ModuleBase
{
	[LocalizedGroup(nameof(Groups.DeleteEmote))]
	[LocalizedAlias(nameof(Aliases.DeleteEmote))]
	[LocalizedSummary(nameof(Summaries.DeleteEmote))]
	[Meta("104da53d-1cb6-4ee4-8260-ac7398512351", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageEmojisAndStickers)]
	public sealed class DeleteEmote : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(GuildEmote emote)
		{
			await Context.Guild.DeleteEmoteAsync(emote, GetOptions()).CAF();
			return Responses.Snowflakes.Deleted(emote);
		}
	}

	[LocalizedGroup(nameof(Groups.DisplayEmotes))]
	[LocalizedAlias(nameof(Aliases.DisplayEmotes))]
	[LocalizedSummary(nameof(Summaries.DisplayEmotes))]
	[Meta("fd5ae4a2-52af-44eb-aef0-347a0df1437b", IsEnabled = true)]
	[RequireGenericGuildPermissions]
	public sealed class DisplayEmotes : AdvobotModuleBase
	{
		[LocalizedCommand(nameof(Groups.Animated))]
		[LocalizedAlias(nameof(Aliases.Animated))]
		public Task<RuntimeResult> Animated()
			=> Responses.Emotes.DisplayMany(Context.Guild.Emotes.Where(x => x.Animated));

		[LocalizedCommand(nameof(Groups.Local))]
		[LocalizedAlias(nameof(Aliases.Local))]
		public Task<RuntimeResult> Local()
			=> Responses.Emotes.DisplayMany(Context.Guild.Emotes.Where(x => !x.IsManaged && !x.Animated));

		[LocalizedCommand(nameof(Groups.Managed))]
		[LocalizedAlias(nameof(Aliases.Managed))]
		public Task<RuntimeResult> Managed()
			=> Responses.Emotes.DisplayMany(Context.Guild.Emotes.Where(x => x.IsManaged));
	}

	[LocalizedGroup(nameof(Groups.ModifyEmoteName))]
	[LocalizedAlias(nameof(Aliases.ModifyEmoteName))]
	[LocalizedSummary(nameof(Summaries.ModifyEmoteName))]
	[Meta("3fe7f72f-c10a-4cc8-b76f-376a1c1aced4", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageEmojisAndStickers)]
	public sealed class ModifyEmoteName : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(
			GuildEmote emote,
			[Remainder, EmoteName]
			string name
		)
		{
			await Context.Guild.ModifyEmoteAsync(emote, x => x.Name = name, GetOptions()).CAF();
			return Responses.Snowflakes.ModifiedName(emote, name);
		}
	}

	[LocalizedGroup(nameof(Groups.ModifyEmoteRoles))]
	[LocalizedAlias(nameof(Aliases.ModifyEmoteRoles))]
	[LocalizedSummary(nameof(Summaries.ModifyEmoteRoles))]
	[Meta("103b0c35-0bd0-4f72-a010-8a4013601258", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageEmojisAndStickers)]
	public sealed class ModifyEmoteRoles : AdvobotModuleBase
	{
		[LocalizedCommand(nameof(Groups.Add))]
		[LocalizedAlias(nameof(Aliases.Add))]
		public async Task<RuntimeResult> Add(
			GuildEmote emote,
			[NotEveryone, NotManaged]
			params IRole[] roles
		)
		{
			await Context.Guild.ModifyEmoteAsync(emote, x =>
			{
				var currentRoles = x.Roles.GetValueOrDefault([]);
				var concat = currentRoles.Concat(roles).Distinct();
				x.Roles = Optional.Create(concat);
			}, GetOptions()).CAF();
			return Responses.Emotes.AddedRequiredRoles(emote, roles);
		}

		[LocalizedCommand(nameof(Groups.Remove))]
		[LocalizedAlias(nameof(Aliases.Remove))]
		public async Task<RuntimeResult> Remove(
			[HasRequiredRoles]
			GuildEmote emote,
			[NotEveryone, NotManaged]
			params IRole[] roles
		)
		{
			await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional.Create(x.Roles.Value.Where(r => !roles.Contains(r))), GetOptions()).CAF();
			return Responses.Emotes.RemoveRequiredRoles(emote, roles);
		}

		[LocalizedCommand(nameof(Groups.RemoveAll))]
		[LocalizedAlias(nameof(Aliases.RemoveAll))]
		public async Task<RuntimeResult> RemoveAll(
			[HasRequiredRoles]
			GuildEmote emote
		)
		{
			var roles = emote.RoleIds.Select(x => Context.Guild.GetRole(x));
			await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = Optional.Create<IEnumerable<IRole>?>(null), GetOptions()).CAF();
			return Responses.Emotes.RemoveRequiredRoles(emote, roles);
		}
	}
}