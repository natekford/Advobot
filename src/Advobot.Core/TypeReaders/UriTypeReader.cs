using System;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to find an image url from the given context.
	/// </summary>
	[TypeReaderTargetType(typeof(Uri))]
	public sealed class UriTypeReader : TypeReader
	{
		/// <summary>
		/// Checks if the input is a valid uri.
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
			if (Uri.TryCreate(input, UriKind.Absolute, out var url))
			{
				return TypeReaderUtils.FromSuccess(url).AsTask();
			}
			return TypeReaderUtils.ParseFailedResult<Uri>().AsTask();
		}
	}
}