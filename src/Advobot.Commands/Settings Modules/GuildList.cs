using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.GuildList
{
	[Category(typeof(ModifyGuildListing)), Group(nameof(ModifyGuildListing)), TopLevelShortAlias(typeof(ModifyGuildListing))]
	[Summary("Adds or removes a guild from the public guild list.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	[RequireServices(typeof(IInviteListService))]
	public sealed class ModifyGuildListing : AdvobotModuleBase
	{
		public IInviteListService Invites { get; set; }

		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(IInvite invite, [Optional] params string[] keywords)
		{
			if (invite is IInviteMetadata metadata && metadata.MaxAge != null)
			{
				await ReplyErrorAsync(new Error("Don't provide invites that expire.")).CAF();
				return;
			}
			var listedInvite = Invites.Add(Context.Guild, invite, keywords);
			await ReplyTimedAsync($"Successfully set the listed invite to the following:\n{listedInvite}.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			Invites.Remove(Context.Guild.Id);
			await ReplyTimedAsync("Successfully removed the listed invite.").CAF();
		}
	}

	[Category(typeof(BumpGuildListing)), Group(nameof(BumpGuildListing)), TopLevelShortAlias(typeof(BumpGuildListing))]
	[Summary("Bumps the invite on the guild.")]
	[PermissionRequirement(new[] { PermissionRequirementAttribute.GenericPerms }, null)]
	[DefaultEnabled(false)]
	[RequireServices(typeof(IInviteListService))]
	public sealed class BumpGuildListing : AdvobotModuleBase
	{
		public IInviteListService Invites { get; set; }

		[Command]
		public async Task Command()
		{
			if (!(Invites.GetListedInvite(Context.Guild.Id) is IListedInvite invite))
			{
				await ReplyErrorAsync(new Error("There is no invite to bump.")).CAF();
				return;
			}
			if ((DateTime.UtcNow - invite.Time).TotalHours < 1)
			{
				await ReplyErrorAsync(new Error("Last bump is too recent.")).CAF();
				return;
			}
			await invite.BumpAsync(Context.Guild).CAF();
			await ReplyTimedAsync("Successfully bumped the invite.").CAF();
		}
	}

	[Category(typeof(GetGuildListing)), Group(nameof(GetGuildListing)), TopLevelShortAlias(typeof(GetGuildListing))]
	[Summary("Gets an invite meeting the given criteria.")]
	[DefaultEnabled(true)]
	[RequireServices(typeof(IInviteListService))]
	public sealed class GetGuildListing : AdvobotModuleBase
	{
		public IInviteListService Invites { get; set; }

		private static readonly string _GHeader = "Guild Name".PadRight(25);
		private static readonly string _UHeader = "URL".PadRight(35);
		private static readonly string _MHeader = "Member Count".PadRight(14);
		private static readonly string _EHeader = "Global Emotes";

		[Command]
		public async Task Command([Remainder] ListedInviteGatherer args)
		{
			var invites = args.GatherInvites(Invites).ToList();
			if (!invites.Any())
			{
				await ReplyErrorAsync(new Error("No guild could be found that matches the given specifications.")).CAF();
				return;
			}
			if (invites.Count <= 5)
			{
				await ReplyEmbedAsync(new EmbedWrapper
				{
					Title = "Guilds",
					Fields = invites.Select(x =>
					{
						var e = x.HasGlobalEmotes ? "**Has global emotes**" : "";
						var text = $"**URL:** {x.Url}\n**Members:** {x.GuildMemberCount}\n{e}";
						return new EmbedFieldBuilder { Name = x.GuildName, Value = text, IsInline = true, };
					}).ToList(),
				}).CAF();
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
				await ReplyFileAsync("**Guilds:**", new TextFileInfo
				{
					Name = "Guilds",
					Text = $"{_GHeader}{_UHeader}{_MHeader}{_EHeader}\n{string.Join("\n", formatted)}",
				}).CAF();
				return;
			}
			await ReplyTimedAsync($"`{invites.Count}` results returned. Please narrow your search.").CAF();
		}
	}
}
