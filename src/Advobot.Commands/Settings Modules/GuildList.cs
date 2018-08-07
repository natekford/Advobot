using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Settings;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.GuildList
{
	[Group(nameof(ModifyGuildListing)), TopLevelShortAlias(typeof(ModifyGuildListing))]
	[Summary("Adds or removes a guild from the public guild list.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyGuildListing : AdvobotModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(IInvite invite, [Optional] params string[] keywords)
		{
			if (Context.GuildSettings.ListedInvite != null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("This guild is already listed.")).CAF();
				return;
			}

			if (invite is IInviteMetadata metadata && metadata.MaxAge != null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Don't provide invites that expire.")).CAF();
				return;
			}

			Context.GuildSettings.ListedInvite = new ListedInvite(invite, keywords);
			var resp = $"Successfully set the listed invite to the following:\n{Context.GuildSettings.ListedInvite}.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			if (Context.GuildSettings.ListedInvite == null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("This guild is already unlisted.")).CAF();
				return;
			}

			Context.GuildSettings.ListedInvite = null;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the listed invite.").CAF();
		}
	}

	[Group(nameof(BumpGuildListing)), TopLevelShortAlias(typeof(BumpGuildListing))]
	[Summary("Bumps the invite on the guild.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(false)]
	public sealed class BumpGuildListing : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			if (Context.GuildSettings.ListedInvite == null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("There is no invite to bump.")).CAF();
				return;
			}

			if ((DateTime.UtcNow - Context.GuildSettings.ListedInvite.LastBumped).TotalHours < 1)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Last bump is too recent.")).CAF();
				return;
			}

			Context.GuildSettings.ListedInvite.UpdateLastBumped();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully bumped the invite.").CAF();
		}
	}

	[Group(nameof(GetGuildListing)), TopLevelShortAlias(typeof(GetGuildListing))]
	[Summary("Gets an invite meeting the given criteria.")]
	[DefaultEnabled(true)]
	public sealed class GetGuildListing : AdvobotModuleBase
	{
		private static readonly string _GHeader = "Guild Name".PadRight(25);
		private static readonly string _UHeader = "URL".PadRight(35);
		private static readonly string _MHeader = "Member Count".PadRight(14);
		private static readonly string _EHeader = "Global Emotes";

		[Command]
		public async Task Command([Remainder] NamedArguments<ListedInviteGatherer> args)
		{
			if (!args.TryCreateObject(new object[0], out var obj, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			var invites = obj.GatherInvites(Context.InviteList).ToList();
			if (!invites.Any())
			{
				error = new Error("No guild could be found that matches the given specifications.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
			}
			else if (invites.Count() <= 5)
			{
				var embed = new EmbedWrapper
				{
					Title = "Guilds"
				};
				foreach (var invite in invites)
				{
					var e = invite.HasGlobalEmotes ? "**Has global emotes**" : "";
					var text = $"**URL:** {invite.Url}\n**Members:** {invite.Guild.MemberCount}\n{e}";
					embed.TryAddField(invite.Guild.Name, text, true, out _);
				}
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
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
				var tf = new TextFileInfo
				{
					Name = "Guilds",
					Text = $"{_GHeader}{_UHeader}{_MHeader}{_EHeader}\n{String.Join("\n", formatted)}",
				};
				await MessageUtils.SendMessageAsync(Context.Channel, "**Guilds:**", textFile: tf).CAF();
			}
			else
			{
				var resp = $"`{invites.Count()}` results returned. Please narrow your search.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
	}
}
