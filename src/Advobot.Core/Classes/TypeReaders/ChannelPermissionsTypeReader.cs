using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes.Permissions;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to get a ulong representing channel permissions.
	/// </summary>
	public sealed  class ChannelPermissionsTypeReader : TypeReader
	{
		/// <summary>
		/// Checks for valid ulong first, then checks permission names.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			//Check numbers first
			if (ulong.TryParse(input, out ulong rawValue))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(rawValue));
			}
			//Then check permission names
			else if (!ChannelPerms.TryGetValidPermissionNamesFromInputString(input, out var validPerms, out var invalidPerms))
			{
				var str = $"Invalid permission{GeneralFormatting.FormatPlural(invalidPerms.Count())} provided: `{String.Join("`, `", invalidPerms)}`.";
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, str));
			}
			else
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(ChannelPerms.ConvertToValue(validPerms)));
			}
		}
	}
}
