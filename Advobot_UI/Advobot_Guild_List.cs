using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Guild_List")]
	public class Advobot_Guild_List : ModuleBase
	{
		[Command("guildlistmodify")]
		[Alias("glm")]
		[Usage("[Add|Remove] [Code] <Keywords ...>")]
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

			var inputArray = input.Split(new char[] { ' ' }, 3);
			if (inputArray.Length < 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(""));
				return;
			}
			var action = inputArray[0];

			if (Actions.CaseInsEquals(action, "add"))
			{
				var listedInvite = Variables.InviteList.FirstOrDefault(x => x.GuildID == Context.Guild.Id);
				if (listedInvite != null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild is already listed."));
					return;
				}

				var code = inputArray[1];
				ulong guildID = 0;

				//Make sure the guild has that code
				var invite = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => Actions.CaseInsEquals(x.Code, code));
				if (invite == null)
				{
					if (!Actions.CaseInsContains(Context.Guild.Features.ToList(), Constants.VANITY_URL))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid invite provided."));
						return;
					}
					else
					{
						var restInv = await Variables.Client.GetInviteAsync(code);
						if (restInv.GuildId != Context.Guild.Id)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please don't try to add other guilds."));
							return;
						}
						guildID = restInv.GuildId;
						code = restInv.Code;
					}
				}
				else if (invite.MaxAge != null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please only give unexpiring invites."));
					return;
				}
				else
				{
					guildID = invite.GuildId;
					code = invite.Code;
				}

				var keywords = inputArray.Length == 3 ? inputArray[2].Split(' ') : null;
				var listedInv = new ListedInvite(guildID, code, keywords);
				Variables.InviteList.ThreadSafeAdd(listedInv);

				guildInfo.SetListedInvite(listedInv);
				Actions.SaveGuildInfo(guildInfo);

				var keywordString = keywords != null ? String.Format(" with the keywords `{1}`", String.Join("`, `", keywords)) : "";
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully added the invite `{0}`{1} to the guild list.", code, keywordString));
			}
			else if (Actions.CaseInsEquals(action, "remove"))
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
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}
		}

		[Command("guildlistbump")]
		[Alias("glb")]
		[Usage("")]
		[Summary("Bumps the invite on the guild.")]
		[UserHasAPermission]
		[DefaultEnabled(false)]
		public async Task BumpInvite()
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

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
		[Usage("<Code:code> <Name:name> <GlobalEmotes> <MoreThan:size> <LessThan:size> <\"Keywords:word ...\">")]
		[Summary("Gets an invite meeting the given criteria.")]
		[DefaultEnabled(true)]
		public async Task GetInvite([Remainder] string input)
		{
			if (!String.IsNullOrWhiteSpace(input))
			{
				var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
				var code = Actions.GetVariable(inputArray, "code");
				var name = Actions.GetVariable(inputArray, "name");
				var emotes = Actions.CaseInsContains(inputArray, "globalemotes");
				var more = Actions.GetVariable(inputArray, "morethan");
				var less = Actions.GetVariable(inputArray, "lessthan");
				var keyword = Actions.GetVariable(inputArray, "keywords");

				var onlyOne = true;
				var matchingInvs = new List<ListedInvite>();
				if (!String.IsNullOrWhiteSpace(code))
				{
					matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => Actions.CaseInsEquals(x.Code, code)).ToList(), onlyOne, out onlyOne);
				}
				if (!String.IsNullOrWhiteSpace(name))
				{
					matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => Actions.CaseInsEquals(x.Guild.Name, name)).ToList(), onlyOne, out onlyOne);
				}
				if (emotes)
				{
					matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => x.HasGlobalEmotes).ToList(), onlyOne, out onlyOne);
				}
				if (!String.IsNullOrWhiteSpace(more) && uint.TryParse(more, out uint moreNum))
				{
					matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => x.Guild.MemberCount >= moreNum).ToList(), onlyOne, out onlyOne);
				}
				if (!String.IsNullOrWhiteSpace(less) && uint.TryParse(less, out uint lessNum))
				{
					matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => x.Guild.MemberCount <= lessNum).ToList(), onlyOne, out onlyOne);
				}
				if (!String.IsNullOrWhiteSpace(keyword))
				{
					var keywords = keyword.Split(' ');
					matchingInvs = Actions.GetMatchingInvites(matchingInvs, Variables.InviteList.Where(x => keywords.Any(y => Actions.CaseInsContains(x.Keywords, y))).ToList(), onlyOne, out onlyOne);
				}

				if (!matchingInvs.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No guild could be found that matches the given specifications."));
					return;
				}
				else if (matchingInvs.Count <= 25)
				{
					var embed = Actions.MakeNewEmbed("Guilds");
					matchingInvs.ForEach(x =>
					{
						Actions.AddField(embed, x.Guild.Name, String.Format("**URL:** {0}\n**Members:** {1}\n{2}", x.URL, x.Guild.MemberCount, x.HasGlobalEmotes ? "**Has global emotes**" : ""));
					});
					await Actions.SendEmbedMessage(Context.Channel, embed);
				}
				else if (matchingInvs.Count <= 100)
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
					Actions.TryToUploadToHastebin(text, out string erl);

					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Guilds", erl));
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("`{0}` results returned. Please narrow your search."));
					return;
				}
			}
		}
	}
}
