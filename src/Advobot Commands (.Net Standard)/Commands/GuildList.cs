using Advobot.Actions;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.GuildList
{
	[Group(nameof(ModifyGuildListing)), TopLevelShortAlias(typeof(ModifyGuildListing))]
	[Summary("Adds or removes a guild from the public guild list.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyGuildListing : SavingModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(IInvite invite, [Optional] params string[] keywords)
		{
			if (Context.GuildSettings.ListedInvite != null)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("This guild is already listed.")).CAF();
				return;
			}
			else if (invite is IInviteMetadata metadata && metadata.MaxAge != null)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Don't provide invites that expire.")).CAF();
				return;
			}

			Context.GuildSettings.ListedInvite = new ListedInvite(invite, keywords);
			var resp = $"Successfully set the listed invite to the following:\n{Context.GuildSettings.ListedInvite}.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			if (Context.GuildSettings.ListedInvite == null)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("This guild is already unlisted.")).CAF();
				return;
			}

			Context.GuildSettings.ListedInvite = null;
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the listed invite.").CAF();
		}
	}

	[Group(nameof(BumpGuildListing)), TopLevelShortAlias(typeof(BumpGuildListing))]
	[Summary("Bumps the invite on the guild.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(false)]
	public sealed class BumpGuildListing : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			if (Context.GuildSettings.ListedInvite == null)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("There is no invite to bump.")).CAF();
				return;
			}
			else if ((DateTime.UtcNow - Context.GuildSettings.ListedInvite.LastBumped).TotalHours < 1)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Last bump is too recent.")).CAF();
				return;
			}

			Context.GuildSettings.ListedInvite.UpdateLastBumped();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully bumped the invite.").CAF();
		}
	}

	[Group(nameof(GetGuildListing)), TopLevelShortAlias(typeof(GetGuildListing))]
	[Summary("Gets an invite meeting the given criteria.")]
	[DefaultEnabled(true)]
	public sealed class GetGuildListing : AdvobotModuleBase
	{
		private static string _GHeader = "Guild Name".PadRight(25);
		private static string _UHeader = "URL".PadRight(35);
		private static string _MHeader = "Member Count".PadRight(14);
		private static string _EHeader = "Global Emotes";

		[Command]
		public async Task Command([Remainder] CustomArguments<ListedInviteGatherer> gatherer)
		{
			var invites = gatherer.CreateObject().GatherInvites(Context.InviteList);
			if (!invites.Any())
			{
				var error = new ErrorReason("No guild could be found that matches the given specifications.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			else if (invites.Count() <= 5)
			{
				var embed = new AdvobotEmbed("Guilds");
				invites.ToList().ForEach(x =>
				{
					var e = x.HasGlobalEmotes ? "**Has global emotes**" : "";
					var text = $"**URL:** {x.Url}\n**Members:** {x.Guild.MemberCount}\n{e}";
					embed.AddField(x.Guild.Name, text);
				});
				await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			else if (invites.Count() <= 15)
			{
				var formatted = invites.Select(x =>
				{
					var n = x.Guild.Name.Substring(0, Math.Min(x.Guild.Name.Length, _GHeader.Length)).PadRight(25);
					var u = x.Url.PadRight(35);
					var m = x.Guild.MemberCount.ToString().PadRight(14);
					var e = x.HasGlobalEmotes ? "Yes" : "";
					return $"{n}{u}{m}{e}";
				});
				var text = $"{_GHeader}{_UHeader}{_MHeader}{_EHeader}\n{String.Join("\n", formatted)}";
				await MessageActions.SendTextFileAsync(Context.Channel, text, "Guilds_").CAF();
			}
			else
			{
				var resp = $"`{invites.Count()}` results returned. Please narrow your search.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
				return;
			}
		}
	}
}
