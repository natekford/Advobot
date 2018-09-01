using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Commands.GuildList
{
	[Category(typeof(ModifyGuildListing)), Group(nameof(ModifyGuildListing)), TopLevelShortAlias(typeof(ModifyGuildListing))]
	[Summary("Adds or removes a guild from the public guild list.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	[RequiredServices(typeof(IInviteListService))]
	public sealed class ModifyGuildListing : AdvobotModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(IInvite invite, [Optional] params string[] keywords)
		{
			var inviteList = Context.Provider.GetRequiredService<IInviteListService>();
			if (invite is IInviteMetadata metadata && metadata.MaxAge != null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Don't provide invites that expire.")).CAF();
				return;
			}
			var listedInvite = inviteList.Add(Context.Guild, invite, keywords);
			var resp = $"Successfully set the listed invite to the following:\n{listedInvite}.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			var inviteList = Context.Provider.GetRequiredService<IInviteListService>();
			inviteList.Remove(Context.Guild.Id);
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the listed invite.").CAF();
		}
	}

	[Category(typeof(BumpGuildListing)), Group(nameof(BumpGuildListing)), TopLevelShortAlias(typeof(BumpGuildListing))]
	[Summary("Bumps the invite on the guild.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(false)]
	[RequiredServices(typeof(IInviteListService))]
	public sealed class BumpGuildListing : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var inviteList = Context.Provider.GetRequiredService<IInviteListService>();
			if (!(inviteList.GetListedInvite(Context.Guild.Id) is IListedInvite invite))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("There is no invite to bump.")).CAF();
				return;
			}
			if ((DateTime.UtcNow - invite.Time).TotalHours < 1)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Last bump is too recent.")).CAF();
				return;
			}
			await invite.BumpAsync(Context.Guild).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully bumped the invite.").CAF();
		}
	}

	[Category(typeof(GetGuildListing)), Group(nameof(GetGuildListing)), TopLevelShortAlias(typeof(GetGuildListing))]
	[Summary("Gets an invite meeting the given criteria.")]
	[DefaultEnabled(true)]
	[RequiredServices(typeof(IInviteListService))]
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
			var inviteList = Context.Provider.GetRequiredService<IInviteListService>();
			var invites = obj.GatherInvites(inviteList).ToList();
			if (!invites.Any())
			{
				error = new Error("No guild could be found that matches the given specifications.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			if (invites.Count <= 5)
			{
				var embed = new EmbedWrapper
				{
					Title = "Guilds"
				};
				foreach (var invite in invites)
				{
					var e = invite.HasGlobalEmotes ? "**Has global emotes**" : "";
					var text = $"**URL:** {invite.Url}\n**Members:** {invite.GuildMemberCount}\n{e}";
					embed.TryAddField(invite.GuildName, text, true, out _);
				}
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
				return;
			}
			if (invites.Count <= 50)
			{
				var formatted = invites.Select(x =>
				{
					var n = x.GuildName.Substring(0, Math.Min(x.GuildName.Length, _GHeader.Length)).PadRight(25);
					var u = x.Url.PadRight(35);
					var m = x.GuildMemberCount.ToString().PadRight(14);
					var e = x.HasGlobalEmotes ? "Yes" : "";
					return $"{n}{u}{m}{e}";
				});
				var tf = new TextFileInfo
				{
					Name = "Guilds",
					Text = $"{_GHeader}{_UHeader}{_MHeader}{_EHeader}\n{string.Join("\n", formatted)}",
				};
				await MessageUtils.SendMessageAsync(Context.Channel, "**Guilds:**", textFile: tf).CAF();
				return;
			}
			var resp = $"`{invites.Count}` results returned. Please narrow your search.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}
}
