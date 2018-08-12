using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Nicknames
{
	[Category(typeof(ModifyNickName)), Group(nameof(ModifyNickName)), TopLevelShortAlias(typeof(ModifyNickName))]
	[Summary("Gives the user a nickname. " +
		"Inputting no nickname resets their nickname.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyNickName : AdvobotModuleBase
	{
		[Command]
		public async Task Command(
			[VerifyObject(false, Verif.CanBeEdited)] SocketGuildUser user,
			[Optional, VerifyStringLength(Target.Nickname)] string nickname)
		{
			await user.ModifyAsync(x => x.Nickname = nickname ?? user.Username, GetRequestOptions()).CAF();
			var response = nickname == null
				? $"Successfully removed the nickname from `{user.Format()}`."
				: $"Successfully gave `{user.Format()}` the nickname `{nickname}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, response).CAF();
		}
	}

	[Category(typeof(ReplaceWordsInNames)), Group(nameof(ReplaceWordsInNames)), TopLevelShortAlias(typeof(ReplaceWordsInNames))]
	[Summary("Gives users a new nickname if their nickname or username contains the search phrase. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class ReplaceWordsInNames : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(
			[VerifyStringLength(Target.Nickname)] string search,
			[VerifyStringLength(Target.Nickname)] string replace,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = Context.Guild.GetEditableUsers(Context.User as SocketGuildUser).Where(x =>
			{
				return (x.Nickname != null && x.Nickname.CaseInsContains(search))
					|| (x.Nickname == null && x.Username.CaseInsContains(search));
			}).Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await new MultiUserAction(Context, users).ModifyNicknamesAsync(replace, GetRequestOptions()).CAF();
		}
	}

	[Category(typeof(ReplaceByUtf16)), Group(nameof(ReplaceByUtf16)), TopLevelShortAlias(typeof(ReplaceByUtf16))]
	[Summary("Replaces nickname/usernames that contain any characters above the supplied character value in UTF-16. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class ReplaceByUtf16 : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(
			uint upperLimit,
			[VerifyStringLength(Target.Nickname)] string replace,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = Context.Guild.GetEditableUsers(Context.User as SocketGuildUser).Where(x =>
			{
				return (x.Nickname != null && !x.Nickname.AllCharsWithinLimit((int)upperLimit))
				|| (x.Nickname == null && !x.Username.AllCharsWithinLimit((int)upperLimit));
			}).Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await new MultiUserAction(Context, users).ModifyNicknamesAsync(replace, GetRequestOptions()).CAF();
		}
	}

	[Category(typeof(RemoveAllNickNames)), Group(nameof(RemoveAllNickNames)), TopLevelShortAlias(typeof(RemoveAllNickNames))]
	[Summary("Remove all nicknames of users on the guild. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class RemoveAllNickNames : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = Context.Guild.GetEditableUsers(Context.User as SocketGuildUser)
				.Where(x => x.Nickname != null).Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await new MultiUserAction(Context, users).ModifyNicknamesAsync(null, GetRequestOptions()).CAF();
		}
	}
}
