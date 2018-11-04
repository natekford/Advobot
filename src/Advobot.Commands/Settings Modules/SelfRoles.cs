﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.Settings;
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
		//[SaveGuildSettings]
		public sealed class ModifySelfRoles : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public async Task Create(uint groupNumber)
				=> await CommandRunner(groupNumber).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Delete(uint groupNumber)
				=> await CommandRunner(groupNumber).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Add(uint groupNumber, [ValidateRole] params SocketRole[] roles)
				=> await CommandRunner(groupNumber, roles).CAF();
			[ImplicitCommand, ImplicitAlias]
			public async Task Remove(uint groupNumber, [ValidateRole] params SocketRole[] roles)
				=> await CommandRunner(groupNumber, roles).CAF();

#warning rewrite this trash (put into typereaders and other things)
			private async Task CommandRunner(uint groupNum, [CallerMemberName] string caller = "")
			{
				var selfAssignableGroups = Context.GuildSettings.SelfAssignableGroups;
				switch (caller)
				{
					case nameof(Create):
						if (selfAssignableGroups.Count >= BotSettings.MaxSelfAssignableRoleGroups)
						{
							await ReplyErrorAsync($"You have too many groups. `{BotSettings.MaxSelfAssignableRoleGroups}` is the maximum.").CAF();
							return;
						}
						else if (selfAssignableGroups.Any(x => x.Group == groupNum))
						{
							await ReplyErrorAsync("A group already exists with that position.").CAF();
							return;
						}
						selfAssignableGroups.Add(new SelfAssignableRoles((int)groupNum));
						break;
					case nameof(Delete):
						if (selfAssignableGroups.Count <= 0)
						{
							await ReplyErrorAsync("There are no groups to delete.").CAF();
							return;
						}
						else if (!selfAssignableGroups.Any(x => x.Group == groupNum))
						{
							await ReplyErrorAsync("A group needs to exist with that position before it can be deleted.").CAF();
							return;
						}
						selfAssignableGroups.RemoveAll(x => x.Group == groupNum);
						break;
					default:
						throw new InvalidOperationException("Invalid action for modifying a self assignable role group.");
				}

				var actionName = caller.ToLower() + "d";
				await ReplyTimedAsync($"Successfully {actionName} group `{groupNum}`.").CAF();
			}
			private async Task CommandRunner(uint groupNum, IRole[] roles, [CallerMemberName] string caller = "")
			{
				var groups = Context.GuildSettings.SelfAssignableGroups;
				if (!groups.Any())
				{
					await ReplyErrorAsync("There are no groups to edit.").CAF();
					return;
				}
				if (groups.TryGetSingle(x => x.Group == groupNum, out var group))
				{
					await ReplyErrorAsync($"A group needs to exist with the position `{groupNum}` before you can modify it.").CAF();
					return;
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
						group.RemoveRoles(rolesModified);
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
				await ReplyTimedAsync(new[] { modified, notModified }.JoinNonNullStrings(" ")).CAF();
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
			public async Task Command()
				=> await ReplyIfAny(Context.GuildSettings.SelfAssignableGroups.Select(x => x.Group).OrderBy(x => x),
					"self assignable role groups", x => x.ToString()).CAF();
			[Command]
			public async Task Command(uint groupNum)
			{
				if (Context.GuildSettings.SelfAssignableGroups.TryGetSingle(x => x.Group == groupNum, out var group))
				{
					await ReplyErrorAsync($"There is no group with the number `{groupNum}`.").CAF();
					return;
				}
				await ReplyEmbedAsync(new EmbedWrapper
				{
					Title = $"Self Roles Group {groupNum}",
					Description = group.Roles.Any() ? group.ToString() : "`Nothing`"
				}).CAF();
			}
		}
	}
}
