using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Emotes;
using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands;

[Category(nameof(Emotes))]
public sealed class Emotes : ModuleBase
{
	// I don't know if roles affect emotes anymore, but there is no way to do this on mobile
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
				x.Roles = new(concat);
			}, GetOptions()).ConfigureAwait(false);
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
			await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = new(x.Roles.Value.Where(r => !roles.Contains(r))), GetOptions()).ConfigureAwait(false);
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
			await Context.Guild.ModifyEmoteAsync(emote, x => x.Roles = new(null), GetOptions()).ConfigureAwait(false);
			return Responses.Emotes.RemoveRequiredRoles(emote, roles);
		}
	}
}