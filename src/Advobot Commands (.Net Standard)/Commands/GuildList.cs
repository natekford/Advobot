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
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("This guild is already listed."));
				return;
			}
			else if (invite is IInviteMetadata metadata && metadata.MaxAge != null)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Don't provide invites that won't expire."));
				return;
			}

			Context.GuildSettings.ListedInvite = new ListedInvite(invite, keywords);
			var resp = $"Successfully set the listed invite to the following:\n{Context.GuildSettings.ListedInvite}.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp);
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			if (Context.GuildSettings.ListedInvite == null)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("This guild is already unlisted."));
				return;
			}

			Context.GuildSettings.ListedInvite = null;
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the listed invite.");
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
			//Context.
		}
	}
	/*

		[Command("bumpguildlisting")]
		[Alias("bump")]
		[Usage("")]
		[Summary("Bumps the invite on the guild.")]
		[OtherRequirement(Precondition.UserHasAPerm)]
		[DefaultEnabled(false)]
		public async Task BumpInvite()
		{
			var listedInvite = Variables.InviteList.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
			if ((DateTime.UtcNow - listedInvite.LastBumped).TotalHours < 1)
			{
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("Last bump is too recent."));
				return;
			}

			listedInvite.Bump();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully bumped the guild.");
		}

		[Command("getguildlisting")]
		[Alias("ggl")]
		[Usage("<Code:Code> <Name:Name> <GlobalEmotes:True|False> <MoreThan:Number> <LessThan:Number> <\"Keywords:Word/...\">")]
		[Summary("Gets an invite meeting the given criteria.")]
		[DefaultEnabled(true)]
		public async Task GetInvite([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 6), new[] { "code", "name", "globalemotes", "morethan", "lessthan", "keywords" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var codeStr = returnedArgs.GetSpecifiedArg("code");
			var nameStr = returnedArgs.GetSpecifiedArg("name");
			var emoteStr = returnedArgs.GetSpecifiedArg("globalemotes");
			var moreStr = returnedArgs.GetSpecifiedArg("morethan");
			var lessStr = returnedArgs.GetSpecifiedArg("lessthan");
			var keywordStr = returnedArgs.GetSpecifiedArg("keywords");

			var onlyOne = true;
			var matchingInvs = new List<ListedInvite>();
			if (!String.IsNullOrWhiteSpace(codeStr))
			{
				matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => Actions.CaseInsEquals(x.Code, codeStr)).ToList(), onlyOne, out onlyOne);
			}
			if (!String.IsNullOrWhiteSpace(nameStr))
			{
				matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => Actions.CaseInsEquals(x.Guild.Name, nameStr)).ToList(), onlyOne, out onlyOne);
			}
			if (!String.IsNullOrWhiteSpace(emoteStr))
			{
				if (bool.TryParse(emoteStr, out bool emotes))
				{
					matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => x.HasGlobalEmotes == emotes).ToList(), onlyOne, out onlyOne);
				}
				else
				{
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("Unable to get the bool for the globalemotes argument."));
					return;
				}
			}
			if (!String.IsNullOrWhiteSpace(moreStr))
			{
				if (uint.TryParse(moreStr, out uint moreNum))
				{
					matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => x.Guild.MemberCount >= moreNum).ToList(), onlyOne, out onlyOne);
				}
				else
				{
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("Unable to get the number for the morethan argument."));
					return;
				}
			}
			if (!String.IsNullOrWhiteSpace(lessStr))
			{
				if (uint.TryParse(lessStr, out uint lessNum))
				{
					matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => x.Guild.MemberCount <= lessNum).ToList(), onlyOne, out onlyOne);
				}
				else
				{
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("Unable to get the number for the lessthan argument."));
					return;
				}
			}
			if (!String.IsNullOrWhiteSpace(keywordStr))
			{
				var keywords = keywordStr.Split(' ');
				matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => keywords.Any(y => x.Keywords.CaseInsContains(y))).ToList(), onlyOne, out onlyOne);
			}

			if (!matchingInvs.Any())
			{
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("No guild could be found that matches the given specifications."));
				return;
			}
			else if (matchingInvs.Count <= 10)
			{
				var embed = Messages.MakeNewEmbed("Guilds");
				matchingInvs.ForEach(x =>
				{
					Messages.AddField(embed, x.Guild.Name, $"**URL:** {0}\n**Members:** {1}\n{2}", x.URL, x.Guild.MemberCount, x.HasGlobalEmotes ? "**Has global emotes**" : ""));
				});
				await MessageActions.SendEmbedMessageAsync(Context.Channel, embed);
			}
			else if (matchingInvs.Count <= 50)
			{
				var guildName = "Guild Name".PadRight(25);
				var URL = "URL".PadRight(35);
				var memC = "Member Count";
				var emo = "Global Emotes";
				var formatted = matchingInvs.Select(x =>
				{
					return $"{0}{1}{2}{3}",
						x.Guild.Name.Substring(0, Math.Min(x.Guild.Name.Length, guildName.Length)).PadRight(25),
						x.URL.PadRight(35),
						x.Guild.MemberCount.ToString().PadRight(memC.Length),
						x.HasGlobalEmotes ? "  Yes" : "");
				});
				var text = $"{0}{1}{2}  {3}\n{4}", guildName, URL, memC, emo, String.Join("\n", formatted));

				await Actions.WriteAndUploadTextFile(Context.Guild, Context.Channel, text, "Guilds_");
			}
			else
			{
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, Formatting.ERROR("`{0}` results returned. Please narrow your search."));
				return;
			}
		}
	}
	*/
}
