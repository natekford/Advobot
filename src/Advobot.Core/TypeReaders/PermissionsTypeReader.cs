
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
			//Check with standard TryParse
			if (Enum.TryParse<T>(input, out var result))
			{
				return TypeReaderResult.FromSuccess(result).AsTask();
			}
			//Then check permission names
			var split = input
				.Split(_SplitChars, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim(_TrimChars));
			if (EnumUtils.TryParseFlags(split, out T value, out var invalidPerms))
			{
				return TypeReaderResult.FromSuccess(value).AsTask();
			}
			return TypeReaderUtils.ParseFailedResult<T>().AsTask();
		}
	}
}