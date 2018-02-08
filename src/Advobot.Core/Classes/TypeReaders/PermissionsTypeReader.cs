using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.TypeReaders
{
	public abstract class PermissionsTypeReader<T> : TypeReader where T : struct, IComparable, IConvertible, IFormattable
	{
		private static char[] _SplitChars = new[] { '/', ' ', ',' };
		private static char[] _TrimChars = new[] { '"' };

		internal PermissionsTypeReader() { }

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
			if (EnumUtils.TryParseFlags(input.Split(_SplitChars).Select(x => x.Trim(_TrimChars)), out T value, out var invalidPerms))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess((ulong)(object)value));
			}

			var perms = invalidPerms.ToList();
			var str = $"Invalid permission{Formatting.FormatPlural(perms.Count)} provided: `{String.Join("`, `", perms)}`.";
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, str));
		}
	}

	/// <summary>
	/// Attempts to get a ulong representing channel permissions.
	/// </summary>
	public sealed class ChannelPermissionsTypeReader : PermissionsTypeReader<ChannelPermission> { }

	/// <summary>
	/// Attempts to get a ulong representing guild permissions.
	/// </summary>
	public sealed class GuildPermissionsTypeReader : PermissionsTypeReader<GuildPermission> { }
}
