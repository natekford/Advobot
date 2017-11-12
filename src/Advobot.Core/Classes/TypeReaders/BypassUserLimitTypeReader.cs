using Advobot.Core.Actions;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to see if the input matches <see cref="BYPASS_STRING"/>.
	/// </summary>
	public sealed class BypassUserLimitTypeReader : TypeReader
	{
		internal const string BYPASS_STRING = "Bypass100";

		/// <summary>
		/// Returns true if the input is equal to <see cref="BYPASS_STRING"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
			=> Task.FromResult(TypeReaderResult.FromSuccess(BYPASS_STRING.CaseInsEquals(input)));
	}
}
