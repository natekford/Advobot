
using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to see if the input matches <see cref="BYPASS_STRING"/>.
	/// </summary>
	public sealed class BypassUserLimitTypeReader : TypeReader
	{
		/// <summary>
		/// What the input must match.
		/// </summary>
		public const string BYPASS_STRING = "Bypass100";

		/// <summary>
		/// Returns true if the input is equal to <see cref="BYPASS_STRING"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
			=> TypeReaderResult.FromSuccess(BYPASS_STRING == input).AsTask();
	}
}