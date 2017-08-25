using Advobot.Actions;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	/*
	[Name("GuildList")]
	public class Advobot_Commands_Guild_List : ModuleBase
	{
		[Command("modifyguildlisting")]
		[Alias("mgl")]
		[Usage("[Add|Remove] [Code] <\"Keywords:Keywords/...>\"")]
		[Summary("Adds a guild to the guild list.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task AddInvite([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 3), new[] { "keywords" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var codeStr = returnedArgs.Arguments[1];
			var keywordStr = returnedArgs.GetSpecifiedArg("keywords");

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			switch (action)
			{
				case ActionType.Add:
				{
					var listedInvite = Variables.InviteList.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
					if (listedInvite != null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("This guild is already listed."));
						return;
					}

					//Make sure the guild has that code
					var invite = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => Actions.CaseInsEquals(x.Code, codeStr));
					if (invite == null)
					{
						if (!Context.Guild.Features.CaseInsContains(Constants.VANITY_URL))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid invite provided."));
							return;
						}
						else
						{
							var restInv = await Variables.Client.GetInviteAsync(codeStr);
							if (restInv.GuildId != Context.Guild.Id)
							{
								await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Please don't try to add other guilds."));
								return;
							}
							codeStr = restInv.Code;
						}
					}
					else if (invite.MaxAge != null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Please only give unexpiring invites."));
						return;
					}
					else
					{
						codeStr = invite.Code;
					}

					var keywords = keywordStr?.Split('/');
					var listedInv = new ListedInvite(Context.Guild.Id, codeStr, keywords);
					Variables.InviteList.ThreadSafeAdd(listedInv);

					if (guildInfo.SetSetting(SettingOnGuild.ListedInvite, listedInv))
					{
						var keywordString = keywords != null ? $" with the keywords `{1}`", String.Join("`, `", keywords)) : "";
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully added the invite `{0}` to the guild list{1}.", codeStr, keywordString));
					}
					else
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Failed to save the listed invite."));
					}
					break;
				}
				case ActionType.Remove:
				{
					var listedInvite = Variables.InviteList.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
					if (listedInvite == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("This guild is not listed."));
						return;
					}
					Variables.InviteList.ThreadSafeRemove(listedInvite);

					if (guildInfo.SetSetting(SettingOnGuild.ListedInvite, null))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the invite on the guild list.");
					}
					else
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Failed to remove the listed invite."));
					}
					break;
				}
			}
		}

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
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Last bump is too recent."));
				return;
			}

			listedInvite.Bump();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully bumped the guild.");
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
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Unable to get the bool for the globalemotes argument."));
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
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Unable to get the number for the morethan argument."));
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
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Unable to get the number for the lessthan argument."));
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
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("No guild could be found that matches the given specifications."));
				return;
			}
			else if (matchingInvs.Count <= 10)
			{
				var embed = Messages.MakeNewEmbed("Guilds");
				matchingInvs.ForEach(x =>
				{
					Messages.AddField(embed, x.Guild.Name, $"**URL:** {0}\n**Members:** {1}\n{2}", x.URL, x.Guild.MemberCount, x.HasGlobalEmotes ? "**Has global emotes**" : ""));
				});
				await MessageActions.SendEmbedMessage(Context.Channel, embed);
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
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("`{0}` results returned. Please narrow your search."));
				return;
			}
		}
	}
	*/
}
