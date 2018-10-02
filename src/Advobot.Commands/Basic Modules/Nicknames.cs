using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.TypeReaders;
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
		public async Task Command([ValidateUser] SocketGuildUser user, [Optional, ValidateNickname] string nickname)
		{
			await user.ModifyAsync(x => x.Nickname = nickname ?? user.Username, GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully gave `{user.Format()}` the nickname `{nickname ?? "Nothing"}`.").CAF();
		}
	}

	[Category(typeof(ReplaceWordsInNames)), Group(nameof(ReplaceWordsInNames)), TopLevelShortAlias(typeof(ReplaceWordsInNames))]
	[Summary("Gives users a new nickname if their nickname or username contains the search phrase. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class ReplaceWordsInNames : MultiUserActionModule
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(
			[ValidateNickname] string search,
			[ValidateNickname] string replace,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			=> await Process(bypass, x => IsMatch(x, search), (u, o) => u.ModifyAsync(x => x.Nickname = replace, o)).CAF();

		private bool IsMatch(SocketGuildUser user, string search)
		{
			return (user.Nickname != null && user.Nickname.CaseInsContains(search))
				|| (user.Nickname == null && user.Username.CaseInsContains(search));
		}
	}

	[Category(typeof(ReplaceByUtf16)), Group(nameof(ReplaceByUtf16)), TopLevelShortAlias(typeof(ReplaceByUtf16))]
	[Summary("Replaces nickname/usernames that contain any characters above the supplied character value in UTF-16. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class ReplaceByUtf16 : MultiUserActionModule
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(
			[ValidatePositiveNumber] int upperLimit,
			[ValidateNickname] string replace,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			=> await Process(bypass, x => IsMatch(x, upperLimit), (u, o) => u.ModifyAsync(x => x.Nickname = replace, o)).CAF();

		private bool IsMatch(SocketGuildUser user, int upperLimit)
		{
			return (user.Nickname != null && !user.Nickname.AllCharsWithinLimit(upperLimit))
				|| (user.Nickname == null && !user.Username.AllCharsWithinLimit(upperLimit));
		}
	}

	[Category(typeof(RemoveAllNickNames)), Group(nameof(RemoveAllNickNames)), TopLevelShortAlias(typeof(RemoveAllNickNames))]
	[Summary("Remove all nicknames of users on the guild. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class RemoveAllNickNames : MultiUserActionModule
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			=> await Process(bypass, x => x.Nickname != null, (u, o) => u.ModifyAsync(x => x.Nickname = u.Username, o)).CAF();
	}
}
