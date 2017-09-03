using Advobot.Actions;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.TypeReaders
{
	public class GuildPermissionsTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			//Check numbers first
			if (ulong.TryParse(input, out ulong rawValue))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(rawValue));
			}
			//Then check permission names
			else if (!GetActions.TryGetValidGuildPermissionNamesFromInputString(input, out var validPerms, out var invalidPerms))
			{
				var failureStr = FormattingActions.ERROR($"Invalid permission{GetActions.GetPlural(invalidPerms.Count())} provided: `{String.Join("`, `", invalidPerms)}`.");
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, failureStr));
			}
			else
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(GuildActions.ConvertGuildPermissionNamesToUlong(validPerms)));
			}
		}
	}

	public class ChannelPermissionsTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			//Check numbers first
			if (ulong.TryParse(input, out ulong rawValue))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(rawValue));
			}
			//Then check permission names
			else if (!GetActions.TryGetValidChannelPermissionNamesFromInputString(input, out var validPerms, out var invalidPerms))
			{
				var failureStr = FormattingActions.ERROR($"Invalid permission{GetActions.GetPlural(invalidPerms.Count())} provided: `{String.Join("`, `", invalidPerms)}`.");
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, failureStr));
			}
			else
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(ChannelActions.ConvertChannelPermissionNamesToUlong(validPerms)));
			}
		}
	}
}
