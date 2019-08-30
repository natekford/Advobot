using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to parse permissions.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class PermissionsTypeReader<T> : TypeReader where T : struct, Enum
	{
		private static readonly char[] _SplitChars = new[] { '/', ' ', ',' };
		private static readonly char[] _TrimChars = new[] { '"' };

		/// <summary>
		/// Checks for valid ulong first, then checks permission names.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			//Check numbers first
			if (ulong.TryParse(input, out var rawValue))
			{
				return TypeReaderUtils.FromSuccessAsync(rawValue);
			}
			//Then check permission names
			if (EnumUtils.TryParseFlags(input.Split(_SplitChars).Select(x => x.Trim(_TrimChars)), out T value, out var invalidPerms))
			{
				return TypeReaderUtils.FromSuccessAsync(value);
			}
			return TypeReaderUtils.ParseFailedResultAsync<T>();
		}
	}
}