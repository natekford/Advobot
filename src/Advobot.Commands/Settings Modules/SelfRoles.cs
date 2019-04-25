using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.Settings;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands
{
	public sealed class SelfRoles : ModuleBase
	{
		[Group(nameof(ModifySelfRoles)), ModuleInitialismAlias(typeof(ModifySelfRoles))]
		[Summary("Adds a role to the self assignable list. " +
			"Roles can be grouped together which means only one role in the group can be self assigned at a time. " +
			"Create and Delete modify the entire group. " +
			"Add and Remove modify a single role in a group.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifySelfRoles : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias]
			public Task Create([ValidatePositiveNumber] int group)
				=> CommandRunner(group);
			[ImplicitCommand, ImplicitAlias]
			public Task Delete([ValidatePositiveNumber] int group)
				=> CommandRunner(group);
			[ImplicitCommand, ImplicitAlias]
			public Task Add([ValidatePositiveNumber] int group, [ValidateRole] params SocketRole[] roles)
				=> CommandRunner(group, roles);
			[ImplicitCommand, ImplicitAlias]
			public Task Remove([ValidatePositiveNumber] int group, [ValidateRole] params SocketRole[] roles)
				=> CommandRunner(group, roles);

			private Task CommandRunner(int number, [CallerMemberName] string caller = "")
			{
				var groups = Settings.SelfAssignableGroups;
				switch (caller)
				{
					case nameof(Create):
						if (groups.Count >= BotSettings.MaxSelfAssignableRoleGroups)
						{
							return ReplyErrorAsync($"You have too many groups. `{BotSettings.MaxSelfAssignableRoleGroups}` is the maximum.");
						}
						if (groups.Any(x => x.Group == number))
						{
							return ReplyErrorAsync("A group already exists with that position.");
						}
						groups.Add(new SelfAssignableRoles(number));
						break;
					case nameof(Delete):
						if (groups.Count <= 0)
						{
							return ReplyErrorAsync("There are no groups to delete.");
						}
						if (groups.RemoveAll(x => x.Group == number) == 0)
						{
							return ReplyErrorAsync("A group needs to exist with that position before it can be deleted.");
						}
						break;
					default:
						throw new InvalidOperationException("Invalid action for modifying a self assignable role group.");
				}
				return ReplyTimedAsync($"Successfully {caller.ToLower() + "d"} group `{number}`.");
			}
			private Task CommandRunner(int number, IRole[] roles, [CallerMemberName] string caller = "")
			{
				var groups = Settings.SelfAssignableGroups;
				if (!groups.Any())
				{
					return ReplyErrorAsync("There are no groups to edit.");
				}
				if (groups.TryGetSingle(x => x.Group == number, out var group))
				{
					return ReplyErrorAsync($"A group needs to exist with the position `{number}` before you can modify it.");
				}

				var rolesModified = new List<IRole>();
				var rolesNotModified = new List<IRole>();
				switch (caller)
				{
					case nameof(Add):
						foreach (var role in roles)
						{
							if (!groups.Any(x => x.Roles.Contains(role.Id)))
							{
								rolesModified.Add(role);
							}
							else
							{
								rolesNotModified.Add(role);
							}
						}
						group.AddRoles(rolesModified);
						break;
					case nameof(Remove):
						foreach (var role in roles)
						{
							if (groups.Any(x => x.Roles.Contains(role.Id)))
							{
								rolesModified.Add(role);
							}
							else
							{
								rolesNotModified.Add(role);
							}
						}
						group.RemoveRoles(rolesModified.Select(x => x.Id));
						break;
					default:
						throw new InvalidOperationException("Invalid action for modifying roles in a self assignable role group.");
				}

				var actionName = caller.ToLower() + "d";
				var modified = rolesModified.Any()
					? $"Successfully {actionName} the following role(s): `{rolesModified.Join("`, `", x => x.Format())}`."
					: null;
				var notModified = rolesNotModified.Any()
					? $"Failed to {actionName} the following role(s): `{rolesNotModified.Join("`, `", x => x.Format())}`."
					: null;
				return ReplyTimedAsync(new[] { modified, notModified }.JoinNonNullStrings(" "));
			}
		}

		[Group(nameof(AssignSelfRole)), ModuleInitialismAlias(typeof(AssignSelfRole))]
		[Summary("Gives or takes a role depending on if the user has it already. " +
			"Removes all other roles in the same group unless the group is `0`.")]
		[EnabledByDefault(false)]
		public sealed class AssignSelfRole : AdvobotModuleBase
		{
			[Command]
			public async Task Command(SocketRole role)
			{
				if (Context.GuildSettings.SelfAssignableGroups.TryGetSingle(x => x.Roles.Contains(role.Id), out var group))
				{
					await ReplyErrorAsync($"`{role.Format()}` is not a self assignable role.").CAF();
					return;
				}
				var user = Context.User;
				if (user.Roles.Any(x => x.Id == role.Id))
				{
					await user.AddRoleAsync(role, GenerateRequestOptions("self role removal")).CAF();
					await ReplyTimedAsync($"Successfully removed `{role.Format()}`.").CAF();
					return;
				}

				//Remove roles the user already has from the group if they're targetting an exclusivity group
				var removedRoles = "";
				var otherRoles = user.Roles.Where(x => group.Roles.Contains(x.Id));
				if (group.Group != 0 && otherRoles.Any())
				{
					await user.RemoveRolesAsync(otherRoles, GenerateRequestOptions("self role removal")).CAF();
					removedRoles = $", and removed `{otherRoles.Join("`, `", x => x.Format())}`";
				}

				await user.AddRoleAsync(role, GenerateRequestOptions("self role giving")).CAF();
				await ReplyTimedAsync($"Successfully gave `{role.Name}`{removedRoles}.").CAF();
			}
		}

		[Group(nameof(DisplaySelfRoles)), ModuleInitialismAlias(typeof(DisplaySelfRoles))]
		[Summary("Shows the current group numbers that exists on the guild. " +
			"If a number is input then it shows the roles in that group.")]
		[EnabledByDefault(false)]
		public sealed class DisplaySelfRoles : AdvobotModuleBase
		{
			[Command]
			public Task Command()
				=> ReplyIfAny(Context.GuildSettings.SelfAssignableGroups.Select(x => x.Group).OrderBy(x => x),
					"self assignable role groups", x => x.ToString());
			[Command]
			public Task Command([ValidatePositiveNumber] int group)
			{
				if (Context.GuildSettings.SelfAssignableGroups.TryGetSingle(x => x.Group == group, out var g))
				{
					return ReplyErrorAsync($"There is no group with the number `{group}`.");
				}
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = $"Self Roles Group {group}",
					Description = g.Roles.Any() ? g.ToString() : "`Nothing`"
				});
			}
		}
	}
}
