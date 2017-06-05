using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Guild_List")]
	public class Advobot_Commands_Guild_List : ModuleBase
	{
		[Command("guildlistmodify")]
		[Alias("glm")]
		[Usage("[Add|Remove] [Code] <\"Keywords:Keywords/...>\"")]
		[Summary("Adds a guild to the guild list.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task AddInvite([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 3), new[] { "keywords" });
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var codeStr = returnedArgs.Arguments[1];
			var keywordStr = returnedArgs.GetSpecifiedArg("keywords");

			var returnedType = Actions.GetType(actionStr, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Type;

			switch (action)
			{
				case ActionType.Add:
				{
					var listedInvite = Variables.InviteList.FirstOrDefault(x => x.GuildID == Context.Guild.Id);
					if (listedInvite != null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild is already listed."));
						return;
					}

					//Make sure the guild has that code
					var invite = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => Actions.CaseInsEquals(x.Code, codeStr));
					if (invite == null)
					{
						if (!Context.Guild.Features.CaseInsContains(Constants.VANITY_URL))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid invite provided."));
							return;
						}
						else
						{
							var restInv = await Variables.Client.GetInviteAsync(codeStr);
							if (restInv.GuildId != Context.Guild.Id)
							{
								await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please don't try to add other guilds."));
								return;
							}
							codeStr = restInv.Code;
						}
					}
					else if (invite.MaxAge != null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please only give unexpiring invites."));
						return;
					}
					else
					{
						codeStr = invite.Code;
					}

					var keywords = keywordStr?.Split('/');
					var listedInv = new ListedInvite(Context.Guild.Id, codeStr, keywords);
					Variables.InviteList.ThreadSafeAdd(listedInv);
					guildInfo.SetListedInvite(listedInv);
					Actions.SaveGuildInfo(guildInfo);

					var keywordString = keywords != null ? String.Format(" with the keywords `{1}`", String.Join("`, `", keywords)) : "";
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully added the invite `{0}` to the guild list{1}.", codeStr, keywordString));
					break;
				}
				case ActionType.Remove:
				{
					var listedInvite = Variables.InviteList.FirstOrDefault(x => x.GuildID == Context.Guild.Id);
					if (listedInvite == null)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild is not listed."));
						return;
					}
					Variables.InviteList.ThreadSafeRemove(listedInvite);

					guildInfo.SetListedInvite(null);
					Actions.SaveGuildInfo(guildInfo);
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the invite on the guild list.");
					break;
				}
			}
		}

		[Command("guildlistbump")]
		[Alias("glb")]
		[Usage("")]
		[Summary("Bumps the invite on the guild.")]
		[OtherRequirement(1U << (int)Precondition.User_Has_A_Perm)]
		[DefaultEnabled(false)]
		public async Task BumpInvite()
		{
			var listedInvite = Variables.InviteList.FirstOrDefault(x => x.GuildID == Context.Guild.Id);
			if ((DateTime.UtcNow - listedInvite.LastBumped).TotalHours < 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Last bump is too recent."));
				return;
			}

			listedInvite.Bump();
			await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully bumped the guild.");
		}

		[Command("guildlistget")]
		[Alias("glg")]
		[Usage("<Code:Code> <Name:Name> <GlobalEmotes:True|False> <MoreThan:Number> <LessThan:Number> <\"Keywords:Word/...\">")]
		[Summary("Gets an invite meeting the given criteria.")]
		[DefaultEnabled(true)]
		public async Task GetInvite([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 6), new[] { "code", "name", "globalemotes", "morethan", "lessthan", "keywords" });
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
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
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to get the bool for the globalemotes argument."));
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
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to get the number for the morethan argument."));
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
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to get the number for the lessthan argument."));
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
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No guild could be found that matches the given specifications."));
				return;
			}
			else if (matchingInvs.Count <= 10)
			{
				var embed = Actions.MakeNewEmbed("Guilds");
				matchingInvs.ForEach(x =>
				{
					Actions.AddField(embed, x.Guild.Name, String.Format("**URL:** {0}\n**Members:** {1}\n{2}", x.URL, x.Guild.MemberCount, x.HasGlobalEmotes ? "**Has global emotes**" : ""));
				});
				await Actions.SendEmbedMessage(Context.Channel, embed);
			}
			else if (matchingInvs.Count <= 50)
			{
				var guildName = "Guild Name".PadRight(25);
				var URL = "URL".PadRight(35);
				var memC = "Member Count";
				var emo = "Global Emotes";
				var formatted = matchingInvs.Select(x =>
				{
					return String.Format("{0}{1}{2}{3}",
						x.Guild.Name.Substring(0, Math.Min(x.Guild.Name.Length, guildName.Length)).PadRight(25),
						x.URL.PadRight(35),
						x.Guild.MemberCount.ToString().PadRight(memC.Length),
						x.HasGlobalEmotes ? "  Yes" : "");
				});
				var text = String.Format("{0}{1}{2}  {3}\n{4}", guildName, URL, memC, emo, String.Join("\n", formatted));

				if (Actions.TryToUploadToHastebin(text, out string erl))
				{
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Guilds", erl));
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(erl));
				}
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("`{0}` results returned. Please narrow your search."));
				return;
			}
		}
	}
}
