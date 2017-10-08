using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to see if the input matches <see cref="Constants.BYPASS_STRING"/>.
	/// </summary>
	internal class BypassUserLimitTypeReader : TypeReader
	{
		/// <summary>
		/// Returns true if the input is equal to <see cref="Constants.BYPASS_STRING"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			return Task.FromResult(TypeReaderResult.FromSuccess(Constants.BYPASS_STRING.CaseInsEquals(input)));
		}
	}
}
