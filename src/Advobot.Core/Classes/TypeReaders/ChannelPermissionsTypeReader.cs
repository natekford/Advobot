using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord.Commands;

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
			if (ulong.TryParse(input, out var rawValue))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(rawValue));
			}
			//Then check permission names
			if (ChannelPermsUtils.TryGetValidPermissionNamesFromInputString(input, out var validPerms, out var invalidPerms))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(ChannelPermsUtils.ConvertToValue(validPerms)));
			}

			var perms = invalidPerms.ToList();
			var str = $"Invalid permission{GeneralFormatting.FormatPlural(perms.Count)} provided: `{String.Join("`, `", perms)}`.";
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, str));

		}
	}
}
