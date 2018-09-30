using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
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
			[ValidateUser(Verif.CanBeEdited)] SocketGuildUser user,
			[Optional, ValidateNickname] string nickname)
		{
			await user.ModifyAsync(x => x.Nickname = nickname ?? user.Username, GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully gave `{user.Format()}` the nickname `{nickname ?? "Nothing"}`.").CAF();
		}
	}

#warning a lot of the guts of the following three commands is very similar, can try to put together
	[Category(typeof(ReplaceWordsInNames)), Group(nameof(ReplaceWordsInNames)), TopLevelShortAlias(typeof(ReplaceWordsInNames))]
	[Summary("Gives users a new nickname if their nickname or username contains the search phrase. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class ReplaceWordsInNames : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(
			[ValidateNickname] string search,
			[ValidateNickname] string replace,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = Context.Guild.GetEditableUsers(Context.User as SocketGuildUser).Where(x =>
			{
				return (x.Nickname != null && x.Nickname.CaseInsContains(search))
					|| (x.Nickname == null && x.Username.CaseInsContains(search));
			}).Take(bypass ? int.MaxValue : BotSettings.MaxUserGatherCount);
			await new MultiUserActionModule(Context, users).ModifyNicknamesAsync(replace, GenerateRequestOptions()).CAF();
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
			[ValidateNickname] string replace,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = Context.Guild.GetEditableUsers(Context.User as SocketGuildUser).Where(x =>
			{
				return (x.Nickname != null && !x.Nickname.AllCharsWithinLimit((int)upperLimit))
					|| (x.Nickname == null && !x.Username.AllCharsWithinLimit((int)upperLimit));
			}).Take(bypass ? int.MaxValue : BotSettings.MaxUserGatherCount);
			await new MultiUserActionModule(Context, users).ModifyNicknamesAsync(replace, GenerateRequestOptions()).CAF();
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
				.Where(x => x.Nickname != null).Take(bypass ? int.MaxValue : BotSettings.MaxUserGatherCount);
			await new MultiUserActionModule(Context, users).ModifyNicknamesAsync(null, GenerateRequestOptions()).CAF();
		}
	}
}
